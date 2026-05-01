namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATAS.Indicators.Heatmap;
using OFT.Rendering.Heatmap;
using static ATAS.Indicators.Technical.Heatmap.HeatmapSettingsColorHelpers;

// Reference indicator: tick-driven sub-panel scalar (Cumulative Volume Delta).
// Patterns: SubPanelScalar visual with persistent State.BeginUpdate lease,
//           presentation override with positive/negative scalar render mode,
//           HeatmapIndicatorFallbackReWarmGuard for deferred re-warm when
//           the data start becomes known, settings-aware state-reset
//           request in ConfigureAsync.
// Copy as a starting point for any sub-panel scalar that anchors at the
// chart data start and can re-warm itself once the host narrows the range.
[HeatmapIndicator(id: "heatmap.cvd", DisplayName = "CVD")]
public sealed class HeatmapCvdIndicator
	: HeatmapIndicator<HeatmapCvdSettings>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
	#region Const fields

	public const string ValueMetricId = "cvd.value";

	#endregion

	#region Static fields

	private static readonly HeatmapIndicatorDescriptor _descriptor;
	private static readonly HeatmapIndicatorVisualHandle _panel;
	private static readonly HeatmapIndicatorSeriesHandle<long> _value;

	#endregion

	#region Static constructors

	static HeatmapCvdIndicator()
	{
		var build = Describe<HeatmapCvdIndicator>();
		_panel = build.SubPanelScalar("cvd.panel", "CVD");
		_value = _panel.Series<long>(
			"cvd.value",
			HeatmapIndicatorSeriesRole.Scalar,
			HeatmapIndicatorValueKind.Integer,
			value => value,
			metricId: ValueMetricId,
			unit: "lots");
		_descriptor = build.Done();
	}

	#endregion

	#region Readonly initialized fields

	private readonly HeatmapIndicatorFallbackReWarmGuard _reWarmGuard = new();

	#endregion

	#region Fields

	private HeatmapCvdSettings _settings = new();
	private HeatmapPeriodKey _currentPeriodKey = HeatmapPeriodKey.Empty;
	private decimal _delta;
	private CvdCalculationMode _configuredMode = CvdCalculationMode.FromDataStart;
	private decimal _configuredLotSize = 1m;

	#endregion

	#region Properties

	public override HeatmapIndicatorDescriptor Descriptor => _descriptor;

	#endregion

	#region Public methods

	public override async ValueTask ConfigureAsync(HeatmapCvdSettings settings, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		ArgumentNullException.ThrowIfNull(settings);

		var lotSize = NormalizeLotSize(settings.LotSize);
		var needsRebuild = _configuredMode != settings.CalculationMode || _configuredLotSize != lotSize;

		_settings = settings;
		_configuredMode = settings.CalculationMode;
		_configuredLotSize = lotSize;

		ApplyVisualOverrides();

		if (needsRebuild && Runtime is { } runtime)
		{
			await runtime
				.RequestStateResetAsync("cvd: calculation parameters changed", cancellationToken)
				.ConfigureAwait(false);
		}
	}

	protected override ValueTask OnStateResetCoreAsync(
		IHeatmapIndicatorContext context,
		IHeatmapIndicatorRuntime runtime,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_delta = 0m;
		_currentPeriodKey = HeatmapPeriodKey.Empty;
		_reWarmGuard.Reset();

		return ValueTask.CompletedTask;
	}

	public async ValueTask WarmUpAsync(
		HeatmapIndicatorWarmupRequest request,
		IHeatmapIndicatorDataSources dataSources,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_reWarmGuard.OnWarmedUp(request);

		var ticks = await dataSources.Trades.GetTradeTicksAsync(request, cancellationToken).ConfigureAwait(false);
		cancellationToken.ThrowIfCancellationRequested();
		ProcessHistoricalTicks(ticks);
	}

	public async ValueTask ProcessTicksAsync(
		HeatmapTickBatch ticks,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		AppendTicksToState(ticks, clearExisting: false);

		if (RequiresDataStartAnchor(_settings.CalculationMode) &&
		    Runtime is { } runtime &&
		    _reWarmGuard.ShouldRequestReWarm(ticks))
		{
			await runtime
				.RequestReWarmAsync("cvd: data-start became known", cancellationToken)
				.ConfigureAwait(false);
		}
	}

	public void ProcessHistoricalTicks(HeatmapTickBatch ticks) =>
		AppendTicksToState(ticks, clearExisting: true);

	#endregion

	#region Private methods

	private void AppendTicksToState(HeatmapTickBatch ticks, bool clearExisting)
	{
		var span = ticks.AsSpan();
		var ordered = new List<HeatmapTradeTick>(span.Length);
		for (var i = 0; i < span.Length; i++)
		{
			if (IsEligibleTick(span[i]))
				ordered.Add(span[i]);
		}
		ordered.Sort(static (left, right) => left.TimestampNanos.CompareTo(right.TimestampNanos));

		if (!clearExisting && ordered.Count == 0)
			return;

		using var lease = State.BeginUpdate();
		var visualLease = lease.Visual(_panel);
		ApplyVisualOverridesUnderLease(visualLease);
		var seriesLease = visualLease.Series(_value);

		if (clearExisting)
		{
			seriesLease.Clear();
			_delta = 0m;
			_currentPeriodKey = HeatmapPeriodKey.Empty;
		}

		foreach (var tick in ordered)
		{
			ApplyTick(tick);
			seriesLease.Append(tick.TimestampNanos, ToLots(_delta, _configuredLotSize));
		}
	}

	private void ApplyTick(HeatmapTradeTick tick)
	{
		var scope = _settings.CalculationMode switch
		{
			CvdCalculationMode.CurrentDay => HeatmapProfileScope.CurrentDay,
			CvdCalculationMode.CurrentWeek => HeatmapProfileScope.CurrentWeek,
			_ => HeatmapProfileScope.DataStart
		};

		var key = HeatmapPeriodResolver.Resolve(tick.Time, Context, scope);
		if (_currentPeriodKey != key)
		{
			_currentPeriodKey = key;
			_delta = 0m;
		}

		_delta += tick.Direction == HeatmapTradeDirection.Buy
			? tick.Volume
			: -tick.Volume;
	}

	private void ApplyVisualOverrides()
	{
		using var lease = State.BeginUpdate();
		ApplyVisualOverridesUnderLease(lease.Visual(_panel));
	}

	private void ApplyVisualOverridesUnderLease(IHeatmapVisualLease visualLease)
	{
		visualLease.Presentation = new HeatmapIndicatorVisualPresentation(
			PanelHeight: _settings.PanelHeight,
			PanelRenderMode: HeatmapIndicatorPanelRenderMode.PositiveNegativeScalar,
			ScalarScaleMode: HeatmapIndicatorScalarScaleMode.AutoVisibleSymmetric,
			ReferenceValue: 0f);
		visualLease.Style = new HeatmapIndicatorVisualStyle(Color: ToHex(_settings.Color));
	}

	#endregion

	#region Private static methods

	private static bool RequiresDataStartAnchor(CvdCalculationMode mode) =>
		mode is CvdCalculationMode.FromDataStart or CvdCalculationMode.FromVisibleRangeStart;

	private static long ToLots(decimal delta, decimal lotSize) =>
		(long)decimal.Round(delta / NormalizeLotSize(lotSize), 0, MidpointRounding.AwayFromZero);

	private static decimal NormalizeLotSize(decimal lotSize) =>
		lotSize <= 0 ? 1m : lotSize;

	private static bool IsEligibleTick(HeatmapTradeTick tick) =>
		tick.TimestampNanos > 0 &&
		tick.Volume > 0 &&
		(tick.Direction == HeatmapTradeDirection.Buy || tick.Direction == HeatmapTradeDirection.Sell);

	#endregion
}
