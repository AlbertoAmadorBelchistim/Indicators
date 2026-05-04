#nullable enable

namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATAS.Indicators.Heatmap;
using OFT.Rendering.Heatmap;

// Reference indicator: profile-driven dynamically-typed level lines (OHLC).
// Patterns: IHeatmapProfileConsumer instead of IHeatmapTradeTickConsumer,
//           LevelLine visual with one HeatmapIndicatorSeriesHandle<decimal>
//           per HeatmapOhlcLevelKind, persistent State.BeginUpdate lease
//           refreshed on each profile arrival, settings-driven series
//           selection (clear + re-append on every profile to honour
//           current visibility choices and styles).
// Copy as a starting point for any indicator that draws horizontal levels
// chosen at runtime by user settings.
[HeatmapIndicator(id: "heatmap.ohlc-levels", DisplayName = "OHLC Levels")]
public sealed class HeatmapOhlcLevelsIndicator
	: HeatmapIndicator<HeatmapOhlcLevelsSettings>
	, IHeatmapProfileConsumer
{
	#region Const fields

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
		var build = Describe<HeatmapOhlcLevelsIndicator>();
		_lines = build.LevelLine(LevelsVisualId, "OHLC Levels");
		_seriesByKind = new Dictionary<HeatmapOhlcLevelKind, HeatmapIndicatorSeriesHandle<decimal>>();
		foreach (var kind in Enum.GetValues<HeatmapOhlcLevelKind>())
		{
			_seriesByKind[kind] = _lines.Series(
				$"ohlc-levels.{kind}",
				HeatmapIndicatorSeriesRole.Level,
				HeatmapIndicatorValueKind.Price);
		}
		_descriptor = build.Done();
	}

	#endregion

	#region Readonly initialized fields

	private readonly Dictionary<HeatmapProfilePeriod, HeatmapMarketProfileSnapshot> _profiles = new();

	#endregion

	#region Properties

	public override HeatmapIndicatorDescriptor Descriptor => _descriptor;

	#endregion

	#region Public methods

	public override ValueTask ConfigureAsync(HeatmapOhlcLevelsSettings settings, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		RefreshLevels();

		return ValueTask.CompletedTask;
	}

	public ValueTask ProcessProfileAsync(HeatmapMarketProfileSnapshot profile, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		ProcessProfile(profile);

		return ValueTask.CompletedTask;
	}

	public void ProcessProfile(HeatmapMarketProfileSnapshot profile)
	{
		_profiles[profile.Period] = profile;
		RefreshLevels();
	}

	#endregion

	#region Protected methods

	protected override ValueTask OnStateResetCoreAsync(
		IHeatmapIndicatorContext context,
		IHeatmapIndicatorRuntime runtime,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_profiles.Clear();
		return ValueTask.CompletedTask;
	}

	#endregion

	#region Private methods

	private void RefreshLevels()
	{
		var resolved = new List<(HeatmapOhlcLevelSettings Level, decimal Price)>(Settings.Levels.Count);
		foreach (var level in Settings.Levels)
		{
			if (!TryResolvePrice(level.Kind, out var price) || price <= 0)
				continue;

			resolved.Add((level, price));
		}

		long latestTimestampNanos = 0;
		foreach (var profile in _profiles.Values)
		{
			if (profile.TimestampNanos > latestTimestampNanos)
				latestTimestampNanos = profile.TimestampNanos;
		}

		using var lease = State.BeginUpdate();
		var visualLease = lease.Visual(_lines);

		// Clear every series so dropped / disabled levels disappear.
		foreach (var seriesHandle in _seriesByKind.Values)
			visualLease.Series(seriesHandle).Clear();

		if (resolved.Count == 0 || latestTimestampNanos <= 0)
			return;

		// Re-append the active levels with their per-entry styles.
		foreach (var (level, price) in resolved)
		{
			if (!_seriesByKind.TryGetValue(level.Kind, out var seriesHandle))
				continue;

			var seriesLease = visualLease.Series(seriesHandle);
			seriesLease.Style = level.Style;
			seriesLease.Append(latestTimestampNanos, price);
		}
	}

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
