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

    private enum ScopeKind
    {
        CurrentDay,
        PreviousDay,
        CurrentEth,
        CurrentRth,
        PreviousRth,
        CurrentWeek,
        PreviousWeek,
        CurrentMonth,
        PreviousMonth
    }

    private sealed class ScopeState
    {
        public SessionRange Current = new();
        public SessionRange Prev = new();
        public SessionRange LastCompletedDayWithData = new(); // Only used for Day scopes
        public int SessionNumber;
        public int LastDefaultSession;

        // Used to detect incomplete history for current Week/Month
        public bool HasSeenWeekBoundary;
        public bool HasSeenMonthBoundary;
    }

    [Flags]
    public enum LevelMask
    {
        None = 0,
        Open = 1 << 0,
        High = 1 << 1,
        Low = 1 << 2,
        Close = 1 << 3,
        HalfGap = 1 << 4,

        Ohlc = Open | High | Low | Close,
        All = Ohlc | HalfGap
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
	private bool _newSessionWait;
	private PeriodType _per = PeriodType.PreviousDay;
	private bool _showText = true;
	private int _lastDefaultSession;
	private FilterTimeSpan _tradingDayStart;
    private bool _useMultiScope;
    private bool _showCurrentDay;
    private bool _showPreviousDay;
    private bool _showEth;
    private bool _showCurrentWeek;
    private bool _showPreviousWeek;
    private bool _showCurrentMonth;
    private bool _showPreviousMonth;
    private bool _showHalfGap;
    private bool _showCurrentRth;
    private bool _showPreviousRth;

    private readonly Dictionary<ScopeKind, ScopeState> _scopeStates = new();

    private static bool IsCurrentWeekScope(ScopeKind kind) => kind == ScopeKind.CurrentWeek;
    private static bool IsCurrentMonthScope(ScopeKind kind) => kind == ScopeKind.CurrentMonth;

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

	#region Filters

    [Parameter]
    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Period), GroupName = nameof(Resources.Filters),
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

    #region MultiScope

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.MultiScope),
        Description = nameof(Resources.MultiScopeDescription),
        GroupName = nameof(Resources.Filters), Order = 111)]
    public bool UseMultiScope
    {
        get => _useMultiScope;
        set
        {
            if (_useMultiScope == value)
                return;

            _useMultiScope = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowCurrentDay),
    Description = nameof(Resources.ShowCurrentDayDescription),
    GroupName = nameof(Resources.MultiScope), Order = 112)]
    public bool ShowCurrentDay
    {
        get => _showCurrentDay;
        set
        {
            if (_showCurrentDay == value)
                return;

            _showCurrentDay = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowPreviousDay),
        Description = nameof(Resources.ShowPreviousDayDescription),
        GroupName = nameof(Resources.MultiScope), Order = 113)]
    public bool ShowPreviousDay
    {
        get => _showPreviousDay;
        set
        {
            if (_showPreviousDay == value)
                return;

            _showPreviousDay = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowEth),
        Description = nameof(Resources.ShowEthDescription),
        GroupName = nameof(Resources.MultiScope), Order = 114)]
    public bool ShowEth
    {
        get => _showEth;
        set
        {
            if (_showEth == value)
                return;

            _showEth = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowCurrentRth),
    Description = nameof(Resources.ShowCurrentRthDescription),
    GroupName = nameof(Resources.MultiScope), Order = 1141)]
    public bool ShowCurrentRth
    {
        get => _showCurrentRth;
        set
        {
            if (_showCurrentRth == value)
                return;

            _showCurrentRth = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowPreviousRth),
        Description = nameof(Resources.ShowPreviousRthDescription),
        GroupName = nameof(Resources.MultiScope), Order = 1142)]
    public bool ShowPreviousRth
    {
        get => _showPreviousRth;
        set
        {
            if (_showPreviousRth == value)
                return;

            _showPreviousRth = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowCurrentWeek),
        Description = nameof(Resources.ShowCurrentWeekDescription),
        GroupName = nameof(Resources.MultiScope), Order = 115)]
    public bool ShowCurrentWeek
    {
        get => _showCurrentWeek;
        set
        {
            if (_showCurrentWeek == value)
                return;

            _showCurrentWeek = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowPreviousWeek),
        Description = nameof(Resources.ShowPreviousWeekDescription),
        GroupName = nameof(Resources.MultiScope), Order = 116)]
    public bool ShowPreviousWeek
    {
        get => _showPreviousWeek;
        set
        {
            if (_showPreviousWeek == value)
                return;

            _showPreviousWeek = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowCurrentMonth),
        Description = nameof(Resources.ShowCurrentMonthDescription),
        GroupName = nameof(Resources.MultiScope), Order = 117)]
    public bool ShowCurrentMonth
    {
        get => _showCurrentMonth;
        set
        {
            if (_showCurrentMonth == value)
                return;

            _showCurrentMonth = value;
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowPreviousMonth),
        Description = nameof(Resources.ShowPreviousMonthDescription),
        GroupName = nameof(Resources.MultiScope), Order = 118)]
    public bool ShowPreviousMonth
    {
        get => _showPreviousMonth;
        set
        {
            if (_showPreviousMonth == value)
                return;

            _showPreviousMonth = value;
            ApplySettingsChange();
        }
    }

    #endregion

    #region DayFamily Levels

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.DayFamilyLevels),
        Description = nameof(Resources.DayFamilyLevelsDescription),
        GroupName = nameof(Resources.Filters), Order = 140)]
    public bool DayFamilyLevelsHeader => true; // dummy header (ATAS groups by GroupName; keep minimal)

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.DayLevels),
        Description = nameof(Resources.DayLevelsDescription),
        GroupName = nameof(Resources.DayFamilyLevels), Order = 141)]
    public LevelMask DayLevels { get; set; } = LevelMask.Ohlc;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.PrevDayLevels),
        Description = nameof(Resources.PrevDayLevelsDescription),
        GroupName = nameof(Resources.DayFamilyLevels), Order = 142)]
    public LevelMask PrevDayLevels { get; set; } = LevelMask.Ohlc;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.EthLevels),
        Description = nameof(Resources.EthLevelsDescription),
        GroupName = nameof(Resources.DayFamilyLevels), Order = 143)]
    public LevelMask EthLevels { get; set; } = LevelMask.Ohlc;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.RthLevels),
        Description = nameof(Resources.RthLevelsDescription),
        GroupName = nameof(Resources.DayFamilyLevels), Order = 144)]
    public LevelMask RthLevels { get; set; } = LevelMask.Ohlc;

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.PrevRthLevels),
        Description = nameof(Resources.PrevRthLevelsDescription),
        GroupName = nameof(Resources.DayFamilyLevels), Order = 145)]
    public LevelMask PrevRthLevels { get; set; } = LevelMask.Ohlc;

    #endregion

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.CustomSession), GroupName = nameof(Resources.Filters),
        Description = nameof(Resources.IsCustomSessionDescription), Order = 120)]
    public bool CustomSession
    {
        get => _customSession;
        set
        {
            _customSession = value;
            FilterStartTime.Enabled = FilterEndTime.Enabled = _customSession;

            if (_customSession)
            {
                // Backward-compatible default:
                // If the user turns on CustomSession and TradingDayStart is still at zero,
                // align TradingDayStart to the session start to preserve the legacy rollover behavior.
                if (_tradingDayStart != null && _tradingDayStart.Value == TimeSpan.Zero)
                    _tradingDayStart.Value = FilterStartTime.Value;
            }

            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionBegin), GroupName = nameof(Resources.Filters),
        Description = nameof(Resources.SessionBeginDescription), Order = 120)]
    public FilterTimeSpan FilterStartTime { get; set; } = new(false);

    [Browsable(false)]
    public TimeSpan StartTime
    {
        get => FilterStartTime.Value;
        set => FilterStartTime.Value = value;
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionEnd), GroupName = nameof(Resources.Filters),
        Description = nameof(Resources.SessionEndDescription), Order = 120)]
    public FilterTimeSpan FilterEndTime { get; set; } = new(false);

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.TradingDayStart), Description = nameof(Resources.TradingDayStartDescription),
    GroupName = nameof(Resources.Filters), Order = 125)]
    public FilterTimeSpan TradingDayStart
    {
        get => _tradingDayStart;
        set
        {
            _tradingDayStart = value ?? new FilterTimeSpan(false) { Value = TimeSpan.Zero };
            _tradingDayStart.PropertyChanged -= OnFilterPropertyChanged;
            _tradingDayStart.PropertyChanged += OnFilterPropertyChanged;
            ApplySettingsChange();
        }
    }

    [Browsable(false)]
    public TimeSpan EndTime
    {
        get => FilterEndTime.Value;
        set => FilterEndTime.Value = value;
    }

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

    #region HalfGap

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowHalfGap),
        Description = nameof(Resources.ShowHalfGapDescription),
        GroupName = nameof(Resources.HalfGap), Order = 360)]
    public bool ShowHalfGap
    {
        get => _showHalfGap;
        set
        {
            if (_showHalfGap == value)
                return;

            _showHalfGap = value;

            // HalfGap is render-only for existing ranges, but keep recalc for safety
            // because it affects whether half-gap is drawn at all.
            ApplySettingsChange();
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.Line),
        Description = nameof(Resources.PenSettingsDescription),
        GroupName = nameof(Resources.HalfGap), Order = 361)]
    public PenSettings HalfGapPen { get; set; } = new()
    {
        Color = DefaultColors.Blue.Convert(),
        Width = 2
    };

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.HalfGapText),
        Description = nameof(Resources.HalfGapTextDescription),
        GroupName = nameof(Resources.HalfGap), Order = 362)]
    public string HalfGapText { get; set; } = Resources.HalfGap;

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

		FilterStartTime.PropertyChanged += OnFilterPropertyChanged;
		FilterEndTime.PropertyChanged += OnFilterPropertyChanged;
		TextSize.PropertyChanged += OnFilterPropertyChanged;

		TextSize.Enabled = ShowText;
		TextSize.Value = _fontSetting.Size;

        _tradingDayStart = new FilterTimeSpan(false) { Value = TimeSpan.Zero };
        _tradingDayStart.PropertyChanged += OnFilterPropertyChanged;
    }

    #endregion

    #region Protected methods

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (ChartInfo is null)
            return;

        // Build active set once.
        var active = new HashSet<ScopeKind>(GetActiveScopes());

        var isOutsideSession = false;

        // Custom session inactive message:
        // show it when the viewport is outside the custom session AND at least one session-dependent scope is enabled.
        // Do NOT return: non-session scopes (Week/Month) must still render.
        if (CustomSession)
        {
            var hasSessionDependentScope =
                active.Contains(ScopeKind.CurrentDay) ||
                active.Contains(ScopeKind.PreviousDay) ||
                active.Contains(ScopeKind.CurrentEth);

            if (hasSessionDependentScope)
            {
                // Pick a deterministic gate scope among session-dependent ones.
                var gateKind =
                    active.Contains(ScopeKind.CurrentDay) ? ScopeKind.CurrentDay :
                    active.Contains(ScopeKind.CurrentEth) ? ScopeKind.CurrentEth :
                    ScopeKind.PreviousDay;

                var gateState = GetScopeState(gateKind);

                if (_lastDefaultSession > gateState.Current.OpenBar && !InsideSession(LastVisibleBarNumber))
                {
                    isOutsideSession = true;
                    DrawMessage(context, Strings.CustomSessionInactive);
                }
            }
        }

        // Build warning overlay (non-blocking): missing data / incomplete history.
        // In multi-scope, show a single overlay containing both categories if needed.
        var noDataScopes = new List<string>();
        var incompleteScopes = new List<string>();

        foreach (var kind in _renderOrder)
        {
            if (!active.Contains(kind))
                continue;

            // If outside custom session, skip only session-dependent scopes.
            if (CustomSession && isOutsideSession && IsSessionDependentScope(kind))
                continue;

            var state = GetScopeState(kind);
            var range = IsCurrentScope(kind) ? state.Current : state.Prev;

            // PreviousDay special-case (skip-empty) – keep your existing logic if you have it.
            // If you already swap "range" for LastCompletedDayWithData, do it here too.

            if (range.OpenBar < 0)
            {
                noDataScopes.Add(GetScopeLabel(kind));
                continue;
            }

            if (IsIncompleteHistory(kind, state, range))
                incompleteScopes.Add(GetScopeLabel(kind));
        }

        if (noDataScopes.Count > 0 || incompleteScopes.Count > 0)
        {
            var lines = new List<string>();

            if (noDataScopes.Count > 0)
                lines.Add(string.Format(Resources.NoDataForPeriod, string.Join(", ", noDataScopes)));

            if (incompleteScopes.Count > 0)
                lines.Add(string.Format(Resources.IncompleteDataForPeriod, string.Join(", ", incompleteScopes)));

            DrawMessage(context, string.Join(Environment.NewLine, lines));
        }

        var high = ChartInfo.PriceChartContainer.High;
        var low = ChartInfo.PriceChartContainer.Low;

        foreach (var scopeKind in _renderOrder)
        {
            if (!active.Contains(scopeKind))
                continue;

            // If outside custom session, skip only session-dependent scopes.
            if (CustomSession && isOutsideSession && IsSessionDependentScope(scopeKind))
                continue;

            var state = GetScopeState(scopeKind);

            var isCurrent = IsCurrentScope(scopeKind);

            var range = isCurrent
                ? state.Current
                : state.Prev;

            // PreviousDay special: skip empty buckets when TradingDayStart-based day bucketing is used.
            if (scopeKind == ScopeKind.PreviousDay && UseTradingDayStartForDay() && state.LastCompletedDayWithData.OpenBar >= 0)
            {
                range = state.LastCompletedDayWithData;
            }

            if (range.OpenBar < 0)
                continue;

            var periodStr = GetScopeLabel(scopeKind);

            var mask = GetLevelMaskForScope(scopeKind);

            // Half Gap (midpoint between previous session close and current session open).
            // Option A: only for CurrentDay when CustomSession is enabled.
            if (ShowHalfGap
                && mask.HasFlag(LevelMask.HalfGap)
                && CustomSession
                && scopeKind == ScopeKind.CurrentDay
                && state.Prev is not null
                && state.Prev.IsFinished
                && state.Prev.OpenBar >= 0
                && range.OpenBar >= 0)
            {
                var prevClose = state.Prev.ClosePrice;
                var currOpen = range.OpenPrice;
                var halfGap = prevClose + (currOpen - prevClose) / 2m;

                if (halfGap >= low && halfGap <= high)
                    DrawLevel(context, HalfGapPen, range.OpenBar, halfGap, HalfGapText, "HalfGap", periodStr);
            }

            if (mask.HasFlag(LevelMask.Open) && range.OpenPrice >= low && range.OpenPrice <= high)
                DrawLevel(context, OpenPen, range.OpenBar, range.OpenPrice, OpenText, "Open", periodStr);

            if (mask.HasFlag(LevelMask.High) && range.HighPrice >= low && range.HighPrice <= high)
                DrawLevel(context, HighPen, range.HighBar, range.HighPrice, HighText, "High", periodStr);

            if (mask.HasFlag(LevelMask.Low) && range.LowPrice >= low && range.LowPrice <= high)
                DrawLevel(context, LowPen, range.LowBar, range.LowPrice, LowText, "Low", periodStr);

            if (mask.HasFlag(LevelMask.Close) && range.IsFinished && range.ClosePrice >= low && range.ClosePrice <= high)
                DrawLevel(context, ClosePen, range.CloseBar, range.ClosePrice, CloseText, "Close", periodStr);
        }
    }

    protected override void OnRecalculate()
	{
        _scopeStates.Clear();
        _newSessionWait = false;
		_newWeekWait = false;
        _lastDefaultSession = 0;
    }

    /// <summary>
    /// Determines if a new custom session starts at the specified bar.
    /// Uses a two-phase detection to correctly handle gaps (e.g., weekends):
    /// 1. First, a new default session must be detected (sets _newSessionWait = true)
    /// 2. Then, the custom session start time must fall within the current bar
    /// This prevents false triggers when custom session time falls within weekend gaps.
    /// </summary>
    private bool IsNewSessionForScope(int bar, bool isNewDefaultSession)
    {
        if (!CustomSession)
            return isNewDefaultSession;

        // Phase 1: Track when a new default (exchange) session starts
        if (isNewDefaultSession)
            _newSessionWait = true;

        var candle = GetCandle(bar);

        var startTime = candle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
        var endTime = candle.LastTime.AddHours(InstrumentInfo.TimeZone).TimeOfDay;

        // Phase 2: Check if custom session start time falls within this bar
        bool isNewCustomSession;

        if (bar == 0)
        {
            // First bar: check if custom start time is within bar's time range
            if (startTime <= endTime)
                isNewCustomSession = FilterStartTime.Value >= startTime && FilterStartTime.Value <= endTime;
            else
                isNewCustomSession = FilterStartTime.Value >= startTime || FilterStartTime.Value <= endTime;
        }
        else
        {
            // Check if custom start time falls inside current bar
            var insideBar =
                (startTime <= endTime && FilterStartTime.Value >= startTime && FilterStartTime.Value <= endTime)
                ||
                (startTime > endTime && (FilterStartTime.Value >= startTime || FilterStartTime.Value <= endTime));

            if (insideBar)
            {
                isNewCustomSession = true;
            }
            else
            {
                // Check if custom start time falls in the gap between previous bar and current bar
                var prevCandle = GetCandle(bar - 1);
                startTime = prevCandle.LastTime.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
                endTime = candle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay;

                if (startTime <= endTime)
                    isNewCustomSession = FilterStartTime.Value >= startTime && FilterStartTime.Value <= endTime;
                else
                    isNewCustomSession = FilterStartTime.Value >= startTime || FilterStartTime.Value <= endTime;
            }
        }

        // Only trigger new session when BOTH conditions are met:
        // - Default session has changed (we're in a new trading day)
        // - Custom session start time is reached
        if (!isNewCustomSession || !_newSessionWait)
            return false;

        _newSessionWait = false;
        return true;
    }

    private bool IsNewWeekForScope(int bar, bool isNewBaseWeek)
    {
        if (!CustomSession)
            return isNewBaseWeek;

        if (isNewBaseWeek)
            _newWeekWait = true;

        if (!InsideSession(bar) || !_newWeekWait)
            return false;

        _newWeekWait = false;
        return true;
    }

    protected override void OnCalculate(int bar, decimal value)
    {
        var candle = GetCandle(bar);

        // Cache base period boundaries ONCE to avoid side-effects when multi-scope is enabled.
        var isNewDefaultSession = base.IsNewSession(bar);
        if (isNewDefaultSession)
            _lastDefaultSession = bar;

        var isNewBaseWeek = base.IsNewWeek(bar);
        var isNewBaseMonth = IsNewMonth(bar);

        foreach (var scopeKind in GetActiveScopes())
        {
            var state = GetScopeState(scopeKind);

            // Track boundary visibility to detect incomplete history later.
            if (isNewBaseWeek)
                state.HasSeenWeekBoundary = true;

            if (isNewBaseMonth)
                state.HasSeenMonthBoundary = true;

            if (bar == state.Current.OpenBar)
            {
                // Same bar as current open: just keep expanding the current range (if applicable).
                ProcessSameOpenBar(scopeKind, state, bar, candle);
                continue;
            }

            var isNewPeriod = IsNewPeriod(bar, scopeKind, isNewDefaultSession, isNewBaseWeek, isNewBaseMonth);

            if (isNewPeriod)
            {
                FinalizeAndShiftOnNewPeriod(scopeKind, state);
                StartNewPeriod(scopeKind, state, bar, candle);
            }
            else
            {
                ExpandCurrentPeriod(scopeKind, state, bar, candle);
            }
        }
    }

    private void ProcessSameOpenBar(ScopeKind scopeKind, ScopeState state, int bar, IndicatorCandle candle)
    {
        if (IsDayScope(scopeKind))
        {
            if (InsideWindow(scopeKind, bar))
            {
                state.Current.IncCandle(candle, bar);
            }
            else
            {
                if (state.Current.OpenBar >= 0)
                    state.Current.IsFinished = true;
            }
        }
        else
        {
            if (state.Current.OpenBar >= 0)
                state.Current.IncCandle(candle, bar);
        }
    }

    private void FinalizeAndShiftOnNewPeriod(ScopeKind scopeKind, ScopeState state)
    {
        if (state.Current.OpenBar >= 0)
        {
            state.Current.IsFinished = true;

            // Preserve upstream behavior for all scopes.
            state.Prev = state.Current;

            // For Day scopes, track the last completed range that actually has session-window data.
            if (IsDayScope(scopeKind))
            {
                // OpenBar >= 0 implies we have at least one in-window candle
                // (because we only IncCandle when InsideSession is true).
                state.LastCompletedDayWithData = state.Current;
            }
        }
    }

    private void StartNewPeriod(ScopeKind scopeKind, ScopeState state, int bar, IndicatorCandle candle)
    {
        if (IsDayScope(scopeKind))
        {
            // For Day scopes compute OHLC only from the selected session window.
            // When using TradingDayStart-based bucketing, the first bar of the bucket might be outside the session window.
            state.Current = UseTradingDayStartForDay()
                ? new SessionRange()
                : new SessionRange(candle, bar);

            if (InsideWindow(scopeKind, bar))
                state.Current.IncCandle(candle, bar);
        }
        else
        {
            state.Current = new SessionRange(candle, bar);
        }
    }

    private void ExpandCurrentPeriod(ScopeKind scopeKind, ScopeState state, int bar, IndicatorCandle candle)
    {
        if (IsDayScope(scopeKind))
        {
            if (InsideWindow(scopeKind, bar))
            {
                state.Current.IncCandle(candle, bar);
            }
            else
            {
                if (state.Current.OpenBar >= 0)
                    state.Current.IsFinished = true;
            }
        }
        else
        {
            if (state.Current.OpenBar >= 0)
                state.Current.IncCandle(candle, bar);
        }
    }

    #endregion

    #region Private methods

    private bool InsideSession(int bar)
	{
		if (!CustomSession)
			return true;

		if (FilterStartTime.Value == FilterEndTime.Value)
			return true;

		var candle = GetCandle(bar);

		var sessionStart = FilterStartTime.Value;
		var sessionEnd = FilterEndTime.Value;

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

    private static bool IsEthScope(ScopeKind kind)
    {
        return kind == ScopeKind.CurrentEth;
    }

    private bool InsideEth(int bar)
    {
        // ETH depends on the RTH start (CustomSession start time).
        if (!CustomSession)
            return false;

        // ETH is defined as: TradingDayStart -> RTH start (FilterStartTime).
        var ethStart = TradingDayStart?.Value ?? TimeSpan.Zero;
        var ethEnd = FilterStartTime.Value;

        if (ethStart == ethEnd)
            return true;

        var candle = GetCandle(bar);
        var startTime = candle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
        var endTime = candle.LastTime.AddHours(InstrumentInfo.TimeZone).TimeOfDay;

        if (ethStart < ethEnd)
        {
            return (startTime >= ethStart && startTime <= ethEnd) ||
                (endTime >= ethStart && endTime <= ethEnd) ||
                (startTime <= ethStart && endTime >= ethEnd);
        }

        // Crossing midnight (typical: 18:00 -> 09:30)
        return startTime >= ethStart || endTime >= ethStart ||
            startTime <= ethEnd || endTime <= ethEnd;
    }

    private bool InsideWindow(ScopeKind scopeKind, int bar)
    {
        if (IsEthScope(scopeKind))
            return InsideEth(bar);

        // Multi-scope: "Day" scopes represent FULL DAY (no session filter).
        // RTH scopes use the custom session window.
        if (UseMultiScope)
        {
            if (scopeKind == ScopeKind.CurrentDay || scopeKind == ScopeKind.PreviousDay)
                return true;

            if (scopeKind == ScopeKind.CurrentRth || scopeKind == ScopeKind.PreviousRth)
                return InsideSession(bar);
        }

        // Legacy behavior: day scopes can be session-filtered via CustomSession.
        return InsideSession(bar);
    }

    private bool UseTradingDayStartForDay()
    {
        // Only use TradingDayStart-based day bucketing when explicitly needed.
        // This preserves upstream behavior for default sessions unless the user enables CustomSession
        // or sets a non-zero TradingDayStart.
        return CustomSession || (TradingDayStart?.Value ?? TimeSpan.Zero) != TimeSpan.Zero;
    }

    private bool IsNewPeriod(
    int bar,
    ScopeKind scope,
    bool isNewDefaultSession,
    bool isNewBaseWeek,
    bool isNewBaseMonth)
    {
        if (IsDayScope(scope))
        {
            // ETH: day-like scope with an independent window, always anchored by TradingDayStart.
            if (scope == ScopeKind.CurrentEth)
                return IsNewTradingDay(bar);

            // Multi-scope RTH: also anchored by TradingDayStart (bucket), but filtered by session window.
            if (UseMultiScope && (scope == ScopeKind.CurrentRth || scope == ScopeKind.PreviousRth))
                return IsNewTradingDay(bar);

            return UseTradingDayStartForDay()
                ? IsNewTradingDay(bar)
                : IsNewSessionForScope(bar, isNewDefaultSession);
        }

        if (IsWeekScope(scope))
            return IsNewWeekForScope(bar, isNewBaseWeek);

        if (IsMonthScope(scope))
            return isNewBaseMonth;

        return false;
    }

    private ScopeKind GetLegacyScopeKind()
    {
        return Period switch
        {
            PeriodType.CurrentDay => ScopeKind.CurrentDay,
            PeriodType.PreviousDay => ScopeKind.PreviousDay,
            PeriodType.CurrenWeek => ScopeKind.CurrentWeek,
            PeriodType.PreviousWeek => ScopeKind.PreviousWeek,
            PeriodType.CurrentMonth => ScopeKind.CurrentMonth,
            PeriodType.PreviousMonth => ScopeKind.PreviousMonth,
            _ => ScopeKind.CurrentDay
        };
    }

    private static bool IsDayScope(ScopeKind kind)
    {
        return kind is ScopeKind.CurrentDay or ScopeKind.PreviousDay or ScopeKind.CurrentEth
            or ScopeKind.CurrentRth or ScopeKind.PreviousRth;
    }

    private static bool IsWeekScope(ScopeKind kind)
    {
        return kind is ScopeKind.CurrentWeek or ScopeKind.PreviousWeek;
    }

    private static bool IsMonthScope(ScopeKind kind)
    {
        return kind is ScopeKind.CurrentMonth or ScopeKind.PreviousMonth;
    }

    private IEnumerable<ScopeKind> GetActiveScopes()
    {
        if (!UseMultiScope)
        {
            yield return GetLegacyScopeKind();
            yield break;
        }

        if (ShowCurrentDay) yield return ScopeKind.CurrentDay;
        if (ShowPreviousDay) yield return ScopeKind.PreviousDay;
        if (ShowEth) yield return ScopeKind.CurrentEth;
        if (ShowCurrentRth) yield return ScopeKind.CurrentRth;
        if (ShowPreviousRth) yield return ScopeKind.PreviousRth;

        if (ShowCurrentWeek) yield return ScopeKind.CurrentWeek;
        if (ShowPreviousWeek) yield return ScopeKind.PreviousWeek;

        if (ShowCurrentMonth) yield return ScopeKind.CurrentMonth;
        if (ShowPreviousMonth) yield return ScopeKind.PreviousMonth;

        // Safety fallback: if user enables multi-scope but forgets to tick any scope,
        // keep legacy behavior to avoid "empty" indicator.
        if (!ShowCurrentDay && !ShowPreviousDay && !ShowEth &&
            !ShowCurrentRth && !ShowPreviousRth &&
            !ShowCurrentWeek && !ShowPreviousWeek &&
            !ShowCurrentMonth && !ShowPreviousMonth)
        {
            yield return GetLegacyScopeKind();
        }
    }

    private static readonly ScopeKind[] _renderOrder =
    {
    ScopeKind.CurrentDay,
    ScopeKind.PreviousDay,
    ScopeKind.CurrentEth,
    ScopeKind.CurrentRth,
    ScopeKind.PreviousRth,
    ScopeKind.CurrentWeek,
    ScopeKind.PreviousWeek,
    ScopeKind.CurrentMonth,
    ScopeKind.PreviousMonth
};

    private static bool IsCurrentScope(ScopeKind kind)
    {
        return kind is ScopeKind.CurrentDay or ScopeKind.CurrentEth or ScopeKind.CurrentRth or ScopeKind.CurrentWeek or ScopeKind.CurrentMonth;
    }

    private static string GetScopeLabel(ScopeKind kind)
    {
        return kind switch
        {
            ScopeKind.CurrentDay => Strings.CurrentDay,
            ScopeKind.PreviousDay => Strings.PreviousDay,
            ScopeKind.CurrentEth => Resources.Eth,
            ScopeKind.CurrentRth => Resources.Rth,
            ScopeKind.PreviousRth => Resources.Rth,
            ScopeKind.CurrentWeek => Strings.CurrentWeek,
            ScopeKind.PreviousWeek => Strings.PreviousWeek,
            ScopeKind.CurrentMonth => Strings.CurrentMonth,
            ScopeKind.PreviousMonth => Strings.PreviousMonth,
            _ => string.Empty
        };
    }

    private ScopeState GetScopeState(ScopeKind kind)
    {
        if (_scopeStates.TryGetValue(kind, out var state))
            return state;

        state = new ScopeState();
        _scopeStates[kind] = state;
        return state;
    }

    private DateTime GetLocalBarTime(int bar)
    {
        var candle = GetCandle(bar);
        return candle.Time.AddHours(InstrumentInfo.TimeZone);
    }

    private DateTime GetTradingDayAnchorDate(DateTime localTime)
    {
        // Trading day is anchored at TradingDayStartTime.
        // If local time is before the anchor, it belongs to the previous anchor date.
        var start = TradingDayStart?.Value ?? TimeSpan.Zero;

        // NOTE: TimeSpan.Zero is a valid value (calendar-day semantics).
        if (localTime.TimeOfDay < start)
            return localTime.Date.AddDays(-1);

        return localTime.Date;
    }

    private bool IsNewTradingDay(int bar)
    {
        if (bar <= 0)
            return false;

        var curr = GetTradingDayAnchorDate(GetLocalBarTime(bar));
        var prev = GetTradingDayAnchorDate(GetLocalBarTime(bar - 1));
        return curr != prev;
    }

    private static bool IsIncompleteHistory(ScopeKind kind, ScopeState state, SessionRange range)
    {
        if (range.OpenBar < 0)
            return false; // handled as "no data"

        if (IsCurrentWeekScope(kind))
            return !state.HasSeenWeekBoundary;

        if (IsCurrentMonthScope(kind))
            return !state.HasSeenMonthBoundary;

        return false;
    }

    private static bool IsSessionDependentScope(ScopeKind kind)
    {
        return kind == ScopeKind.CurrentDay
            || kind == ScopeKind.PreviousDay
            || kind == ScopeKind.CurrentEth
            || kind == ScopeKind.CurrentRth
            || kind == ScopeKind.PreviousRth;
    }

    private LevelMask GetLevelMaskForScope(ScopeKind scopeKind)
    {
        return scopeKind switch
        {
            ScopeKind.CurrentDay => DayLevels,
            ScopeKind.PreviousDay => PrevDayLevels,
            ScopeKind.CurrentEth => EthLevels,
            ScopeKind.CurrentRth => RthLevels,
            ScopeKind.PreviousRth => PrevRthLevels,
            _ => LevelMask.All // non-day-family: keep unchanged (no masking)
        };
    }

    private void OnFilterPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != "Value")
			return;

		if (sender.Equals(FilterStartTime))
		{
			RecalculateValues();
			RedrawChart();
		}
		else if (sender.Equals(FilterEndTime))
		{
			RecalculateValues();
			RedrawChart();
		}
		else if (sender.Equals(TextSize))
			_fontSetting.Size = TextSize.Value;

        else if (sender.Equals(_tradingDayStart))
        {
            RecalculateValues();
            RedrawChart();
        }
    }

    private void ApplySettingsChange(bool recalc = true)
    {
        if (recalc)
            RecalculateValues();

        RedrawChart();
    }

    private void DrawString(RenderContext context, RenderFont font, string renderText, int yPrice, Color color)
	{
		var textSize = context.MeasureString(renderText, font);
		context.DrawString(renderText, font, color, Container.Region.Right - textSize.Width - 5, yPrice - textSize.Height);
	}

	private void DrawPrice(RenderContext context, decimal price, RenderPen pen)
	{
		var y = ChartInfo.GetYByPrice(price, false);

        if (y < 0)
            return;

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

    private void DrawMessage(RenderContext g, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        var maxWidth = 0f;
        var totalHeight = 0f;

        foreach (var line in lines)
        {
            var size = g.MeasureString(line, ChartInfo.PriceAxisFont);
            if (size.Width > maxWidth)
                maxWidth = size.Width;

            totalHeight += size.Height;
        }

        var x = Container.Region.X;
        var y = Container.Region.Bottom - totalHeight;

        foreach (var line in lines)
        {
            var size = g.MeasureString(line, ChartInfo.PriceAxisFont);
            var rect = new Rectangle(x, (int)y, (int)maxWidth, (int)size.Height);
            g.DrawString(line, ChartInfo.PriceAxisFont, DefaultColors.Red, rect);
            y += size.Height;
        }
    }

    #endregion
}