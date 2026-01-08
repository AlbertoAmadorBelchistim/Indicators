namespace ATAS.Indicators.Technical;

using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical.Properties;
using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using Color = System.Drawing.Color;

[DisplayName("Daily Lines")]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.DailyLinesDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602284")]
public class DailyLines : Indicator
{
	#region Nested types

	private class SessionRange
	{
		#region Properties

		public int OpenBar { get; private set; } = -1;

		public decimal OpenPrice { get; private set; }

		public int HighBar { get; private set; }

		public decimal HighPrice { get; private set; } = decimal.MinValue;

		public int LowBar { get; private set; }

		public decimal LowPrice { get; private set; } = decimal.MaxValue;

		public int CloseBar { get; private set; }

		public decimal ClosePrice { get; private set; }

		public bool IsFinished { get; set; }

		#endregion

		#region ctor

		public SessionRange()
		{
		}

		public SessionRange(IndicatorCandle candle, int bar)
		{
			OpenBar = CloseBar = HighBar = LowBar = bar;
			OpenPrice = candle.Open;
			HighPrice = candle.High;
			LowPrice = candle.Low;
			ClosePrice = candle.Close;
		}

		#endregion

		internal void IncCandle(IndicatorCandle candle, int bar)
		{
			if (OpenBar < 0)
			{
				OpenPrice = candle.Open;
				OpenBar = bar;
			}

			if (candle.High > HighPrice)
			{
				HighPrice = candle.High;
				HighBar = bar;
			}

			if (candle.Low < LowPrice)
			{
				LowPrice = candle.Low;
				LowBar = bar;
			}

			ClosePrice = candle.Close;
			CloseBar = bar;
		}
	}

    private sealed class SessionState
    {
        public SessionRange Current { get; set; } = new();
        public SessionRange Previous { get; set; } = new();

        public void Reset()
        {
            Current = new SessionRange();
            Previous = new SessionRange();
        }

        public void RollToNext(IndicatorCandle candle, int bar)
        {
            if (Current.OpenBar >= 0)
            {
                Current.IsFinished = true;
                Previous = Current;
            }

            Current = new SessionRange(candle, bar);
        }
    }

    [Serializable]
	[Obfuscation(Feature = "renaming", ApplyToMembers = true, Exclude = true)]
	public enum PeriodType
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.CurrentDay))]
		CurrentDay,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.PreviousDay))]
		PreviousDay,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.CurrentWeek))]
		CurrenWeek,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.PreviousWeek))]
		PreviousWeek,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.CurrentMonth))]
		CurrentMonth,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.PreviousMonth))]
		PreviousMonth
	}

    private enum SessionTemplateId
    {
        Primary = 0,
        Secondary = 1
    }

    private enum PeriodBucket
    {
        Day = 0,
        Week = 1,
        Month = 2
    }

    [Flags]
    public enum DisplayScopes
    {
        None = 0,

        Day = 1 << 0,
        Week = 1 << 1,
        Month = 1 << 2,

        Rth = 1 << 3,
        Eth = 1 << 4,
        FullDay = 1 << 5
    }

    [Flags]
    public enum LevelMask
    {
        None = 0,
        Open = 1 << 0,
        High = 1 << 1,
        Low = 1 << 2,
        Close = 1 << 3,
        All = Open | High | Low | Close
    }

    private readonly struct SessionTemplate
    {
        public SessionTemplate(bool enabled, TimeSpan start, TimeSpan end, bool applyCustomSessionFilter)
        {
            Enabled = enabled;
            Start = start;
            End = end;
            ApplyCustomSessionFilter = applyCustomSessionFilter;
        }

        public bool Enabled { get; }
        public TimeSpan Start { get; }
        public TimeSpan End { get; }

        // For day bucket we apply session filter; for week/month we don't.
        public bool ApplyCustomSessionFilter { get; }
    }

    #endregion

    #region Fields

    private readonly RenderFont _axisFont = new("Arial", 9);
	private readonly FontSetting _fontSetting = new("Arial", 9);

	[Browsable(false)]
	public readonly RenderStringFormat _format = new()
	{
		Alignment = StringAlignment.Near,
		LineAlignment = StringAlignment.Center,
		Trimming = StringTrimming.EllipsisCharacter
	};

	private bool _customSession;
	private int _days = 60;
	private bool _drawOverChart;
	private bool _newWeekWait;
	private PeriodType _per = PeriodType.PreviousDay;
	private bool _showText = true;
	private int _lastDefaultSession;
    private DateTime _lastTradingDayKey = DateTime.MinValue;

    private const int TemplateCount = 2;
    private const int BucketCount = 3;

    private readonly SessionState[,] _states = new SessionState[TemplateCount, BucketCount];

    // Templates resolved from current properties.
    // Primary uses the current session settings; Secondary derives ETH as the complement of RTH.
    private SessionTemplate _primaryTemplate;
    private SessionTemplate _secondaryTemplate;

    #endregion

    #region Properties

    #region Calculation

    [Browsable(false)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Calculation), Name = nameof(Strings.DaysLookBack), Order = int.MaxValue,
		Description = nameof(Strings.DaysLookBackDescription))]
	[Range(1, 1000)]
	public int Days
	{
		get => _days;
		set
		{
			_days = value;
			RecalculateValues();
		}
	}

    #endregion

	#region Session

    [Parameter]
    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Period), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.PeriodDescription), Order = 110)]
    public PeriodType Period
    {
        get => _per;
        set
        {
            _per = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.CustomSession), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.IsCustomSessionDescription), Order = 120)]
    public bool CustomSession
    {
        get => _customSession;
        set
        {
            _customSession = value;
            FilterStartTime.Enabled = FilterEndTime.Enabled = _customSession;
            RebuildTemplates();
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionBegin), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.SessionBeginDescription), Order = 120)]
    public FilterTimeSpan FilterStartTime { get; set; } = new(false);

    [Browsable(false)]
    public TimeSpan StartTime
    {
        get => FilterStartTime.Value;
        set => FilterStartTime.Value = value;
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionEnd), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.SessionEndDescription), Order = 120)]
    public FilterTimeSpan FilterEndTime { get; set; } = new(false);

    [Browsable(false)]
    public TimeSpan EndTime
    {
        get => FilterEndTime.Value;
        set => FilterEndTime.Value = value;
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.TradingDayStart), GroupName = nameof(Resources.Session), 
        Description = nameof(Resources.TradingDayStartDescription), Order = 121)]
    public FilterTimeSpan TradingDayStart { get; set; } = new(false) { Value = new TimeSpan(18, 0, 0) };

    [Browsable(false)]
    public TimeSpan DayStartTime
    {
        get => TradingDayStart.Value;
        set => TradingDayStart.Value = value;
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.DisplayScopes), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.DisplayScopesDescription), Order = 200)]
    public DisplayScopes Scopes { get; set; } = DisplayScopes.Day | DisplayScopes.Rth | DisplayScopes.Eth;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.HistoryDepth), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.HistoryDepthDescription), Order = 201)]
    public int HistoryDepth
    {
        get => _historyDepth;
        set
        {
            var v = Math.Max(1, Math.Min(2, value)); // 5.3 supports up to 2 (Current/Previous)
            if (_historyDepth == v) return;
            _historyDepth = v;
            RecalculateValues();
        }
    }
    private int _historyDepth = 2;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.DayLevels), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.DayLevelsDescription), Order = 210)]
    public LevelMask DayLevels { get; set; } = LevelMask.All;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.WeekLevels), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.WeekLevelsDescription), Order = 211)]
    public LevelMask WeekLevels { get; set; } = LevelMask.All;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.MonthLevels), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.MonthLevelsDescription), Order = 212)]
    public LevelMask MonthLevels { get; set; } = LevelMask.All;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.RthLevels), GroupName = nameof(Resources.Session),
        Description = nameof(Resources.RthLevelsDescription), Order = 213)]
    public LevelMask RthLevels { get; set; } = LevelMask.All;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.EthLevels), GroupName = nameof(Resources.Session), 
        Description = nameof(Resources.EthLevelsDescription), Order = 214)]
    public LevelMask EthLevels { get; set; } = LevelMask.All;

    #endregion

    #region Show

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Text), GroupName = nameof(Strings.Show),
        Description = nameof(Strings.IsNeedShowLabelDescription), Order = 200)]
    public bool ShowText
    {
        get => _showText;
        set
        {
            _showText = value;
            TextSize.Enabled = _showText;
            RecalculateValues();
        }
    }

    [Range(5, 30)]
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.TextSize), GroupName = nameof(Strings.Show),
        Description = nameof(Strings.FontSizeDescription), Order = 205)]
    public FilterInt TextSize { get; set; } = new(false);

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowPriceLabels), GroupName = nameof(Strings.Show),
        Description = nameof(Strings.ShowSelectedPriceOnPriceAxisDescription), Order = 210)]
    public bool ShowPrice { get; set; } = true;

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.FirstBar), GroupName = nameof(Strings.Show),
        Description = nameof(Strings.FirstBarDescription), Order = 220)]
    public bool DrawFromBar { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.AbovePrice), GroupName = nameof(Strings.Show),
        Description = nameof(Strings.DrawAbovePriceDescription), Order = 230)]
    public bool DrawOverChart
    {
        get => _drawOverChart;
        set => _drawOverChart = DrawAbovePrice = value;
    }

    #endregion

    #region Open

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Line), GroupName = nameof(Strings.Open),
        Description = nameof(Strings.PenSettingsDescription), Order = 310)]
    public PenSettings OpenPen { get; set; } = new() { Color = DefaultColors.Red.Convert(), Width = 2 };

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Text), GroupName = nameof(Strings.Open), Description = nameof(Strings.LabelTextDescription),
        Order = 315)]
    public string OpenText { get; set; }

    #endregion

    #region Close

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Line), GroupName = nameof(Strings.Close),
        Description = nameof(Strings.PenSettingsDescription), Order = 320)]
    public PenSettings ClosePen { get; set; } = new() { Color = DefaultColors.Red.Convert(), Width = 2 };

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Text), GroupName = nameof(Strings.Close), Description = nameof(Strings.LabelTextDescription),
        Order = 325)]
    public string CloseText { get; set; }

    #endregion

    #region High

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Line), GroupName = nameof(Strings.High),
        Description = nameof(Strings.PenSettingsDescription), Order = 330)]
    public PenSettings HighPen { get; set; } = new() { Color = DefaultColors.Red.Convert(), Width = 2 };

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Text), GroupName = nameof(Strings.High), Description = nameof(Strings.LabelTextDescription),
        Order = 335)]
    public string HighText { get; set; }

    #endregion

    #region Low

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Line), GroupName = nameof(Strings.Low), Description = nameof(Strings.PenSettingsDescription),
        Order = 340)]
    public PenSettings LowPen { get; set; } = new() { Color = DefaultColors.Red.Convert(), Width = 2 };

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Text), GroupName = nameof(Strings.Low), Description = nameof(Strings.LabelTextDescription),
        Order = 345)]
    public string LowText { get; set; }

    #endregion

    #endregion

    #region ctor

    public DailyLines()
		: base(true)
	{
		DenyToChangePanel = true;
		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Historical);
		DrawAbovePrice = true;

		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

        TradingDayStart.PropertyChanged += OnFilterPropertyChanged;
        FilterStartTime.PropertyChanged += OnFilterPropertyChanged;
		FilterEndTime.PropertyChanged += OnFilterPropertyChanged;
		TextSize.PropertyChanged += OnFilterPropertyChanged;

		TextSize.Enabled = ShowText;
		TextSize.Value = _fontSetting.Size;

        for (var t = 0; t < TemplateCount; t++)
        {
            for (var b = 0; b < BucketCount; b++)
                _states[t, b] = new SessionState();
        }

        RebuildTemplates();
    }

    #endregion

    #region Protected methods

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (ChartInfo is null)
            return;

        // Backward-compatible path: behaves like upstream selection.
        if (Scopes == DisplayScopes.None)
        {
            RenderSinglePeriod(context);
            return;
        }

        RenderScopes(context);
    }

    private void RenderSinglePeriod(RenderContext context)
    {
        var bucket = GetBucket(Period);
        var state = _states[(int)SessionTemplateId.Primary, (int)bucket];

        var isCurrent = Period is PeriodType.CurrentDay or PeriodType.CurrenWeek or PeriodType.CurrentMonth;

        if (isCurrent && _lastDefaultSession > state.Current.OpenBar && state.Current.IsFinished)
        {
            DrawMessage(context);
            return;
        }

        var range = isCurrent || (Period is PeriodType.PreviousDay && state.Current.OpenBar <= _lastDefaultSession && CustomSession)
            ? state.Current
            : state.Previous;

        var periodStr = Period switch
        {
            PeriodType.CurrentDay => "Curr. Day",
            PeriodType.PreviousDay => "Prev. Day",
            PeriodType.CurrenWeek => "Curr. Week",
            PeriodType.PreviousWeek => "Prev. Week",
            PeriodType.CurrentMonth => "Curr. Month",
            PeriodType.PreviousMonth => "Prev. Month",
            _ => throw new ArgumentOutOfRangeException()
        };

        DrawRange(context, range, periodStr, LevelMask.All, zoneLabel: "RTH");
    }

    private void RenderScopes(RenderContext context)
    {
        // Message is only relevant when user asked for ETH but custom session is off.
        // Keep message behavior conservative to not block rendering.
        if ((Scopes.HasFlag(DisplayScopes.Eth) && !CustomSession) ||
            (Scopes.HasFlag(DisplayScopes.Eth) && !_secondaryTemplate.Enabled))
        {
            // Optional: you can show a clearer message later.
            // For now keep existing message semantics.
        }

        if (Scopes.HasFlag(DisplayScopes.Day))
            RenderScopeBucket(context, PeriodBucket.Day, DayLevels);

        if (Scopes.HasFlag(DisplayScopes.Week))
            RenderScopeBucket(context, PeriodBucket.Week, WeekLevels);

        if (Scopes.HasFlag(DisplayScopes.Month))
            RenderScopeBucket(context, PeriodBucket.Month, MonthLevels);
    }

    private void RenderScopeBucket(RenderContext context, PeriodBucket bucket, LevelMask bucketMask)
    {
        if (bucket == PeriodBucket.Day)
        {
            if (Scopes.HasFlag(DisplayScopes.Rth))
                RenderTemplateBucket(context, SessionTemplateId.Primary, bucket, bucketMask & RthLevels, zoneLabel: "RTH");

            if (Scopes.HasFlag(DisplayScopes.Eth) && _secondaryTemplate.Enabled)
                RenderTemplateBucket(context, SessionTemplateId.Secondary, bucket, bucketMask & EthLevels, zoneLabel: "ETH");

            // Full-Day (optional in 5.3; see below)
            if (Scopes.HasFlag(DisplayScopes.FullDay))
            {
                var mask = bucketMask; // Full-Day uses DayLevels only (or add FullDayLevels later)
                                       // FullDay requires merged range (RTH+ETH) and will be implemented in a dedicated commit.

            }

            return;
        }

        // Week / Month: ignore RTH/ETH flags
        RenderTemplateBucket(context, SessionTemplateId.Primary, bucket, bucketMask, zoneLabel: "HTF");
    }

    private void RenderTemplateBucket(RenderContext context, SessionTemplateId templateId, PeriodBucket bucket, LevelMask mask, string zoneLabel)
    {
        if (mask == LevelMask.None)
            return;

        var state = _states[(int)templateId, (int)bucket];

        // Current
        RenderRangeInstance(context, state.Current, bucket, mask, zoneLabel, offsetLabel: GetOffsetLabel(bucket, isPrevious: false));

        // Previous
        if (HistoryDepth >= 2)
            RenderRangeInstance(context, state.Previous, bucket, mask, zoneLabel, offsetLabel: GetOffsetLabel(bucket, isPrevious: true));
    }

    private static string GetOffsetLabel(PeriodBucket bucket, bool isPrevious)
    {
        return bucket switch
        {
            PeriodBucket.Day => isPrevious ? "Y" : "T",   // Yesterday / Today (trading day)
            PeriodBucket.Week => isPrevious ? "PW" : "CW", // Previous Week / Current Week
            PeriodBucket.Month => isPrevious ? "PM" : "CM", // Previous Month / Current Month
            _ => isPrevious ? "P" : "C"
        };
    }



    private void RenderRangeInstance(RenderContext context, SessionRange range, PeriodBucket bucket, LevelMask mask, string zoneLabel, string offsetLabel)
    {
        if (range is null || range.OpenBar < 0)
            return;

        var bucketLabel = bucket switch
        {
            PeriodBucket.Day => "D",
            PeriodBucket.Week => "W",
            PeriodBucket.Month => "M",
            _ => "P"
        };

        // Day: "RTH D T" / "ETH D Y"
        // Week/Month: "W CW" / "M PM" (no session zone)
        var suffix = bucket == PeriodBucket.Day
            ? $"{zoneLabel} {bucketLabel} {offsetLabel}"
            : $"{bucketLabel} {offsetLabel}";

        DrawRange(context, range, suffix, mask, zoneLabel);
    }


    private void DrawRange(RenderContext context, SessionRange range, string suffix, LevelMask mask, string zoneLabel)
    {
        var high = ChartInfo.PriceChartContainer.High;
        var low = ChartInfo.PriceChartContainer.Low;

        // ETH special naming (optional)
        var highName = zoneLabel == "ETH" ? "ONH" : "High";
        var lowName = zoneLabel == "ETH" ? "ONL" : "Low";

        if (mask.HasFlag(LevelMask.Open) && range.OpenPrice >= low && range.OpenPrice <= high)
            DrawLevel(context, OpenPen, range.OpenBar, range.OpenPrice, OpenText, "Open", suffix);

        if (mask.HasFlag(LevelMask.High) && range.HighPrice >= low && range.HighPrice <= high)
            DrawLevel(context, HighPen, range.HighBar, range.HighPrice, HighText, highName, suffix);

        if (mask.HasFlag(LevelMask.Low) && range.LowPrice >= low && range.LowPrice <= high)
            DrawLevel(context, LowPen, range.LowBar, range.LowPrice, LowText, lowName, suffix);

        if (mask.HasFlag(LevelMask.Close) && range.IsFinished && range.ClosePrice >= low && range.ClosePrice <= high)
            DrawLevel(context, ClosePen, range.CloseBar, range.ClosePrice, CloseText, "Close", suffix);
    }



    protected override void OnRecalculate()
    {
        for (var t = 0; t < TemplateCount; t++)
        {
            for (var b = 0; b < BucketCount; b++)
                _states[t, b].Reset();
        }

        _lastTradingDayKey = DateTime.MinValue;
    }

    private DateTime GetLocalTime(IndicatorCandle candle)
    {
        // Keep consistency with existing session logic.
        return candle.Time.AddHours(InstrumentInfo.TimeZone);
    }

    private DateTime GetTradingDayKey(IndicatorCandle candle, TimeSpan dayStart)
    {
        var localTime = GetLocalTime(candle);
        var tod = localTime.TimeOfDay;

        // Before anchor => previous trading day.
        return tod < dayStart ? localTime.Date.AddDays(-1) : localTime.Date;
    }

    private bool IsNewTradingDay(IndicatorCandle candle)
    {
        var key = GetTradingDayKey(candle, DayStartTime);

        if (key == _lastTradingDayKey)
            return false;

        _lastTradingDayKey = key;
        return true;
    }


    protected new bool IsNewSession(int bar)
	{
		if (!CustomSession)
			return base.IsNewSession(bar);

		var candle = GetCandle(bar);

		var startTime = candle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
		var endTime = candle.LastTime.AddHours(InstrumentInfo.TimeZone).TimeOfDay;

		if (bar == 0)
		{
			if (startTime <= endTime)
				return FilterStartTime.Value >= startTime && FilterStartTime.Value <= endTime;

			return FilterStartTime.Value >= startTime || FilterStartTime.Value <= endTime;
		}

		var insideBar = (startTime <= endTime && FilterStartTime.Value >= startTime && FilterStartTime.Value <= endTime)
			||
			(startTime > endTime && (FilterStartTime.Value >= startTime || FilterStartTime.Value <= endTime));

		if (insideBar)
			return true;

		var prevCandle = GetCandle(bar - 1);
		startTime = prevCandle.LastTime.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
		endTime = candle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay;

		if (startTime <= endTime)
			return FilterStartTime.Value >= startTime && FilterStartTime.Value <= endTime;

		return FilterStartTime.Value >= startTime || FilterStartTime.Value <= endTime;
	}

	protected new bool IsNewWeek(int bar)
	{
		var isNew = base.IsNewWeek(bar);

		if (!CustomSession)
			return isNew;

		if (isNew)
			_newWeekWait = true;

		if (!InsidePrimarySession(bar) || !_newWeekWait)
			return false;

		_newWeekWait = false;
		return true;
	}

    protected override void OnCalculate(int bar, decimal value)
    {
        var candle = GetCandle(bar);

        if (base.IsNewSession(bar))
            _lastDefaultSession = bar;

        var isNewDay = IsNewTradingDay(candle);
        var isNewWeek = IsNewWeek(bar);
        var isNewMonth = IsNewMonth(bar);

        // Primary
        UpdateBucket(SessionTemplateId.Primary, PeriodBucket.Day, candle, bar, isNewDay, _primaryTemplate);
        UpdateBucket(SessionTemplateId.Primary, PeriodBucket.Week, candle, bar, isNewWeek, _primaryTemplate);
        UpdateBucket(SessionTemplateId.Primary, PeriodBucket.Month, candle, bar, isNewMonth, _primaryTemplate);

        // Secondary
        if (_secondaryTemplate.Enabled)
        {
            UpdateBucket(SessionTemplateId.Secondary, PeriodBucket.Day, candle, bar, isNewDay, _secondaryTemplate);
            // Skip week/month: ETH semantics are daily-only
        }
    }


    #endregion

    #region Private methods

    private bool InsidePrimarySession(int bar)
    {
        var candle = GetCandle(bar);
        return _primaryTemplate.ApplyCustomSessionFilter
            ? InsideSessionCore(candle, _primaryTemplate.Start, _primaryTemplate.End)
            : true;
    }

    private bool InsideSession(IndicatorCandle candle, TimeSpan start, TimeSpan end)
    {
        return InsideSessionCore(candle, start, end);
    }

    private bool InsideSessionCore(IndicatorCandle candle, TimeSpan sessionStart, TimeSpan sessionEnd)
    {
        // Preserve upstream semantics.
        if (sessionStart == sessionEnd)
            return true;

        var startTime = candle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
        var endTime = candle.LastTime.AddHours(InstrumentInfo.TimeZone).TimeOfDay;

        if (sessionStart < sessionEnd)
        {
            return (startTime >= sessionStart && startTime <= sessionEnd) ||
                (endTime >= sessionStart && endTime <= sessionEnd) ||
                (startTime <= sessionStart && endTime >= sessionEnd);
        }

        return startTime >= sessionStart || endTime >= sessionStart ||
            startTime <= sessionEnd || endTime <= sessionEnd;
    }

    private void UpdateBucket(SessionTemplateId templateId, PeriodBucket bucket, IndicatorCandle candle, int bar, bool isNewBucketPeriod, SessionTemplate template)
    {
        var state = _states[(int)templateId, (int)bucket];

        if (isNewBucketPeriod)
        {
            state.RollToNext(candle, bar);
            return;
        }

        if (bar == state.Current.OpenBar)
            return;

        if (bucket == PeriodBucket.Day && template.ApplyCustomSessionFilter)
        {
            if (InsideSession(candle, template.Start, template.End))
                state.Current.IncCandle(candle, bar);
            else if (state.Current.OpenBar >= 0)
                state.Current.IsFinished = true;

            return;
        }

        if (state.Current.OpenBar >= 0)
            state.Current.IncCandle(candle, bar);
    }

    private static PeriodBucket GetBucket(PeriodType period)
    {
        return period switch
        {
            PeriodType.CurrentDay or PeriodType.PreviousDay => PeriodBucket.Day,
            PeriodType.CurrenWeek or PeriodType.PreviousWeek => PeriodBucket.Week,
            PeriodType.CurrentMonth or PeriodType.PreviousMonth => PeriodBucket.Month,
            _ => PeriodBucket.Day
        };
    }

    void RebuildTemplates()
    {
        // Primary mirrors current indicator settings (RTH when CustomSession is enabled).
        _primaryTemplate = new SessionTemplate(
            enabled: true,
            start: FilterStartTime.Value,
            end: FilterEndTime.Value,
            applyCustomSessionFilter: CustomSession);

        // Secondary derives ETH as the complement of RTH (wraps midnight).
        // Enabled only when CustomSession is on, otherwise ETH definition is undefined.
        _secondaryTemplate = new SessionTemplate(
            enabled: CustomSession,
            start: FilterEndTime.Value,
            end: FilterStartTime.Value,
            applyCustomSessionFilter: true);
    }

    private void OnFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != "Value")
            return;

        if (sender.Equals(FilterStartTime) || sender.Equals(FilterEndTime) || sender.Equals(TradingDayStart))
        {
            RebuildTemplates();
            RecalculateValues();
            RedrawChart();
            return;
        }

        if (sender.Equals(TextSize))
        {
            _fontSetting.Size = TextSize.Value;
            RedrawChart();
            return;
        }
    }


    private void DrawString(RenderContext context, RenderFont font, string renderText, int yPrice, Color color)
	{
		var textSize = context.MeasureString(renderText, font);
		context.DrawString(renderText, font, color, Container.Region.Right - textSize.Width - 5, yPrice - textSize.Height);
	}

	private void DrawPrice(RenderContext context, decimal price, RenderPen pen)
	{
		var y = ChartInfo.GetYByPrice(price, false);

		if (y + 8 > Container.Region.Height)
			return;

		var renderText = price.ToString(CultureInfo.InvariantCulture);
		var size = context.MeasureString(renderText, _axisFont);
		var priceHeight = size.Height / 2;
		var x = Container.Region.Right;

		var points = new Point[5];
		points[0] = new Point(x, y);
		points[1] = new Point(x + priceHeight, y - priceHeight);
		points[2] = new Point(x + size.Width + 2 * priceHeight, y - priceHeight);
		points[3] = new Point(points[2].X, y + priceHeight + 1);
		points[4] = new Point(x + priceHeight, y + priceHeight + 1);

		var textRect = new Rectangle(points[1], new Size(size.Width + priceHeight, 2 * priceHeight));
		context.FillPolygon(pen.Color, points);
		context.DrawString(renderText, _axisFont, Color.White, textRect, _format);
	}

	private void DrawLevel(RenderContext context, PenSettings pen, int bar, decimal price, string text, string ohlc, string periodStr)
	{
		if (DrawFromBar && bar > LastVisibleBarNumber)
			return;
		
		var x1 = DrawFromBar ? ChartInfo.GetXByBar(bar) : 0;
		var x2 = Container.Region.Right;
		var y = ChartInfo.GetYByPrice(price, false);
		context.DrawLine(pen.RenderObject, x1, y, x2, y);

		var offset = 3;
		var renderText = string.IsNullOrEmpty(text) ? $"{periodStr} {ohlc}" : text;

		if (ShowText)
			DrawString(context, _fontSetting.RenderObject, renderText, y - offset, pen.RenderObject.Color);

		if (ShowPrice)
		{
			var bounds = context.ClipBounds;
			context.ResetClip();
			context.SetTextRenderingHint(RenderTextRenderingHint.Aliased);
			DrawPrice(context, price, pen.RenderObject);
			context.SetTextRenderingHint(RenderTextRenderingHint.AntiAlias);
			context.SetClip(bounds);
		}
	}

	private void DrawMessage(RenderContext g)
	{
		var text = Strings.CustomSessionInactive;

		var textSize = g.MeasureString(text, ChartInfo.PriceAxisFont);

		var rect = new Rectangle(Container.Region.X, Container.Region.Bottom - textSize.Height, textSize.Width, textSize.Height);
		g.DrawString(text, ChartInfo.PriceAxisFont, DefaultColors.Red, rect);
	}

	#endregion
}