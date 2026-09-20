namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ATAS.DataFeedsCore;
using ATAS.Indicators.Technical.PropRisk;

using OFT.Rendering.Context;
using OFT.Rendering.Tools;

using Utils.Common.Logging;

/// <summary>
/// Account panel for funded (prop firm) accounts: balance and PnL of the selected account, with
/// the trailing drawdown, the daily loss limit and profit target, the trades of the chart
/// instrument and a suggested trading status, plus optional stop and target price rails.
/// </summary>
/// <remarks>
/// All the state is updated from trading events and a one-second timer (never from rendering),
/// by the engines in Core/, and kept per account on disk. OnRender only draws the last snapshot.
/// </remarks>
[Category(IndicatorCategories.Trading)]
[DisplayName("Prop Risk Monitor")]
[Description("Account panel for funded accounts: trailing drawdown, daily loss limit and profit target, session statistics and a suggested trading status.")]
public class PropRiskMonitor : Indicator
{
	#region Nested types

	public enum HorizontalAlignment
	{
		Left,
		Center,
		Right
	}

	public enum VerticalAlignment
	{
		Top,
		Middle,
		Bottom
	}

	// One row of the panel. RawForColoring colours the value by sign; ValueColor overrides it.
	private sealed record DisplayLine(string Label, string Value, decimal? RawForColoring, Color? ValueColor = null);

	// Everything the panel shows, computed outside rendering.
	private sealed record View(
		Portfolio Portfolio,
		TrailingResult Trailing,
		DailyResult Daily,
		LedgerStats Trades,
		TradeRecord CurrentTrade,
		TradeRecord LastTrade,
		IReadOnlyList<RiskFinding> Findings,
		RiskStatus Status,
		decimal Equity,
		decimal PositionVolume,
		decimal PositionPrice);

	#endregion

	#region Fields

	private const int Padding = 10;
	private static readonly TimeSpan TimerPeriod = TimeSpan.FromSeconds(1);
	private static readonly TimeSpan SaveDelay = TimeSpan.FromSeconds(2);

	private Color _backgroundColor = Color.FromArgb(200, 20, 25, 35);
	private Color _textColor = Color.FromArgb(220, 220, 220);
	private Color _positiveColor = Color.FromArgb(0, 230, 118);
	private Color _negativeColor = Color.FromArgb(255, 82, 82);
	private Color _neutralColor = Color.FromArgb(150, 150, 150);
	private Color _cautionColor = Color.FromArgb(255, 193, 7);
	private Color _stopRailColor = Color.FromArgb(255, 82, 82);
	private Color _targetRailColor = Color.FromArgb(0, 230, 118);
	private readonly RenderPen _borderPen = new(Color.Gray, 1);
	private RenderFont _font = new("Arial", 11);

	// Engine state, guarded by _sync. _state belongs to _accountKey.
	private readonly object _sync = new();
	private readonly StateStore _store = new(StateStore.DefaultDirectory);
	private readonly TrailingSettings _trailing = new();
	private readonly DailySettings _daily = new();
	private readonly RecommendationSettings _recommendations = new()
	{
		Enabled = true,
		MaxTradesPerDay = 3,
		CautionTradesPercent = 75m,
		MaxConsecutiveLosses = 2,
		CautionRemainingLossPercent = 20m,
		GivebackPercentOfTarget = 30m,
		ConsistencyCautionPercent = 30m,
		ConsistencyStopPercent = 40m
	};

	private SessionClock _clock = new(DayResetZone.NewYork, new TimeSpan(17, 0, 0));
	private DayResetZone _dayResetZone = DayResetZone.NewYork;
	private TimeSpan _dayResetTime = new(17, 0, 0);
	private string _accountKey;
	private AccountState _state;
	private Position _trackedPosition;
	private int _saveScheduled;
	private string _lastSavedFingerprint;
	private readonly HashSet<string> _ownInstruments = new(StringComparer.Ordinal);
	private EquitySource _equitySource = EquitySource.BalancePlusOpenPnl;
	private bool _timerSubscribed;

	// Last snapshot for rendering; replaced as a whole.
	private volatile View _view;

	#endregion

	#region Properties

	#region Visualization

	[Display(Name = "Background", GroupName = "Visualization", Order = 10)]
	public CrossColor BackgroundColor
	{
		get => _backgroundColor.Convert();
		set => _backgroundColor = value.Convert();
	}

	[Display(Name = "Text color", GroupName = "Visualization", Order = 20)]
	public CrossColor TextColor
	{
		get => _textColor.Convert();
		set => _textColor = value.Convert();
	}

	[Display(Name = "Positive color", GroupName = "Visualization", Order = 30)]
	public CrossColor PositiveColor
	{
		get => _positiveColor.Convert();
		set => _positiveColor = value.Convert();
	}

	[Display(Name = "Negative color", GroupName = "Visualization", Order = 40)]
	public CrossColor NegativeColor
	{
		get => _negativeColor.Convert();
		set => _negativeColor = value.Convert();
	}

	[Display(Name = "Neutral color", GroupName = "Visualization", Order = 50)]
	public CrossColor NeutralColor
	{
		get => _neutralColor.Convert();
		set => _neutralColor = value.Convert();
	}

	[Display(Name = "Caution color", GroupName = "Visualization", Order = 55)]
	public CrossColor CautionColor
	{
		get => _cautionColor.Convert();
		set => _cautionColor = value.Convert();
	}

	[Display(Name = "Font size", GroupName = "Visualization", Order = 60)]
	[Range(6, 30)]
	public float FontSize
	{
		get => _font.Size;
		set
		{
			if (Math.Abs(_font.Size - value) < 0.01f)
				return;

			_font = new RenderFont("Arial", value);
		}
	}

	#endregion

	#region Trading day

	[Display(Name = "Equity", GroupName = "Trading day", Order = 5,
		Description = "How the account equity used by every limit is read: balance plus open PnL (when the connection reports the balance without the open trades), or the balance alone (when it already includes them).")]
	public EquitySource EquitySource
	{
		get => _equitySource;
		set => SetSetting(() => _equitySource = value);
	}

	[Display(Name = "Day reset time zone", GroupName = "Trading day", Order = 10,
		Description = "Time zone of the end of the trading day: New York (with its daylight saving changes) or this computer's local time.")]
	public DayResetZone DayResetZone
	{
		get => _dayResetZone;
		set
		{
			_dayResetZone = value;
			UpdateClock();
		}
	}

	[Display(Name = "Day reset time", GroupName = "Trading day", Order = 20,
		Description = "Time at which the trading day ends (17:00 New York for most futures prop firms).")]
	public TimeSpan DayResetTime
	{
		get => _dayResetTime;
		set
		{
			_dayResetTime = value < TimeSpan.Zero || value >= TimeSpan.FromDays(1) ? new TimeSpan(17, 0, 0) : value;
			UpdateClock();
		}
	}

	#endregion

	#region Trailing drawdown

	[Display(Name = "Enabled", GroupName = "Trailing drawdown", Order = 10)]
	public bool TrailingEnabled
	{
		get => _trailing.Enabled;
		set => SetSetting(() => _trailing.Enabled = value);
	}

	[Display(Name = "Maximum drawdown", GroupName = "Trailing drawdown", Order = 20,
		Description = "Distance between the peak equity and the stop equity, in account currency.")]
	[Range(0, 100000000)]
	public decimal TrailingMaxDrawdown
	{
		get => _trailing.MaxDrawdown;
		set => SetSetting(() => _trailing.MaxDrawdown = Math.Max(0m, value));
	}

	[Display(Name = "Start from", GroupName = "Trailing drawdown", Order = 30,
		Description = "Current equity, or a stop equity given by hand (for example the one your prop firm shows).")]
	public TrailingInitMode TrailingInitMode
	{
		get => _trailing.InitMode;
		set => SetSetting(() => _trailing.InitMode = value);
	}

	[Display(Name = "Manual stop equity", GroupName = "Trailing drawdown", Order = 40)]
	[Range(0, 100000000)]
	public decimal TrailingManualStopEquity
	{
		get => _trailing.ManualStopEquity;
		set => SetSetting(() => _trailing.ManualStopEquity = Math.Max(0m, value));
	}

	[Display(Name = "Peak update", GroupName = "Trailing drawdown", Order = 50,
		Description = "Realtime: the peak follows the intraday high of the equity, open PnL included. End of day: once per day, with the previous day's last equity.")]
	public TrailingPeakMode TrailingPeakMode
	{
		get => _trailing.PeakMode;
		set => SetSetting(() => _trailing.PeakMode = value);
	}

	[Display(Name = "Lock the stop", GroupName = "Trailing drawdown", Order = 60,
		Description = "The stop stops trailing once it reaches the start equity plus the lock offset.")]
	public bool TrailingLockEnabled
	{
		get => _trailing.LockEnabled;
		set => SetSetting(() => _trailing.LockEnabled = value);
	}

	[Display(Name = "Lock offset", GroupName = "Trailing drawdown", Order = 70,
		Description = "0 locks the stop at the start equity (Topstep style); 100 at the start plus 100 (Apex style).")]
	public decimal TrailingLockOffset
	{
		get => _trailing.LockOffset;
		set => SetSetting(() => _trailing.LockOffset = value);
	}

	[Display(Name = "Monthly reset", GroupName = "Trailing drawdown", Order = 80)]
	public bool TrailingMonthlyReset
	{
		get => _trailing.MonthlyReset;
		set => SetSetting(() => _trailing.MonthlyReset = value);
	}

	[Display(Name = "Monthly reset day", GroupName = "Trailing drawdown", Order = 90,
		Description = "Day of the month; the first trading day on or after it starts the trailing drawdown again.")]
	[Range(1, 31)]
	public int TrailingMonthlyResetDay
	{
		get => _trailing.MonthlyResetDay;
		set => SetSetting(() => _trailing.MonthlyResetDay = Math.Clamp(value, 1, 31));
	}

	[Display(Name = "Restart now", GroupName = "Trailing drawdown", Order = 100,
		Description = "Starts the trailing drawdown again from the current equity (or the manual stop). Self-resets.")]
	public bool TrailingRestartNow
	{
		get => false;
		set
		{
			if (!value)
				return;

			lock (_sync)
			{
				if (_state != null)
					TrailingDrawdownEngine.Reset(_state.Trailing);
			}

			Evaluate();
		}
	}

	#endregion

	#region Daily limits

	[Display(Name = "Enabled", GroupName = "Daily limits", Order = 10)]
	public bool DailyEnabled
	{
		get => _daily.Enabled;
		set => SetSetting(() => _daily.Enabled = value);
	}

	[Display(Name = "Daily loss limit", GroupName = "Daily limits", Order = 20)]
	[Range(0, 100000000)]
	public decimal DailyLossLimit
	{
		get => _daily.LossLimit;
		set => SetSetting(() => _daily.LossLimit = Math.Max(0m, value));
	}

	[Display(Name = "Loss measured from", GroupName = "Daily limits", Order = 30,
		Description = "The start equity of the day, or the day's highest equity (a daily trailing loss).")]
	public DailyLossMode DailyLossMode
	{
		get => _daily.LossMode;
		set => SetSetting(() => _daily.LossMode = value);
	}

	[Display(Name = "Daily profit target", GroupName = "Daily limits", Order = 40)]
	[Range(0, 100000000)]
	public decimal DailyProfitTarget
	{
		get => _daily.ProfitTarget;
		set => SetSetting(() => _daily.ProfitTarget = Math.Max(0m, value));
	}

	[Display(Name = "Day start equity", GroupName = "Daily limits", Order = 50,
		Description = "The previous day's last equity (the end-of-day balance), or the first equity of the day.")]
	public DailyBaseMode DailyBaseMode
	{
		get => _daily.BaseMode;
		set => SetSetting(() => _daily.BaseMode = value);
	}

	#endregion

	#region Suggested status

	[Display(Name = "Enabled", GroupName = "Suggested status", Order = 10)]
	public bool StatusEnabled
	{
		get => _recommendations.Enabled;
		set => SetSetting(() => _recommendations.Enabled = value);
	}

	[Display(Name = "Maximum trades per day", GroupName = "Suggested status", Order = 20,
		Description = "Trades of the chart instrument in the trading day; 0 turns the rule off.")]
	[Range(0, 1000)]
	public int MaxTradesPerDay
	{
		get => _recommendations.MaxTradesPerDay;
		set => SetSetting(() => _recommendations.MaxTradesPerDay = Math.Max(0, value));
	}

	[Display(Name = "Caution at % of maximum trades", GroupName = "Suggested status", Order = 30)]
	[Range(0, 100)]
	public decimal CautionTradesPercent
	{
		get => _recommendations.CautionTradesPercent;
		set => SetSetting(() => _recommendations.CautionTradesPercent = Math.Clamp(value, 0m, 100m));
	}

	[Display(Name = "Maximum consecutive losses", GroupName = "Suggested status", Order = 40)]
	[Range(0, 100)]
	public int MaxConsecutiveLosses
	{
		get => _recommendations.MaxConsecutiveLosses;
		set => SetSetting(() => _recommendations.MaxConsecutiveLosses = Math.Max(0, value));
	}

	[Display(Name = "Caution near the trailing stop", GroupName = "Suggested status", Order = 50,
		Description = "Caution when the distance to the trailing stop is this amount or less; 0 turns the rule off.")]
	[Range(0, 100000000)]
	public decimal TrailingCautionAmount
	{
		get => _recommendations.TrailingCautionAmount;
		set => SetSetting(() => _recommendations.TrailingCautionAmount = Math.Max(0m, value));
	}

	[Display(Name = "Caution at % of daily loss left", GroupName = "Suggested status", Order = 60,
		Description = "Caution when what is left of the daily loss limit is this percentage of it or less.")]
	[Range(0, 100)]
	public decimal CautionRemainingLossPercent
	{
		get => _recommendations.CautionRemainingLossPercent;
		set => SetSetting(() => _recommendations.CautionRemainingLossPercent = Math.Clamp(value, 0m, 100m));
	}

	[Display(Name = "Caution at loss from start", GroupName = "Suggested status", Order = 70,
		Description = "Caution when the day's loss reaches this amount; 0 turns the rule off.")]
	[Range(0, 100000000)]
	public decimal CautionLossFromStart
	{
		get => _recommendations.CautionLossFromStart;
		set => SetSetting(() => _recommendations.CautionLossFromStart = Math.Max(0m, value));
	}

	[Display(Name = "Caution at giveback", GroupName = "Suggested status", Order = 80,
		Description = "Caution when this amount of the day's peak profit has been given back; 0 uses the percentage of the target below.")]
	[Range(0, 100000000)]
	public decimal GivebackAmount
	{
		get => _recommendations.GivebackAmount;
		set => SetSetting(() => _recommendations.GivebackAmount = Math.Max(0m, value));
	}

	[Display(Name = "Caution at giveback, % of target", GroupName = "Suggested status", Order = 90)]
	[Range(0, 100)]
	public decimal GivebackPercentOfTarget
	{
		get => _recommendations.GivebackPercentOfTarget;
		set => SetSetting(() => _recommendations.GivebackPercentOfTarget = Math.Clamp(value, 0m, 100m));
	}

	[Display(Name = "Payout objective", GroupName = "Suggested status", Order = 100,
		Description = "Consistency rules: today's realized PnL against a percentage of this objective; 0 turns them off.")]
	[Range(0, 100000000)]
	public decimal PayoutObjective
	{
		get => _recommendations.PayoutObjective;
		set => SetSetting(() => _recommendations.PayoutObjective = Math.Max(0m, value));
	}

	[Display(Name = "Consistency caution, %", GroupName = "Suggested status", Order = 110)]
	[Range(0, 100)]
	public decimal ConsistencyCautionPercent
	{
		get => _recommendations.ConsistencyCautionPercent;
		set => SetSetting(() => _recommendations.ConsistencyCautionPercent = Math.Clamp(value, 0m, 100m));
	}

	[Display(Name = "Consistency stop, %", GroupName = "Suggested status", Order = 120)]
	[Range(0, 100)]
	public decimal ConsistencyStopPercent
	{
		get => _recommendations.ConsistencyStopPercent;
		set => SetSetting(() => _recommendations.ConsistencyStopPercent = Math.Clamp(value, 0m, 100m));
	}

	#endregion

	#region Rows

	[Display(Name = "Account ID", GroupName = "Account rows", Order = 10)]
	public bool ShowAccountId { get; set; } = true;

	[Display(Name = "Currency", GroupName = "Account rows", Order = 20)]
	public bool ShowCurrency { get; set; }

	[Display(Name = "Balance", GroupName = "Account rows", Order = 30)]
	public bool ShowBalance { get; set; } = true;

	[Display(Name = "Available balance", GroupName = "Account rows", Order = 40)]
	public bool ShowAvailableBalance { get; set; }

	[Display(Name = "Blocked margin", GroupName = "Account rows", Order = 50)]
	public bool ShowMargin { get; set; }

	[Display(Name = "Leverage", GroupName = "Account rows", Order = 60)]
	public bool ShowLeverage { get; set; }

	[Display(Name = "Open PnL", GroupName = "Account rows", Order = 70)]
	public bool ShowOpenPnL { get; set; } = true;

	[Display(Name = "Closed PnL", GroupName = "Account rows", Order = 80)]
	public bool ShowClosedPnL { get; set; }

	[Display(Name = "Total PnL", GroupName = "Account rows", Order = 90)]
	public bool ShowTotalPnL { get; set; }

	[Display(Name = "Trailing start and peak", GroupName = "Risk rows", Order = 10)]
	public bool ShowTrailingStartPeak { get; set; }

	[Display(Name = "Trailing stop", GroupName = "Risk rows", Order = 20)]
	public bool ShowTrailingStop { get; set; } = true;

	[Display(Name = "Distance to the trailing stop", GroupName = "Risk rows", Order = 30)]
	public bool ShowTrailingRemaining { get; set; } = true;

	[Display(Name = "Day start equity", GroupName = "Risk rows", Order = 40)]
	public bool ShowDayStart { get; set; }

	[Display(Name = "Day PnL", GroupName = "Risk rows", Order = 50)]
	public bool ShowDayPnl { get; set; } = true;

	[Display(Name = "Realized today", GroupName = "Risk rows", Order = 60)]
	public bool ShowRealizedToday { get; set; }

	[Display(Name = "Daily loss left", GroupName = "Risk rows", Order = 70)]
	public bool ShowRemainingLoss { get; set; } = true;

	[Display(Name = "To the daily target", GroupName = "Risk rows", Order = 80)]
	public bool ShowRemainingTarget { get; set; } = true;

	[Display(Name = "Trades today", GroupName = "Trade rows", Order = 10)]
	public bool ShowTradesToday { get; set; } = true;

	[Display(Name = "Wins / losses", GroupName = "Trade rows", Order = 20)]
	public bool ShowWinsLosses { get; set; } = true;

	[Display(Name = "Streak", GroupName = "Trade rows", Order = 30)]
	public bool ShowStreak { get; set; }

	[Display(Name = "Max / min open PnL of the trade", GroupName = "Trade rows", Order = 40,
		Description = "MFE and MAE of the open trade, or of the last trade while flat.")]
	public bool ShowTradeExcursion { get; set; } = true;

	[Display(Name = "Last trade PnL", GroupName = "Trade rows", Order = 50)]
	public bool ShowLastTradePnl { get; set; } = true;

	[Display(Name = "Position", GroupName = "Trade rows", Order = 60)]
	public bool ShowPosition { get; set; }

	[Display(Name = "Status", GroupName = "Trade rows", Order = 70)]
	public bool ShowStatus { get; set; } = true;

	[Display(Name = "Reasons", GroupName = "Trade rows", Order = 80)]
	public bool ShowReasons { get; set; } = true;

	#endregion

	#region Price rails

	[Display(Name = "Show price rails", GroupName = "Price rails", Order = 10,
		Description = "While a position is open, lines at the prices where it would take the account to the stop (the stricter of the daily and trailing stops) and to the daily target.")]
	public bool ShowPriceRails { get; set; }

	[Display(Name = "Stop rail", GroupName = "Price rails", Order = 20)]
	public bool ShowStopRail { get; set; } = true;

	[Display(Name = "Target rail", GroupName = "Price rails", Order = 30)]
	public bool ShowTargetRail { get; set; } = true;

	[Display(Name = "Stop rail color", GroupName = "Price rails", Order = 40)]
	public CrossColor StopRailColor
	{
		get => _stopRailColor.Convert();
		set => _stopRailColor = value.Convert();
	}

	[Display(Name = "Target rail color", GroupName = "Price rails", Order = 50)]
	public CrossColor TargetRailColor
	{
		get => _targetRailColor.Convert();
		set => _targetRailColor = value.Convert();
	}

	[Display(Name = "Rail width", GroupName = "Price rails", Order = 60)]
	[Range(1, 6)]
	public int RailWidth { get; set; } = 2;

	[Display(Name = "Rail labels", GroupName = "Price rails", Order = 70)]
	public bool ShowRailLabels { get; set; } = true;

	#endregion

	#region Layout

	[Display(Name = "Horizontal position", GroupName = "Layout", Order = 10)]
	public HorizontalAlignment HorizontalPosition { get; set; } = HorizontalAlignment.Left;

	[Display(Name = "Vertical position", GroupName = "Layout", Order = 20)]
	public VerticalAlignment VerticalPosition { get; set; } = VerticalAlignment.Bottom;

	[Display(Name = "Offset X", GroupName = "Layout", Order = 30)]
	[Range(0, 1000)]
	public int OffsetX { get; set; } = 20;

	[Display(Name = "Offset Y", GroupName = "Layout", Order = 40)]
	[Range(0, 1000)]
	public int OffsetY { get; set; } = 20;

	[Display(Name = "Column spacing", GroupName = "Layout", Order = 50)]
	[Range(5, 50)]
	public int ColumnSpacing { get; set; } = 15;

	#endregion

	#endregion

	#region ctor

	public PropRiskMonitor()
		: base(true)
	{
		DenyToChangePanel = true;
		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
		_store.Warning += message => this.LogWarn($"PropRiskMonitor: {message}");
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"PropRiskMonitor: initialized ({typeof(PropRiskMonitor).Assembly.GetName().Version}), state folder {StateStore.DefaultDirectory}.");

		if (TradingManager != null)
		{
			TradingManager.PortfolioSelected += OnPortfolioSelected;
			TradingManager.SecuritySelected += OnSecuritySelected;
		}

		SubscribeToTimer(TimerPeriod, Evaluate);
		_timerSubscribed = true;
		Evaluate();
	}

	protected override void OnDispose()
	{
		if (_timerSubscribed)
			UnsubscribeFromTimer(TimerPeriod, Evaluate);

		if (TradingManager != null)
		{
			TradingManager.PortfolioSelected -= OnPortfolioSelected;
			TradingManager.SecuritySelected -= OnSecuritySelected;
		}

		AttachPosition(null);
		SaveNow();
	}

	protected override void OnCalculate(int bar, decimal value)
	{
	}

	protected override void OnPositionChanged(Position position)
	{
		if (!IsChartPosition(position))
			return;

		AttachPosition(position);
		Evaluate();
	}

	protected override void OnPortfolioChanged(Portfolio portfolio)
	{
		Evaluate();
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		var view = _view;

		if (ChartInfo == null || Container == null || view?.Portfolio == null)
			return;

		if (ShowPriceRails && view.PositionVolume != 0m)
			DrawRails(context, view);

		var lines = BuildLines(view);

		if (lines.Count == 0)
			return;

		var lineHeight = context.MeasureString("A", _font).Height;
		var maxLabelWidth = 0;
		var maxValueWidth = 0;

		foreach (var line in lines)
		{
			maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(line.Label, _font).Width);
			maxValueWidth = Math.Max(maxValueWidth, context.MeasureString(line.Value, _font).Width);
		}

		var rectWidth = maxLabelWidth + ColumnSpacing + maxValueWidth + Padding * 2;
		var rectHeight = lines.Count * lineHeight + Padding * 2;
		var rectangle = new Rectangle(CalculateXPosition(rectWidth), CalculateYPosition(rectHeight), rectWidth, rectHeight);

		context.FillRectangle(_backgroundColor, rectangle);
		context.DrawRectangle(_borderPen, rectangle);

		var labelX = rectangle.X + Padding;
		var valueX = labelX + maxLabelWidth + ColumnSpacing;
		var y = rectangle.Y + Padding;

		foreach (var line in lines)
		{
			context.DrawString(line.Label, _font, _textColor, labelX, y);
			context.DrawString(line.Value, _font, line.ValueColor ?? ColorFor(line.RawForColoring), valueX, y);
			y += lineHeight;
		}
	}

	#endregion

	#region Private Methods: engine

	private void OnPortfolioSelected(Portfolio portfolio)
	{
		Evaluate();
	}

	private void OnSecuritySelected(Security security)
	{
		AttachPosition(null);
		Evaluate();
	}

	private void SetSetting(Action change)
	{
		lock (_sync)
			change();

		Evaluate();
	}

	private void UpdateClock()
	{
		lock (_sync)
			_clock = new SessionClock(_dayResetZone, _dayResetTime);

		Evaluate();
	}

	private bool IsChartPosition(Position position)
	{
		var portfolio = TradingManager?.Portfolio;
		var security = TradingManager?.Security;

		return position != null && portfolio != null && security != null
			&& string.Equals(position.AccountID, portfolio.AccountID, StringComparison.Ordinal)
			&& position.Security != null
			&& string.Equals(position.Security.Code, security.Code, StringComparison.Ordinal);
	}

	private void AttachPosition(Position position)
	{
		lock (_sync)
		{
			if (ReferenceEquals(_trackedPosition, position))
				return;

			if (_trackedPosition != null)
				_trackedPosition.PropertyChanged -= OnTrackedPositionChanged;

			_trackedPosition = position;

			if (_trackedPosition != null)
				_trackedPosition.PropertyChanged += OnTrackedPositionChanged;
		}
	}

	private void OnTrackedPositionChanged(object sender, PropertyChangedEventArgs e)
	{
		// UnrealizedPnL raises PropertyChanged on every PnL recalculation, so every excursion is seen.
		Evaluate();
	}

	// Updates every engine with the current account and position and publishes a new snapshot.
	// Called from trading events, the one-second timer and setting changes; never from rendering.
	private void Evaluate()
	{
		try
		{
			var portfolio = TradingManager?.Portfolio;

			if (portfolio == null)
			{
				_view = null;
				return;
			}

			var position = TradingManager.Position;

			if (IsChartPosition(position))
				AttachPosition(position);
			else
				position = null;

			var instrument = TradingManager.Security?.Code ?? InstrumentInfo?.Instrument ?? string.Empty;
			var now = DateTime.UtcNow;
			var equity = _equitySource == EquitySource.Balance ? portfolio.Balance : portfolio.Balance + portfolio.OpenPnL;

			View view;

			lock (_sync)
			{
				EnsureAccount(portfolio.AccountID);

				var day = _clock.TradingDay(now);
				var trailing = TrailingDrawdownEngine.Update(_state.Trailing, _trailing, equity, day);
				var daily = DailyRailsEngine.Update(_state.Daily, _daily, equity, portfolio.ClosedPnL, day);

				_ownInstruments.Add(instrument);

				if (!_state.Ledgers.TryGetValue(instrument, out var ledger))
					_state.Ledgers[instrument] = ledger = new LedgerState { Day = day };

				TradeRecord closed = null;

				if (position != null)
				{
					var volume = position.IsInPosition ? position.Volume : 0m;
					closed = TradeLedger.Update(ledger, new PositionSnapshot(volume, position.AveragePrice, position.UnrealizedPnL, position.RealizedPnL), day, now);
				}
				else if (day > ledger.Day)
				{
					ledger.Day = day;
					ledger.ClosedToday.Clear();
				}

				var stats = TradeLedger.Stats(ledger, day);
				var findings = RecommendationRules.Evaluate(new RecommendationInput(trailing, daily, stats, _daily.LossLimit, _daily.ProfitTarget), _recommendations);

				view = new View(
					portfolio,
					trailing,
					daily,
					stats,
					ledger.Current?.Clone(),
					ledger.Last?.Clone(),
					findings,
					RecommendationRules.StatusOf(findings),
					equity,
					position != null && position.IsInPosition ? position.Volume : 0m,
					position?.AveragePrice ?? 0m);

				if (closed != null)
					this.LogInfo($"PropRiskMonitor: {instrument} trade closed, {(closed.Side > 0 ? "long" : "short")} PnL {closed.Pnl:N2}, MFE {closed.MaxOpenPnl:N2}, MAE {closed.MinOpenPnl:N2}.");

				if (daily.NewDay)
					this.LogInfo($"PropRiskMonitor: {_accountKey} trading day {day:yyyy-MM-dd}, start equity {daily.StartEquity:N2}.");
			}

			var previous = _view;
			_view = view;
			ScheduleSave();

			if (previous == null || previous.Status != view.Status)
				this.LogInfo($"PropRiskMonitor: status {view.Status}{Reasons(view.Findings, " — ")}.");

			RedrawChart();
		}
		catch (Exception ex)
		{
			this.LogError($"PropRiskMonitor: update failed: {ex.Message}");
		}
	}

	// Caller holds _sync. Loads the state of a newly selected account, saving the previous one.
	private void EnsureAccount(string accountId)
	{
		var key = accountId ?? string.Empty;

		if (_state != null && string.Equals(_accountKey, key, StringComparison.Ordinal))
			return;

		if (_state != null)
			SaveState(_accountKey, _state.Clone(), new List<string>(_ownInstruments));

		_lastSavedFingerprint = null;

		_accountKey = key;
		_state = _store.Load(key);
		this.LogInfo($"PropRiskMonitor: account {key}, state {(_state.Trailing.Initialized || _state.Daily.Initialized ? "restored" : "new")} ({_store.PathOf(key)}).");
	}

	private void ScheduleSave()
	{
		if (Interlocked.Exchange(ref _saveScheduled, 1) == 1)
			return;

		Task.Delay(SaveDelay).ContinueWith(_ => SaveNow(), TaskScheduler.Default);
	}

	private void SaveNow()
	{
		Interlocked.Exchange(ref _saveScheduled, 0);

		string key;
		AccountState snapshot;
		List<string> own;

		lock (_sync)
		{
			if (_state == null)
				return;

			key = _accountKey;
			snapshot = _state.Clone();
			own = new List<string>(_ownInstruments);
		}

		// Nothing to write when the state is the same as at the last save.
		var fingerprint = key + "|" + JsonSerializer.Serialize(snapshot);

		if (fingerprint == _lastSavedFingerprint)
			return;

		var merged = SaveState(key, snapshot, own);

		if (merged != null)
			_lastSavedFingerprint = fingerprint;

		if (merged == null)
			return;

		// Adopt what other charts on the same account wrote (higher peaks, hits), unless the
		// account changed meanwhile.
		lock (_sync)
		{
			if (string.Equals(_accountKey, key, StringComparison.Ordinal))
			{
				_state.Trailing.PeakEquity = Math.Max(_state.Trailing.PeakEquity, merged.Trailing.StartEquity == _state.Trailing.StartEquity ? merged.Trailing.PeakEquity : _state.Trailing.PeakEquity);
				_state.Trailing.Breached |= merged.Trailing.StartEquity == _state.Trailing.StartEquity && merged.Trailing.Breached;

				if (merged.Daily.Day == _state.Daily.Day)
				{
					_state.Daily.PeakEquity = Math.Max(_state.Daily.PeakEquity, merged.Daily.PeakEquity);
					_state.Daily.StopHit |= merged.Daily.StopHit;
					_state.Daily.TargetHit |= merged.Daily.TargetHit;
				}
			}
		}
	}

	// Only this chart's instruments are written; the other charts' trades on disk are kept.
	private AccountState SaveState(string key, AccountState snapshot, List<string> own)
	{
		try
		{
			return _store.Save(key, snapshot, own);
		}
		catch (Exception ex)
		{
			this.LogWarn($"PropRiskMonitor: could not save the state of {key}: {ex.Message}");
			return null;
		}
	}

	#endregion

	#region Private Methods: rendering

	private List<DisplayLine> BuildLines(View view)
	{
		var p = view.Portfolio;
		var lines = new List<DisplayLine>();

		if (ShowAccountId)
			lines.Add(new("Account", p.AccountID, null));

		if (ShowCurrency && p.Currency.HasValue)
			lines.Add(new("Currency", p.Currency.Value.ToString(), null));

		if (ShowBalance)
			lines.Add(new("Balance", Money(p.Balance), null));

		if (ShowAvailableBalance && p.BalanceAvailable.HasValue)
			lines.Add(new("Available", Money(p.BalanceAvailable.Value), null));

		if (ShowMargin)
			lines.Add(new("Blocked margin", Money(p.BlockedMargin), null));

		if (ShowLeverage && p.Leverage != 1)
			lines.Add(new("Leverage", $"{p.Leverage:F2}x", null));

		if (ShowOpenPnL)
			lines.Add(new("Open PnL", Money(p.OpenPnL), p.OpenPnL));

		if (ShowClosedPnL)
			lines.Add(new("Closed PnL", Money(p.ClosedPnL), p.ClosedPnL));

		if (ShowTotalPnL)
			lines.Add(new("Total PnL", Money(p.ClosedPnL + p.OpenPnL), p.ClosedPnL + p.OpenPnL));

		var trailing = view.Trailing;

		if (trailing.Active)
		{
			if (ShowTrailingStartPeak)
			{
				lines.Add(new("Trailing start", Money(trailing.StartEquity), null));
				lines.Add(new("Trailing peak", Money(trailing.PeakEquity), null));
			}

			if (ShowTrailingStop)
				lines.Add(new("Trailing stop", Money(trailing.StopEquity) + (trailing.Locked ? " (locked)" : string.Empty), null));

			if (ShowTrailingRemaining)
				lines.Add(new("To the trailing stop", Money(trailing.Remaining), trailing.Remaining, trailing.Breached ? _negativeColor : null));
		}

		var daily = view.Daily;

		if (ShowDayStart)
			lines.Add(new("Day start", Money(daily.StartEquity), null));

		if (ShowDayPnl)
			lines.Add(new("Day PnL", Money(daily.DayPnl), daily.DayPnl));

		if (ShowRealizedToday)
			lines.Add(new("Realized today", Money(daily.RealizedToday), daily.RealizedToday));

		if (ShowRemainingLoss && daily.RemainingLoss is { } remainingLoss)
			lines.Add(new("Daily loss left", Money(remainingLoss), remainingLoss, daily.StopHitToday ? _negativeColor : null));

		if (ShowRemainingTarget && daily.RemainingTarget is { } remainingTarget)
			lines.Add(new("To the daily target", Money(remainingTarget), null, daily.TargetHitToday ? _positiveColor : null));

		var trades = view.Trades;

		if (ShowTradesToday)
			lines.Add(new("Trades today", MaxTradesPerDay > 0 ? $"{trades.Trades} / {MaxTradesPerDay}" : trades.Trades.ToString(CultureInfo.CurrentCulture), null));

		if (ShowWinsLosses)
			lines.Add(new("Wins / losses", $"{trades.Wins} / {trades.Losses}", null));

		if (ShowStreak)
			lines.Add(new("Streak", trades.Streak > 0 ? $"{trades.Streak} W" : trades.Streak < 0 ? $"{-trades.Streak} L" : "-", trades.Streak));

		var excursionTrade = view.CurrentTrade ?? view.LastTrade;

		if (ShowTradeExcursion && excursionTrade != null)
		{
			var prefix = view.CurrentTrade != null ? "Trade" : "Last trade";
			lines.Add(new($"{prefix} max / min", $"{Money(excursionTrade.MaxOpenPnl)} / {Money(excursionTrade.MinOpenPnl)}", null));
		}

		if (ShowLastTradePnl && view.LastTrade != null)
			lines.Add(new("Last trade PnL", Money(view.LastTrade.Pnl), view.LastTrade.Pnl));

		if (ShowPosition)
		{
			lines.Add(view.PositionVolume == 0m
				? new("Position", "Flat", null)
				: new("Position", $"{(view.PositionVolume > 0 ? "Long" : "Short")} {Math.Abs(view.PositionVolume):0.##} @ {view.PositionPrice:0.#####}", null));
		}

		if (StatusEnabled && ShowStatus)
		{
			var color = view.Status switch
			{
				RiskStatus.Stop => _negativeColor,
				RiskStatus.Caution => _cautionColor,
				_ => _positiveColor
			};

			lines.Add(new("Status", view.Status.ToString().ToUpperInvariant(), null, color));

			if (ShowReasons)
			{
				foreach (var finding in view.Findings)
					lines.Add(new(finding.Severity == RiskStatus.Stop ? "  stop" : "  caution", Describe(finding), null, finding.Severity == RiskStatus.Stop ? _negativeColor : _cautionColor));
			}
		}

		return lines;
	}

	private void DrawRails(RenderContext context, View view)
	{
		var security = TradingManager?.Security;
		var tickSize = InstrumentInfo?.TickSize ?? 0m;

		if (security == null || tickSize <= 0m || CurrentBar == 0)
			return;

		var price = GetCandle(CurrentBar - 1).Close;
		var region = ChartInfo.PriceChartContainer.Region;
		var x1 = ChartInfo.GetXByBar(FirstVisibleBarNumber, false);

		if (ShowStopRail)
		{
			decimal? trailingStop = view.Trailing.Active ? view.Trailing.StopEquity : null;
			var stop = RailPriceSolver.Stop(view.PositionVolume, price, view.Equity, tickSize, security.TickCost, view.Daily.StopEquity, trailingStop);

			if (stop is { } s)
				DrawRail(context, region, x1, s.Price, _stopRailColor, $"Stop {s.Price.ToString(CultureInfo.CurrentCulture)} ({(s.Source == RailSource.Trailing ? "trailing" : "daily")})");
		}

		if (ShowTargetRail)
		{
			var target = RailPriceSolver.Target(view.PositionVolume, price, view.Equity, tickSize, security.TickCost, view.Daily.TargetEquity);

			if (target is { } t)
				DrawRail(context, region, x1, t.Price, _targetRailColor, $"Target {t.Price.ToString(CultureInfo.CurrentCulture)}");
		}
	}

	private void DrawRail(RenderContext context, Rectangle region, int x1, decimal price, Color color, string label)
	{
		var y = ChartInfo.GetYByPrice(price, false);

		if (y < region.Top || y > region.Bottom)
			return;

		context.DrawLine(new RenderPen(color, RailWidth), Math.Max(region.Left, x1), y, region.Right, y);

		if (!ShowRailLabels)
			return;

		var size = context.MeasureString(label, _font);
		context.DrawString(label, _font, color, region.Right - size.Width - 5, y - size.Height - 2);
	}

	private static string Describe(RiskFinding f)
	{
		return f.Reason switch
		{
			RiskReason.TrailingBreached => "trailing drawdown breached",
			RiskReason.TrailingNearStop => $"{Money(f.Value)} to the trailing stop",
			RiskReason.DailyStop => "daily loss limit reached",
			RiskReason.DailyStopHitEarlier => "daily loss limit hit earlier today",
			RiskReason.DailyTarget => "daily target reached",
			RiskReason.MaxTrades => $"trades {f.Value:0} / {f.Limit:0}",
			RiskReason.NearMaxTrades => $"trades {f.Value:0} / {f.Limit:0}",
			RiskReason.MaxLosses => $"{f.Value:0} losses in a row",
			RiskReason.OneLossAway => $"{f.Value:0} of {f.Limit:0} losses in a row",
			RiskReason.NearDailyStop => $"{Money(f.Value)} of daily loss left",
			RiskReason.LossFromStart => $"day loss {Money(-f.Value)}",
			RiskReason.Giveback => $"gave back {Money(f.Value)} from the day's peak",
			RiskReason.ConsistencyStop => $"realized {Money(f.Value)} >= {Money(f.Limit)} (consistency)",
			RiskReason.ConsistencyCaution => $"realized {Money(f.Value)} >= {Money(f.Limit)} (consistency)",
			_ => f.Reason.ToString()
		};
	}

	private static string Reasons(IReadOnlyList<RiskFinding> findings, string prefix)
	{
		if (findings.Count == 0)
			return string.Empty;

		var parts = new List<string>(findings.Count);

		foreach (var finding in findings)
			parts.Add(Describe(finding));

		return prefix + string.Join("; ", parts);
	}

	private Color ColorFor(decimal? raw)
	{
		return raw is null ? _textColor
			: raw > 0m ? _positiveColor
			: raw < 0m ? _negativeColor
			: _neutralColor;
	}

	private static string Money(decimal value)
	{
		return value.ToString("N2", CultureInfo.CurrentCulture);
	}

	private int CalculateXPosition(int width)
	{
		return HorizontalPosition switch
		{
			HorizontalAlignment.Center => (Container.Region.Width - width) / 2,
			HorizontalAlignment.Right => Container.Region.Width - width - OffsetX,
			_ => OffsetX
		};
	}

	private int CalculateYPosition(int height)
	{
		return VerticalPosition switch
		{
			VerticalAlignment.Middle => (Container.Region.Height - height) / 2,
			VerticalAlignment.Bottom => Container.Region.Height - height - OffsetY,
			_ => OffsetY
		};
	}

	#endregion
}
