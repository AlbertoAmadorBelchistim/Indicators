namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

using ATAS.Indicators.Filters;
using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;

[DisplayName("OHLC Plus")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.MaxLevelsIndDescription))]
[HelpLink("https://help.atas.net/")]
public class OHLCPlus : Indicator
{
    #region Nested types

    public enum DisplayMode
    {
        [Display(Name = "Price Only", Order = 100)]
        PriceOnly = 0,
        
        [Display(Name = "Price + Label", Order = 100)]
        PriceAndLabel = 1,
        
        [Display(Name = "Price + Label + Line To Bar", Order = 100)]
        PriceAndLabelWithLineToBar = 2,
        
        [Display(Name = "Price + Label + Full Line", Order = 100)]
        PriceAndLabelWithFullLine = 3,
        
        [Display(Name = "Label At Bar + Price", Order = 100)]
        LabelAtBarWithPrice = 4,
        
        [Display(Name = "Label At Bar + Price + Full Line", Order = 100)]
        LabelAtBarWithPriceAndFullLine = 5
    }

    private class LevelData
    {
        public decimal Price { get; set; }
        public string Label { get; set; } = string.Empty;
        public bool IsValid { get; set; }
    }

    #endregion

    #region Fields

    private readonly Dictionary<FixedProfilePeriods, IndicatorCandle> _profileCandles = new();
    private readonly Dictionary<string, LevelData> _levels = new();
    private RenderFont _font = new("Arial", 10);
    private RenderFont _axisFont = new("Arial", 11);
    private RenderStringFormat _stringFormat = new()
    {
        Alignment = StringAlignment.Far,
        LineAlignment = StringAlignment.Center,
        Trimming = StringTrimming.EllipsisCharacter,
        FormatFlags = StringFormatFlags.NoWrap
    };
    
    #endregion

    #region Properties

    #region Day Settings

    [Category("Day")]
    [Display(Name = "dOpen", Order = 10)]
    public FilterRenderPen DayOpenPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Blue.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid }
    };

    [Category("Day")]
    [Display(Name = "dOpen Display Mode", Order = 11)]
    public DisplayMode DayOpenDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Day")]
    [Display(Name = "dHigh", Order = 20)]
    public FilterRenderPen DayHighPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Green.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid }
    };

    [Category("Day")]
    [Display(Name = "dHigh Display Mode", Order = 21)]
    public DisplayMode DayHighDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Day")]
    [Display(Name = "dLow", Order = 30)]
    public FilterRenderPen DayLowPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Red.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid }
    };

    [Category("Day")]
    [Display(Name = "dLow Display Mode", Order = 100)]
    public DisplayMode DayLowDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Day")]
    [Display(Name = "dClose", Order = 100)]
    public FilterRenderPen DayClosePen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Gray.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid }
    };

    [Category("Day")]
    [Display(Name = "dClose Display Mode", Order = 100)]
    public DisplayMode DayCloseDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Day")]
    [Display(Name = "dEquilibrium", Order = 100)]
    public FilterRenderPen DayEquilibriumPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Yellow.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dash }
    };

    [Category("Day")]
    [Display(Name = "dEquilibrium Display Mode", Order = 100)]
    public DisplayMode DayEquilibriumDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Day")]
    [Display(Name = "dPOC", Order = 100)]
    public FilterRenderPen DayPOCPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Orange.Convert(), Width = 2, LineDashStyle = LineDashStyle.Solid }
    };

    [Category("Day")]
    [Display(Name = "dPOC Display Mode", Order = 100)]
    public DisplayMode DayPOCDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Day")]
    [Display(Name = "dVAH", Order = 100)]
    public FilterRenderPen DayVAHPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot }
    };

    [Category("Day")]
    [Display(Name = "dVAH Display Mode", Order = 100)]
    public DisplayMode DayVAHDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Day")]
    [Display(Name = "dVAL", Order = 100)]
    public FilterRenderPen DayVALPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot }
    };

    [Category("Day")]
    [Display(Name = "dVAL Display Mode", Order = 100)]
    public DisplayMode DayVALDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    #endregion

    #region Prev.Day Settings

    [Category("Prev.Day")]
    [Display(Name = "pdOpen", Order = 100)]
    public FilterRenderPen PrevDayOpenPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Blue.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Day")]
    [Display(Name = "pdOpen Display Mode", Order = 100)]
    public DisplayMode PrevDayOpenDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Day")]
    [Display(Name = "pdHigh", Order = 100)]
    public FilterRenderPen PrevDayHighPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Green.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = true
    };

    [Category("Prev.Day")]
    [Display(Name = "pdHigh Display Mode", Order = 100)]
    public DisplayMode PrevDayHighDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Day")]
    [Display(Name = "pdLow", Order = 100)]
    public FilterRenderPen PrevDayLowPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Red.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = true
    };

    [Category("Prev.Day")]
    [Display(Name = "pdLow Display Mode", Order = 100)]
    public DisplayMode PrevDayLowDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Day")]
    [Display(Name = "pdClose", Order = 100)]
    public FilterRenderPen PrevDayClosePen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Gray.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Day")]
    [Display(Name = "pdClose Display Mode", Order = 100)]
    public DisplayMode PrevDayCloseDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Day")]
    [Display(Name = "pdEquilibrium", Order = 100)]
    public FilterRenderPen PrevDayEquilibriumPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Yellow.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dash },
        Enabled = false
    };

    [Category("Prev.Day")]
    [Display(Name = "pdEquilibrium Display Mode", Order = 100)]
    public DisplayMode PrevDayEquilibriumDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Day")]
    [Display(Name = "pdPOC", Order = 100)]
    public FilterRenderPen PrevDayPOCPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Orange.Convert(), Width = 2, LineDashStyle = LineDashStyle.Solid },
        Enabled = true
    };

    [Category("Prev.Day")]
    [Display(Name = "pdPOC Display Mode", Order = 100)]
    public DisplayMode PrevDayPOCDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Day")]
    [Display(Name = "pdVAH", Order = 100)]
    public FilterRenderPen PrevDayVAHPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Prev.Day")]
    [Display(Name = "pdVAH Display Mode", Order = 100)]
    public DisplayMode PrevDayVAHDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Day")]
    [Display(Name = "pdVAL", Order = 100)]
    public FilterRenderPen PrevDayVALPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Prev.Day")]
    [Display(Name = "pdVAL Display Mode", Order = 100)]
    public DisplayMode PrevDayVALDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    #endregion

    #region Week Settings

    [Category("Week")]
    [Display(Name = "wOpen", Order = 100)]
    public FilterRenderPen WeekOpenPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Blue.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Week")]
    [Display(Name = "wOpen Display Mode", Order = 100)]
    public DisplayMode WeekOpenDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Week")]
    [Display(Name = "wHigh", Order = 100)]
    public FilterRenderPen WeekHighPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Green.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Week")]
    [Display(Name = "wHigh Display Mode", Order = 100)]
    public DisplayMode WeekHighDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Week")]
    [Display(Name = "wLow", Order = 100)]
    public FilterRenderPen WeekLowPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Red.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Week")]
    [Display(Name = "wLow Display Mode", Order = 100)]
    public DisplayMode WeekLowDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Week")]
    [Display(Name = "wClose", Order = 100)]
    public FilterRenderPen WeekClosePen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Gray.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Week")]
    [Display(Name = "wClose Display Mode", Order = 100)]
    public DisplayMode WeekCloseDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Week")]
    [Display(Name = "wEquilibrium", Order = 100)]
    public FilterRenderPen WeekEquilibriumPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Yellow.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dash },
        Enabled = false
    };

    [Category("Week")]
    [Display(Name = "wEquilibrium Display Mode", Order = 100)]
    public DisplayMode WeekEquilibriumDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Week")]
    [Display(Name = "wPOC", Order = 100)]
    public FilterRenderPen WeekPOCPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Orange.Convert(), Width = 2, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Week")]
    [Display(Name = "wPOC Display Mode", Order = 100)]
    public DisplayMode WeekPOCDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Week")]
    [Display(Name = "wVAH", Order = 100)]
    public FilterRenderPen WeekVAHPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Week")]
    [Display(Name = "wVAH Display Mode", Order = 100)]
    public DisplayMode WeekVAHDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Week")]
    [Display(Name = "wVAL", Order = 100)]
    public FilterRenderPen WeekVALPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Week")]
    [Display(Name = "wVAL Display Mode", Order = 100)]
    public DisplayMode WeekVALDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    #endregion

    #region Prev.Week Settings

    [Category("Prev.Week")]
    [Display(Name = "pwOpen", Order = 100)]
    public FilterRenderPen PrevWeekOpenPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Blue.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Week")]
    [Display(Name = "pwOpen Display Mode", Order = 100)]
    public DisplayMode PrevWeekOpenDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Week")]
    [Display(Name = "pwHigh", Order = 100)]
    public FilterRenderPen PrevWeekHighPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Green.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Week")]
    [Display(Name = "pwHigh Display Mode", Order = 100)]
    public DisplayMode PrevWeekHighDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Week")]
    [Display(Name = "pwLow", Order = 100)]
    public FilterRenderPen PrevWeekLowPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Red.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Week")]
    [Display(Name = "pwLow Display Mode", Order = 100)]
    public DisplayMode PrevWeekLowDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Week")]
    [Display(Name = "pwClose", Order = 100)]
    public FilterRenderPen PrevWeekClosePen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Gray.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Week")]
    [Display(Name = "pwClose Display Mode", Order = 100)]
    public DisplayMode PrevWeekCloseDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Week")]
    [Display(Name = "pwEquilibrium", Order = 100)]
    public FilterRenderPen PrevWeekEquilibriumPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Yellow.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dash },
        Enabled = false
    };

    [Category("Prev.Week")]
    [Display(Name = "pwEquilibrium Display Mode", Order = 100)]
    public DisplayMode PrevWeekEquilibriumDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Week")]
    [Display(Name = "pwPOC", Order = 100)]
    public FilterRenderPen PrevWeekPOCPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Orange.Convert(), Width = 2, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Week")]
    [Display(Name = "pwPOC Display Mode", Order = 100)]
    public DisplayMode PrevWeekPOCDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Week")]
    [Display(Name = "pwVAH", Order = 100)]
    public FilterRenderPen PrevWeekVAHPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Prev.Week")]
    [Display(Name = "pwVAH Display Mode", Order = 100)]
    public DisplayMode PrevWeekVAHDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Week")]
    [Display(Name = "pwVAL", Order = 100)]
    public FilterRenderPen PrevWeekVALPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Prev.Week")]
    [Display(Name = "pwVAL Display Mode", Order = 100)]
    public DisplayMode PrevWeekVALDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    #endregion

    #region Month Settings

    [Category("Month")]
    [Display(Name = "mOpen", Order = 100)]
    public FilterRenderPen MonthOpenPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Blue.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Month")]
    [Display(Name = "mOpen Display Mode", Order = 100)]
    public DisplayMode MonthOpenDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Month")]
    [Display(Name = "mHigh", Order = 100)]
    public FilterRenderPen MonthHighPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Green.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Month")]
    [Display(Name = "mHigh Display Mode", Order = 100)]
    public DisplayMode MonthHighDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Month")]
    [Display(Name = "mLow", Order = 100)]
    public FilterRenderPen MonthLowPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Red.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Month")]
    [Display(Name = "mLow Display Mode", Order = 100)]
    public DisplayMode MonthLowDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Month")]
    [Display(Name = "mClose", Order = 100)]
    public FilterRenderPen MonthClosePen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Gray.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Month")]
    [Display(Name = "mClose Display Mode", Order = 100)]
    public DisplayMode MonthCloseDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Month")]
    [Display(Name = "mEquilibrium", Order = 100)]
    public FilterRenderPen MonthEquilibriumPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Yellow.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dash },
        Enabled = false
    };

    [Category("Month")]
    [Display(Name = "mEquilibrium Display Mode", Order = 100)]
    public DisplayMode MonthEquilibriumDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Month")]
    [Display(Name = "mPOC", Order = 100)]
    public FilterRenderPen MonthPOCPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Orange.Convert(), Width = 2, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Month")]
    [Display(Name = "mPOC Display Mode", Order = 100)]
    public DisplayMode MonthPOCDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Month")]
    [Display(Name = "mVAH", Order = 100)]
    public FilterRenderPen MonthVAHPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Month")]
    [Display(Name = "mVAH Display Mode", Order = 100)]
    public DisplayMode MonthVAHDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Month")]
    [Display(Name = "mVAL", Order = 100)]
    public FilterRenderPen MonthVALPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Month")]
    [Display(Name = "mVAL Display Mode", Order = 100)]
    public DisplayMode MonthVALDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    #endregion

    #region Prev.Month Settings

    [Category("Prev.Month")]
    [Display(Name = "pmOpen", Order = 100)]
    public FilterRenderPen PrevMonthOpenPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Blue.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Month")]
    [Display(Name = "pmOpen Display Mode", Order = 100)]
    public DisplayMode PrevMonthOpenDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Month")]
    [Display(Name = "pmHigh", Order = 100)]
    public FilterRenderPen PrevMonthHighPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Green.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Month")]
    [Display(Name = "pmHigh Display Mode", Order = 100)]
    public DisplayMode PrevMonthHighDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Month")]
    [Display(Name = "pmLow", Order = 100)]
    public FilterRenderPen PrevMonthLowPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Red.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Month")]
    [Display(Name = "pmLow Display Mode", Order = 100)]
    public DisplayMode PrevMonthLowDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Month")]
    [Display(Name = "pmClose", Order = 100)]
    public FilterRenderPen PrevMonthClosePen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Gray.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Month")]
    [Display(Name = "pmClose Display Mode", Order = 100)]
    public DisplayMode PrevMonthCloseDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Month")]
    [Display(Name = "pmEquilibrium", Order = 100)]
    public FilterRenderPen PrevMonthEquilibriumPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Yellow.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dash },
        Enabled = false
    };

    [Category("Prev.Month")]
    [Display(Name = "pmEquilibrium Display Mode", Order = 100)]
    public DisplayMode PrevMonthEquilibriumDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Month")]
    [Display(Name = "pmPOC", Order = 100)]
    public FilterRenderPen PrevMonthPOCPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Orange.Convert(), Width = 2, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Prev.Month")]
    [Display(Name = "pmPOC Display Mode", Order = 100)]
    public DisplayMode PrevMonthPOCDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Month")]
    [Display(Name = "pmVAH", Order = 100)]
    public FilterRenderPen PrevMonthVAHPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Prev.Month")]
    [Display(Name = "pmVAH Display Mode", Order = 100)]
    public DisplayMode PrevMonthVAHDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Prev.Month")]
    [Display(Name = "pmVAL", Order = 100)]
    public FilterRenderPen PrevMonthVALPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Prev.Month")]
    [Display(Name = "pmVAL Display Mode", Order = 100)]
    public DisplayMode PrevMonthVALDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    #endregion

    #region Contract Settings

    [Category("Contract")]
    [Display(Name = "cOpen", Order = 100)]
    public FilterRenderPen ContractOpenPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Blue.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Contract")]
    [Display(Name = "cOpen Display Mode", Order = 100)]
    public DisplayMode ContractOpenDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Contract")]
    [Display(Name = "cHigh", Order = 100)]
    public FilterRenderPen ContractHighPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Green.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Contract")]
    [Display(Name = "cHigh Display Mode", Order = 100)]
    public DisplayMode ContractHighDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Contract")]
    [Display(Name = "cLow", Order = 100)]
    public FilterRenderPen ContractLowPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Red.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Contract")]
    [Display(Name = "cLow Display Mode", Order = 100)]
    public DisplayMode ContractLowDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Contract")]
    [Display(Name = "cClose", Order = 100)]
    public FilterRenderPen ContractClosePen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Gray.Convert(), Width = 1, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Contract")]
    [Display(Name = "cClose Display Mode", Order = 100)]
    public DisplayMode ContractCloseDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Contract")]
    [Display(Name = "cEquilibrium", Order = 100)]
    public FilterRenderPen ContractEquilibriumPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Yellow.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dash },
        Enabled = false
    };

    [Category("Contract")]
    [Display(Name = "cEquilibrium Display Mode", Order = 100)]
    public DisplayMode ContractEquilibriumDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Contract")]
    [Display(Name = "cPOC", Order = 100)]
    public FilterRenderPen ContractPOCPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Orange.Convert(), Width = 2, LineDashStyle = LineDashStyle.Solid },
        Enabled = false
    };

    [Category("Contract")]
    [Display(Name = "cPOC Display Mode", Order = 100)]
    public DisplayMode ContractPOCDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Contract")]
    [Display(Name = "cVAH", Order = 100)]
    public FilterRenderPen ContractVAHPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Contract")]
    [Display(Name = "cVAH Display Mode", Order = 100)]
    public DisplayMode ContractVAHDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    [Category("Contract")]
    [Display(Name = "cVAL", Order = 100)]
    public FilterRenderPen ContractVALPen { get; set; } = new(true)
    {
        Value = new PenSettings { Color = System.Drawing.Color.Purple.Convert(), Width = 1, LineDashStyle = LineDashStyle.Dot },
        Enabled = false
    };

    [Category("Contract")]
    [Display(Name = "cVAL Display Mode", Order = 100)]
    public DisplayMode ContractVALDisplayMode { get; set; } = DisplayMode.PriceAndLabel;

    #endregion

    #endregion

    #region Constructor

    public OHLCPlus()
        : base(true)
    {
        DataSeries[0].IsHidden = true;
        DenyToChangePanel = true;
        EnableCustomDrawing = true;
        SubscribeToDrawingEvents(DrawingLayouts.Final);
        DrawAbovePrice = true;
    }

    #endregion

    #region Protected methods

    protected override void OnApplyDefaultColors()
    {
        if (ChartInfo is null)
            return;

        base.OnApplyDefaultColors();
    }

    protected override void OnCalculate(int bar, decimal value)
    {
        if (bar == 0)
        {
            _profileCandles.Clear();
            _levels.Clear();
        }

        if (bar != CurrentBar - 1)
            return;

        // Request all needed profiles
        RequestProfiles();
    }

    protected override void OnFixedProfilesResponse(IndicatorCandle fixedProfileScaled, IndicatorCandle fixedProfileOriginScale, FixedProfilePeriods period)
    {
        _profileCandles[period] = fixedProfileOriginScale;
        UpdateLevels(period, fixedProfileOriginScale);
        RedrawChart();
    }

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (ChartInfo is null || InstrumentInfo is null)
            return;

        // Render Day levels
        RenderLevel(context, "dOpen", DayOpenPen, DayOpenDisplayMode);
        RenderLevel(context, "dHigh", DayHighPen, DayHighDisplayMode);
        RenderLevel(context, "dLow", DayLowPen, DayLowDisplayMode);
        RenderLevel(context, "dClose", DayClosePen, DayCloseDisplayMode);
        RenderLevel(context, "dEQ", DayEquilibriumPen, DayEquilibriumDisplayMode);
        RenderLevel(context, "dPOC", DayPOCPen, DayPOCDisplayMode);
        RenderLevel(context, "dVAH", DayVAHPen, DayVAHDisplayMode);
        RenderLevel(context, "dVAL", DayVALPen, DayVALDisplayMode);

        // Render Prev.Day levels
        RenderLevel(context, "pOpen", PrevDayOpenPen, PrevDayOpenDisplayMode);
        RenderLevel(context, "pHigh", PrevDayHighPen, PrevDayHighDisplayMode);
        RenderLevel(context, "pLow", PrevDayLowPen, PrevDayLowDisplayMode);
        RenderLevel(context, "pClose", PrevDayClosePen, PrevDayCloseDisplayMode);
        RenderLevel(context, "pEQ", PrevDayEquilibriumPen, PrevDayEquilibriumDisplayMode);
        RenderLevel(context, "pPOC", PrevDayPOCPen, PrevDayPOCDisplayMode);
        RenderLevel(context, "pVAH", PrevDayVAHPen, PrevDayVAHDisplayMode);
        RenderLevel(context, "pVAL", PrevDayVALPen, PrevDayVALDisplayMode);

        // Render Week levels
        RenderLevel(context, "wOpen", WeekOpenPen, WeekOpenDisplayMode);
        RenderLevel(context, "wHigh", WeekHighPen, WeekHighDisplayMode);
        RenderLevel(context, "wLow", WeekLowPen, WeekLowDisplayMode);
        RenderLevel(context, "wClose", WeekClosePen, WeekCloseDisplayMode);
        RenderLevel(context, "wEQ", WeekEquilibriumPen, WeekEquilibriumDisplayMode);
        RenderLevel(context, "wPOC", WeekPOCPen, WeekPOCDisplayMode);
        RenderLevel(context, "wVAH", WeekVAHPen, WeekVAHDisplayMode);
        RenderLevel(context, "wVAL", WeekVALPen, WeekVALDisplayMode);

        // Render Prev.Week levels
        RenderLevel(context, "pwOpen", PrevWeekOpenPen, PrevWeekOpenDisplayMode);
        RenderLevel(context, "pwHigh", PrevWeekHighPen, PrevWeekHighDisplayMode);
        RenderLevel(context, "pwLow", PrevWeekLowPen, PrevWeekLowDisplayMode);
        RenderLevel(context, "pwClose", PrevWeekClosePen, PrevWeekCloseDisplayMode);
        RenderLevel(context, "pwEQ", PrevWeekEquilibriumPen, PrevWeekEquilibriumDisplayMode);
        RenderLevel(context, "pwPOC", PrevWeekPOCPen, PrevWeekPOCDisplayMode);
        RenderLevel(context, "pwVAH", PrevWeekVAHPen, PrevWeekVAHDisplayMode);
        RenderLevel(context, "pwVAL", PrevWeekVALPen, PrevWeekVALDisplayMode);

        // Render Month levels
        RenderLevel(context, "mOpen", MonthOpenPen, MonthOpenDisplayMode);
        RenderLevel(context, "mHigh", MonthHighPen, MonthHighDisplayMode);
        RenderLevel(context, "mLow", MonthLowPen, MonthLowDisplayMode);
        RenderLevel(context, "mClose", MonthClosePen, MonthCloseDisplayMode);
        RenderLevel(context, "mEQ", MonthEquilibriumPen, MonthEquilibriumDisplayMode);
        RenderLevel(context, "mPOC", MonthPOCPen, MonthPOCDisplayMode);
        RenderLevel(context, "mVAH", MonthVAHPen, MonthVAHDisplayMode);
        RenderLevel(context, "mVAL", MonthVALPen, MonthVALDisplayMode);

        // Render Prev.Month levels
        RenderLevel(context, "pmOpen", PrevMonthOpenPen, PrevMonthOpenDisplayMode);
        RenderLevel(context, "pmHigh", PrevMonthHighPen, PrevMonthHighDisplayMode);
        RenderLevel(context, "pmLow", PrevMonthLowPen, PrevMonthLowDisplayMode);
        RenderLevel(context, "pmClose", PrevMonthClosePen, PrevMonthCloseDisplayMode);
        RenderLevel(context, "pmEQ", PrevMonthEquilibriumPen, PrevMonthEquilibriumDisplayMode);
        RenderLevel(context, "pmPOC", PrevMonthPOCPen, PrevMonthPOCDisplayMode);
        RenderLevel(context, "pmVAH", PrevMonthVAHPen, PrevMonthVAHDisplayMode);
        RenderLevel(context, "pmVAL", PrevMonthVALPen, PrevMonthVALDisplayMode);

        // Render Contract levels
        RenderLevel(context, "cOpen", ContractOpenPen, ContractOpenDisplayMode);
        RenderLevel(context, "cHigh", ContractHighPen, ContractHighDisplayMode);
        RenderLevel(context, "cLow", ContractLowPen, ContractLowDisplayMode);
        RenderLevel(context, "cClose", ContractClosePen, ContractCloseDisplayMode);
        RenderLevel(context, "cEQ", ContractEquilibriumPen, ContractEquilibriumDisplayMode);
        RenderLevel(context, "cPOC", ContractPOCPen, ContractPOCDisplayMode);
        RenderLevel(context, "cVAH", ContractVAHPen, ContractVAHDisplayMode);
        RenderLevel(context, "cVAL", ContractVALPen, ContractVALDisplayMode);
    }

    #endregion

    #region Private methods

    private void RequestProfiles()
    {
        // Request current day profile
        if (NeedsDayData())
            GetFixedProfile(new FixedProfileRequest(FixedProfilePeriods.CurrentDay));

        // Request previous day profile
        if (NeedsPrevDayData())
            GetFixedProfile(new FixedProfileRequest(FixedProfilePeriods.LastDay));

        // Request current week profile
        if (NeedsWeekData())
            GetFixedProfile(new FixedProfileRequest(FixedProfilePeriods.CurrentWeek));

        // Request previous week profile
        if (NeedsPrevWeekData())
            GetFixedProfile(new FixedProfileRequest(FixedProfilePeriods.LastWeek));

        // Request current month profile
        if (NeedsMonthData())
            GetFixedProfile(new FixedProfileRequest(FixedProfilePeriods.CurrentMonth));

        // Request previous month profile
        if (NeedsPrevMonthData())
            GetFixedProfile(new FixedProfileRequest(FixedProfilePeriods.LastMonth));

        // Request contract profile
        if (NeedsContractData())
            GetFixedProfile(new FixedProfileRequest(FixedProfilePeriods.Contract));
    }

    private bool NeedsDayData()
    {
        return DayOpenPen.Enabled || DayHighPen.Enabled || DayLowPen.Enabled || DayClosePen.Enabled ||
               DayEquilibriumPen.Enabled || DayPOCPen.Enabled || DayVAHPen.Enabled || DayVALPen.Enabled;
    }

    private bool NeedsPrevDayData()
    {
        return PrevDayOpenPen.Enabled || PrevDayHighPen.Enabled || PrevDayLowPen.Enabled || PrevDayClosePen.Enabled ||
               PrevDayEquilibriumPen.Enabled || PrevDayPOCPen.Enabled || PrevDayVAHPen.Enabled || PrevDayVALPen.Enabled;
    }

    private bool NeedsWeekData()
    {
        return WeekOpenPen.Enabled || WeekHighPen.Enabled || WeekLowPen.Enabled || WeekClosePen.Enabled ||
               WeekEquilibriumPen.Enabled || WeekPOCPen.Enabled || WeekVAHPen.Enabled || WeekVALPen.Enabled;
    }

    private bool NeedsPrevWeekData()
    {
        return PrevWeekOpenPen.Enabled || PrevWeekHighPen.Enabled || PrevWeekLowPen.Enabled || PrevWeekClosePen.Enabled ||
               PrevWeekEquilibriumPen.Enabled || PrevWeekPOCPen.Enabled || PrevWeekVAHPen.Enabled || PrevWeekVALPen.Enabled;
    }

    private bool NeedsMonthData()
    {
        return MonthOpenPen.Enabled || MonthHighPen.Enabled || MonthLowPen.Enabled || MonthClosePen.Enabled ||
               MonthEquilibriumPen.Enabled || MonthPOCPen.Enabled || MonthVAHPen.Enabled || MonthVALPen.Enabled;
    }

    private bool NeedsPrevMonthData()
    {
        return PrevMonthOpenPen.Enabled || PrevMonthHighPen.Enabled || PrevMonthLowPen.Enabled || PrevMonthClosePen.Enabled ||
               PrevMonthEquilibriumPen.Enabled || PrevMonthPOCPen.Enabled || PrevMonthVAHPen.Enabled || PrevMonthVALPen.Enabled;
    }

    private bool NeedsContractData()
    {
        return ContractOpenPen.Enabled || ContractHighPen.Enabled || ContractLowPen.Enabled || ContractClosePen.Enabled ||
               ContractEquilibriumPen.Enabled || ContractPOCPen.Enabled || ContractVAHPen.Enabled || ContractVALPen.Enabled;
    }

    private void UpdateLevels(FixedProfilePeriods period, IndicatorCandle candle)
    {
        if (candle == null)
            return;

        var prefix = GetPrefixForPeriod(period);
        
        // Update OHLC levels
        UpdateLevel($"{prefix}Open", candle.Open);
        UpdateLevel($"{prefix}High", candle.High);
        UpdateLevel($"{prefix}Low", candle.Low);
        UpdateLevel($"{prefix}Close", candle.Close);
        UpdateLevel($"{prefix}EQ", (candle.High + candle.Low) / 2);

        // Update Volume Profile levels
        if (candle.MaxVolumePriceInfo != null)
            UpdateLevel($"{prefix}POC", candle.MaxVolumePriceInfo.Price);

        if (candle.ValueArea != null)
        {
            UpdateLevel($"{prefix}VAH", candle.ValueArea.ValueAreaHigh);
            UpdateLevel($"{prefix}VAL", candle.ValueArea.ValueAreaLow);
        }
    }

    private void UpdateLevel(string key, decimal price)
    {
        if (!_levels.ContainsKey(key))
            _levels[key] = new LevelData();

        _levels[key].Price = price;
        _levels[key].Label = key;
        _levels[key].IsValid = true;
    }

    private string GetPrefixForPeriod(FixedProfilePeriods period)
    {
        return period switch
        {
            FixedProfilePeriods.CurrentDay => "d",
            FixedProfilePeriods.LastDay => "p",
            FixedProfilePeriods.CurrentWeek => "w",
            FixedProfilePeriods.LastWeek => "pw",
            FixedProfilePeriods.CurrentMonth => "m",
            FixedProfilePeriods.LastMonth => "pm",
            FixedProfilePeriods.Contract => "c",
            _ => ""
        };
    }

    private void RenderLevel(RenderContext context, string levelKey, FilterRenderPen pen, DisplayMode displayMode)
    {
        if (!pen.Enabled || !_levels.TryGetValue(levelKey, out var level) || !level.IsValid)
            return;

        var y = ChartInfo.GetYByPrice(level.Price, false);
        var chartWidth = ChartInfo.PriceChartContainer.Region.Width;
        var currentBarX = ChartInfo.GetXByBar(CurrentBar - 1);
        var barWidth = (int)ChartInfo.PriceChartContainer.BarsWidth;
        var currentBarRightX = currentBarX + barWidth;

        // Get pen from FilterRenderPen
        var renderPen = pen.Value.RenderObject;

        switch (displayMode)
        {
            case DisplayMode.PriceOnly:
                DrawPriceLabel(context, level.Price, y, renderPen);
                break;

            case DisplayMode.PriceAndLabel:
                DrawPriceLabel(context, level.Price, y, renderPen);
                // Label should be positioned at the left edge of price axis (right edge of chart area)
                var labelX = chartWidth - 5;
                DrawTextLabel(context, level.Label, labelX, y, renderPen, true);
                break;

            case DisplayMode.PriceAndLabelWithLineToBar:
                // Draw line first, then labels on top
                context.DrawLine(renderPen, currentBarRightX, y, chartWidth, y);
                DrawPriceLabel(context, level.Price, y, renderPen);
                var labelX2 = chartWidth - 5;
                DrawTextLabel(context, level.Label, labelX2, y, renderPen, true);
                break;

            case DisplayMode.PriceAndLabelWithFullLine:
                // Draw line first, then labels on top
                context.DrawLine(renderPen, 0, y, chartWidth, y);
                DrawPriceLabel(context, level.Price, y, renderPen);
                var labelX3 = chartWidth - 5;
                DrawTextLabel(context, level.Label, labelX3, y, renderPen, true);
                break;

            case DisplayMode.LabelAtBarWithPrice:
                // Draw line first, then labels on top
                context.DrawLine(renderPen, currentBarRightX, y, chartWidth, y);
                DrawPriceLabel(context, level.Price, y, renderPen);
                // Label should be to the right of the last bar
                var barLabelX = currentBarRightX + 5;
                DrawTextLabel(context, level.Label, barLabelX, y, renderPen, false);
                break;

            case DisplayMode.LabelAtBarWithPriceAndFullLine:
                // Draw line first, then labels on top
                context.DrawLine(renderPen, 0, y, chartWidth, y);
                DrawPriceLabel(context, level.Price, y, renderPen);
                // Label should be to the right of the last bar
                var barLabelX2 = currentBarRightX + 5;
                DrawTextLabel(context, level.Label, barLabelX2, y, renderPen, false);
                break;
        }
    }

    private void DrawPriceLabel(RenderContext context, decimal price, int y, RenderPen pen)
    {
        var priceText = string.Format(ChartInfo.StringFormat, price);
        var axisTextColor = ChartInfo.ColorsStore.MouseTextColor;
        this.DrawLabelOnPriceAxis(context, priceText, y, _axisFont, pen.Color, axisTextColor);
    }

    private void DrawTextLabel(RenderContext context, string text, int x, int y, RenderPen pen, bool alignRight)
    {
        var size = context.MeasureString(text, _font);
        var textColor = ChartInfo.ColorsStore.MouseTextColor;
        
        // Calculate rectangle position based on alignment
        var rectX = alignRight ? x - size.Width : x;
        var rect = new Rectangle(rectX - 2, y - size.Height / 2 - 1, size.Width + 4, size.Height + 2);
        
        // Draw background with border
        var backgroundColor = ChartInfo.ColorsStore.BaseBackgroundColor;
        context.FillRectangle(backgroundColor, rect);
        context.DrawRectangle(pen, rect);
        
        // Draw text
        var textRect = new Rectangle(rectX, y - size.Height / 2, size.Width, size.Height);
        var format = alignRight ? _stringFormat : new RenderStringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };
        context.DrawString(text, _font, textColor, textRect, format);
    }

    #endregion
}