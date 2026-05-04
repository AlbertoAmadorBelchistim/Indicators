#nullable enable

namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ATAS.Indicators.Heatmap;
using OFT.Localization;
using OFT.Rendering.Heatmap;
using static ATAS.Indicators.Heatmap.HeatmapIndicatorColors;

#region Enums

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
#endregion
#region OHLC Level entry

/// <summary>
/// Single OHLC level entry inside <see cref="HeatmapOhlcLevelsSettings"/>.
/// </summary>
public readonly record struct HeatmapOhlcLevelSettings(
	string Id,
	HeatmapOhlcLevelKind Kind,
	string Label,
	HeatmapIndicatorVisualStyle? Style);

#endregion
#region Settings DTOs

public sealed class HeatmapVwapSettings
{
	#region Auto properties

	/// <summary>Schema version. Increment when changing field semantics.</summary>
	[Browsable(false)]
	public int Version { get; set; } = 1;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapVwapInterval))]
	public HeatmapProfileScope Scope { get; set; } = HeatmapProfileScope.CurrentDay;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapVwapLineColor))]
	public CrossColor Color { get; set; } = ParseHex("#FF2962FF");

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapVwapLineThickness))]
	public float Thickness { get; set; } = 2f;

	#endregion
}

public sealed class HeatmapValueAreaSettings
{
	#region Auto properties

	/// <summary>Schema version. Increment when changing field semantics.</summary>
	[Browsable(false)]
	public int Version { get; set; } = 1;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapPocInterval))]
	public HeatmapProfileScope Scope { get; set; } = HeatmapProfileScope.CurrentDay;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapPocLineColor))]
	public CrossColor PocColor { get; set; } = ParseHex("#FFFF6D00");

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapPocLineThickness))]
	public float PocThickness { get; set; } = 2f;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapShowValueArea))]
	public bool ShowValueArea { get; set; } = false;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapValueAreaColor))]
	public CrossColor ValueAreaColor { get; set; } = ParseHex("#80FF6D00");

	#endregion
}

public sealed class HeatmapCvdSettings
{
	#region Auto properties

	/// <summary>Schema version. Increment when changing field semantics.</summary>
	[Browsable(false)]
	public int Version { get; set; } = 1;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapCvdCalculationMode))]
	public CvdCalculationMode CalculationMode { get; set; } = CvdCalculationMode.FromDataStart;

	[Display(Name = "Lot Size")]
	public decimal LotSize { get; set; } = 1m;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapCvdPanelHeight))]
	public int PanelHeight { get; set; } = 120;

	[Display(Name = "CVD Line Color")]
	public CrossColor Color { get; set; } = ParseHex("#FF42A5F5");

	#endregion
}

public sealed class HeatmapPriceChangeSettings
{
	#region Auto properties

	/// <summary>Schema version. Increment when changing field semantics.</summary>
	[Browsable(false)]
	public int Version { get; set; } = 1;

	[Display(Name = "Mode")]
	public HeatmapPriceChangeMode Mode { get; set; } = HeatmapPriceChangeMode.StandardDeviation;

	[Display(Name = "Period")]
	public HeatmapPriceChangePeriod Period { get; set; } = HeatmapPriceChangePeriod.OneMinute;

	[Display(Name = "Training Period")]
	public HeatmapTrainingPeriod TrainingPeriod { get; set; } = HeatmapTrainingPeriod.FifteenMinutes;

	[Display(Name = "Panel Height")]
	public int PanelHeight { get; set; } = 80;

	#endregion
}

public sealed class HeatmapPressureSettings
{
	#region Auto properties

	/// <summary>Schema version. Increment when changing field semantics.</summary>
	[Browsable(false)]
	public int Version { get; set; } = 1;

	[Display(Name = "Mode")]
	public HeatmapPressureMode Mode { get; set; } = HeatmapPressureMode.Volume;

	[Display(Name = "Preset")]
	public HeatmapPressurePreset Preset { get; set; } = HeatmapPressurePreset.Medium;

	[Display(Name = "Panel Height")]
	public int PanelHeight { get; set; } = 80;

	[Display(Name = "Threshold")]
	public int Threshold { get; set; } = 80;

	#endregion
}

public sealed class HeatmapOhlcLevelsSettings
{
	#region Auto properties

	/// <summary>Schema version. Increment when changing field semantics.</summary>
	[Browsable(false)]
	public int Version { get; set; } = 1;

	[Display(Name = "Levels")]
	public List<HeatmapOhlcLevelSettings> Levels { get; set; } = CreateDefaultLevels();

	#endregion

	#region Public methods

	public static List<HeatmapOhlcLevelSettings> CreateDefaultLevels() =>
	[
		new("currentDayHigh", HeatmapOhlcLevelKind.CurrentDayHigh, "Current Day High",
			new HeatmapIndicatorVisualStyle(Color: "#FF26A69A", Thickness: 1.5f)),
		new("currentDayLow", HeatmapOhlcLevelKind.CurrentDayLow, "Current Day Low",
			new HeatmapIndicatorVisualStyle(Color: "#FFEF5350", Thickness: 1.5f)),
		new("currentDayOpen", HeatmapOhlcLevelKind.CurrentDayOpen, "Current Day Open",
			new HeatmapIndicatorVisualStyle(Color: "#FFFFEE58", Thickness: 1.5f)),
		new("previousDayHigh", HeatmapOhlcLevelKind.PreviousDayHigh, "Previous Day High",
			new HeatmapIndicatorVisualStyle(Color: "#8026A69A", Thickness: 1f)),
		new("previousDayLow", HeatmapOhlcLevelKind.PreviousDayLow, "Previous Day Low",
			new HeatmapIndicatorVisualStyle(Color: "#80EF5350", Thickness: 1f)),
		new("previousDayClose", HeatmapOhlcLevelKind.PreviousDayClose, "Previous Day Close",
			new HeatmapIndicatorVisualStyle(Color: "#80FFEE58", Thickness: 1f)),
		new("currentWeekHigh", HeatmapOhlcLevelKind.CurrentWeekHigh, "Current Week High",
			new HeatmapIndicatorVisualStyle(Color: "#FF42A5F5", Thickness: 1.5f)),
		new("currentWeekLow", HeatmapOhlcLevelKind.CurrentWeekLow, "Current Week Low",
			new HeatmapIndicatorVisualStyle(Color: "#FFAB47BC", Thickness: 1.5f)),
		new("currentWeekOpen", HeatmapOhlcLevelKind.CurrentWeekOpen, "Current Week Open",
			new HeatmapIndicatorVisualStyle(Color: "#FF78909C", Thickness: 1.5f)),
		new("previousWeekHigh", HeatmapOhlcLevelKind.PreviousWeekHigh, "Previous Week High",
			new HeatmapIndicatorVisualStyle(Color: "#8042A5F5", Thickness: 1f)),
		new("previousWeekLow", HeatmapOhlcLevelKind.PreviousWeekLow, "Previous Week Low",
			new HeatmapIndicatorVisualStyle(Color: "#80AB47BC", Thickness: 1f)),
		new("previousWeekClose", HeatmapOhlcLevelKind.PreviousWeekClose, "Previous Week Close",
			new HeatmapIndicatorVisualStyle(Color: "#8078909C", Thickness: 1f)),
	];

	#endregion
}

#endregion
