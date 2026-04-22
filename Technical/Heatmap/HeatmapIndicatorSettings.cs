namespace ATAS.Indicators.Technical.Heatmap;

using OFT.Rendering.Heatmap;

public enum HeatmapPriceChangeMode
{
	StandardDeviation,
	RateOfChange
}

public enum HeatmapPriceChangePeriod
{
	TenSeconds = 10,
	ThirtySeconds = 30,
	OneMinute = 60,
	TwoMinutes = 120,
	ThreeMinutes = 180,
	FiveMinutes = 300
}

public enum HeatmapTrainingPeriod
{
	FiveMinutes = 300,
	FifteenMinutes = 900,
	OneHour = 3600
}

public enum HeatmapPressureMode
{
	Pace,
	Volume
}

public enum HeatmapPressurePreset
{
	Short,
	Medium,
	Long
}

public enum HeatmapOhlcLevelKind
{
	CurrentDayHigh,
	CurrentDayLow,
	CurrentDayOpen,
	PreviousDayHigh,
	PreviousDayLow,
	PreviousDayClose,
	CurrentWeekHigh,
	CurrentWeekLow,
	CurrentWeekOpen,
	PreviousWeekHigh,
	PreviousWeekLow,
	PreviousWeekClose
}

public enum HeatmapProfileScope
{
	DataStart,
	CurrentDay,
	LastDay,
	CurrentWeek,
	LastWeek,
	CurrentMonth,
	LastMonth,
	Contract
}

public readonly record struct HeatmapOhlcLevelSettings(
	string Id,
	HeatmapOhlcLevelKind Kind,
	string Label,
	HeatmapIndicatorVisualStyle? Style);

public readonly record struct HeatmapVwapSettings(
	bool IsEnabled,
	HeatmapProfileScope Scope)
{
	public static HeatmapVwapSettings Default { get; } = new(true, HeatmapProfileScope.CurrentDay);
}

public readonly record struct HeatmapValueAreaSettings(
	bool IsEnabled,
	HeatmapProfileScope Scope)
{
	public static HeatmapValueAreaSettings Default { get; } = new(true, HeatmapProfileScope.CurrentDay);
}

public readonly record struct HeatmapCvdSettings(
	bool IsEnabled,
	CvdCalculationMode Mode,
	decimal LotSize);

public readonly record struct HeatmapPriceChangeSettings(
	HeatmapPriceChangeMode Mode,
	HeatmapPriceChangePeriod Period,
	HeatmapTrainingPeriod TrainingPeriod)
{
	public static HeatmapPriceChangeSettings Default { get; } = new(
		HeatmapPriceChangeMode.StandardDeviation,
		HeatmapPriceChangePeriod.OneMinute,
		HeatmapTrainingPeriod.FifteenMinutes);
}

public readonly record struct HeatmapPressureSettings(
	HeatmapPressureMode Mode,
	HeatmapPressurePreset Preset)
{
	public static HeatmapPressureSettings Default { get; } = new(
		HeatmapPressureMode.Volume,
		HeatmapPressurePreset.Medium);
}

