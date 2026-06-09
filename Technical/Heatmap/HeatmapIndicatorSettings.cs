#nullable enable

namespace ATAS.Indicators.Technical.Heatmap;

using ATAS.Indicators.Heatmap;
using OFT.Localization;
using OFT.Rendering.Heatmap;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

#region Settings DTOs

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

public sealed class HeatmapCvdSettings : IHeatmapPanelHeightSettings
{
    #region Auto properties

    /// <summary>Schema version. Increment when changing field semantics.</summary>
    [Browsable(false)]
    public int Version { get; set; } = 1;

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapCvdCalculationMode), GroupName = nameof(Strings.Settings), Order = 10)]
    public CvdCalculationMode CalculationMode { get; set; } = CvdCalculationMode.FromDataStart;

    [Display(Name = nameof(Strings.LotSize), GroupName = nameof(Strings.Settings), ResourceType = typeof(Strings), Order = 20)]
    public decimal LotSize { get; set; } = 1m;

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.HeatmapCvdPanelHeight), GroupName = nameof(Strings.Drawing), Order = 10)]
    public int PanelHeight { get; set; } = 120;

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Positive), GroupName = nameof(Strings.Drawing), Order = 20)]
    public CrossColor PosColor { get; set; } = ParseHex("#FF008000");

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Negative), GroupName = nameof(Strings.Drawing), Order = 30)]
    public CrossColor NegColor { get; set; } = ParseHex("#FFFF0000");

    #endregion
}

public sealed class HeatmapPriceChangeSettings : IHeatmapPanelHeightSettings
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

public sealed class HeatmapPressureSettings : IHeatmapPanelHeightSettings
{
    #region Auto properties

    /// <summary>Schema version. Increment when changing field semantics.</summary>
    [Browsable(false)]
    public int Version { get; set; } = 1;

    [Display(Name = nameof(Strings.Mode), GroupName = nameof(Strings.Settings), ResourceType = typeof(Strings), Order = 10)]
    public HeatmapPressureMode Mode { get; set; } = HeatmapPressureMode.Volume;

    [Display(Name = nameof(Strings.Preset), GroupName = nameof(Strings.Settings), ResourceType = typeof(Strings), Order = 20)]
    public HeatmapPressurePreset Preset { get; set; } = HeatmapPressurePreset.Medium;

    [Display(Name = nameof(Strings.PanelHeight), GroupName = nameof(Strings.Drawing), ResourceType = typeof(Strings), Order = 10)]
    public int PanelHeight { get; set; } = 80;

    [Display(Name = nameof(Strings.Threshold), GroupName = nameof(Strings.Drawing), ResourceType = typeof(Strings), Order = 20)]
    public int Threshold { get; set; } = 80;

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.BuyColor), GroupName = nameof(Strings.Drawing), Order = 30)]
    public CrossColor BuyColor { get; set; } = ParseHex("#FF00A572");

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.SellColor), GroupName = nameof(Strings.Drawing), Order = 40)]
    public CrossColor SellColor { get; set; } = ParseHex("#FFD53930");

    #endregion
}

#endregion