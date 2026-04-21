namespace ATAS.Indicators.Technical.Heatmap;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OFT.Rendering.Heatmap;

public sealed class HeatmapCvdIndicator
	: IHeatmapIndicator<HeatmapCvdSettings, HeatmapIndicatorSnapshot?>
	, IHeatmapProfileConsumer
{
	public const string PeriodDeltaMetricId = "cvd.period-delta";

	private HeatmapCvdSettings _settings;
	private HeatmapMarketProfileSnapshot? _currentDay;
	private HeatmapMarketProfileSnapshot? _currentWeek;

	public string Id => "heatmap.cvd";

	public void Configure(HeatmapCvdSettings settings) =>
		_settings = settings;

	public void Reset(HeatmapIndicatorContext context)
	{
		_currentDay = null;
		_currentWeek = null;
	}

	public void ProcessProfile(HeatmapMarketProfileSnapshot profile)
	{
		switch (profile.Period)
		{
			case HeatmapProfilePeriod.CurrentDay:
				_currentDay = profile;
				break;
			case HeatmapProfilePeriod.CurrentWeek:
				_currentWeek = profile;
				break;
		}
	}

	public void ProcessTimer(HeatmapIndicatorTimerTick tick)
	{
	}

	public HeatmapIndicatorSnapshot? GetSnapshot(HeatmapIndicatorSnapshotRequest request)
	{
		var state = GetStateSnapshot(request);
		if (state == null)
			return null;

		return new HeatmapIndicatorSnapshot(
			Id,
			"cvd",
			IsEnabled: true,
			IsTraining: false,
			[
				new HeatmapIndicatorVisual(
					"cvd.period-delta",
					HeatmapIndicatorVisualKind.SubPanelScalar,
					"CVD",
					[
						new HeatmapIndicatorVisualSeries(
							"cvd.period-delta.value",
							HeatmapIndicatorSeriesRole.Scalar,
							HeatmapIndicatorValueKind.Integer,
							state.Visuals[0].Series[0].Samples,
							MetricId: PeriodDeltaMetricId,
							Unit: "lots")
					])
			]);
	}

	public HeatmapIndicatorStateSnapshot? GetStateSnapshot(HeatmapIndicatorSnapshotRequest request)
	{
		if (!_settings.IsEnabled)
			return null;

		var profile = _settings.Mode switch
		{
			CvdCalculationMode.CurrentDay => _currentDay,
			CvdCalculationMode.CurrentWeek => _currentWeek,
			_ => null
		};

		if (profile == null)
			return null;
		if (request.CutoffTimeNanos > 0 && profile.Value.TimestampNanos > request.CutoffTimeNanos)
			return null;

		var lotSize = _settings.LotSize <= 0
			? 1m
			: _settings.LotSize;
		var periodDelta = (long)decimal.Round(
			profile.Value.Delta / lotSize,
			0,
			System.MidpointRounding.AwayFromZero);

		return new HeatmapIndicatorStateSnapshot(
			Id,
			IsEnabled: true,
			IsTraining: false,
			[
				new HeatmapIndicatorVisualState(
					"cvd.period-delta",
					[
						new HeatmapIndicatorSeriesState(
							"cvd.period-delta.value",
							[
								new HeatmapIndicatorVisualSample(
									profile.Value.TimestampNanos,
									HeatmapIndicatorValue.FromInteger(periodDelta))
							])
					])
			]);
	}
}
