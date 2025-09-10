namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

using ATAS.Indicators.Filters;
using OFT.Attributes;
using OFT.Attributes.Editors;
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

    public enum LabelPosition
    {
        [Display(Name = "None")]
        None = 0,
        
        [Display(Name = "Bar")]
        Bar = 1,
        
        [Display(Name = "Right (Price Scale)")]
        Right = 2,
        
        [Display(Name = "Left")]
        Left = 3
    }

    public enum LineType
    {
        [Display(Name = "None")]
        None = 0,
        
        [Display(Name = "Bar")]
        Bar = 1,
        
        [Display(Name = "Full Width")]
        Full = 2
    }

    [Editor(typeof(Editors.LevelSettingsEditor), typeof(Editors.LevelSettingsEditor))]
    public class LevelSettings
    {
        public LevelSettings(bool enabled = false, CrossColor color = default, int width = 1, LineDashStyle lineStyle = LineDashStyle.Solid, bool showPrice = true, LabelPosition labelPosition = LabelPosition.Bar, LineType lineType = LineType.Bar)
        {
            Enabled = enabled;
            Color = color == default ? System.Drawing.Color.Blue.Convert() : color;
            Width = width;
            LineStyle = lineStyle;
            ShowPrice = showPrice;
            LabelPosition = labelPosition;
            LineType = lineType;
        }

        [Display(Name = "Enabled")]
        public bool Enabled { get; set; }

        [Display(Name = "Color")]
        public CrossColor Color { get; set; }

        [Display(Name = "Price")]
        public bool ShowPrice { get; set; }

        [Display(Name = "Line")]
        public LineType LineType { get; set; }

        [Display(Name = "Width")]
        [Range(1, 10)]
        public int Width { get; set; }

        [Display(Name = "Style")]
        public LineDashStyle LineStyle { get; set; }

        [Display(Name = "Label")]
        public LabelPosition LabelPosition { get; set; }

        [Browsable(false)]
        public RenderPen RenderPen => new PenSettings { Color = Color, Width = Width, LineDashStyle = LineStyle }.RenderObject;
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
    public LevelSettings DayOpenLevel { get; set; } = new(
        enabled: true,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Day")]
    [Display(Name = "dHigh", Order = 20)]
    public LevelSettings DayHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Day")]
    [Display(Name = "dLow", Order = 30)]
    public LevelSettings DayLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Day")]
    [Display(Name = "dClose", Order = 40)]
    public LevelSettings DayCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Day")]
    [Display(Name = "dEquilibrium", Order = 50)]
    public LevelSettings DayEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Day")]
    [Display(Name = "dPOC", Order = 60)]
    public LevelSettings DayPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Day")]
    [Display(Name = "dVAH", Order = 70)]
    public LevelSettings DayVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Day")]
    [Display(Name = "dVAL", Order = 80)]
    public LevelSettings DayVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Prev.Day Settings

    [Category("Prev.Day")]
    [Display(Name = "pdOpen", Order = 10)]
    public LevelSettings PrevDayOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Day")]
    [Display(Name = "pdHigh", Order = 20)]
    public LevelSettings PrevDayHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Day")]
    [Display(Name = "pdLow", Order = 30)]
    public LevelSettings PrevDayLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Day")]
    [Display(Name = "pdClose", Order = 40)]
    public LevelSettings PrevDayCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Day")]
    [Display(Name = "pdEquilibrium", Order = 50)]
    public LevelSettings PrevDayEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Day")]
    [Display(Name = "pdPOC", Order = 60)]
    public LevelSettings PrevDayPOCLevel { get; set; } = new(
        enabled: true,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Day")]
    [Display(Name = "pdVAH", Order = 70)]
    public LevelSettings PrevDayVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Day")]
    [Display(Name = "pdVAL", Order = 80)]
    public LevelSettings PrevDayVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Week Settings

    [Category("Week")]
    [Display(Name = "wOpen", Order = 10)]
    public LevelSettings WeekOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Week")]
    [Display(Name = "wHigh", Order = 20)]
    public LevelSettings WeekHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Week")]
    [Display(Name = "wLow", Order = 30)]
    public LevelSettings WeekLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Week")]
    [Display(Name = "wClose", Order = 40)]
    public LevelSettings WeekCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Week")]
    [Display(Name = "wEquilibrium", Order = 50)]
    public LevelSettings WeekEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Week")]
    [Display(Name = "wPOC", Order = 60)]
    public LevelSettings WeekPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Week")]
    [Display(Name = "wVAH", Order = 70)]
    public LevelSettings WeekVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Week")]
    [Display(Name = "wVAL", Order = 80)]
    public LevelSettings WeekVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Prev.Week Settings

    [Category("Prev.Week")]
    [Display(Name = "pwOpen", Order = 10)]
    public LevelSettings PrevWeekOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Week")]
    [Display(Name = "pwHigh", Order = 20)]
    public LevelSettings PrevWeekHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Week")]
    [Display(Name = "pwLow", Order = 30)]
    public LevelSettings PrevWeekLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Week")]
    [Display(Name = "pwClose", Order = 40)]
    public LevelSettings PrevWeekCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Week")]
    [Display(Name = "pwEquilibrium", Order = 50)]
    public LevelSettings PrevWeekEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Week")]
    [Display(Name = "pwPOC", Order = 60)]
    public LevelSettings PrevWeekPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Week")]
    [Display(Name = "pwVAH", Order = 70)]
    public LevelSettings PrevWeekVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Week")]
    [Display(Name = "pwVAL", Order = 80)]
    public LevelSettings PrevWeekVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Month Settings

    [Category("Month")]
    [Display(Name = "mOpen", Order = 10)]
    public LevelSettings MonthOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Month")]
    [Display(Name = "mHigh", Order = 20)]
    public LevelSettings MonthHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Month")]
    [Display(Name = "mLow", Order = 30)]
    public LevelSettings MonthLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Month")]
    [Display(Name = "mClose", Order = 40)]
    public LevelSettings MonthCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Month")]
    [Display(Name = "mEquilibrium", Order = 50)]
    public LevelSettings MonthEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Month")]
    [Display(Name = "mPOC", Order = 60)]
    public LevelSettings MonthPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Month")]
    [Display(Name = "mVAH", Order = 70)]
    public LevelSettings MonthVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Month")]
    [Display(Name = "mVAL", Order = 80)]
    public LevelSettings MonthVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Prev.Month Settings

    [Category("Prev.Month")]
    [Display(Name = "pmOpen", Order = 10)]
    public LevelSettings PrevMonthOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Month")]
    [Display(Name = "pmHigh", Order = 20)]
    public LevelSettings PrevMonthHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Month")]
    [Display(Name = "pmLow", Order = 30)]
    public LevelSettings PrevMonthLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Month")]
    [Display(Name = "pmClose", Order = 40)]
    public LevelSettings PrevMonthCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Month")]
    [Display(Name = "pmEquilibrium", Order = 50)]
    public LevelSettings PrevMonthEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Month")]
    [Display(Name = "pmPOC", Order = 60)]
    public LevelSettings PrevMonthPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Month")]
    [Display(Name = "pmVAH", Order = 70)]
    public LevelSettings PrevMonthVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Prev.Month")]
    [Display(Name = "pmVAL", Order = 80)]
    public LevelSettings PrevMonthVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    #endregion

    #region Contract Settings

    [Category("Contract")]
    [Display(Name = "cOpen", Order = 10)]
    public LevelSettings ContractOpenLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Contract")]
    [Display(Name = "cHigh", Order = 20)]
    public LevelSettings ContractHighLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Green.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Contract")]
    [Display(Name = "cLow", Order = 30)]
    public LevelSettings ContractLowLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Red.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Contract")]
    [Display(Name = "cClose", Order = 40)]
    public LevelSettings ContractCloseLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Gray.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Contract")]
    [Display(Name = "cEquilibrium", Order = 50)]
    public LevelSettings ContractEquilibriumLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Yellow.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dash,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Contract")]
    [Display(Name = "cPOC", Order = 60)]
    public LevelSettings ContractPOCLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Orange.Convert(),
        width: 2,
        lineStyle: LineDashStyle.Solid,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Contract")]
    [Display(Name = "cVAH", Order = 70)]
    public LevelSettings ContractVAHLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

    [Category("Contract")]
    [Display(Name = "cVAL", Order = 80)]
    public LevelSettings ContractVALLevel { get; set; } = new(
        enabled: false,
        color: System.Drawing.Color.Purple.Convert(),
        width: 1,
        lineStyle: LineDashStyle.Dot,
        showPrice: true,
        labelPosition: LabelPosition.Bar,
        lineType: LineType.Bar
    );

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
        RenderLevel(context, "dOpen", DayOpenLevel);
        RenderLevel(context, "dHigh", DayHighLevel);
        RenderLevel(context, "dLow", DayLowLevel);
        RenderLevel(context, "dClose", DayCloseLevel);
        RenderLevel(context, "dEQ", DayEquilibriumLevel);
        RenderLevel(context, "dPOC", DayPOCLevel);
        RenderLevel(context, "dVAH", DayVAHLevel);
        RenderLevel(context, "dVAL", DayVALLevel);

        // Render Prev.Day levels
        RenderLevel(context, "pOpen", PrevDayOpenLevel);
        RenderLevel(context, "pHigh", PrevDayHighLevel);
        RenderLevel(context, "pLow", PrevDayLowLevel);
        RenderLevel(context, "pClose", PrevDayCloseLevel);
        RenderLevel(context, "pEQ", PrevDayEquilibriumLevel);
        RenderLevel(context, "pPOC", PrevDayPOCLevel);
        RenderLevel(context, "pVAH", PrevDayVAHLevel);
        RenderLevel(context, "pVAL", PrevDayVALLevel);

        // Render Week levels
        RenderLevel(context, "wOpen", WeekOpenLevel);
        RenderLevel(context, "wHigh", WeekHighLevel);
        RenderLevel(context, "wLow", WeekLowLevel);
        RenderLevel(context, "wClose", WeekCloseLevel);
        RenderLevel(context, "wEQ", WeekEquilibriumLevel);
        RenderLevel(context, "wPOC", WeekPOCLevel);
        RenderLevel(context, "wVAH", WeekVAHLevel);
        RenderLevel(context, "wVAL", WeekVALLevel);

        // Render Prev.Week levels
        RenderLevel(context, "pwOpen", PrevWeekOpenLevel);
        RenderLevel(context, "pwHigh", PrevWeekHighLevel);
        RenderLevel(context, "pwLow", PrevWeekLowLevel);
        RenderLevel(context, "pwClose", PrevWeekCloseLevel);
        RenderLevel(context, "pwEQ", PrevWeekEquilibriumLevel);
        RenderLevel(context, "pwPOC", PrevWeekPOCLevel);
        RenderLevel(context, "pwVAH", PrevWeekVAHLevel);
        RenderLevel(context, "pwVAL", PrevWeekVALLevel);

        // Render Month levels
        RenderLevel(context, "mOpen", MonthOpenLevel);
        RenderLevel(context, "mHigh", MonthHighLevel);
        RenderLevel(context, "mLow", MonthLowLevel);
        RenderLevel(context, "mClose", MonthCloseLevel);
        RenderLevel(context, "mEQ", MonthEquilibriumLevel);
        RenderLevel(context, "mPOC", MonthPOCLevel);
        RenderLevel(context, "mVAH", MonthVAHLevel);
        RenderLevel(context, "mVAL", MonthVALLevel);

        // Render Prev.Month levels
        RenderLevel(context, "pmOpen", PrevMonthOpenLevel);
        RenderLevel(context, "pmHigh", PrevMonthHighLevel);
        RenderLevel(context, "pmLow", PrevMonthLowLevel);
        RenderLevel(context, "pmClose", PrevMonthCloseLevel);
        RenderLevel(context, "pmEQ", PrevMonthEquilibriumLevel);
        RenderLevel(context, "pmPOC", PrevMonthPOCLevel);
        RenderLevel(context, "pmVAH", PrevMonthVAHLevel);
        RenderLevel(context, "pmVAL", PrevMonthVALLevel);

        // Render Contract levels
        RenderLevel(context, "cOpen", ContractOpenLevel);
        RenderLevel(context, "cHigh", ContractHighLevel);
        RenderLevel(context, "cLow", ContractLowLevel);
        RenderLevel(context, "cClose", ContractCloseLevel);
        RenderLevel(context, "cEQ", ContractEquilibriumLevel);
        RenderLevel(context, "cPOC", ContractPOCLevel);
        RenderLevel(context, "cVAH", ContractVAHLevel);
        RenderLevel(context, "cVAL", ContractVALLevel);
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
        return DayOpenLevel.Enabled || DayHighLevel.Enabled || DayLowLevel.Enabled || DayCloseLevel.Enabled ||
               DayEquilibriumLevel.Enabled || DayPOCLevel.Enabled || DayVAHLevel.Enabled || DayVALLevel.Enabled;
    }

    private bool NeedsPrevDayData()
    {
        return PrevDayOpenLevel.Enabled || PrevDayHighLevel.Enabled || PrevDayLowLevel.Enabled || PrevDayCloseLevel.Enabled ||
               PrevDayEquilibriumLevel.Enabled || PrevDayPOCLevel.Enabled || PrevDayVAHLevel.Enabled || PrevDayVALLevel.Enabled;
    }

    private bool NeedsWeekData()
    {
        return WeekOpenLevel.Enabled || WeekHighLevel.Enabled || WeekLowLevel.Enabled || WeekCloseLevel.Enabled ||
               WeekEquilibriumLevel.Enabled || WeekPOCLevel.Enabled || WeekVAHLevel.Enabled || WeekVALLevel.Enabled;
    }

    private bool NeedsPrevWeekData()
    {
        return PrevWeekOpenLevel.Enabled || PrevWeekHighLevel.Enabled || PrevWeekLowLevel.Enabled || PrevWeekCloseLevel.Enabled ||
               PrevWeekEquilibriumLevel.Enabled || PrevWeekPOCLevel.Enabled || PrevWeekVAHLevel.Enabled || PrevWeekVALLevel.Enabled;
    }

    private bool NeedsMonthData()
    {
        return MonthOpenLevel.Enabled || MonthHighLevel.Enabled || MonthLowLevel.Enabled || MonthCloseLevel.Enabled ||
               MonthEquilibriumLevel.Enabled || MonthPOCLevel.Enabled || MonthVAHLevel.Enabled || MonthVALLevel.Enabled;
    }

    private bool NeedsPrevMonthData()
    {
        return PrevMonthOpenLevel.Enabled || PrevMonthHighLevel.Enabled || PrevMonthLowLevel.Enabled || PrevMonthCloseLevel.Enabled ||
               PrevMonthEquilibriumLevel.Enabled || PrevMonthPOCLevel.Enabled || PrevMonthVAHLevel.Enabled || PrevMonthVALLevel.Enabled;
    }

    private bool NeedsContractData()
    {
        return ContractOpenLevel.Enabled || ContractHighLevel.Enabled || ContractLowLevel.Enabled || ContractCloseLevel.Enabled ||
               ContractEquilibriumLevel.Enabled || ContractPOCLevel.Enabled || ContractVAHLevel.Enabled || ContractVALLevel.Enabled;
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

    private void RenderLevel(RenderContext context, string levelKey, LevelSettings levelSettings)
    {
        if (!levelSettings.Enabled || !_levels.TryGetValue(levelKey, out var level) || !level.IsValid)
            return;

        var y = ChartInfo.GetYByPrice(level.Price, false);
        var chartWidth = ChartInfo.PriceChartContainer.Region.Width;
        var currentBarX = ChartInfo.GetXByBar(CurrentBar - 1);
        var barWidth = (int)ChartInfo.PriceChartContainer.BarsWidth;
        var currentBarRightX = currentBarX + barWidth;

        // Get pen from LevelSettings
        var renderPen = levelSettings.RenderPen;

        // Draw line first (if LineType != None)
        switch (levelSettings.LineType)
        {
            case LineType.Bar:
                // If label is at bar position, start line after the label to avoid overlap
                if (levelSettings.LabelPosition == LabelPosition.Bar)
                {
                    // Estimate label width and start line after it
                    var labelStartX = currentBarRightX + 5;
                    var estimatedLabelWidth = 30; // Approximate width for label
                    var lineStartX = labelStartX + estimatedLabelWidth + 2;
                    context.DrawLine(renderPen, lineStartX, y, chartWidth, y);
                }
                else
                {
                    // Normal bar line from right edge of bar to price axis
                    context.DrawLine(renderPen, currentBarRightX, y, chartWidth, y);
                }
                break;
            case LineType.Full:
                context.DrawLine(renderPen, 0, y, chartWidth, y);
                break;
            case LineType.None:
                // No line to draw
                break;
        }

        // Draw price label (if ShowPrice == true)
        if (levelSettings.ShowPrice)
        {
            DrawPriceLabel(context, level.Price, y, renderPen, levelSettings);
        }

        // Draw text label (if LabelPosition != None)
        switch (levelSettings.LabelPosition)
        {
            case LabelPosition.Bar:
                var barLabelX = currentBarRightX + 5;
                DrawTextLabel(context, level.Label, barLabelX, y, renderPen, false);
                break;
            case LabelPosition.Right:
                var rightLabelX = chartWidth - 5;
                DrawTextLabel(context, level.Label, rightLabelX, y, renderPen, true);
                break;
            case LabelPosition.Left:
                var leftLabelX = 5;
                DrawTextLabel(context, level.Label, leftLabelX, y, renderPen, false);
                break;
            case LabelPosition.None:
                // No text label to draw
                break;
        }
    }

    private void DrawPriceLabel(RenderContext context, decimal price, int y, RenderPen pen, LevelSettings levelSettings)
    {
        var priceText = string.Format(ChartInfo.StringFormat, price);
        
        // Calculate contrasting text color based on background color
        var backgroundColor = levelSettings.Color;
        var textColor = GetContrastingColor(backgroundColor);
        
        this.DrawLabelOnPriceAxis(context, priceText, y, _axisFont, backgroundColor.Convert(), textColor.Convert());
    }
    
    private CrossColor GetContrastingColor(CrossColor backgroundColor)
    {
        // Calculate luminance using relative luminance formula
        // See: https://www.w3.org/TR/WCAG20/#relativeluminancedef
        double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B) / 255;
        
        // If background is dark, use white text; if light, use black text
        if (luminance > 0.5)
        {
            // Dark text for light backgrounds
            return CrossColors.Black;
        }
        else
        {
            // Light text for dark backgrounds
            return CrossColors.White;
        }
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