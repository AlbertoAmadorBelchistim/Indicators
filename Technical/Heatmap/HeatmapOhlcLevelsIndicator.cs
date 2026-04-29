namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATAS.Indicators.Heatmap;
using OFT.Rendering.Heatmap;

[HeatmapIndicator("heatmap.ohlc-levels", DisplayName = "OHLC Levels")]
public sealed class HeatmapOhlcLevelsIndicator
	: HeatmapIndicator<HeatmapOhlcLevelsSettings>
	, IHeatmapProfileConsumer
{
	#region Const fields

	private const string IndicatorTypeId = "heatmap.ohlc-levels";
	private const string LevelsVisualId = "ohlc-levels.lines";

	#endregion

	#region Static fields

	private static readonly HeatmapIndicatorDescriptor _descriptor;
	private static readonly HeatmapIndicatorVisualHandle _lines;
	private static readonly Dictionary<HeatmapOhlcLevelKind, HeatmapIndicatorSeriesHandle<decimal>> _seriesByKind;

	#endregion

	#region Static constructors

	static HeatmapOhlcLevelsIndicator()
	{
		var build = HeatmapIndicator.Describe(IndicatorTypeId, "OHLC Levels");
		_lines = build.LevelLine(LevelsVisualId, "OHLC Levels");
		_seriesByKind = new Dictionary<HeatmapOhlcLevelKind, HeatmapIndicatorSeriesHandle<decimal>>();
		foreach (var kind in Enum.GetValues<HeatmapOhlcLevelKind>())
		{
			_seriesByKind[kind] = _lines.Series<decimal>(
				$"ohlc-levels.{kind}",
				HeatmapIndicatorSeriesRole.Level,
				HeatmapIndicatorValueKind.Price);
		}
		_descriptor = build.Done();
	}

	#endregion

	#region Readonly initialized fields

	private readonly object _sync = new();
	private readonly Dictionary<HeatmapProfilePeriod, HeatmapMarketProfileSnapshot> _profiles = new();

	#endregion

	#region Fields

	private HeatmapOhlcLevelsSettings _settings = new();

	#endregion

	#region Properties

	public override HeatmapIndicatorDescriptor Descriptor => _descriptor;

	#endregion

	#region Public methods

	public override ValueTask ConfigureAsync(HeatmapOhlcLevelsSettings settings, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		lock (_sync)
			_settings = settings;

		return ValueTask.CompletedTask;
	}

	public override ValueTask ResetAsync(
		HeatmapIndicatorContext context,
		IHeatmapIndicatorRuntime runtime,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		lock (_sync)
			_profiles.Clear();

		return ValueTask.CompletedTask;
	}

	public override ValueTask<HeatmapIndicatorStateSnapshot?> GetSnapshotAsync(
		HeatmapIndicatorSnapshotRequest request,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var resolved = new List<(HeatmapOhlcLevelSettings Level, decimal Price)>(_settings.Levels.Count);

		lock (_sync)
		{
			foreach (var level in _settings.Levels)
			{
				if (level.Style?.IsVisible == false)
					continue;

				if (!TryResolvePrice(level.Kind, out var price) || price <= 0)
					continue;

				resolved.Add((level, price));
			}
		}

		if (resolved.Count == 0)
			return new ValueTask<HeatmapIndicatorStateSnapshot?>((HeatmapIndicatorStateSnapshot?)null);

		var state = _descriptor.NewState()
			.Visual(_lines, v =>
			{
				foreach (var (level, price) in resolved)
				{
					var samples = new[]
					{
						new HeatmapIndicatorVisualSample(request.CutoffTimeNanos, price),
					};
					v.Series(_seriesByKind[level.Kind], samples, level.Style);
				}
			})
			.Build();

		return new ValueTask<HeatmapIndicatorStateSnapshot?>(state);
	}

	public ValueTask ProcessProfileAsync(HeatmapMarketProfileSnapshot profile, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		ProcessProfile(profile);

		return ValueTask.CompletedTask;
	}

	public void ProcessProfile(HeatmapMarketProfileSnapshot profile)
	{
		lock (_sync)
			_profiles[profile.Period] = profile;
	}

	#endregion

	#region Private methods

	private bool TryResolvePrice(HeatmapOhlcLevelKind kind, out decimal price)
	{
		var period = kind switch
		{
			HeatmapOhlcLevelKind.CurrentDayHigh or
			HeatmapOhlcLevelKind.CurrentDayLow or
			HeatmapOhlcLevelKind.CurrentDayOpen => HeatmapProfilePeriod.CurrentDay,

			HeatmapOhlcLevelKind.PreviousDayHigh or
			HeatmapOhlcLevelKind.PreviousDayLow or
			HeatmapOhlcLevelKind.PreviousDayClose => HeatmapProfilePeriod.LastDay,

			HeatmapOhlcLevelKind.CurrentWeekHigh or
			HeatmapOhlcLevelKind.CurrentWeekLow or
			HeatmapOhlcLevelKind.CurrentWeekOpen => HeatmapProfilePeriod.CurrentWeek,

			HeatmapOhlcLevelKind.PreviousWeekHigh or
			HeatmapOhlcLevelKind.PreviousWeekLow or
			HeatmapOhlcLevelKind.PreviousWeekClose => HeatmapProfilePeriod.LastWeek,

			_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
		};

		if (!_profiles.TryGetValue(period, out var profile))
		{
			price = 0;
			return false;
		}

		price = kind switch
		{
			HeatmapOhlcLevelKind.CurrentDayHigh or
			HeatmapOhlcLevelKind.PreviousDayHigh or
			HeatmapOhlcLevelKind.CurrentWeekHigh or
			HeatmapOhlcLevelKind.PreviousWeekHigh => profile.High,

			HeatmapOhlcLevelKind.CurrentDayLow or
			HeatmapOhlcLevelKind.PreviousDayLow or
			HeatmapOhlcLevelKind.CurrentWeekLow or
			HeatmapOhlcLevelKind.PreviousWeekLow => profile.Low,

			HeatmapOhlcLevelKind.CurrentDayOpen or
			HeatmapOhlcLevelKind.CurrentWeekOpen => profile.Open,

			HeatmapOhlcLevelKind.PreviousDayClose or
			HeatmapOhlcLevelKind.PreviousWeekClose => profile.Close,

			_ => 0
		};

		return price > 0;
	}

	#endregion

}
