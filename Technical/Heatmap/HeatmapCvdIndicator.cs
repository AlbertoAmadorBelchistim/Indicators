namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATAS.Indicators.Heatmap;
using OFT.Rendering.Heatmap;
using static ATAS.Indicators.Technical.Heatmap.HeatmapSettingsColorHelpers;

[HeatmapIndicator("heatmap.cvd")]
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
		var build = HeatmapIndicator.Describe("heatmap.cvd", "CVD");
		_panel = build.SubPanelScalar("cvd.panel", "CVD");
		_value = _panel.Series<long>(
			"cvd.value",
			HeatmapIndicatorSeriesRole.Scalar,
			HeatmapIndicatorValueKind.Integer,
			metricId: ValueMetricId,
			unit: "lots");
		_descriptor = build.Done();
	}

	#endregion

	#region Readonly initialized fields

	private readonly HeatmapSeriesBuffer<long> _samples = new();
	private readonly HeatmapIndicatorFallbackReWarmGuard _reWarmGuard = new();

	#endregion

	#region Fields

	private HeatmapCvdSettings _settings = new();
	private HeatmapIndicatorContext _context = new()
	{
		InstrumentId = string.Empty,
		TickSize = 0m,
		LotSize = 1m,
		TimeZone = TimeZoneInfo.Local,
	};
	private HeatmapPeriodKey _currentPeriodKey = HeatmapPeriodKey.Empty;
	private decimal _delta;
	private CvdCalculationMode _configuredMode = CvdCalculationMode.FromDataStart;
	private decimal _configuredLotSize = 1m;
	private IHeatmapIndicatorRuntime? _runtime;

	#endregion

	#region Properties

	public override HeatmapIndicatorDescriptor Descriptor => _descriptor;

	#endregion

	#region Public methods

	public override ValueTask ConfigureAsync(HeatmapCvdSettings settings, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		ArgumentNullException.ThrowIfNull(settings);

		var lotSize = NormalizeLotSize(settings.LotSize);
		if (_configuredMode != settings.CalculationMode || _configuredLotSize != lotSize)
		{
			_samples.Clear();
			_delta = 0m;
			_currentPeriodKey = HeatmapPeriodKey.Empty;
		}

		_settings = settings;
		_configuredMode = settings.CalculationMode;
		_configuredLotSize = lotSize;

		return ValueTask.CompletedTask;
	}

	public override ValueTask ResetAsync(
		HeatmapIndicatorContext context,
		IHeatmapIndicatorRuntime runtime,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_context = context;
		_runtime = runtime;
		_samples.Clear();
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
		IReadOnlyList<HeatmapTradeTick> ticks,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		ProcessHistoricalTicks(ticks);

		if (RequiresDataStartAnchor(_settings.CalculationMode) &&
		    _runtime is { } runtime &&
		    _reWarmGuard.ShouldRequestReWarm(ticks))
		{
			await runtime
				.RequestReWarmAsync("cvd: data-start became known", cancellationToken)
				.ConfigureAwait(false);
		}
	}

	public void ProcessHistoricalTicks(IEnumerable<HeatmapTradeTick> ticks)
	{
		var orderedTicks = ticks
			.Where(IsEligibleTick)
			.OrderBy(static tick => tick.TimestampNanos)
			.ToArray();

		_samples.Clear();
		_delta = 0m;
		_currentPeriodKey = HeatmapPeriodKey.Empty;

		foreach (var tick in orderedTicks)
			ProcessTickCore(tick);
	}

	public void ProcessTick(HeatmapTradeTick tick)
	{
		if (!IsEligibleTick(tick))
			return;

		ProcessTickCore(tick);
	}

	public override ValueTask<HeatmapIndicatorStateSnapshot?> GetSnapshotAsync(
		HeatmapIndicatorSnapshotRequest request,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var source = _samples.GetSnapshot(request);
		if (_settings.CalculationMode == CvdCalculationMode.FromVisibleRangeStart && source.Samples.Count > 0)
		{
			var baseline = request.ViewStartTimeNanos > 0
				? _samples.GetLatestAtOrBefore(request.ViewStartTimeNanos)?.Value ?? 0
				: 0;

			if (baseline != 0)
			{
				var normalized = source.Samples
					.Select(sample => new HeatmapIndicatorSample<long>(
						sample.TimestampNanos,
						sample.Value - baseline))
					.ToArray();

				source = new HeatmapIndicatorSeriesSnapshot<long>(normalized, source.IsTraining);
			}
		}

		var presentation = new HeatmapIndicatorVisualPresentation(
			PanelHeight: _settings.PanelHeight,
			PanelRenderMode: HeatmapIndicatorPanelRenderMode.PositiveNegativeScalar,
			ScalarScaleMode: HeatmapIndicatorScalarScaleMode.AutoVisibleSymmetric,
			ReferenceValue: 0f);
		var style = new HeatmapIndicatorVisualStyle(Color: ToHex(_settings.Color));

		var state = _descriptor.NewState()
			.Training(source.IsTraining)
			.Visual(_panel, v => v
				.PresentationOverride(presentation)
				.Series(_value, source, sample => sample, style))
			.Build();

		return new ValueTask<HeatmapIndicatorStateSnapshot?>(state);
	}

	#endregion

	#region Private methods

	private void ProcessTickCore(HeatmapTradeTick tick)
	{
		var scope = _settings.CalculationMode switch
		{
			CvdCalculationMode.CurrentDay => HeatmapProfileScope.CurrentDay,
			CvdCalculationMode.CurrentWeek => HeatmapProfileScope.CurrentWeek,
			_ => HeatmapProfileScope.DataStart
		};

		var key = HeatmapPeriodResolver.Resolve(tick.Time, _context, scope);
		if (_currentPeriodKey != key)
		{
			_currentPeriodKey = key;
			_delta = 0m;
		}

		_delta += tick.Direction == HeatmapTradeDirection.Buy
			? tick.Volume
			: -tick.Volume;

		_samples.Append(tick.TimestampNanos, ToLots(_delta, _configuredLotSize));
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
