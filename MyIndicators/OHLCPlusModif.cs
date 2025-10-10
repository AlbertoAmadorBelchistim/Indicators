namespace MyIndicators
{
    using ATAS.Indicators;
    using ATAS.Indicators.Filters;
    using OFT.Attributes;
    using OFT.Attributes.Editors;
    using OFT.Localization;
    using OFT.Rendering.Context;
    using OFT.Rendering.Settings;
    using OFT.Rendering.Tools;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Drawing;

    using Color = System.Drawing.Color;
    using Parameter = OFT.Attributes.ParameterAttribute;

    public enum LabelPosition
    {
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.None))]
        None = 0,

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bar))]
        Bar = 1,

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Right))]
        Right = 2,

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Left))]
        Left = 3
    }

    public enum LineType
    {
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.None))]
        None = 0,

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.TillBar))]
        Bar = 1,

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.FullWidth))]
        Full = 2
    }

    [Editor(typeof(ATAS.Indicators.Technical.Editors.LevelSettingsEditor), typeof(ATAS.Indicators.Technical.Editors.LevelSettingsEditor))]
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

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled))]
        public bool Enabled { get; set; }

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color))]
        public CrossColor Color { get; set; }

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowPrice))]
        public bool ShowPrice { get; set; }

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Line))]
        public LineType LineType { get; set; }

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Width))]
        [Range(1, 10)]
        public int Width { get; set; }

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineStyle))]
        public LineDashStyle LineStyle { get; set; }

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Label))]
        public LabelPosition LabelPosition { get; set; }

        [Display(Name = "Override label (optional)")]
        public string OverrideLabel { get; set; } = string.Empty;

        [Browsable(false)]
        public RenderPen RenderPen => new PenSettings { Color = Color, Width = Width, LineDashStyle = LineStyle }.RenderObject;
    }

    [DisplayName("OHLC Plus Modif")]
    [Category(IndicatorCategories.VolumeOrderFlow)]
    [Display(ResourceType = typeof(Strings), Description = nameof(Strings.OHLCPlusDescription))]
    public class OHLCPlus : Indicator
    {
        #region Nested types

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
        private RenderStringFormat _stringRightFormat = new()
        {
            Alignment = StringAlignment.Far,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };

        private RenderStringFormat _stringLeftFormat = new()
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };

        #endregion

        #region Properties

        #region Day Settings

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarOpen), Order = 10)]
        public LevelSettings DayOpenLevel { get; set; } = new(
            enabled: true,
            color: System.Drawing.Color.Orange.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarHigh), Order = 20)]
        public LevelSettings DayHighLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Green.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarLow), Order = 30)]
        public LevelSettings DayLowLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Red.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.BarClose), Order = 40)]
        public LevelSettings DayCloseLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Gray.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.Equilibrium), Order = 50)]
        public LevelSettings DayEquilibriumLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Yellow.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dash,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.POC), Order = 60)]
        public LevelSettings DayPOCLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentDay), Order = 65)]
        public LevelSettings DayVWAPLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.SteelBlue.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.VAH), Order = 70)]
        public LevelSettings DayVAHLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Purple.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dot,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentDay), Name = nameof(Strings.VAL), Order = 80)]
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

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarOpen), Order = 10)]
        public LevelSettings PrevDayOpenLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarHigh), Order = 20)]
        public LevelSettings PrevDayHighLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Green.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarLow), Order = 30)]
        public LevelSettings PrevDayLowLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Red.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.BarClose), Order = 40)]
        public LevelSettings PrevDayCloseLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Gray.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.Equilibrium), Order = 50)]
        public LevelSettings PrevDayEquilibriumLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Yellow.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dash,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.POC), Order = 60)]
        public LevelSettings PrevDayPOCLevel { get; set; } = new(
            enabled: true,
            color: System.Drawing.Color.Orange.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousDay), Order = 65)]
        public LevelSettings PrevDayVWAPLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.SteelBlue.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.VAH), Order = 70)]
        public LevelSettings PrevDayVAHLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Purple.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dot,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousDay), Name = nameof(Strings.VAL), Order = 80)]
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

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarOpen), Order = 10)]
        public LevelSettings WeekOpenLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarHigh), Order = 20)]
        public LevelSettings WeekHighLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Green.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarLow), Order = 30)]
        public LevelSettings WeekLowLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Red.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.BarClose), Order = 40)]
        public LevelSettings WeekCloseLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Gray.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.Equilibrium), Order = 50)]
        public LevelSettings WeekEquilibriumLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Yellow.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dash,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.POC), Order = 60)]
        public LevelSettings WeekPOCLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentWeek), Order = 65)]
        public LevelSettings WeekVWAPLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.SteelBlue.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.VAH), Order = 70)]
        public LevelSettings WeekVAHLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Purple.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dot,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentWeek), Name = nameof(Strings.VAL), Order = 80)]
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

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarOpen), Order = 10)]
        public LevelSettings PrevWeekOpenLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarHigh), Order = 20)]
        public LevelSettings PrevWeekHighLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Green.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarLow), Order = 30)]
        public LevelSettings PrevWeekLowLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Red.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.BarClose), Order = 40)]
        public LevelSettings PrevWeekCloseLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Gray.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.Equilibrium), Order = 50)]
        public LevelSettings PrevWeekEquilibriumLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Yellow.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dash,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.POC), Order = 60)]
        public LevelSettings PrevWeekPOCLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousWeek), Order = 65)]
        public LevelSettings PrevWeekVWAPLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.SteelBlue.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.VAH), Order = 70)]
        public LevelSettings PrevWeekVAHLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Purple.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dot,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousWeek), Name = nameof(Strings.VAL), Order = 80)]
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

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarOpen), Order = 10)]
        public LevelSettings MonthOpenLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarHigh), Order = 20)]
        public LevelSettings MonthHighLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Green.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarLow), Order = 30)]
        public LevelSettings MonthLowLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Red.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.BarClose), Order = 40)]
        public LevelSettings MonthCloseLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Gray.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.Equilibrium), Order = 50)]
        public LevelSettings MonthEquilibriumLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Yellow.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dash,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.POC), Order = 60)]
        public LevelSettings MonthPOCLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.CurrentMonth), Order = 65)]
        public LevelSettings MonthVWAPLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.SteelBlue.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.VAH), Order = 70)]
        public LevelSettings MonthVAHLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Purple.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dot,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.CurrentMonth), Name = nameof(Strings.VAL), Order = 80)]
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

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarOpen), Order = 10)]
        public LevelSettings PrevMonthOpenLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarHigh), Order = 20)]
        public LevelSettings PrevMonthHighLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Green.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarLow), Order = 30)]
        public LevelSettings PrevMonthLowLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Red.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.BarClose), Order = 40)]
        public LevelSettings PrevMonthCloseLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Gray.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.Equilibrium), Order = 50)]
        public LevelSettings PrevMonthEquilibriumLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Yellow.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dash,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.POC), Order = 60)]
        public LevelSettings PrevMonthPOCLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.PreviousMonth), Order = 65)]
        public LevelSettings PrevMonthVWAPLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.SteelBlue.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.VAH), Order = 70)]
        public LevelSettings PrevMonthVAHLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Purple.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dot,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.PreviousMonth), Name = nameof(Strings.VAL), Order = 80)]
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

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarOpen), Order = 10)]
        public LevelSettings ContractOpenLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarHigh), Order = 20)]
        public LevelSettings ContractHighLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Green.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarLow), Order = 30)]
        public LevelSettings ContractLowLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Red.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.BarClose), Order = 40)]
        public LevelSettings ContractCloseLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Gray.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.Equilibrium), Order = 50)]
        public LevelSettings ContractEquilibriumLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Yellow.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dash,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.POC), Order = 60)]
        public LevelSettings ContractPOCLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Orange.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.VWAP), GroupName = nameof(Strings.Contract), Order = 65)]
        public LevelSettings ContractVWAPLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.SteelBlue.Convert(),
            width: 2,
            lineStyle: LineDashStyle.Solid,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.VAH), Order = 70)]
        public LevelSettings ContractVAHLevel { get; set; } = new(
            enabled: false,
            color: System.Drawing.Color.Purple.Convert(),
            width: 1,
            lineStyle: LineDashStyle.Dot,
            showPrice: true,
            labelPosition: LabelPosition.Bar,
            lineType: LineType.Bar
        );

        [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Contract), Name = nameof(Strings.VAL), Order = 80)]
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

        // Template supports {prefix} and {level}
        public string LabelTemplate = "{prefix}{level}";

        #region Prefixes
        // Defaults kept to current behavior
        private string _prefixCurrentDay = "d";
        [Display(GroupName = "Prefixes", Name = "Current Day", Order = 10)]
        public string PrefixCurrentDay
        {
            get => _prefixCurrentDay;
            set => SetProperty(ref _prefixCurrentDay, value, OnUiSettingChanged);
        }

        private string _prefixPrevDay = "p";
        [Display(GroupName = "Prefixes", Name = "Previous Day", Order = 20)]
        public string PrefixPrevDay
        {
            get => _prefixPrevDay;
            set => SetProperty(ref _prefixPrevDay, value, OnUiSettingChanged);
        }

        private string _prefixCurrentWeek = "w";
        [Display(GroupName = "Prefixes", Name = "Current Week", Order = 30)]
        public string PrefixCurrentWeek
        {
            get => _prefixCurrentWeek;
            set => SetProperty(ref _prefixCurrentWeek, value, OnUiSettingChanged);
        }

        private string _prefixPrevWeek = "pw";
        [Display(GroupName = "Prefixes", Name = "Previous Week", Order = 40)]
        public string PrefixPrevWeek
        {
            get => _prefixPrevWeek;
            set => SetProperty(ref _prefixPrevWeek, value, OnUiSettingChanged);
        }

        private string _prefixCurrentMonth = "m";
        [Display(GroupName = "Prefixes", Name = "Current Month", Order = 50)]
        public string PrefixCurrentMonth
        {
            get => _prefixCurrentMonth;
            set => SetProperty(ref _prefixCurrentMonth, value, OnUiSettingChanged);
        }

        private string _prefixPrevMonth = "pm";
        [Display(GroupName = "Prefixes", Name = "Previous Month", Order = 60)]
        public string PrefixPrevMonth
        {
            get => _prefixPrevMonth;
            set => SetProperty(ref _prefixPrevMonth, value, OnUiSettingChanged);
        }

        private string _prefixContract = "c";
        [Display(GroupName = "Prefixes", Name = "Contract", Order = 70)]
        public string PrefixContract
        {
            get => _prefixContract;
            set => SetProperty(ref _prefixContract, value, OnUiSettingChanged);
        }
        #endregion Prefixes

        #region Labels
        private string _openLabel = "Open";
        [Display(GroupName = "Labels", Name = "Open", Order = 10)]
        public string OpenLabel
        {
            get => _openLabel;
            set => SetProperty(ref _openLabel, value, OnUiSettingChanged);
        }

        private string _highLabel = "High";
        [Display(GroupName = "Labels", Name = "High", Order = 20)]
        public string HighLabel
        {
            get => _highLabel;
            set => SetProperty(ref _highLabel, value, OnUiSettingChanged);
        }

        private string _lowLabel = "Low";
        [Display(GroupName = "Labels", Name = "Low", Order = 30)]
        public string LowLabel
        {
            get => _lowLabel;
            set => SetProperty(ref _lowLabel, value, OnUiSettingChanged);
        }

        private string _closeLabel = "Close";
        [Display(GroupName = "Labels", Name = "Close", Order = 40)]
        public string CloseLabel
        {
            get => _closeLabel;
            set => SetProperty(ref _closeLabel, value, OnUiSettingChanged);
        }

        private string _eqLabel = "EQ";
        [Display(GroupName = "Labels", Name = "Equilibrium", Order = 50)]
        public string EqLabel
        {
            get => _eqLabel;
            set => SetProperty(ref _eqLabel, value, OnUiSettingChanged);
        }

        private string _pocLabel = "POC";
        [Display(GroupName = "Labels", Name = "POC", Order = 60)]
        public string POCLabel
        {
            get => _pocLabel;
            set => SetProperty(ref _pocLabel, value, OnUiSettingChanged);
        }

        private string _vwapLabel = "VWAP";
        [Display(GroupName = "Labels", Name = "VWAP", Order = 70)]
        public string VWAPLabel
        {
            get => _vwapLabel;
            set => SetProperty(ref _vwapLabel, value, OnUiSettingChanged);
        }

        private string _vahLabel = "VAH";
        [Display(GroupName = "Labels", Name = "VAH", Order = 80)]
        public string VAHLabel
        {
            get => _vahLabel;
            set => SetProperty(ref _vahLabel, value, OnUiSettingChanged);
        }

        private string _valLabel = "VAL";
        [Display(GroupName = "Labels", Name = "VAL", Order = 90)]
        public string VALLabel
        {
            get => _valLabel;
            set => SetProperty(ref _valLabel, value, OnUiSettingChanged);
        }

        #endregion Labels

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

            // Render all levels in groups for better organization
            RenderLevelGroup(context, PrefixCurrentDay, DayOpenLevel, DayHighLevel, DayLowLevel, DayCloseLevel, DayEquilibriumLevel, DayPOCLevel, DayVWAPLevel, DayVAHLevel, DayVALLevel);
            RenderLevelGroup(context, PrefixPrevDay, PrevDayOpenLevel, PrevDayHighLevel, PrevDayLowLevel, PrevDayCloseLevel, PrevDayEquilibriumLevel, PrevDayPOCLevel, PrevDayVWAPLevel, PrevDayVAHLevel, PrevDayVALLevel);
            RenderLevelGroup(context, PrefixCurrentWeek, WeekOpenLevel, WeekHighLevel, WeekLowLevel, WeekCloseLevel, WeekEquilibriumLevel, WeekPOCLevel, WeekVWAPLevel, WeekVAHLevel, WeekVALLevel);
            RenderLevelGroup(context, PrefixPrevWeek, PrevWeekOpenLevel, PrevWeekHighLevel, PrevWeekLowLevel, PrevWeekCloseLevel, PrevWeekEquilibriumLevel, PrevWeekPOCLevel, PrevWeekVWAPLevel, PrevWeekVAHLevel, PrevWeekVALLevel);
            RenderLevelGroup(context, PrefixCurrentMonth, MonthOpenLevel, MonthHighLevel, MonthLowLevel, MonthCloseLevel, MonthEquilibriumLevel, MonthPOCLevel, MonthVWAPLevel, MonthVAHLevel, MonthVALLevel);
            RenderLevelGroup(context, PrefixPrevMonth, PrevMonthOpenLevel, PrevMonthHighLevel, PrevMonthLowLevel, PrevMonthCloseLevel, PrevMonthEquilibriumLevel, PrevMonthPOCLevel, PrevMonthVWAPLevel, PrevMonthVAHLevel, PrevMonthVALLevel);
            RenderLevelGroup(context, PrefixContract, ContractOpenLevel, ContractHighLevel, ContractLowLevel, ContractCloseLevel, ContractEquilibriumLevel, ContractPOCLevel, ContractVWAPLevel, ContractVAHLevel, ContractVALLevel);
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
                   DayEquilibriumLevel.Enabled || DayPOCLevel.Enabled || DayVWAPLevel.Enabled || DayVAHLevel.Enabled || DayVALLevel.Enabled;
        }

        private bool NeedsPrevDayData()
        {
            return PrevDayOpenLevel.Enabled || PrevDayHighLevel.Enabled || PrevDayLowLevel.Enabled || PrevDayCloseLevel.Enabled ||
                   PrevDayEquilibriumLevel.Enabled || PrevDayPOCLevel.Enabled || PrevDayVWAPLevel.Enabled || PrevDayVAHLevel.Enabled || PrevDayVALLevel.Enabled;
        }

        private bool NeedsWeekData()
        {
            return WeekOpenLevel.Enabled || WeekHighLevel.Enabled || WeekLowLevel.Enabled || WeekCloseLevel.Enabled ||
                   WeekEquilibriumLevel.Enabled || WeekPOCLevel.Enabled || WeekVWAPLevel.Enabled || WeekVAHLevel.Enabled || WeekVALLevel.Enabled;
        }

        private bool NeedsPrevWeekData()
        {
            return PrevWeekOpenLevel.Enabled || PrevWeekHighLevel.Enabled || PrevWeekLowLevel.Enabled || PrevWeekCloseLevel.Enabled ||
                   PrevWeekEquilibriumLevel.Enabled || PrevWeekPOCLevel.Enabled || PrevWeekVWAPLevel.Enabled || PrevWeekVAHLevel.Enabled || PrevWeekVALLevel.Enabled;
        }

        private bool NeedsMonthData()
        {
            return MonthOpenLevel.Enabled || MonthHighLevel.Enabled || MonthLowLevel.Enabled || MonthCloseLevel.Enabled ||
                   MonthEquilibriumLevel.Enabled || MonthPOCLevel.Enabled || MonthVWAPLevel.Enabled || MonthVAHLevel.Enabled || MonthVALLevel.Enabled;
        }

        private bool NeedsPrevMonthData()
        {
            return PrevMonthOpenLevel.Enabled || PrevMonthHighLevel.Enabled || PrevMonthLowLevel.Enabled || PrevMonthCloseLevel.Enabled ||
                   PrevMonthEquilibriumLevel.Enabled || PrevMonthPOCLevel.Enabled || PrevMonthVWAPLevel.Enabled || PrevMonthVAHLevel.Enabled || PrevMonthVALLevel.Enabled;
        }

        private bool NeedsContractData()
        {
            return ContractOpenLevel.Enabled || ContractHighLevel.Enabled || ContractLowLevel.Enabled || ContractCloseLevel.Enabled ||
                   ContractEquilibriumLevel.Enabled || ContractPOCLevel.Enabled || ContractVWAPLevel.Enabled || ContractVAHLevel.Enabled || ContractVALLevel.Enabled;
        }

        private void UpdateLevels(FixedProfilePeriods period, IndicatorCandle candle)
        {
            if (candle == null)
                return;

            var prefix = PrefixFor(period);

            // OHLC
            SetLevel($"{prefix}Open", candle.Open);
            SetLevel($"{prefix}High", candle.High);
            SetLevel($"{prefix}Low", candle.Low);
            SetLevel($"{prefix}Close", candle.Close);
            SetLevel($"{prefix}EQ", (candle.High + candle.Low) / 2m);

            // Volume-based
            if (candle.MaxVolumePriceInfo != null && candle.MaxVolumePriceInfo.Price > 0)
                SetLevel($"{prefix}POC", candle.MaxVolumePriceInfo.Price);

            if (candle.VWAP > 0)
                SetLevel($"{prefix}VWAP", candle.VWAP);

            if (candle.ValueArea != null &&
                candle.ValueArea.ValueAreaHigh > 0 &&
                candle.ValueArea.ValueAreaLow > 0 &&
                candle.ValueArea.ValueAreaHigh >= candle.ValueArea.ValueAreaLow)
            {
                SetLevel($"{prefix}VAH", candle.ValueArea.ValueAreaHigh);
                SetLevel($"{prefix}VAL", candle.ValueArea.ValueAreaLow);
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

        private void RenderLevel(RenderContext context, string levelKey, LevelSettings levelSettings, string displayLabel)
        {
            if (!levelSettings.Enabled || !_levels.TryGetValue(levelKey, out var level) || !level.IsValid)
                return;

            if (level.Price <= 0)
                return;

            var y = ChartInfo.GetYByPrice(level.Price, false);
            if (y < 0 || y > ChartInfo.PriceChartContainer.Region.Height)
                return;

            var chartWidth = ChartInfo.PriceChartContainer.Region.Width;
            var currentBarX = ChartInfo.GetXByBar(CurrentBar - 1);
            var barWidth = (int)ChartInfo.PriceChartContainer.BarsWidth;
            var currentBarRightX = currentBarX + barWidth;

            var renderPen = levelSettings.RenderPen;

            // Draw line
            switch (levelSettings.LineType)
            {
                case LineType.Bar:
                    if (levelSettings.LabelPosition == LabelPosition.Bar)
                    {
                        var size = context.MeasureString(displayLabel, _font);
                        var labelStartX = currentBarRightX + 5;
                        var lineStartX = labelStartX + size.Width + 4;
                        context.DrawLine(renderPen, lineStartX, y, chartWidth, y);
                    }
                    else
                    {
                        context.DrawLine(renderPen, currentBarRightX, y, chartWidth, y);
                    }
                    break;
                case LineType.Full:
                    context.DrawLine(renderPen, 0, y, chartWidth, y);
                    break;
                case LineType.None:
                    break;
            }

            if (levelSettings.ShowPrice)
                DrawPriceLabel(context, level.Price, y, renderPen, levelSettings);

            // Draw text label at chosen position
            switch (levelSettings.LabelPosition)
            {
                case LabelPosition.Bar:
                    DrawTextLabel(context, displayLabel, currentBarRightX + 5, y, renderPen, false);
                    break;
                case LabelPosition.Right:
                    DrawTextLabel(context, displayLabel, chartWidth - 5, y, renderPen, true);
                    break;
                case LabelPosition.Left:
                    DrawTextLabel(context, displayLabel, 5, y, renderPen, false);
                    break;
                case LabelPosition.None:
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
            var format = alignRight ? _stringRightFormat : _stringLeftFormat;
            context.DrawString(text, _font, textColor, textRect, format);
        }

        private void RenderLevelGroup(
        RenderContext context,
        string prefix,
        LevelSettings openLevel, LevelSettings highLevel, LevelSettings lowLevel, LevelSettings closeLevel,
        LevelSettings eqLevel, LevelSettings pocLevel, LevelSettings vwapLevel, LevelSettings vahLevel, LevelSettings valLevel)
        {
            var items = new (string Suffix, LevelSettings LS)[]
            {
        ("Open", openLevel),
        ("High", highLevel),
        ("Low",  lowLevel),
        ("Close", closeLevel),
        ("EQ",   eqLevel),
        ("POC",  pocLevel),
        ("VWAP", vwapLevel),
        ("VAH",  vahLevel),
        ("VAL",  valLevel)
            };

            foreach (var (suffix, ls) in items)
            {
                var key = $"{prefix}{suffix}";
                var levelText = ResolveLevelText(prefix, suffix, ls);
                var display = BuildDisplayLabel(prefix, levelText);
                RenderLevel(context, key, ls, display);
            }
        }

        // Returns the UI-selected prefix for a fixed profile period
        private string PrefixFor(FixedProfilePeriods period) => period switch
        {
            FixedProfilePeriods.CurrentDay => PrefixCurrentDay,
            FixedProfilePeriods.LastDay => PrefixPrevDay,
            FixedProfilePeriods.CurrentWeek => PrefixCurrentWeek,
            FixedProfilePeriods.LastWeek => PrefixPrevWeek,
            FixedProfilePeriods.CurrentMonth => PrefixCurrentMonth,
            FixedProfilePeriods.LastMonth => PrefixPrevMonth,
            FixedProfilePeriods.Contract => PrefixContract,
            _ => string.Empty
        };

        // Builds the human readable label from template + level name
        private string BuildLabel(string prefix, string levelName)
            => (LabelTemplate ?? "{prefix} {level}")
                .Replace("{prefix}", prefix ?? string.Empty)
                .Replace("{level}", levelName ?? string.Empty);

        // New setter that accepts an explicit label (instead of tying to the key)
        private void SetLevel(string key, decimal price)
        {
            if (price <= 0) return;

            if (!_levels.TryGetValue(key, out var lvl))
                _levels[key] = lvl = new LevelData();

            lvl.Price = price;
            lvl.IsValid = true;
        }

        // Rebuild _levels from cached profile candles using current UI prefixes/labels
        private void RebuildLevelsFromCachedProfiles()
        {
            _levels.Clear();

            // Re-apply all cached profile candles with current prefixes & labels
            foreach (var kv in _profileCandles)
            {
                var period = kv.Key;
                var candle = kv.Value;
                UpdateLevels(period, candle);
            }
        }

        // Highest precedence: LevelSettings.OverrideLabel
        // Then specific global overrides (e.g., PrevDayCloseOverride)
        // Then default per-level label (OpenLabel, etc.)
        private string ResolveLevelText(string prefix, string levelName, LevelSettings ls)
        {
            if (!string.IsNullOrWhiteSpace(ls?.OverrideLabel))
                return ls.OverrideLabel;

            // Fall back to default configured labels
            return levelName switch
            {
                "Open" => OpenLabel,
                "High" => HighLabel,
                "Low" => LowLabel,
                "Close" => CloseLabel,
                "EQ" => EqLabel,
                "POC" => POCLabel,
                "VWAP" => VWAPLabel,
                "VAH" => VAHLabel,
                "VAL" => VALLabel,
                _ => levelName
            };
        }

        private string BuildDisplayLabel(string prefix, string levelText)
            => (LabelTemplate ?? "{prefix} {level}")
                .Replace("{prefix}", prefix ?? string.Empty)
                .Replace("{level}", levelText ?? string.Empty);

        // Llama a esto desde el onChanged de SetProperty
        private void OnUiSettingChanged()
        {
            // Si ya tenemos perfiles cacheados, rehacer claves/etiquetas y redibujar
            if (_profileCandles.Count > 0)
            {
                RebuildLevelsFromCachedProfiles();
                RedrawChart();
            }
            else
            {
                // Fuerza recálculo si aún no hay cache
                RecalculateValues();
            }
        }

        #endregion
    }
}
