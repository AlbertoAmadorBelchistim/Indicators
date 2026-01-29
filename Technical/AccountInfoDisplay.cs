namespace ATAS.Indicators.Technical;

using ATAS.DataFeedsCore;
using ATAS.Indicators.Technical.Properties;
using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// Displays account information on the chart including account ID, balance, blocked margin, available balance, and PnL.
/// </summary>
[HelpLink("https://help.atas.net/en/support/solutions/articles/72000648751-account-info-display")]
[Category(IndicatorCategories.Trading)]
[DisplayName("Account Info Display")]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.AccountInfoDisplayDescription))]
public class AccountInfoDisplay : Indicator
{
    #region class

    #region class - UI rows
    private sealed class DisplayRow
    {
        public string Label { get; }
        public string ValueText { get; }

        // Not used yet in Phase 0 (kept for Phase 1/2).
        public decimal? NumericValue { get; }
        public CrossColor? ValueColorOverride { get; }

        public DisplayRow(string label, string valueText, decimal? numericValue = null, CrossColor? valueColorOverride = null)
        {
            Label = label ?? string.Empty;
            ValueText = valueText ?? string.Empty;
            NumericValue = numericValue;
            ValueColorOverride = valueColorOverride;
        }
    }

    #endregion

    #region class - Daily Rails state


    private sealed class DailyRailsState
    {
        public int LastDailyResetKey;


        // Phase D/E will use these. Phase C only initializes/resets them.
        public decimal DailyRealizedPnlBaseline;
        public int TradesToday;
        public int WinsToday;
        public int LossesToday;

        // Single-position trade event tracking (OPEN->FLAT) to derive "closed trades today"
        public bool WasPositionOpen;
        public decimal TradeClosedPnlBaseline;

        public int CurrentWinStreak;
        public int CurrentLossStreak;


        public void ResetForNewDay(int dailyResetKey)
        {
            LastDailyResetKey = dailyResetKey;

            DailyRealizedPnlBaseline = 0m;

            TradesToday = 0;
            WinsToday = 0;
            LossesToday = 0;

            CurrentWinStreak = 0;
            CurrentLossStreak = 0;

            WasPositionOpen = false;
            TradeClosedPnlBaseline = 0m;
        }
    }


    #endregion

    #region class - Trailing DD state

    private sealed class TrailingDrawdownState
    {
        public bool IsInitialized;
        public decimal StartEquity;
        public decimal PeakEquity;
        public decimal StopEquity;
        public DateTime LastEodPeakCaptureDate;
        public int LastMonthlyResetKey;
        public bool WasBreachedBeforeSession;
        public int LastSessionKey;

        public void Reset()
        {
            IsInitialized = false;
            StartEquity = 0m;
            PeakEquity = 0m;
            StopEquity = 0m;
            LastEodPeakCaptureDate = DateTime.MinValue;
            LastMonthlyResetKey = 0;
            WasBreachedBeforeSession = false;
            LastSessionKey = 0;
        }
    }

    #endregion

    #region class - Persistence DTOs

    private sealed class PersistedRootV1
    {
        public int SchemaVersion { get; set; }
        public Dictionary<string, PersistedAccountV1> Accounts { get; set; } = new();
    }

    private sealed class PersistedAccountV1
    {
        public PersistedTrailingDdV1 Trailing { get; set; } = new();
    }

    private sealed class PersistedTrailingDdV1
    {
        public bool IsInitialized { get; set; }
        public decimal StartEquity { get; set; }
        public decimal PeakEquity { get; set; }
        public decimal StopEquity { get; set; }

        public DateTime LastEodPeakCaptureDate { get; set; }
        public int LastMonthlyResetKey { get; set; }

        public bool WasBreachedBeforeSession { get; set; }
        public int LastSessionKey { get; set; }
    }

    #endregion

    #endregion

    #region Fields

    #region Fields - Visualization

    private Color _backgroundColor = Color.FromArgb(200, 20, 25, 35);
	private Color _textColor = Color.FromArgb(220, 220, 220);
	private Color _positiveColor = Color.FromArgb(0, 230, 118);
	private Color _negativeColor = Color.FromArgb(255, 82, 82);
	private Color _neutralColor = Color.FromArgb(150, 150, 150);
	private RenderFont _font = new("Arial", 11);
	private RenderStringFormat _stringFormat = new()
	{
		LineAlignment = StringAlignment.Near,
		Alignment = StringAlignment.Near
	};

    #endregion

    #region Fields - Portfolio

    private Portfolio _currentPortfolio;

    #endregion

    #region Fields - Trailing DD runtime

    private readonly Dictionary<string, TrailingDrawdownState> _trailingStatesByAccount = new();
    private readonly Dictionary<string, DailyRailsState> _dailyRailsStatesByAccount = new();

    #endregion

    #region Fields - Persistence

    private const int _persistenceSchemaVersion = 1;
    private const string _persistenceFileName = "AccountInfoDisplay.states.v1.json";
    private string _persistencePath;
    private PersistedRootV1 _persisted; // in-memory loaded snapshot
    private readonly HashSet<string> _loadedAccounts = new();
    private bool _persistenceDirty;
    private string _lastSavedSha256;
    private DateTime _lastSaveAttemptUtc = DateTime.MinValue;

    // Tuneable: prevents excessive disk IO on frequent peak updates.
    private static readonly TimeSpan _saveThrottle = TimeSpan.FromSeconds(2);

    #endregion

    #endregion

    #region Properties

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.BackGround),
		Description = nameof(Strings.LabelFillColorDescription), GroupName = nameof(Strings.Visualization))]
	public CrossColor BackgroundColor
	{
		get => _backgroundColor.Convert();
		set => _backgroundColor = value.Convert();
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.TextColor),
		Description = nameof(Strings.LabelTextColorDescription), GroupName = nameof(Strings.Visualization))]
	public CrossColor TextColor
	{
		get => _textColor.Convert();
		set => _textColor = value.Convert();
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.PositiveColor),
		Description = nameof(Strings.PositiveColorDescription), GroupName = nameof(Strings.Visualization))]
	public CrossColor PositiveColor
	{
		get => _positiveColor.Convert();
		set => _positiveColor = value.Convert();
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.NegativeColor),
		Description = nameof(Strings.NegativeColorDescription), GroupName = nameof(Strings.Visualization))]
	public CrossColor NegativeColor
	{
		get => _negativeColor.Convert();
		set => _negativeColor = value.Convert();
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.NeutralColor),
		Description = nameof(Strings.NeutralColorDescription), GroupName = nameof(Strings.Visualization))]
	public CrossColor NeutralColor
	{
		get => _neutralColor.Convert();
		set => _neutralColor = value.Convert();
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.FontSize),
		Description = nameof(Strings.FontSizeDescription), GroupName = nameof(Strings.Visualization))]
	[Range(6, 30)]
	public float FontSize
	{
		get => _font.Size;
		set => _font = new RenderFont("Arial", value);
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowAccountId),
		Description = nameof(Strings.ShowAccountIdDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowAccountId { get; set; } = true;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowCurrency),
		Description = nameof(Strings.ShowCurrencyDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowCurrency { get; set; } = true;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowBalance),
		Description = nameof(Strings.ShowBalanceDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowBalance { get; set; } = true;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowAvailableBalance),
		Description = nameof(Strings.ShowAvailableBalanceDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowAvailableBalance { get; set; } = true;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowBlockedMargin),
		Description = nameof(Strings.ShowBlockedMarginDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowMargin { get; set; } = false;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowLeverage),
		Description = nameof(Strings.ShowLeverageDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowLeverage { get; set; } = true;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowOpenPnL),
		Description = nameof(Strings.ShowOpenPnLDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowOpenPnL { get; set; } = true;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowClosedPnL),
		Description = nameof(Strings.ShowClosedPnLDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowClosedPnL { get; set; } = true;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowTotalPnL),
		Description = nameof(Strings.ShowTotalPnLDescription), GroupName = nameof(Strings.Settings))]
	public bool ShowTotalPnL { get; set; } = false;

    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.ShowTradesTodayRow),
    Description = nameof(Resources.ShowTradesTodayRowDescription),
    GroupName = nameof(Resources.DailyRails),
    Order = 200)]
    public bool ShowTradesTodayRow { get; set; } = true;


    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.ShowWinsLossesTodayRow),
    Description = nameof(Resources.ShowWinsLossesTodayRowDescription),
    GroupName = nameof(Resources.DailyRails),
    Order = 210)]
    public bool ShowWinsLossesTodayRow { get; set; } = true;


    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.ShowCurrentStreakRow),
    Description = nameof(Resources.ShowCurrentStreakRowDescription),
    GroupName = nameof(Resources.DailyRails),
    Order = 220)]
    public bool ShowCurrentStreakRow { get; set; } = true;


    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.ShowRealizedPnlTodayRow),
    Description = nameof(Resources.ShowRealizedPnlTodayRowDescription),
    GroupName = nameof(Resources.DailyRails),
    Order = 230)]
    public bool ShowRealizedPnlTodayRow { get; set; } = true;

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.HorizontalPosition),
		GroupName = nameof(Strings.LayoutGroup))]
	public HorizontalAlignment HorizontalPosition { get; set; } = HorizontalAlignment.Left;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.VerticalPosition),
		GroupName = nameof(Strings.LayoutGroup))]
	public VerticalAlignment VerticalPosition { get; set; } = VerticalAlignment.Bottom;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.OffsetX),
		Description = nameof(Strings.OffsetXDescription), GroupName = nameof(Strings.LayoutGroup))]
	[Range(0, 1000)]
	public int OffsetX { get; set; } = 20;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.OffsetY),
		Description = nameof(Strings.OffsetYDescription), GroupName = nameof(Strings.LayoutGroup))]
	[Range(0, 1000)]
	public int OffsetY { get; set; } = 20;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ColumnSpacing),
		Description = nameof(Strings.ColumnSpacingDescription), GroupName = nameof(Strings.LayoutGroup))]
	[Range(5, 50)]
	public int ColumnSpacing { get; set; } = 15;

    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.EnableTrailingDrawdown),
    Description = nameof(Resources.EnableTrailingDrawdownDescription),
    GroupName = nameof(Resources.FundingTrailingDd),
    Order = 10)]
    public bool EnableTrailingDrawdown { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.MaxTrailingDrawdown),
        Description = nameof(Resources.MaxTrailingDrawdownDescription),
        GroupName = nameof(Resources.FundingTrailingDd),
        Order = 20)]
    public decimal MaxTrailingDrawdown { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.TrailingInitializationMode),
        Description = nameof(Resources.TrailingInitializationModeDescription),
        GroupName = nameof(Resources.FundingTrailingDd),
        Order = 30)]
    public TrailingInitializationMode TrailingInitMode { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.TrailingManualStopEquity),
        Description = nameof(Resources.TrailingManualStopEquityDescription),
        GroupName = nameof(Resources.FundingTrailingDd),
        Order = 40)]
    public decimal TrailingManualStopEquity { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.ReinitializeNow),
        Description = nameof(Resources.ReinitializeNowDescription),
        GroupName = nameof(Resources.FundingTrailingDd),
        Order = 50)]
    public bool ReinitializeNow { get; set; }

    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.TrailingPeakUpdateMode),
    Description = nameof(Resources.TrailingPeakUpdateModeDescription),
    GroupName = nameof(Resources.FundingTrailingDd),
    Order = 60)]
    public TrailingPeakUpdateMode PeakUpdateMode { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.TrailingEodTimeLocal),
        Description = nameof(Resources.TrailingEodTimeLocalDescription),
        GroupName = nameof(Resources.FundingTrailingDd),
        Order = 70)]
    public TimeSpan TrailingEodTimeLocal { get; set; }

    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.EnableMonthlyReset),
    Description = nameof(Resources.EnableMonthlyResetDescription),
    GroupName = nameof(Resources.FundingTrailingDd),
    Order = 80)]
    public bool EnableMonthlyReset { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.MonthlyResetDay),
        Description = nameof(Resources.MonthlyResetDayDescription),
        GroupName = nameof(Resources.FundingTrailingDd),
        Order = 90)]
    public int MonthlyResetDay { get; set; }

    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.DailyResetMode),
    Description = nameof(Resources.DailyResetModeDescription),
    GroupName = nameof(Resources.DailyRails),
    Order = 100)]
    public DailyResetModeKind DailyResetMode { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.DailyResetTimeLocal),
        Description = nameof(Resources.DailyResetTimeLocalDescription),
        GroupName = nameof(Resources.DailyRails),
        Order = 110)]
    public TimeSpan DailyResetTimeLocal { get; set; }

    #endregion

    #region Enums

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

    public enum TrailingInitializationMode
    {
        CurrentEquity,
        ManualStopEquity
    }

    public enum TrailingPeakUpdateMode
    {
        Realtime,
        EndOfDay
    }

    public enum DailyResetModeKind
    {
        NewYork1700,
        LocalCustomTime
    }

    public enum DailyLossModeKind
    {
        FromSessionStart,
        FromSessionPeak
    }

    #endregion

    #region ctor

    public AccountInfoDisplay()
		: base(true)
	{
		DenyToChangePanel = true;
		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

        EnableTrailingDrawdown = false;
        MaxTrailingDrawdown = 0m;
        TrailingInitMode = TrailingInitializationMode.CurrentEquity;
        TrailingManualStopEquity = 0m;
        PeakUpdateMode = TrailingPeakUpdateMode.Realtime;
        TrailingEodTimeLocal = new TimeSpan(17, 0, 0); // sensible default
        EnableMonthlyReset = false;
        MonthlyResetDay = 1;
        DailyResetMode = DailyResetModeKind.NewYork1700;
        DailyResetTimeLocal = new TimeSpan(17, 0, 0);
    }

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		if (TradingManager != null)
		{
			TradingManager.PortfolioSelected += OnPortfolioSelected;
			_currentPortfolio = TradingManager.Portfolio;
		}
	}

	protected override void OnDispose()
	{
		if (TradingManager != null)
		{
			TradingManager.PortfolioSelected -= OnPortfolioSelected;
		}
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		// No calculation needed
	}

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
    {
        if (ChartInfo == null || Container?.Region == null)
            return;

        // Get current portfolio
        var portfolio = _currentPortfolio ?? TradingManager?.Portfolio;
        if (portfolio == null)
            return;

        if (_persisted == null)
            LoadPersistenceSafe();

        var accountKey = GetAccountKey();
        if (!_loadedAccounts.Contains(accountKey))
        {
            ApplyLoadedTrailingStateIfAny(accountKey);
            _loadedAccounts.Add(accountKey);
        }

        var dailyState = GetDailyRailsState(accountKey);
        MaybeResetDailyRails(dailyState, portfolio);
        UpdateDailySessionMetricsFromTradeEvents(dailyState, portfolio);

        var equity = portfolio.Balance + portfolio.OpenPnL;
        UpdateTrailingDrawdown(equity);

        // Build structured rows (Phase 0 replacement)
        var rows = BuildRows(portfolio);
        if (rows == null || rows.Count == 0)
            return;

        // Calculate proper dimensions for table layout
        var lineHeight = context.MeasureString("A", _font).Height;

        var maxLabelWidth = 0;
        var maxValueWidth = 0;

        foreach (var row in rows)
        {
            var labelWidth = (int)context.MeasureString(row.Label, _font).Width;
            var valueWidth = (int)context.MeasureString(row.ValueText, _font).Width;

            if (labelWidth > maxLabelWidth)
                maxLabelWidth = labelWidth;

            if (valueWidth > maxValueWidth)
                maxValueWidth = valueWidth;
        }

        var padding = 10;
        var rectWidth = maxLabelWidth + ColumnSpacing + maxValueWidth + padding * 2;
        var rectHeight = rows.Count * lineHeight + padding * 2;

        // Calculate position
        var x = CalculateXPosition(rectWidth);
        var y = CalculateYPosition(rectHeight);

        // Draw background
        var rectangle = new Rectangle(x, y, rectWidth, rectHeight);
        context.FillRectangle(_backgroundColor, rectangle);

        // Draw border
        context.DrawRectangle(new RenderPen(Color.Gray, 1), rectangle);

        // Draw text (keep existing coloring logic for Phase 0)
        var textRect = new Rectangle(
            x + padding,
            y + padding,
            rectWidth - padding * 2,
            rectHeight - padding * 2
        );

        DrawColoredRows(context, rows, textRect, portfolio, maxLabelWidth);

        SavePersistenceIfNeeded();
    }

    #endregion

    #region Private Methods

    #region Private Methods - Portfolio selection

    private void OnPortfolioSelected(Portfolio portfolio)
	{
		_currentPortfolio = portfolio;
		RedrawChart();
	}

    #endregion

    #region Private Methods - UI rows (render model)

    private List<DisplayRow> BuildRows(Portfolio portfolio)
    {
        var rows = new List<DisplayRow>();

        TrailingDrawdownState trailingState = null;

        if (portfolio == null)
            return rows;

        if (EnableTrailingDrawdown && MaxTrailingDrawdown > 0m)
            trailingState = GetTrailingState();

        if (ShowAccountId)
            rows.Add(new DisplayRow(
                Resources.RowAccount,
                portfolio.AccountID
            ));

        if (ShowCurrency && portfolio.Currency.HasValue)
            rows.Add(new DisplayRow(
                Resources.RowCurrency,
                portfolio.Currency.Value.ToString()
            ));

        if (ShowBalance)
            rows.Add(new DisplayRow(
                Resources.RowBalance,
                FormatCurrency(portfolio.Balance)
            ));

        if (ShowAvailableBalance && portfolio.BalanceAvailable.HasValue)
            rows.Add(new DisplayRow(
                Resources.RowAvailable,
                FormatCurrency(portfolio.BalanceAvailable.Value)
            ));

        if (ShowMargin)
            rows.Add(new DisplayRow(
                Resources.RowBlockedMargin,
                FormatCurrency(portfolio.BlockedMargin)
            ));

        if (ShowLeverage && portfolio.Leverage != 1)
            rows.Add(new DisplayRow(
                Resources.RowLeverage,
                $"{portfolio.Leverage:F2}x"
            ));

        if (ShowOpenPnL)
            rows.Add(new DisplayRow(
                Resources.RowOpenPnL,
                FormatCurrency(portfolio.OpenPnL),
                numericValue: portfolio.OpenPnL
            ));

        if (ShowClosedPnL)
            rows.Add(new DisplayRow(
                Resources.RowClosedPnL,
                FormatCurrency(portfolio.ClosedPnL),
                numericValue: portfolio.ClosedPnL
            ));

        if (ShowTotalPnL)
        {
            var totalPnL = portfolio.ClosedPnL + portfolio.OpenPnL;

            rows.Add(new DisplayRow(
                Resources.RowTotalPnL,
                FormatCurrency(totalPnL),
                numericValue: totalPnL
            ));
        }

        if (trailingState != null && trailingState.IsInitialized)
        {
            rows.Add(new DisplayRow(
                label: Resources.RowTrailingStartEquity,
                valueText: trailingState.StartEquity.ToString(CultureInfo.CurrentCulture),
                numericValue: trailingState.StartEquity));

            rows.Add(new DisplayRow(
                label: Resources.RowTrailingPeakEquity,
                valueText: trailingState.PeakEquity.ToString(CultureInfo.CurrentCulture),
                numericValue: trailingState.PeakEquity));

            rows.Add(new DisplayRow(
                label: Resources.RowTrailingStopEquity,
                valueText: trailingState.StopEquity.ToString(CultureInfo.CurrentCulture),
                numericValue: trailingState.StopEquity));
        }

        // -----------------------------
        // Session Metrics
        // -----------------------------
        if (ShowTradesTodayRow || ShowWinsLossesTodayRow || ShowCurrentStreakRow || ShowRealizedPnlTodayRow)
        {
            var accountKey = GetAccountKey();
            var daily = GetDailyRailsState(accountKey);

            var realizedToday = portfolio.ClosedPnL - daily.DailyRealizedPnlBaseline;

            if (ShowTradesTodayRow)
                rows.Add(new DisplayRow(Resources.RowTradesToday, daily.TradesToday.ToString("N0")));

            if (ShowWinsLossesTodayRow)
                rows.Add(new DisplayRow(Resources.RowWinsLossesToday, $"{daily.WinsToday:N0} / {daily.LossesToday:N0}"));

            if (ShowCurrentStreakRow)
            {
                var streakText =
                    daily.CurrentWinStreak > 0 ? $"W{daily.CurrentWinStreak:N0}" :
                    daily.CurrentLossStreak > 0 ? $"L{daily.CurrentLossStreak:N0}" :
                    "-";

                rows.Add(new DisplayRow(Resources.RowStreak, streakText));
            }

            if (ShowRealizedPnlTodayRow)
                rows.Add(new DisplayRow(Resources.RowRealizedPnlToday, FormatCurrency(realizedToday), numericValue: realizedToday));
        }

        return rows;
    }

    private void DrawColoredRows(RenderContext context, List<DisplayRow> rows, Rectangle textRect, Portfolio portfolio, int maxLabelWidth)
    {
        var lineHeight = context.MeasureString("A", _font).Height;

        // Calculate value column position
        var valueColumnX = textRect.X + maxLabelWidth + ColumnSpacing;
        var currentY = textRect.Y;

        foreach (var row in rows)
        {
            var label = row?.Label ?? string.Empty;
            var valueStr = row?.ValueText ?? string.Empty;

            // Draw label
            context.DrawString(label, _font, _textColor, textRect.X, currentY);

            // Determine color for value (Phase 0: keep current behavior)
            var valueColor = _textColor;

            if (row.NumericValue.HasValue)
            {
                var value = row.NumericValue.Value;
                valueColor = value > 0
                    ? _positiveColor
                    : (value < 0 ? _negativeColor : _neutralColor);
            }

            // Draw value
            context.DrawString(valueStr, _font, valueColor, valueColumnX, currentY);

            currentY += lineHeight;
        }
    }

    #endregion

    #region Private Methods - Layout helpers

    private string FormatCurrency(decimal value)
	{
		return value.ToString("N2");
	}

	private int CalculateXPosition(int width)
	{
		return HorizontalPosition switch
		{
			HorizontalAlignment.Left => OffsetX,
			HorizontalAlignment.Center => (Container.Region.Width - width) / 2,
			HorizontalAlignment.Right => Container.Region.Width - width - OffsetX,
			_ => OffsetX
		};
	}

	private int CalculateYPosition(int height)
	{
		return VerticalPosition switch
		{
			VerticalAlignment.Top => OffsetY,
			VerticalAlignment.Middle => (Container.Region.Height - height) / 2,
			VerticalAlignment.Bottom => Container.Region.Height - height - OffsetY,
			_ => OffsetY
		};
	}

    #endregion

    #region Private Methods - Trailing DD (account + state)

    private string GetAccountKey()
    {
        var portfolio = _currentPortfolio ?? TradingManager?.Portfolio;
        return portfolio?.AccountID ?? "UNKNOWN_ACCOUNT";
    }

    private TrailingDrawdownState GetTrailingState()
    {
        var key = GetAccountKey();

        if (!_trailingStatesByAccount.TryGetValue(key, out var state))
        {
            state = new TrailingDrawdownState();
            _trailingStatesByAccount[key] = state;
        }

        return state;
    }

    #endregion

    #region Private Methods - Trailing DD (core)

    private void InitializeTrailingState(TrailingDrawdownState state, decimal currentEquity)
    {
        state.IsInitialized = true;

        if (TrailingInitMode == TrailingInitializationMode.ManualStopEquity && TrailingManualStopEquity > 0m)
        {
            state.StopEquity = TrailingManualStopEquity;
            state.PeakEquity = state.StopEquity + MaxTrailingDrawdown;
            state.StartEquity = state.PeakEquity;
        }
        else
        {
            state.StartEquity = currentEquity;
            state.PeakEquity = currentEquity;
            state.StopEquity = currentEquity - MaxTrailingDrawdown;
        }

        var accountKey = GetAccountKey();
        PersistTrailingStateToMemory(accountKey);
        MarkPersistenceDirty();
    }

    private void UpdateTrailingDrawdown(decimal currentEquity)
    {

        // Button-like behavior: consume trigger and reset state
        if (ReinitializeNow)
        {
            ResetTrailingState();
            ReinitializeNow = false;
            RedrawChart();
             return;
        }

        if (!EnableTrailingDrawdown || MaxTrailingDrawdown <= 0m)
            return;

        var state = GetTrailingState();

        // Session-start processing (boundary is TrailingEodTimeLocal)
        var sessionKey = GetSessionKey();
        var isSessionStart = IsSessionBoundaryCrossed() && sessionKey != 0 && state.LastSessionKey != sessionKey;


        if (isSessionStart)
        {
            state.LastSessionKey = sessionKey;


            // If we're already below stop at session start, the breach is considered pre-session.
            if (state.IsInitialized && currentEquity <= state.StopEquity)
                state.WasBreachedBeforeSession = true;


            MaybeResetMonthlyTrailingAtSessionStart(state, DateTime.Now);
        }

        if (!state.IsInitialized)
        {
            InitializeTrailingState(state, currentEquity);
            return;
        }

        // Update peak equity
        if (PeakUpdateMode == TrailingPeakUpdateMode.Realtime)
        {
            if (currentEquity > state.PeakEquity)
            {
                state.PeakEquity = currentEquity;
                state.StopEquity = state.PeakEquity - MaxTrailingDrawdown;

                var accountKey = GetAccountKey();
                PersistTrailingStateToMemory(accountKey);
                MarkPersistenceDirty();
            }
        }
        else
        {
            var nowLocal = DateTime.Now;
            if (ShouldCaptureEodPeak(state, nowLocal))
            {
                CaptureEodPeak(state, currentEquity, nowLocal);
            }
        }
    }

    private void ResetTrailingState()
    {
        var state = GetTrailingState();
        state.Reset();
        var accountKey = GetAccountKey();
        PersistTrailingStateToMemory(accountKey);
        MarkPersistenceDirty();
        SavePersistenceIfNeeded(force: true);
    }

    #endregion

    #region Private Methods - Trailing DD (EOD peak)

    private bool ShouldCaptureEodPeak(TrailingDrawdownState state, DateTime nowLocal)
    {
        if (PeakUpdateMode != TrailingPeakUpdateMode.EndOfDay)
            return false;

        // Already captured today
        if (state.LastEodPeakCaptureDate.Date == nowLocal.Date)
            return false;

        // Only after configured EOD time
        if (nowLocal.TimeOfDay < TrailingEodTimeLocal)
            return false;

        return true;
    }

    private void CaptureEodPeak(TrailingDrawdownState state, decimal currentEquity, DateTime nowLocal)
    {
        state.PeakEquity = currentEquity;
        state.StopEquity = state.PeakEquity - MaxTrailingDrawdown;
        state.LastEodPeakCaptureDate = nowLocal.Date;
        var accountKey = GetAccountKey();
        PersistTrailingStateToMemory(accountKey);
        MarkPersistenceDirty();
        SavePersistenceIfNeeded(force: true);
    }

    #endregion

    #region Private Methods - Trailing DD (monthly reset)

    private void MaybeResetMonthlyTrailingAtSessionStart(TrailingDrawdownState state, DateTime nowLocal)
    {
        if (!EnableMonthlyReset || MonthlyResetDay <= 0)
            return;

        // Monthly reset is allowed only if the breach already existed before session start.
        if (!state.WasBreachedBeforeSession)
            return;

        int year = nowLocal.Year;
        int month = nowLocal.Month;
        int resetDay = Math.Min(MonthlyResetDay, DateTime.DaysInMonth(year, month));

        if (nowLocal.Day != resetDay)
            return;

        int resetKey = year * 100 + month;
        if (state.LastMonthlyResetKey == resetKey)
            return;

        state.Reset();
        state.LastMonthlyResetKey = resetKey;
        var accountKey = GetAccountKey();
        PersistTrailingStateToMemory(accountKey);
        MarkPersistenceDirty();
        SavePersistenceIfNeeded(force: true);
    }

    #endregion


    #region Private Methods - Trailing DD (session helpers)


    private bool TryGetLastTwoCandles(out IndicatorCandle prev, out IndicatorCandle cur)
    {
        prev = null;
        cur = null;

        if (CurrentBar < 2)
            return false;

        cur = GetCandle(CurrentBar - 1);
        prev = GetCandle(CurrentBar - 2);

        return cur != null && prev != null;
    }

    private TimeSpan GetLocalTimeOfDay(DateTime time)
    {
        // Same approach used in ClusterStatistic: InstrumentInfo.TimeZone as hours offset.
        return time.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
    }

    private DateTime GetLocalDate(DateTime time)
    {
        return time.AddHours(InstrumentInfo.TimeZone).Date;
    }

    private bool IsSessionBoundaryCrossed()
    {
        if (!TryGetLastTwoCandles(out var prev, out var cur))
            return false;

        var boundary = TrailingEodTimeLocal;

        var prevTod = GetLocalTimeOfDay(prev.Time);
        var curTod = GetLocalTimeOfDay(cur.Time);

        return prevTod < boundary && curTod >= boundary;
    }

    private int GetSessionKey()
    {
        // Deterministic session key based on chart candle time (not DateTime.Now)
        if (CurrentBar < 1)
            return 0;

        var cur = GetCandle(CurrentBar - 1);
        if (cur == null)
            return 0;

        var localDate = GetLocalDate(cur.Time);
        return localDate.Year * 10000 + localDate.Month * 100 + localDate.Day;
    }

    #endregion

    #region Private Methods - Daily Rails (session metrics)

    private DailyRailsState GetDailyRailsState(string accountKey)
    {
        var key = accountKey ?? string.Empty;

        if (!_dailyRailsStatesByAccount.TryGetValue(key, out var state))
        {
            state = new DailyRailsState();
            _dailyRailsStatesByAccount[key] = state;
        }

        return state;
    }

    private void MaybeResetDailyRails(DailyRailsState state, Portfolio portfolio)
    {
        if (state == null || portfolio == null)
            return;

        var resetKey = GetDailyResetKey();

        // Not enough data yet (no candles) -> do not reset.
        if (resetKey == 0)
            return;

        if (state.LastDailyResetKey == resetKey)
            return;

        state.ResetForNewDay(resetKey);

        // Baseline for "Realized PnL Today"
        state.DailyRealizedPnlBaseline = portfolio.ClosedPnL;
    }

    private int GetDailyResetKey()
    {
        // Deterministic daily key based on chart candle time (not DateTime.Now, not PC local).
        if (CurrentBar < 1)
            return 0;

        var cur = GetCandle(CurrentBar - 1);
        if (cur == null)
            return 0;

        var localDate = GetLocalDate(cur.Time);
        var localTod = GetLocalTimeOfDay(cur.Time);

        var cutoff = DailyResetMode == DailyResetModeKind.LocalCustomTime
            ? DailyResetTimeLocal
            : new TimeSpan(17, 0, 0);

        // If we're before the cutoff, we still belong to the "previous trading day".
        if (localTod < cutoff)
            localDate = localDate.AddDays(-1);

        return localDate.Year * 10000 + localDate.Month * 100 + localDate.Day;
    }

    private void UpdateDailySessionMetricsFromTradeEvents(DailyRailsState state, Portfolio portfolio)
    {
        if (state == null || portfolio == null)
            return;

        var isOpen = IsActivePositionOpen(portfolio);

        // OPEN event: FLAT -> OPEN
        if (!state.WasPositionOpen && isOpen)
        {
            state.WasPositionOpen = true;
            state.TradeClosedPnlBaseline = portfolio.ClosedPnL;
        }
        // CLOSE event: OPEN -> FLAT
        else if (state.WasPositionOpen && !isOpen)
        {
            state.WasPositionOpen = false;

            var tradePnl = portfolio.ClosedPnL - state.TradeClosedPnlBaseline;

            state.TradesToday++;

            if (tradePnl > 0m)
            {
                state.WinsToday++;
                state.CurrentWinStreak++;
                state.CurrentLossStreak = 0;
            }
            else if (tradePnl < 0m)
            {
                state.LossesToday++;
                state.CurrentLossStreak++;
                state.CurrentWinStreak = 0;
            }
            else
            {
                // Neutral trade: reset both streaks
                state.CurrentWinStreak = 0;
                state.CurrentLossStreak = 0;
            }
        }
    }

    private bool IsActivePositionOpen(Portfolio portfolio)
    {
        var tm = TradingManager;
        var pos = tm?.Position;
        var sec = tm?.Security;

        if (portfolio == null || pos == null || sec == null)
            return false;

        if (!string.Equals(pos.AccountID, portfolio.AccountID, StringComparison.Ordinal))
            return false;

        if (pos.Security == null || !string.Equals(pos.Security.Code, sec.Code, StringComparison.Ordinal))
            return false;

        return pos.IsInPosition && pos.Volume != 0m;
    }

    #endregion

    #region Private Methods - Persistence (path + load/apply)
    private string GetPersistencePath()
    {
        if (!string.IsNullOrEmpty(_persistencePath))
            return _persistencePath;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "ATAS", "JSON");
        Directory.CreateDirectory(dir);

        _persistencePath = Path.Combine(dir, _persistenceFileName);
        return _persistencePath;
    }

    private void LoadPersistenceSafe()
    {
        try
        {
            var path = GetPersistencePath();

            if (!File.Exists(path))
            {
                _persisted = new PersistedRootV1 { SchemaVersion = _persistenceSchemaVersion };
                return;
            }

            var json = File.ReadAllText(path);
            var loaded = System.Text.Json.JsonSerializer.Deserialize<PersistedRootV1>(json);

            if (loaded == null || loaded.SchemaVersion != _persistenceSchemaVersion)
            {
                _persisted = new PersistedRootV1 { SchemaVersion = _persistenceSchemaVersion };
                return;
            }

            _persisted = loaded;
        }
        catch
        {
            // Fail-safe: never break indicator render on persistence issues.
            _persisted = new PersistedRootV1 { SchemaVersion = _persistenceSchemaVersion };
        }
    }

    private void ApplyLoadedTrailingStateIfAny(string accountKey)
    {
        if (_persisted == null)
            return;

        if (!_persisted.Accounts.TryGetValue(accountKey, out var acc))
            return;

        var t = acc.Trailing;
        var state = GetTrailingState();

        state.IsInitialized = t.IsInitialized;
        state.StartEquity = t.StartEquity;
        state.PeakEquity = t.PeakEquity;
        state.StopEquity = t.StopEquity;

        state.LastEodPeakCaptureDate = t.LastEodPeakCaptureDate;
        state.LastMonthlyResetKey = t.LastMonthlyResetKey;

        state.WasBreachedBeforeSession = t.WasBreachedBeforeSession;
        state.LastSessionKey = t.LastSessionKey;
    }

    #endregion

    #region Private Methods - Persistence (atomic save)

    private static string ComputeSha256(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    private static void WriteAllTextAtomic(string path, string content)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var tmp = path + ".tmp";

        File.WriteAllText(tmp, content, Encoding.UTF8);

        if (File.Exists(path))
        {
            // Atomic replace if possible (Windows)
            File.Replace(tmp, path, destinationBackupFileName: null, ignoreMetadataErrors: true);
        }
        else
        {
            File.Move(tmp, path);
        }
    }

    private void PersistTrailingStateToMemory(string accountKey)
    {
        if (_persisted == null)
            LoadPersistenceSafe();

        if (!_persisted.Accounts.TryGetValue(accountKey, out var acc))
        {
            acc = new PersistedAccountV1();
            _persisted.Accounts[accountKey] = acc;
        }

        var state = GetTrailingState();
        var t = acc.Trailing;

        t.IsInitialized = state.IsInitialized;
        t.StartEquity = state.StartEquity;
        t.PeakEquity = state.PeakEquity;
        t.StopEquity = state.StopEquity;

        t.LastEodPeakCaptureDate = state.LastEodPeakCaptureDate;
        t.LastMonthlyResetKey = state.LastMonthlyResetKey;

        t.WasBreachedBeforeSession = state.WasBreachedBeforeSession;
        t.LastSessionKey = state.LastSessionKey;
    }

    private void MarkPersistenceDirty()
    {
        _persistenceDirty = true;
    }

    private void SavePersistenceIfNeeded(bool force = false)
    {
        if (_persisted == null)
            return;

        if (!force)
        {
            if (!_persistenceDirty)
                return;

            var nowUtc = DateTime.UtcNow;
            if (nowUtc - _lastSaveAttemptUtc < _saveThrottle)
                return;

            _lastSaveAttemptUtc = nowUtc;
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(_persisted, options);
            var sha = ComputeSha256(json);

            if (!force && sha == _lastSavedSha256)
            {
                _persistenceDirty = false;
                return;
            }

            var path = GetPersistencePath();
            WriteAllTextAtomic(path, json);

            _lastSavedSha256 = sha;
            _persistenceDirty = false;
        }
        catch
        {
            // Fail-safe: never break indicator render on persistence issues.
            // Keep dirty flag true so we can retry later.
        }
    }

    #endregion

    #endregion
}
