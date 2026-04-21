namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OFT.Rendering.Heatmap;

public sealed class HeatmapOhlcLevelsIndicator
	: IHeatmapIndicator<IReadOnlyList<HeatmapOhlcLevelSettings>, HeatmapIndicatorSnapshot?>
	, IHeatmapProfileConsumer
{
	private readonly object _sync = new();
	private readonly Dictionary<HeatmapProfilePeriod, HeatmapMarketProfileSnapshot> _profiles = new();
	private IReadOnlyList<HeatmapOhlcLevelSettings> _settings = [];

	public string Id => "heatmap.ohlc-levels";

	public void Configure(IReadOnlyList<HeatmapOhlcLevelSettings> settings)
	{
		lock (_sync)
			_settings = settings;
	}

	public void Reset(HeatmapIndicatorContext context)
	{
		lock (_sync)
			_profiles.Clear();
	}

	public void ProcessProfile(HeatmapMarketProfileSnapshot profile)
	{
		lock (_sync)
			_profiles[profile.Period] = profile;
	}

	public void ProcessTimer(HeatmapIndicatorTimerTick tick)
	{
	}

	public HeatmapIndicatorSnapshot? GetSnapshot(HeatmapIndicatorSnapshotRequest request)
	{
		List<HeatmapIndicatorVisual> visuals = [];

		lock (_sync)
		{
			foreach (var level in _settings)
			{
				if (level.Style?.IsVisible == false ||
				    !TryResolvePrice(level.Kind, out var price) ||
				    price <= 0)
					continue;

				var visualId = string.IsNullOrWhiteSpace(level.Id)
					? $"ohlc-level.{level.Kind}"
					: level.Id;
				var label = string.IsNullOrWhiteSpace(level.Label)
					? level.Kind.ToString()
					: level.Label;

				visuals.Add(new HeatmapIndicatorVisual(
					visualId,
					HeatmapIndicatorVisualKind.LevelLine,
					label,
					[
						new HeatmapIndicatorVisualSeries(
							$"{visualId}.price",
							HeatmapIndicatorSeriesRole.Level,
							HeatmapIndicatorValueKind.Price,
							[
								new HeatmapIndicatorVisualSample(
									request.EffectiveEndTimeNanos,
									HeatmapIndicatorValue.FromPrice(price))
							],
							level.Style)
					],
					level.Style));
			}
		}

		if (visuals.Count == 0)
			return null;

		return new HeatmapIndicatorSnapshot(
			Id,
			"ohlc-levels",
			IsEnabled: true,
			IsTraining: false,
			visuals);
	}

	public HeatmapIndicatorStateSnapshot? GetStateSnapshot(HeatmapIndicatorSnapshotRequest request)
	{
		List<HeatmapIndicatorVisualState> visuals = [];

		lock (_sync)
		{
			foreach (var level in _settings)
			{
				if (level.Style?.IsVisible == false ||
				    !TryResolvePrice(level.Kind, out var price) ||
				    price <= 0)
					continue;

				var visualId = string.IsNullOrWhiteSpace(level.Id)
					? $"ohlc-level.{level.Kind}"
					: level.Id;

				visuals.Add(new HeatmapIndicatorVisualState(
					visualId,
					[
						new HeatmapIndicatorSeriesState(
							$"{visualId}.price",
							[
								new HeatmapIndicatorVisualSample(
									request.EffectiveEndTimeNanos,
									HeatmapIndicatorValue.FromPrice(price))
							],
							level.Style)
					],
					level.Style));
			}
		}

		if (visuals.Count == 0)
			return null;

		return new HeatmapIndicatorStateSnapshot(
			Id,
			IsEnabled: true,
			IsTraining: false,
			visuals);
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
}
