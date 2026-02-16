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

        public bool IsInitialized;

        public decimal StartOfDayEquity;
        public decimal PeakEquityToday;
        public decimal FloorEquityToday;

        // "Now" stop state (psychological): can recover
        public bool IsStop;
        // Latch info: stop was hit at least once today
        public bool WasStopHitToday;
        public string StopReasonsText;

        // Session metrics
        public decimal DailyRealizedPnlBaseline;
        public int TradesToday;
        public int WinsToday;
        public int LossesToday;

        // Single-position trade event tracking (OPEN->FLAT)
        public bool WasPositionOpen;
        public decimal TradeClosedPnlBaseline;

        public int CurrentWinStreak;
        public int CurrentLossStreak;


        public void ResetForNewDay(int dailyResetKey)
        {
            LastDailyResetKey = dailyResetKey;

            IsInitialized = false;

            StartOfDayEquity = 0m;
            PeakEquityToday = 0m;
            FloorEquityToday = 0m;

            IsStop = false;
            WasStopHitToday = false;
            StopReasonsText = string.Empty;

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

    #region class - Soft Recommendations

    private enum SuggestedStatus
    {
        Ok = 0,
        Caution = 1,
        Stop = 2
    }

    private sealed class SuggestionResult
    {
        public SuggestedStatus Status { get; init; }
        public string ReasonsText { get; init; } = string.Empty;
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
        public int LastEodPeakSessionKey;
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
            LastEodPeakSessionKey = 0;
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
        public PersistedDailyRailsV1 DailyRails { get; set; } = new();
        public DateTime? LastTradeProcessedTimeUtc { get; set; }
    }


    private sealed class PersistedDailyRailsV1
    {
        public int LastDailyResetKey { get; set; }

        public bool IsInitialized { get; set; }
        public decimal StartOfDayEquity { get; set; }
        public decimal PeakEquityToday { get; set; }
        public decimal FloorEquityToday { get; set; }

        public bool IsStop { get; set; }
        public bool WasStopHitToday { get; set; }
        public string StopReasonsText { get; set; } = string.Empty;
    }

    private sealed class PersistedTrailingDdV1
    {
        public bool IsInitialized { get; set; }
        public decimal StartEquity { get; set; }
        public decimal PeakEquity { get; set; }
        public decimal StopEquity { get; set; }

        public DateTime LastEodPeakCaptureDate { get; set; }
        public int LastEodPeakSessionKey { get; set; }
        public int LastMonthlyResetKey { get; set; }

        public bool WasBreachedBeforeSession { get; set; }
        public int LastSessionKey { get; set; }
    }

    private sealed class AccountConfigV1
    {
        public int SchemaVersion { get; set; } = 1;

        // Trailing DD
        public bool EnableTrailingDrawdown { get; set; }
        public decimal MaxTrailingDrawdown { get; set; }
        public TrailingInitializationMode TrailingInitMode { get; set; }
        public decimal TrailingManualStopEquity { get; set; }
        public TrailingPeakUpdateMode PeakUpdateMode { get; set; }
        public TimeSpan TrailingEodTimeLocal { get; set; }
        public bool EnableMonthlyReset { get; set; }
        public int MonthlyResetDay { get; set; }

        // Daily reset/session
        public DailyResetModeKind DailyResetMode { get; set; }
        public TimeSpan DailyResetTimeLocal { get; set; }

        // Daily rails (hard caps)
        public bool EnableDailyRails { get; set; }
        public decimal DailyLossLimit { get; set; }
        public decimal DailyProfitTarget { get; set; }
        public decimal DailyMaxDrawdownFromPeak { get; set; }

        // Soft recommendations
        public bool EnableSoftRecommendations { get; set; }
        public int MaxTradesPerDay { get; set; }
        public int MaxConsecutiveLosses { get; set; }
        public int CautionTradesThreshold { get; set; }
        public int CautionLossesThreshold { get; set; }

        public decimal CautionLossFromStart { get; set; }
        public decimal CautionGivebackFromPeak { get; set; }
        public decimal CautionLossFromStartPct { get; set; }
        public decimal CautionGivebackFromPeakPct { get; set; }

        // UI toggles (risk-relevant)
        public bool ShowTradesTodayRow { get; set; }
        public bool ShowWinsLossesTodayRow { get; set; }
        public bool ShowCurrentStreakRow { get; set; }
        public bool ShowRealizedPnlTodayRow { get; set; }
        public bool ShowRemainingDailyLossRow { get; set; }
        public bool ShowRemainingDailyProfitTargetRow { get; set; }
        public bool ShowSuggestedStatusRow { get; set; }
        public bool ShowStopReasonsRow { get; set; }
        public bool ShowPositionSnapshot { get; set; }
        public bool ShowFlatRow { get; set; }
    }

    private sealed class TradeCloseEventV1
    {
        public int SchemaVersion { get; set; } = 1;

        public string AccountKey { get; set; } = string.Empty;
        public int EpochId { get; set; } = 0; // reserved for future "reset epochs"

        public DateTime TimestampUtc { get; set; }
        public DateTime TimestampLocal { get; set; }

        public string SecurityCode { get; set; } = string.Empty;
        public string PortfolioId { get; set; } = string.Empty;

        public OrderDirections? Direction { get; set; }
        public decimal? Quantity { get; set; }

        public decimal RealizedPnL { get; set; }

        public decimal? Balance { get; set; }
        public decimal? Equity { get; set; }

        // Best-effort stable identifier for deduplication in future.
        public string EventId { get; set; } = string.Empty;
    }

    private sealed class DailySnapshotV1
    {
        public int DayKey { get; set; } // yyyymmdd (local instrument time)
        public string AccountKey { get; set; } = string.Empty;

        public DateTime TimestampUtc { get; set; }
        public DateTime TimestampLocal { get; set; }

        // Daily rails summary (as-of end of day)
        public int TradesToday { get; set; }
        public int WinsToday { get; set; }
        public int LossesToday { get; set; }
        public int CurrentWinStreak { get; set; }
        public int CurrentLossStreak { get; set; }

        public decimal RealizedPnlToday { get; set; }

        // Trailing snapshot (as-of end of day)
        public bool TrailingIsInitialized { get; set; }
        public decimal TrailingStartEquity { get; set; }
        public decimal TrailingPeakEquity { get; set; }
        public decimal TrailingStopEquity { get; set; }
        public bool TrailingWasBreachedBeforeSession { get; set; }
    }


    #endregion

    #region class - Position Snapshot

    private sealed class PositionSnapshot
    {
        public bool IsOpen;

        public OrderDirections Direction;
        public decimal Volume;
        public decimal AvgEntryPrice;

        public string AccountId = string.Empty;
        public string SecurityCode = string.Empty;
    }

    #endregion

    #region class - Account context (runtime)

    private sealed class AccountContext
    {
        public readonly TrailingDrawdownState Trailing = new();
        public readonly DailyRailsState Daily = new();
        public readonly PositionSnapshot Position = new();

        // One-time flags per account to apply loaded state safely.
        public bool TrailingLoadedFromDisk;

        public AccountConfigV1 Config;
        public bool ConfigLoadedFromDisk;
        public bool ConfigAppliedToUiOnce;
        public bool ConfigDirty;
        public DateTime LastConfigSaveAttemptUtc = DateTime.MinValue;

        public DateTime? LastTradeProcessedTimeUtc;

        // Per-trade max open PnL (runtime-only, per-account; NOT persisted)
        public bool WasTradePositionOpen;
        public decimal TradeOpenPnlBaseline;
        public decimal TradeMaxOpenPnL;

        public decimal LastTradeMaxOpenPnL;
    }

    #endregion

    #endregion

    #region Fields

    #region Fields - Visualization

    private Color _backgroundColor = Color.FromArgb(200, 20, 25, 35);
	private Color _textColor = Color.FromArgb(220, 220, 220);
	private Color _positiveColor = Color.FromArgb(0, 230, 118);
	private Color _negativeColor = Color.FromArgb(255, 82, 82);
    private Color _cautionColor = Color.FromArgb(255, 193, 7); // amber/yellow, used for CAUTION rows
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

    #region Fields - Accounts

    private readonly Dictionary<string, AccountContext> _accountsByKey = new(StringComparer.Ordinal); 
    private string _activeAccountKey = string.Empty;

    // Prevent config sync loops when applying loaded config into UI properties.
    private bool _suppressConfigSync;

    #endregion

    #region Fields - Persistence

    private const int _persistenceSchemaVersion = 1;
    private const string _persistenceFileName = "AccountInfoDisplay.states.v1.json";
    private string _persistencePath;
    private PersistedRootV1 _persisted; // in-memory loaded snapshot
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

    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.ShowRemainingDailyLossRow),
    Description = nameof(Resources.ShowRemainingDailyLossRowDescription),
    GroupName = nameof(Resources.DailyRails),
    Order = 240)]
    public bool ShowRemainingDailyLossRow { get; set; } = true;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.ShowRemainingDailyProfitTargetRow),
        Description = nameof(Resources.ShowRemainingDailyProfitTargetRowDescription),
        GroupName = nameof(Resources.DailyRails),
        Order = 250)]
    public bool ShowRemainingDailyProfitTargetRow { get; set; } = true;


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

    // Daily rails (Phase D)
    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.EnableDailyRails),
        Description = nameof(Resources.EnableDailyRailsDescription),
        GroupName = nameof(Resources.DailyRails),
        Order = 120)]
    public bool EnableDailyRails { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.DailyLossLimit),
        Description = nameof(Resources.DailyLossLimitDescription),
        GroupName = nameof(Resources.DailyRails),
        Order = 130)]
    public decimal DailyLossLimit { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.DailyProfitTarget),
        Description = nameof(Resources.DailyProfitTargetDescription),
        GroupName = nameof(Resources.DailyRails),
        Order = 140)]
    public decimal DailyProfitTarget { get; set; }

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.DailyMaxDrawdownFromPeak),
        Description = nameof(Resources.DailyMaxDrawdownFromPeakDescription),
        GroupName = nameof(Resources.DailyRails),
        Order = 150)]
    public decimal DailyMaxDrawdownFromPeak { get; set; }


    // -----------------------------
    // Soft Recommendations (Phase E)
    // -----------------------------

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.EnableSoftRecommendations),
        Description = nameof(Resources.EnableSoftRecommendationsDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 300)]
    public bool EnableSoftRecommendations { get; set; } = true;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.MaxTradesPerDay),
        Description = nameof(Resources.MaxTradesPerDayDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 310)]
    [Range(0, 500)]
    public int MaxTradesPerDay { get; set; } = 3;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.MaxConsecutiveLosses),
        Description = nameof(Resources.MaxConsecutiveLossesDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 320)]
    [Range(0, 100)]
    public int MaxConsecutiveLosses { get; set; } = 2;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.CautionTradesThreshold),
        Description = nameof(Resources.CautionTradesThresholdDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 330)]
    [Range(0, 500)]
    public int CautionTradesThreshold { get; set; } = 2;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.CautionLossesThreshold),
        Description = nameof(Resources.CautionLossesThresholdDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 340)]
    [Range(0, 100)]
    public int CautionLossesThreshold { get; set; } = 1;

    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.CautionLossFromStart),
    Description = nameof(Resources.CautionLossFromStartDescription),
    GroupName = nameof(Resources.SoftRecommendations),
    Order = 345)]
    public decimal CautionLossFromStart { get; set; } = 0m;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.CautionGivebackFromPeak),
        Description = nameof(Resources.CautionGivebackFromPeakDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 346)]
    public decimal CautionGivebackFromPeak { get; set; } = 0m;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.CautionLossFromStartPct),
        Description = nameof(Resources.CautionLossFromStartPctDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 347)]
    [Range(0, 100)]
    public decimal CautionLossFromStartPct { get; set; } = 0m;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.CautionGivebackFromPeakPct),
        Description = nameof(Resources.CautionGivebackFromPeakPctDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 348)]
    [Range(0, 100)]
    public decimal CautionGivebackFromPeakPct { get; set; } = 0m;


    // Rows toggles (Phase E)
    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.ShowSuggestedStatusRow),
        Description = nameof(Resources.ShowSuggestedStatusRowDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 350)]
    public bool ShowSuggestedStatusRow { get; set; } = true;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.ShowStopReasonsRow),
        Description = nameof(Resources.ShowStopReasonsRowDescription),
        GroupName = nameof(Resources.SoftRecommendations),
        Order = 360)]
    public bool ShowStopReasonsRow { get; set; } = true;

    // Position snapshot
    [Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.ShowPositionSnapshot),
    Description = nameof(Resources.ShowPositionSnapshotDescription),
    GroupName = nameof(Resources.DailyRails),
    Order = 370)]
    public bool ShowPositionSnapshot { get; set; } = true;

    [Display(
        ResourceType = typeof(Resources),
        Name = nameof(Resources.ShowFlatRow),
        Description = nameof(Resources.ShowFlatRowDescription),
        GroupName = nameof(Resources.DailyRails),
        Order = 380)]
    public bool ShowFlatRow { get; set; } = true;




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

        var accountKey = GetAccountKey(portfolio);
        EnsureActiveAccount(accountKey);

        var ctx = GetOrCreateAccountContext(accountKey);

        LoadAccountConfigSafe(accountKey);

        if (ctx.Config != null && !ctx.ConfigAppliedToUiOnce)
        {
            ApplyAccountConfigToUi(ctx.Config);
            ctx.ConfigAppliedToUiOnce = true;
            RedrawChart();
        }

        SyncActiveAccountConfigFromUi(accountKey);
        SaveAccountConfigIfNeeded(accountKey);

        if (!ctx.TrailingLoadedFromDisk)
        {
            ApplyLoadedTrailingStateIfAny(accountKey);
            ApplyLoadedDailyRailsIfAny(accountKey);
            ApplyLoadedTradeLogCursorIfAny(accountKey);
            ctx.TrailingLoadedFromDisk = true;
        }

        var dailyState = GetDailyRailsState(accountKey);
        MaybeResetDailyRails(dailyState, portfolio);
        UpdateDailySessionMetricsFromTradeEvents(dailyState, portfolio);

        var equity = portfolio.Balance + portfolio.OpenPnL;

        UpdateDailyRailsFromEquity(accountKey, dailyState, equity);
        UpdateTrailingDrawdown(equity);

        UpdatePositionSnapshotFromTradingManager(portfolio);
        UpdateTradeMaxOpenPnlFromTradingManager(portfolio);

        // Build structured rows (Phase 0 replacement)
        var rows = BuildRows(portfolio, equity);
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
        if (!string.IsNullOrEmpty(_activeAccountKey))
            SaveAccountConfigIfNeeded(_activeAccountKey, force: true);

        _currentPortfolio = portfolio;
		RedrawChart();
	}

    #endregion

    #region Private Methods - UI rows (render model)

    private List<DisplayRow> BuildRows(Portfolio portfolio, decimal equity)
    {
        var rows = new List<DisplayRow>();

        TrailingDrawdownState trailingState = null;

        if (portfolio == null)
            return rows;

        var accountKey = GetAccountKey(portfolio);
        var ctx = GetOrCreateAccountContext(accountKey);

        if (EnableTrailingDrawdown && MaxTrailingDrawdown > 0m)
            trailingState = GetTrailingState(accountKey);

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
        {
            rows.Add(new DisplayRow(
                Resources.RowOpenPnL,
                FormatCurrency(portfolio.OpenPnL),
                numericValue: portfolio.OpenPnL
            ));

            // Phase 0011–0012: per-trade max open pnl (current vs last)
            if (ctx.WasTradePositionOpen)
            {
                rows.Add(new DisplayRow(
                    Resources.RowTradeMaxOpenPnLCurrent,
                    FormatCurrency(ctx.TradeMaxOpenPnL),
                    numericValue: ctx.TradeMaxOpenPnL));
            }
            else
            {
                rows.Add(new DisplayRow(
                    Resources.RowTradeMaxOpenPnLLast,
                    FormatCurrency(ctx.LastTradeMaxOpenPnL),
                    numericValue: ctx.LastTradeMaxOpenPnL));
            }
        }

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
        if (ShowTradesTodayRow || ShowWinsLossesTodayRow || ShowCurrentStreakRow || ShowRealizedPnlTodayRow
            || ShowRemainingDailyLossRow || ShowRemainingDailyProfitTargetRow)
        {
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

            // Remaining rails (Phase D - remaining rows)
            if (EnableDailyRails && daily.IsInitialized)
            {
                if (ShowRemainingDailyLossRow && DailyLossLimit > 0m)
                {
                    var floor = daily.StartOfDayEquity - DailyLossLimit;
                    var remainingLoss = equity - floor;
                    if (remainingLoss < 0m)
                        remainingLoss = 0m;

                    rows.Add(new DisplayRow(Resources.RowRemainingDailyLoss, FormatCurrency(remainingLoss), numericValue: remainingLoss));
                }

                if (ShowRemainingDailyProfitTargetRow && DailyProfitTarget > 0m)
                {
                    var target = daily.StartOfDayEquity + DailyProfitTarget;
                    var remainingProfit = target - equity;
                    if (remainingProfit < 0m)
                        remainingProfit = 0m;

                    rows.Add(new DisplayRow(Resources.RowRemainingDailyProfitTarget, FormatCurrency(remainingProfit), numericValue: remainingProfit));
                }
            }

        }

        // -----------------------------
        // Soft Recommendations (Phase E)
        // -----------------------------
        if (EnableSoftRecommendations && (ShowSuggestedStatusRow || ShowStopReasonsRow))
        {
            var daily = GetDailyRailsState(accountKey);

            var suggestion = EvaluateSoftRecommendations(daily, equity);

            if (ShowSuggestedStatusRow)
            {
                var statusText =
                    suggestion.Status == SuggestedStatus.Stop ? Resources.SuggestedStatusStop :
                    suggestion.Status == SuggestedStatus.Caution ? Resources.SuggestedStatusCaution :
                    Resources.SuggestedStatusOk;

                CrossColor? overrideColor =
                    suggestion.Status == SuggestedStatus.Stop ? _negativeColor.Convert() :
                    suggestion.Status == SuggestedStatus.Caution ? _cautionColor.Convert() :
                    _positiveColor.Convert();

                rows.Add(new DisplayRow(Resources.RowSuggestedStatus, statusText, valueColorOverride: overrideColor));
            }

            if (ShowStopReasonsRow && suggestion.Status != SuggestedStatus.Ok)
                rows.Add(new DisplayRow(Resources.RowStopReasons, suggestion.ReasonsText));
        }

        // -----------------------------
        // Position Snapshot (Phase F)
        // -----------------------------
        if (ShowPositionSnapshot)
        {
            var pos = GetOrCreateAccountContext(accountKey).Position;

            if (pos != null && pos.IsOpen)
            {
                rows.Add(new DisplayRow(Resources.RowPosDir, pos.Direction.ToString()));
                rows.Add(new DisplayRow(Resources.RowPosQty, pos.Volume.ToString("N0")));
                rows.Add(new DisplayRow(Resources.RowPosEntry, pos.AvgEntryPrice.ToString("N2")));
            }
            else if (ShowFlatRow)
            {
                rows.Add(new DisplayRow(Resources.RowPos, Resources.PosFlatValue));
            }
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

            // Determine color for value
            var valueColor = _textColor;

            // Explicit per-row override has priority (used by soft recommendations, etc.)
            if (row.ValueColorOverride.HasValue)
            {
                valueColor = row.ValueColorOverride.Value.Convert();
            }
            else if (row.NumericValue.HasValue)
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

    private AccountContext GetOrCreateAccountContext(string accountKey)
    {
        accountKey ??= string.Empty;

        if (!_accountsByKey.TryGetValue(accountKey, out var ctx))
        {
            ctx = new AccountContext();
            _accountsByKey[accountKey] = ctx;
        }

        return ctx;
    }

    private void EnsureActiveAccount(string accountKey)
    {
        if (string.Equals(_activeAccountKey, accountKey, StringComparison.Ordinal))
            return;

        _activeAccountKey = accountKey;
        GetOrCreateAccountContext(_activeAccountKey);
    }


    private string GetAccountKey()
    {
        var portfolio = _currentPortfolio ?? TradingManager?.Portfolio;
        return portfolio?.AccountID ?? "UNKNOWN_ACCOUNT";
    }

    private static string GetAccountKey(Portfolio portfolio)
    {
        return portfolio?.AccountID ?? string.Empty;
    }

    private TrailingDrawdownState GetTrailingState(string accountKey)
    {
        var ctx = GetOrCreateAccountContext(accountKey);
        return ctx.Trailing;
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

        var accountKey = GetAccountKey();
        var state = GetTrailingState(accountKey);

        // Session-start processing (boundary is TrailingEodTimeLocal)
        var sessionKey = GetSessionKey();
        var isSessionStart = IsSessionBoundaryCrossed() && sessionKey != 0 && state.LastSessionKey != sessionKey;


        if (isSessionStart)
        {
            state.LastSessionKey = sessionKey;


            // If we're already below stop at session start, the breach is considered pre-session.
            if (state.IsInitialized && currentEquity <= state.StopEquity)
                state.WasBreachedBeforeSession = true;


            var nowLocal = DateTime.UtcNow.AddHours(InstrumentInfo.TimeZone);
            MaybeResetMonthlyTrailingAtSessionStart(state, nowLocal);
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

                PersistTrailingStateToMemory(accountKey);
                MarkPersistenceDirty();
            }
        }
        else
        {
            // Deterministic single-shot EOD capture: trigger only on boundary crossing (chart time),
            // and only once per sessionKey (account-day).
            if (ShouldCaptureEodPeak(state, sessionKey))
            {
                CaptureEodPeak(state, currentEquity, sessionKey);
            }
        }
    }

    private void UpdateDailyRailsFromEquity(string accountKey, DailyRailsState state, decimal equity)
    {
        if (state == null)
            return;

        if (!EnableDailyRails)
        {
            // Keep state but clear stop flag for UI consistency.
            state.IsStop = false;
            state.WasStopHitToday = false;
            state.StopReasonsText = string.Empty;
            return;
        }

        if (!state.IsInitialized)
        {
            state.IsInitialized = true;
            state.StartOfDayEquity = equity;
            state.PeakEquityToday = equity;
            state.FloorEquityToday = equity;

            state.IsStop = false;
            state.StopReasonsText = string.Empty;

            PersistDailyRailsStateToMemory(accountKey);
            MarkPersistenceDirty();
            SavePersistenceIfNeeded();
            return;
        }

        if (equity > state.PeakEquityToday)
            state.PeakEquityToday = equity;

        if (equity < state.FloorEquityToday)
            state.FloorEquityToday = equity;

        var reasons = new List<string>();

        if (DailyLossLimit > 0m && equity <= state.StartOfDayEquity - DailyLossLimit)
            reasons.Add($"{Resources.DailyLossLimit}: {FormatCurrency(equity)} <= {FormatCurrency(state.StartOfDayEquity - DailyLossLimit)}");

        if (DailyProfitTarget > 0m && equity >= state.StartOfDayEquity + DailyProfitTarget)
            reasons.Add($"{Resources.DailyProfitTarget}: {FormatCurrency(equity)} >= {FormatCurrency(state.StartOfDayEquity + DailyProfitTarget)}");

        if (DailyMaxDrawdownFromPeak > 0m && equity <= state.PeakEquityToday - DailyMaxDrawdownFromPeak)
            reasons.Add($"{Resources.DailyMaxDrawdownFromPeak}: {FormatCurrency(equity)} <= {FormatCurrency(state.PeakEquityToday - DailyMaxDrawdownFromPeak)}");

        var prevWasStopHitToday = state.WasStopHitToday;

        var newIsStop = reasons.Count > 0;
        var newReasons = newIsStop ? string.Join("; ", reasons) : string.Empty;

        if (newIsStop)
            state.WasStopHitToday = true;

        if (state.IsStop != newIsStop
            || !string.Equals(state.StopReasonsText, newReasons, StringComparison.Ordinal)
            || state.WasStopHitToday != prevWasStopHitToday)
        {
            state.IsStop = newIsStop;
            state.StopReasonsText = newReasons;

            PersistDailyRailsStateToMemory(accountKey);
            MarkPersistenceDirty();
            SavePersistenceIfNeeded();
        }
    }


    private void ResetTrailingState()
    {
        var accountKey = GetAccountKey();
        var state = GetTrailingState(accountKey);
        state.Reset();
        PersistTrailingStateToMemory(accountKey);
        MarkPersistenceDirty();
        SavePersistenceIfNeeded(force: true);
    }

    #endregion

    #region Private Methods - Trailing DD (EOD peak)

    private bool ShouldCaptureEodPeak(TrailingDrawdownState state, int sessionKey)
    {
        if (PeakUpdateMode != TrailingPeakUpdateMode.EndOfDay)
            return false;

        // Deterministic trigger based on chart candle times.
        if (!IsSessionBoundaryCrossed())
            return false;

        if (sessionKey == 0)
            return false;

        // Single-shot per account-day (sessionKey).
        if (state.LastEodPeakSessionKey == sessionKey)
            return false;

        return true;
    }

    private void CaptureEodPeak(TrailingDrawdownState state, decimal currentEquity, int sessionKey)
    {
        state.PeakEquity = currentEquity;
        state.StopEquity = state.PeakEquity - MaxTrailingDrawdown;

        // Store a deterministic per-day marker.
        state.LastEodPeakSessionKey = sessionKey;

        // Keep legacy date field coherent (used by UI/diagnostics elsewhere).
        var year = sessionKey / 10000;
        var month = (sessionKey / 100) % 100;
        var day = sessionKey % 100;
        state.LastEodPeakCaptureDate = new DateTime(year, month, day);

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

    private static string BuildTradeEventId(string accountKey, DateTime tsUtc, decimal realizedPnl, string securityCode, decimal? qty)
    {
        // Simple stable id (not cryptographically strong; enough for future dedup checks).
        return $"{accountKey}|{tsUtc:O}|{securityCode}|{realizedPnl}|{qty?.ToString(CultureInfo.InvariantCulture) ?? "null"}";
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
        var ctx = GetOrCreateAccountContext(accountKey);
        return ctx.Daily;
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

        var accountKey = GetAccountKey(portfolio);

        // Before resetting counters, finalize the previous day snapshot (single-shot).
        if (state.LastDailyResetKey != 0)
        {
            WriteDailySnapshotIfMissing(accountKey, state.LastDailyResetKey, portfolio, state);
        }

        state.ResetForNewDay(resetKey);

        // Baseline for "Realized PnL Today"
        state.DailyRealizedPnlBaseline = portfolio.ClosedPnL;

        PersistDailyRailsStateToMemory(accountKey);
        MarkPersistenceDirty();
        SavePersistenceIfNeeded();
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
            // Compute realized PnL delta before mutating baselines/flags.
            var tradePnl = portfolio.ClosedPnL - state.TradeClosedPnlBaseline;

            // Best-effort metadata from the last known position snapshot (still "open" in this render cycle).
            var accountKey = GetAccountKey(portfolio);
            var ctx = GetOrCreateAccountContext(accountKey);
            var snap = ctx.Position;

            OrderDirections? dir = null;
            decimal? qty = null;

            if (snap != null && snap.IsOpen)
            {
                dir = snap.Direction;
                qty = snap.Volume;
            }

            var securityCode = (snap != null && !string.IsNullOrWhiteSpace(snap.SecurityCode))
                ? snap.SecurityCode
                : (TradingManager?.Security?.Code ?? string.Empty);

            // Emit trade-close event (append-only JSONL).
            var processedUtc = portfolio.ProcessedTradeTime;

            if (processedUtc.HasValue)
            {
                // Dedup: never log events that are not strictly newer than the last processed time.
                if (ctx.LastTradeProcessedTimeUtc.HasValue && processedUtc.Value <= ctx.LastTradeProcessedTimeUtc.Value)
                {
                    // Skip duplicated close notifications.
                }
                else
                {
                    ctx.LastTradeProcessedTimeUtc = processedUtc.Value;

                    var evt = CreateTradeCloseEventV1(accountKey, portfolio, securityCode, tradePnl, dir, qty, processedUtc.Value);

                    AppendTradeEventJsonl(accountKey, evt);

                    PersistTradeLogCursorToMemory(accountKey);
                    MarkPersistenceDirty();
                    SavePersistenceIfNeeded();
                }
            }
            else
            {
                // Fallback: derive a deterministic timestamp from the last candle (chart time),
                // and enforce monotonicity vs. the persisted cursor to avoid duplicates on re-renders.
                var candle = GetCandle(CurrentBar - 1);
                var tsUtc = candle?.Time ?? DateTime.UtcNow;

                if (ctx.LastTradeProcessedTimeUtc.HasValue && tsUtc <= ctx.LastTradeProcessedTimeUtc.Value)
                    tsUtc = ctx.LastTradeProcessedTimeUtc.Value.AddTicks(1);

                var evt = CreateTradeCloseEventV1(accountKey, portfolio, securityCode, tradePnl, dir, qty, tsUtc);

                // Dedup: skip events not strictly newer than the cursor.
                if (ctx.LastTradeProcessedTimeUtc.HasValue && evt.TimestampUtc <= ctx.LastTradeProcessedTimeUtc.Value)
                {
                    // Skip duplicated close notifications.
                }
                else
                {
                    ctx.LastTradeProcessedTimeUtc = evt.TimestampUtc;

                    AppendTradeEventJsonl(accountKey, evt);

                    PersistTradeLogCursorToMemory(accountKey);
                    MarkPersistenceDirty();
                    SavePersistenceIfNeeded();
                }
            }


            // Update daily counters.
            state.WasPositionOpen = false;

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

    private SuggestionResult EvaluateSoftRecommendations(DailyRailsState state, decimal equity)
    {
        if (state == null)
            return new SuggestionResult { Status = SuggestedStatus.Ok };

        // Treat "0" as disabled.
        var maxTrades = Math.Max(0, MaxTradesPerDay);
        var maxLosses = Math.Max(0, MaxConsecutiveLosses);

        var cautionTrades = Math.Max(0, CautionTradesThreshold);
        var cautionLosses = Math.Max(0, CautionLossesThreshold);

        var reasons = new List<string>();

        // Daily rails stop (psychological) + latch info
        if (EnableDailyRails)
        {
            // STOP if currently in breach
            if (state.IsStop)
            {
                var txt = string.IsNullOrWhiteSpace(state.StopReasonsText)
                    ? Resources.DailyRailsStopReasonGeneric
                    : state.StopReasonsText;

                return new SuggestionResult
                {
                    Status = SuggestedStatus.Stop,
                    ReasonsText = txt
                };
            }

            // If recovered from STOP, show CAUTION with a deterministic message.
            if (state.WasStopHitToday)
            {
                return new SuggestionResult
                {
                    Status = SuggestedStatus.Caution,
                    ReasonsText = Resources.StopHitEarlierToday
                };
            }
        }

        // STOP conditions (hard recommendation)
        if (maxTrades > 0 && state.TradesToday >= maxTrades)
            reasons.Add($"{Resources.MaxTradesPerDay}: {state.TradesToday:N0} / {maxTrades:N0}");

        if (maxLosses > 0 && state.CurrentLossStreak >= maxLosses)
            reasons.Add($"{Resources.MaxConsecutiveLosses}: {state.CurrentLossStreak:N0} / {maxLosses:N0}");

        if (reasons.Count > 0)
        {
            return new SuggestionResult
            {
                Status = SuggestedStatus.Stop,
                ReasonsText = string.Join("; ", reasons)
            };
        }

        if (state.IsInitialized)
        {
            // Loss from start (amount)
            if (CautionLossFromStart > 0m && equity <= state.StartOfDayEquity - CautionLossFromStart)
                reasons.Add($"{Resources.CautionLossFromStart}: {FormatCurrency(equity)} <= {FormatCurrency(state.StartOfDayEquity - CautionLossFromStart)}");

            // Giveback from peak (amount)
            if (CautionGivebackFromPeak > 0m && equity <= state.PeakEquityToday - CautionGivebackFromPeak)
                reasons.Add($"{Resources.CautionGivebackFromPeak}: {FormatCurrency(equity)} <= {FormatCurrency(state.PeakEquityToday - CautionGivebackFromPeak)}");

            // Loss from start (%)
            if (CautionLossFromStartPct > 0m && state.StartOfDayEquity > 0m)
            {
                var lossPct = (state.StartOfDayEquity - equity) / state.StartOfDayEquity * 100m;
                if (lossPct >= CautionLossFromStartPct)
                    reasons.Add($"{Resources.CautionLossFromStartPct}: {lossPct:N2}% / {CautionLossFromStartPct:N2}%");
            }

            // Giveback from peak (%)
            if (CautionGivebackFromPeakPct > 0m && state.PeakEquityToday > 0m)
            {
                var givebackPct = (state.PeakEquityToday - equity) / state.PeakEquityToday * 100m;
                if (givebackPct >= CautionGivebackFromPeakPct)
                    reasons.Add($"{Resources.CautionGivebackFromPeakPct}: {givebackPct:N2}% / {CautionGivebackFromPeakPct:N2}%");
            }
        }

        // CAUTION conditions (warning recommendation)
        if (cautionTrades > 0 && state.TradesToday >= cautionTrades)
            reasons.Add($"{Resources.CautionTradesThreshold}: {state.TradesToday:N0} / {cautionTrades:N0}");

        if (cautionLosses > 0 && state.CurrentLossStreak >= cautionLosses)
            reasons.Add($"{Resources.CautionLossesThreshold}: {state.CurrentLossStreak:N0} / {cautionLosses:N0}");

        if (reasons.Count > 0)
        {
            return new SuggestionResult
            {
                Status = SuggestedStatus.Caution,
                ReasonsText = string.Join("; ", reasons)
            };
        }

        return new SuggestionResult { Status = SuggestedStatus.Ok };
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
        var state = GetTrailingState(accountKey);

        state.IsInitialized = t.IsInitialized;
        state.StartEquity = t.StartEquity;
        state.PeakEquity = t.PeakEquity;
        state.StopEquity = t.StopEquity;

        state.LastEodPeakCaptureDate = t.LastEodPeakCaptureDate;
        state.LastEodPeakSessionKey = t.LastEodPeakSessionKey;
        state.LastMonthlyResetKey = t.LastMonthlyResetKey;

        state.WasBreachedBeforeSession = t.WasBreachedBeforeSession;
        state.LastSessionKey = t.LastSessionKey;
    }

    private void ApplyLoadedTradeLogCursorIfAny(string accountKey)
    {
        if (_persisted == null)
            return;

        if (!_persisted.Accounts.TryGetValue(accountKey, out var acc))
            return;

        var ctx = GetOrCreateAccountContext(accountKey);
        ctx.LastTradeProcessedTimeUtc = acc.LastTradeProcessedTimeUtc;
    }

    private void ApplyLoadedDailyRailsIfAny(string accountKey)
    {
        if (_persisted == null)
            return;

        if (!_persisted.Accounts.TryGetValue(accountKey, out var acc))
            return;

        var ctx = GetOrCreateAccountContext(accountKey);
        var state = ctx.Daily;

        if (state == null)
            return;

        var p = acc.DailyRails;
        if (p == null)
            return;

        state.LastDailyResetKey = p.LastDailyResetKey;

        state.IsInitialized = p.IsInitialized;
        state.StartOfDayEquity = p.StartOfDayEquity;
        state.PeakEquityToday = p.PeakEquityToday;
        state.FloorEquityToday = p.FloorEquityToday;

        state.IsStop = p.IsStop;
        state.WasStopHitToday = p.WasStopHitToday;
        state.StopReasonsText = p.StopReasonsText ?? string.Empty;
    }

    private string GetDailySnapshotPath(string accountKey, int dayKey)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "ATAS", "JSON");
        Directory.CreateDirectory(dir);

        var safeKey = SanitizeFileComponent(accountKey);
        return Path.Combine(dir, $"AccountInfoDisplay.account.{safeKey}.daily.{dayKey}.v1.json");
    }

    private void WriteDailySnapshotIfMissing(string accountKey, int dayKey, Portfolio portfolio, DailyRailsState dailyState)
    {
        if (portfolio == null || dailyState == null)
            return;

        var path = GetDailySnapshotPath(accountKey, dayKey);

        // Single-shot: do not overwrite an existing daily snapshot.
        if (File.Exists(path))
            return;

        var nowUtc = DateTime.UtcNow;
        var nowLocal = nowUtc.AddHours(InstrumentInfo.TimeZone);

        var trailing = GetTrailingState(accountKey);

        // Realized PnL Today is computed relative to the baseline we store on reset.
        var realizedToday = portfolio.ClosedPnL - dailyState.DailyRealizedPnlBaseline;

        var snap = new DailySnapshotV1
        {
            DayKey = dayKey,
            AccountKey = accountKey,
            TimestampUtc = nowUtc,
            TimestampLocal = nowLocal,

            TradesToday = dailyState.TradesToday,
            WinsToday = dailyState.WinsToday,
            LossesToday = dailyState.LossesToday,
            CurrentWinStreak = dailyState.CurrentWinStreak,
            CurrentLossStreak = dailyState.CurrentLossStreak,
            RealizedPnlToday = realizedToday,

            TrailingIsInitialized = trailing.IsInitialized,
            TrailingStartEquity = trailing.StartEquity,
            TrailingPeakEquity = trailing.PeakEquity,
            TrailingStopEquity = trailing.StopEquity,
            TrailingWasBreachedBeforeSession = trailing.WasBreachedBeforeSession
        };

        var json = JsonSerializer.Serialize(snap, new JsonSerializerOptions { WriteIndented = true });
        WriteAllTextAtomic(path, json);
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

        var state = GetTrailingState(accountKey);
        var t = acc.Trailing;

        t.IsInitialized = state.IsInitialized;
        t.StartEquity = state.StartEquity;
        t.PeakEquity = state.PeakEquity;
        t.StopEquity = state.StopEquity;

        t.LastEodPeakCaptureDate = state.LastEodPeakCaptureDate;
        t.LastEodPeakSessionKey = state.LastEodPeakSessionKey;
        t.LastMonthlyResetKey = state.LastMonthlyResetKey;

        t.WasBreachedBeforeSession = state.WasBreachedBeforeSession;
        t.LastSessionKey = state.LastSessionKey;
    }

    private void PersistDailyRailsStateToMemory(string accountKey)
    {
        if (_persisted == null)
            return;

        if (!_persisted.Accounts.TryGetValue(accountKey, out var acc))
        {
            acc = new PersistedAccountV1();
            _persisted.Accounts[accountKey] = acc;
        }

        var ctx = GetOrCreateAccountContext(accountKey);
        var state = ctx.Daily;

        if (state == null)
            return;

        acc.DailyRails.LastDailyResetKey = state.LastDailyResetKey;

        acc.DailyRails.IsInitialized = state.IsInitialized;
        acc.DailyRails.StartOfDayEquity = state.StartOfDayEquity;
        acc.DailyRails.PeakEquityToday = state.PeakEquityToday;
        acc.DailyRails.FloorEquityToday = state.FloorEquityToday;

        acc.DailyRails.IsStop = state.IsStop;
        acc.DailyRails.WasStopHitToday = state.WasStopHitToday;
        acc.DailyRails.StopReasonsText = state.StopReasonsText ?? string.Empty;
    }


    private void PersistTradeLogCursorToMemory(string accountKey)
    {
        if (_persisted == null)
            LoadPersistenceSafe();

        if (!_persisted.Accounts.TryGetValue(accountKey, out var acc))
        {
            acc = new PersistedAccountV1();
            _persisted.Accounts[accountKey] = acc;
        }

        var ctx = GetOrCreateAccountContext(accountKey);
        acc.LastTradeProcessedTimeUtc = ctx.LastTradeProcessedTimeUtc;
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

    private string GetAccountConfigPath(string accountKey)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "ATAS", "JSON");
        Directory.CreateDirectory(dir);

        var safeKey = SanitizeFileComponent(accountKey);
        return Path.Combine(dir, $"AccountInfoDisplay.account.{safeKey}.config.v1.json");
    }

    private string GetAccountTradesLogPath(string accountKey)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "ATAS", "JSON");
        Directory.CreateDirectory(dir);

        var safeKey = SanitizeFileComponent(accountKey);
        return Path.Combine(dir, $"AccountInfoDisplay.account.{safeKey}.trades.v1.jsonl");
    }


    private static string SanitizeFileComponent(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "UNKNOWN";

        foreach (var c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');

        return value.Trim();
    }

    private AccountConfigV1 CreateDefaultConfigFromCurrentUi()
    {
        return new AccountConfigV1
        {
            EnableTrailingDrawdown = EnableTrailingDrawdown,
            MaxTrailingDrawdown = MaxTrailingDrawdown,
            TrailingInitMode = TrailingInitMode,
            TrailingManualStopEquity = TrailingManualStopEquity,
            PeakUpdateMode = PeakUpdateMode,
            TrailingEodTimeLocal = TrailingEodTimeLocal,
            EnableMonthlyReset = EnableMonthlyReset,
            MonthlyResetDay = MonthlyResetDay,

            DailyResetMode = DailyResetMode,
            DailyResetTimeLocal = DailyResetTimeLocal,

            EnableDailyRails = EnableDailyRails,
            DailyLossLimit = DailyLossLimit,
            DailyProfitTarget = DailyProfitTarget,
            DailyMaxDrawdownFromPeak = DailyMaxDrawdownFromPeak,

            EnableSoftRecommendations = EnableSoftRecommendations,
            MaxTradesPerDay = MaxTradesPerDay,
            MaxConsecutiveLosses = MaxConsecutiveLosses,
            CautionTradesThreshold = CautionTradesThreshold,
            CautionLossesThreshold = CautionLossesThreshold,

            CautionLossFromStart = CautionLossFromStart,
            CautionGivebackFromPeak = CautionGivebackFromPeak,
            CautionLossFromStartPct = CautionLossFromStartPct,
            CautionGivebackFromPeakPct = CautionGivebackFromPeakPct,

            ShowTradesTodayRow = ShowTradesTodayRow,
            ShowWinsLossesTodayRow = ShowWinsLossesTodayRow,
            ShowCurrentStreakRow = ShowCurrentStreakRow,
            ShowRealizedPnlTodayRow = ShowRealizedPnlTodayRow,
            ShowRemainingDailyLossRow = ShowRemainingDailyLossRow,
            ShowRemainingDailyProfitTargetRow = ShowRemainingDailyProfitTargetRow,
            ShowSuggestedStatusRow = ShowSuggestedStatusRow,
            ShowStopReasonsRow = ShowStopReasonsRow,
            ShowPositionSnapshot = ShowPositionSnapshot,
            ShowFlatRow = ShowFlatRow
        };
    }

    private void LoadAccountConfigSafe(string accountKey)
    {
        var ctx = GetOrCreateAccountContext(accountKey);

        if (ctx.ConfigLoadedFromDisk)
            return;

        try
        {
            var path = GetAccountConfigPath(accountKey);

            if (!File.Exists(path))
            {
                ctx.Config = CreateDefaultConfigFromCurrentUi();
                ctx.ConfigLoadedFromDisk = true;
                return;
            }

            var json = File.ReadAllText(path, Encoding.UTF8);
            var loaded = JsonSerializer.Deserialize<AccountConfigV1>(json);

            ctx.Config = loaded ?? CreateDefaultConfigFromCurrentUi();
            ctx.ConfigLoadedFromDisk = true;
        }
        catch
        {
            ctx.Config = CreateDefaultConfigFromCurrentUi();
            ctx.ConfigLoadedFromDisk = true;
        }
    }

    private void SaveAccountConfigIfNeeded(string accountKey, bool force = false)
    {
        var ctx = GetOrCreateAccountContext(accountKey);

        if (!ctx.ConfigDirty || ctx.Config == null)
            return;

        var nowUtc = DateTime.UtcNow;

        if (!force && nowUtc - ctx.LastConfigSaveAttemptUtc < _saveThrottle)
            return;

        ctx.LastConfigSaveAttemptUtc = nowUtc;

        try
        {
            var path = GetAccountConfigPath(accountKey);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(ctx.Config, options);

            // Atomic write (same strategy as states.v1.json)
            WriteAllTextAtomic(path, json);

            ctx.ConfigDirty = false;
        }
        catch
        {
            // Keep dirty=true, retry later. Never break render.
        }
    }

    private void ApplyAccountConfigToUi(AccountConfigV1 cfg)
    {
        if (cfg == null)
            return;

        _suppressConfigSync = true;
        try
        {
            EnableTrailingDrawdown = cfg.EnableTrailingDrawdown;
            MaxTrailingDrawdown = cfg.MaxTrailingDrawdown;
            TrailingInitMode = cfg.TrailingInitMode;
            TrailingManualStopEquity = cfg.TrailingManualStopEquity;
            PeakUpdateMode = cfg.PeakUpdateMode;
            TrailingEodTimeLocal = cfg.TrailingEodTimeLocal;
            EnableMonthlyReset = cfg.EnableMonthlyReset;
            MonthlyResetDay = cfg.MonthlyResetDay;

            DailyResetMode = cfg.DailyResetMode;
            DailyResetTimeLocal = cfg.DailyResetTimeLocal;

            EnableDailyRails = cfg.EnableDailyRails;
            DailyLossLimit = cfg.DailyLossLimit;
            DailyProfitTarget = cfg.DailyProfitTarget;
            DailyMaxDrawdownFromPeak = cfg.DailyMaxDrawdownFromPeak;

            EnableSoftRecommendations = cfg.EnableSoftRecommendations;
            MaxTradesPerDay = cfg.MaxTradesPerDay;
            MaxConsecutiveLosses = cfg.MaxConsecutiveLosses;
            CautionTradesThreshold = cfg.CautionTradesThreshold;
            CautionLossesThreshold = cfg.CautionLossesThreshold;

            CautionLossFromStart = cfg.CautionLossFromStart;
            CautionGivebackFromPeak = cfg.CautionGivebackFromPeak;
            CautionLossFromStartPct = cfg.CautionLossFromStartPct;
            CautionGivebackFromPeakPct = cfg.CautionGivebackFromPeakPct;

            ShowTradesTodayRow = cfg.ShowTradesTodayRow;
            ShowWinsLossesTodayRow = cfg.ShowWinsLossesTodayRow;
            ShowCurrentStreakRow = cfg.ShowCurrentStreakRow;
            ShowRealizedPnlTodayRow = cfg.ShowRealizedPnlTodayRow;
            ShowRemainingDailyLossRow = cfg.ShowRemainingDailyLossRow;
            ShowRemainingDailyProfitTargetRow = cfg.ShowRemainingDailyProfitTargetRow;
            ShowSuggestedStatusRow = cfg.ShowSuggestedStatusRow;
            ShowStopReasonsRow = cfg.ShowStopReasonsRow;
            ShowPositionSnapshot = cfg.ShowPositionSnapshot;
            ShowFlatRow = cfg.ShowFlatRow;
        }
        finally
        {
            _suppressConfigSync = false;
        }
    }

    private void SyncActiveAccountConfigFromUi(string accountKey)
    {
        if (_suppressConfigSync)
            return;

        var ctx = GetOrCreateAccountContext(accountKey);
        var cfg = ctx.Config;

        if (cfg == null)
            return;

        var changed = false;

        if (cfg.EnableTrailingDrawdown != EnableTrailingDrawdown) { cfg.EnableTrailingDrawdown = EnableTrailingDrawdown; changed = true; }
        if (cfg.MaxTrailingDrawdown != MaxTrailingDrawdown) { cfg.MaxTrailingDrawdown = MaxTrailingDrawdown; changed = true; }
        if (cfg.TrailingInitMode != TrailingInitMode) { cfg.TrailingInitMode = TrailingInitMode; changed = true; }
        if (cfg.TrailingManualStopEquity != TrailingManualStopEquity) { cfg.TrailingManualStopEquity = TrailingManualStopEquity; changed = true; }
        if (cfg.PeakUpdateMode != PeakUpdateMode) { cfg.PeakUpdateMode = PeakUpdateMode; changed = true; }
        if (cfg.TrailingEodTimeLocal != TrailingEodTimeLocal) { cfg.TrailingEodTimeLocal = TrailingEodTimeLocal; changed = true; }
        if (cfg.EnableMonthlyReset != EnableMonthlyReset) { cfg.EnableMonthlyReset = EnableMonthlyReset; changed = true; }
        if (cfg.MonthlyResetDay != MonthlyResetDay) { cfg.MonthlyResetDay = MonthlyResetDay; changed = true; }

        if (cfg.DailyResetMode != DailyResetMode) { cfg.DailyResetMode = DailyResetMode; changed = true; }
        if (cfg.DailyResetTimeLocal != DailyResetTimeLocal) { cfg.DailyResetTimeLocal = DailyResetTimeLocal; changed = true; }

        if (cfg.EnableDailyRails != EnableDailyRails) { cfg.EnableDailyRails = EnableDailyRails; changed = true; }
        if (cfg.DailyLossLimit != DailyLossLimit) { cfg.DailyLossLimit = DailyLossLimit; changed = true; }
        if (cfg.DailyProfitTarget != DailyProfitTarget) { cfg.DailyProfitTarget = DailyProfitTarget; changed = true; }
        if (cfg.DailyMaxDrawdownFromPeak != DailyMaxDrawdownFromPeak) { cfg.DailyMaxDrawdownFromPeak = DailyMaxDrawdownFromPeak; changed = true; }

        if (cfg.EnableSoftRecommendations != EnableSoftRecommendations) { cfg.EnableSoftRecommendations = EnableSoftRecommendations; changed = true; }
        if (cfg.MaxTradesPerDay != MaxTradesPerDay) { cfg.MaxTradesPerDay = MaxTradesPerDay; changed = true; }
        if (cfg.MaxConsecutiveLosses != MaxConsecutiveLosses) { cfg.MaxConsecutiveLosses = MaxConsecutiveLosses; changed = true; }
        if (cfg.CautionTradesThreshold != CautionTradesThreshold) { cfg.CautionTradesThreshold = CautionTradesThreshold; changed = true; }
        if (cfg.CautionLossesThreshold != CautionLossesThreshold) { cfg.CautionLossesThreshold = CautionLossesThreshold; changed = true; }

        if (cfg.CautionLossFromStart != CautionLossFromStart) { cfg.CautionLossFromStart = CautionLossFromStart; changed = true; }
        if (cfg.CautionGivebackFromPeak != CautionGivebackFromPeak) { cfg.CautionGivebackFromPeak = CautionGivebackFromPeak; changed = true; }
        if (cfg.CautionLossFromStartPct != CautionLossFromStartPct) { cfg.CautionLossFromStartPct = CautionLossFromStartPct; changed = true; }
        if (cfg.CautionGivebackFromPeakPct != CautionGivebackFromPeakPct) { cfg.CautionGivebackFromPeakPct = CautionGivebackFromPeakPct; changed = true; }


        if (cfg.ShowTradesTodayRow != ShowTradesTodayRow) { cfg.ShowTradesTodayRow = ShowTradesTodayRow; changed = true; }
        if (cfg.ShowWinsLossesTodayRow != ShowWinsLossesTodayRow) { cfg.ShowWinsLossesTodayRow = ShowWinsLossesTodayRow; changed = true; }
        if (cfg.ShowCurrentStreakRow != ShowCurrentStreakRow) { cfg.ShowCurrentStreakRow = ShowCurrentStreakRow; changed = true; }
        if (cfg.ShowRealizedPnlTodayRow != ShowRealizedPnlTodayRow) { cfg.ShowRealizedPnlTodayRow = ShowRealizedPnlTodayRow; changed = true; }
        if (cfg.ShowRemainingDailyLossRow != ShowRemainingDailyLossRow) { cfg.ShowRemainingDailyLossRow = ShowRemainingDailyLossRow; changed = true; }
        if (cfg.ShowRemainingDailyProfitTargetRow != ShowRemainingDailyProfitTargetRow) { cfg.ShowRemainingDailyProfitTargetRow = ShowRemainingDailyProfitTargetRow; changed = true; }
        if (cfg.ShowSuggestedStatusRow != ShowSuggestedStatusRow) { cfg.ShowSuggestedStatusRow = ShowSuggestedStatusRow; changed = true; }
        if (cfg.ShowStopReasonsRow != ShowStopReasonsRow) { cfg.ShowStopReasonsRow = ShowStopReasonsRow; changed = true; }
        if (cfg.ShowPositionSnapshot != ShowPositionSnapshot) { cfg.ShowPositionSnapshot = ShowPositionSnapshot; changed = true; }
        if (cfg.ShowFlatRow != ShowFlatRow) { cfg.ShowFlatRow = ShowFlatRow; changed = true; }

        if (changed)
            ctx.ConfigDirty = true;
    }

    private void AppendTradeEventJsonl(string accountKey, TradeCloseEventV1 evt)
    {
        if (evt == null)
            return;

        try
        {
            var path = GetAccountTradesLogPath(accountKey);

            // Keep JSONL compact but readable enough.
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };

            var json = JsonSerializer.Serialize(evt, options);

            // Append line (newline-terminated).
            File.AppendAllText(path, json + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // Fail-safe: never break indicator render/update.
        }
    }

    private TradeCloseEventV1 CreateTradeCloseEventV1(string accountKey, Portfolio portfolio, string securityCode, decimal realizedPnl, OrderDirections? dir, decimal? qty, DateTime? timestampUtc = null)
    {
        var tsUtc = timestampUtc ?? DateTime.UtcNow;
        var tsLocal = tsUtc.AddHours(InstrumentInfo.TimeZone);

        var balance = portfolio?.Balance;
        var equity = portfolio != null ? (portfolio.Balance + portfolio.OpenPnL) : (decimal?)null;

        var evt = new TradeCloseEventV1
        {
            AccountKey = accountKey,
            TimestampUtc = tsUtc,
            TimestampLocal = tsLocal,
            SecurityCode = securityCode ?? string.Empty,
            PortfolioId = portfolio?.AccountID ?? string.Empty,
            RealizedPnL = realizedPnl,
            Direction = dir,
            Quantity = qty,
            Balance = balance,
            Equity = equity
        };

        evt.EventId = BuildTradeEventId(evt.AccountKey, evt.TimestampUtc, evt.RealizedPnL, evt.SecurityCode, evt.Quantity);

        return evt;
    }






    #endregion

    #region Private Methods - Position snapshot

    private void UpdatePositionSnapshotFromTradingManager(Portfolio portfolio)
    {
        var accountKey = GetAccountKey(portfolio);
        var snap = GetOrCreateAccountContext(accountKey).Position;

        var tm = TradingManager;
        var pos = tm?.Position;
        var sec = tm?.Security;

        if (portfolio == null || pos == null || sec == null)
        {
            snap.IsOpen = false;
            snap.AccountId = portfolio?.AccountID ?? string.Empty;
            snap.SecurityCode = string.Empty;
            return;
        }

        // Defensive: ensure the position belongs to the currently selected portfolio/security
        if (!string.Equals(pos.AccountID, portfolio.AccountID, StringComparison.Ordinal))
        {
            snap.IsOpen = false;
            snap.AccountId = portfolio.AccountID;
            snap.SecurityCode = sec.Code;
            return;
        }

        if (pos.Security == null || !string.Equals(pos.Security.Code, sec.Code, StringComparison.Ordinal))
        {
            snap.IsOpen = false;
            snap.AccountId = portfolio.AccountID;
            snap.SecurityCode = sec.Code;
            return;
        }

        var isOpen = pos.IsInPosition && pos.Volume != 0m;

        if (!isOpen)
        {
            snap.IsOpen = false;
            snap.AccountId = portfolio.AccountID;
            snap.SecurityCode = sec.Code;
            return;
        }

        var dir = pos.Volume > 0m ? OrderDirections.Buy : OrderDirections.Sell;

        snap.IsOpen = true;
        snap.Direction = dir;
        snap.Volume = Math.Abs(pos.Volume);
        snap.AvgEntryPrice = pos.AveragePrice;
        snap.AccountId = portfolio.AccountID;
        snap.SecurityCode = sec.Code;
    }

    private void UpdateTradeMaxOpenPnlFromTradingManager(Portfolio portfolio)
    {
        if (portfolio == null)
            return;

        var accountKey = GetAccountKey(portfolio);
        var ctx = GetOrCreateAccountContext(accountKey);

        var isOpen = IsActivePositionOpen(portfolio);

        // OPEN event: FLAT -> OPEN
        if (!ctx.WasTradePositionOpen && isOpen)
        {
            ctx.TradeOpenPnlBaseline = portfolio.OpenPnL; // baseline at entry
            ctx.TradeMaxOpenPnL = 0m;                     // current trade starts at 0
            ctx.WasTradePositionOpen = true;
            return;
        }

        // CLOSE event: OPEN -> FLAT
        if (ctx.WasTradePositionOpen && !isOpen)
        {
            ctx.LastTradeMaxOpenPnL = ctx.TradeMaxOpenPnL;

            ctx.TradeOpenPnlBaseline = 0m;
            ctx.TradeMaxOpenPnL = 0m;
            ctx.WasTradePositionOpen = false;
            return;
        }

        // UPDATE while OPEN
        if (ctx.WasTradePositionOpen && isOpen)
        {
            var tradeOpenPnl = portfolio.OpenPnL - ctx.TradeOpenPnlBaseline;
            if (tradeOpenPnl > ctx.TradeMaxOpenPnL)
                ctx.TradeMaxOpenPnL = tradeOpenPnl;
        }

        // UPDATE while FLAT: do nothing (LastTradeMaxOpenPnL stays visible)
    }

    #endregion

    #endregion
}
