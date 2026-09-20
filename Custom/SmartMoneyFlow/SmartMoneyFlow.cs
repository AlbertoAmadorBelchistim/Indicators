namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using ATAS.Indicators.Drawing;

using OFT.Attributes.Editors;

using Utils.Common;
using Utils.Common.Logging;

[DisplayName("Smart Money Flow")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(SmartMoneyFlowResources), Description = nameof(SmartMoneyFlowResources.SmartMoneyFlow_Description))]
public class SmartMoneyFlow : Indicator
{
	#region Nested Types

	public enum SessionMode
	{
		// The chart's default session.
		[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SessionMode_Default))]
		Default,

		// Sessions that start every day at CustomSessionStart, in chart time.
		[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SessionMode_Custom))]
		Custom,

		// No restart: the lines accumulate from the first calculated bar.
		[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SessionMode_Continuous))]
		Continuous
	}

	public enum ViewMode
	{
		// The five filter lines.
		[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.ViewMode_Filters))]
		Filters,

		// The spread histogram.
		[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.ViewMode_Spread))]
		Spread
	}

	// Side of the spread a filter counts on.
	public enum FilterRole
	{
		[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterRole_None))]
		None,

		[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterRole_Smart))]
		Smart,

		[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterRole_Dumb))]
		Dumb
	}

	#endregion

	#region Fields

	// Number of trade-size filters.
	private const int FilterCount = 5;

	// Number of most recent bars written to the log after the history (detailed log).
	private const int LoggedHistoryBars = 20;

	// One cumulative delta line per trade-size filter. Filter 1 replaces the default series.
	private readonly ValueDataSeries[] _filterSeries = new ValueDataSeries[FilterCount];

	// Smart money minus dumb money, drawn as a histogram.
	private readonly ValueDataSeries _spreadSeries = new("SpreadSeries", SmartMoneyFlowResources.Series_Spread)
	{
		VisualType = VisualMode.Hide,
		Width = 3,
		ShowZeroValue = true,
		UseMinimizedModeIfEnabled = true
	};

	// Simple moving average of the spread.
	private readonly ValueDataSeries _signalSeries = new("SignalSeries", SmartMoneyFlowResources.Series_Signal)
	{
		Color = CrossColor.FromArgb(255, 255, 255, 255),
		VisualType = VisualMode.Hide,
		Width = 2,
		UseMinimizedModeIfEnabled = true
	};

	private ViewMode _viewMode = ViewMode.Filters;

	// Cumulative trades (default) or individual ticks.
	private bool _cumulativeTrades = true;

	// Number of sessions calculated, counting the current one; 0 = every loaded bar.
	private int _sessions = 1;
	private SessionMode _sessionMode = SessionMode.Default;
	private TimeSpan _customSessionStart = new(15, 30, 0);

	// Trade-size range of each filter: a trade matches when Min <= volume <= Max; Max 0 means no maximum.
	// Defaults are the MultiMarketPower ranges.
	private readonly decimal[] _minVolume = { 0, 6, 11, 21, 41 };
	private readonly decimal[] _maxVolume = { 5, 10, 20, 40, 0 };
	private readonly bool[] _useFilter = { true, true, true, true, true };

	// Spread = sum of the smart money filters - sum of the dumb money filters.
	// Default: the two largest sizes against the two smallest.
	private readonly FilterRole[] _role = { FilterRole.Dumb, FilterRole.Dumb, FilterRole.None, FilterRole.Smart, FilterRole.Smart };

	// Spread histogram colors, by sign.
	private CrossColor _spreadPositiveColor = CrossColor.FromArgb(255, 0, 255, 0);
	private CrossColor _spreadNegativeColor = CrossColor.FromArgb(255, 255, 0, 0);

	// Signal: simple moving average of the spread within the session.
	private bool _showSignalLine = true;
	private int _signalPeriod = 14;

	// Four-color spread: sign of the spread and direction of the signal.
	private bool _useFourColors;
	private CrossColor _positiveRisingColor = CrossColor.FromArgb(255, 0, 200, 0);
	private CrossColor _positiveFallingColor = CrossColor.FromArgb(255, 255, 215, 0);
	private CrossColor _negativeRisingColor = CrossColor.FromArgb(255, 65, 105, 225);
	private CrossColor _negativeFallingColor = CrossColor.FromArgb(255, 220, 20, 60);

	private bool _alertOnZeroCross;
	private bool _alertOnSignalCross;
	private string _alertFile = "alert2";
	private int _alertCooldownSeconds = 60;

	private bool _detailedLog;

	// Guards the calculation state: the history response, realtime trades and the
	// calculation thread can all update it.
	private readonly object _calcLock = new();

	// Running signed volume (buy +, sell -) of each filter since the first calculated bar.
	private readonly decimal[] _delta = new decimal[FilterCount];

	// First bar of the calculation and last bar written.
	private int _firstBar;
	private int _lastBar = -1;

	// Whether each written bar starts a session: the lines restart from 0 there.
	private readonly List<bool> _sessionStart = new();

	// Id of the pending cumulative trades request; responses to older requests are ignored.
	private int _requestId;

	// False from a recalculation until the history response has been calculated and the
	// trades received meanwhile have been replayed. Until then realtime trades are buffered.
	private bool _historyReady;

	// Realtime cumulative trade events received while the history is pending, in arrival order.
	// Each entry is a copy taken when the event arrived; updates are kept as separate events.
	private readonly List<(CumulativeTrade Trade, bool IsUpdate)> _pendingTrades = new();

	// Realtime ticks received while the history is pending (tick mode).
	private readonly List<MarketDataArg> _pendingTicks = new();

	// Time of the last trade in the history response, and the response trades counted at that
	// time with their bar. Buffered trades before it, or equal to one of those, are already counted
	// in the history. Several trades can share the last timestamp and the response does not say
	// which one is still aggregating, so an update of any of them replaces its volume.
	private DateTime _historyEndTime;
	private readonly List<(CumulativeTrade Trade, int Bar)> _boundaryTrades = new();

	// Last trade counted and the bar it was counted in. A cumulative trade keeps growing while
	// it aggregates, and its updates replace the volume counted for it.
	private CumulativeTrade _lastTrade;
	private int _lastTradeBar;

	// Tick mode: number of response ticks with the last response time. Ticks carry no identity,
	// so the buffered ticks with that time are matched by count: the response ticks at that time
	// that the buffer does not have were received before the recalculation.
	private int _boundaryTickCount;

	// Tick mode: time of the last realtime tick processed and how many ticks had that time, and
	// the same values when the last recalculation started buffering.
	private DateTime _lastTickTime;
	private int _lastTickCount;
	private DateTime _bufferStartTickTime;
	private int _bufferStartTickCount;

	// Realtime updates that did not match the last trade and were counted as new trades.
	private int _orphanUpdates;

	// Last non-zero sign seen in realtime of the spread and of spread - signal; 0 until the first
	// observation after a recalculation or a new session, which never alerts.
	private int _spreadSign;
	private int _signalSign;
	private DateTime _lastZeroAlertUtc = DateTime.MinValue;
	private DateTime _lastSignalAlertUtc = DateTime.MinValue;

	// Alerts produced under _calcLock and raised after it is released.
	private readonly List<(string Message, bool Positive)> _pendingAlerts = new();

	#endregion

	#region Properties

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.View_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_View), Order = 1,
		Description = nameof(SmartMoneyFlowResources.View_Description))]
	public ViewMode View
	{
		get => _viewMode;
		set
		{
			if (_viewMode == value)
				return;

			_viewMode = value;
			UpdateVisibility();
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.CumulativeTrades_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Calculation), Order = 10,
		Description = nameof(SmartMoneyFlowResources.CumulativeTrades_Description))]
	public bool CumulativeTrades
	{
		get => _cumulativeTrades;
		set
		{
			if (_cumulativeTrades == value)
				return;

			_cumulativeTrades = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Properties: Session

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SessionType_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Session), Order = 20,
		Description = nameof(SmartMoneyFlowResources.SessionType_Description))]
	public SessionMode SessionType
	{
		get => _sessionMode;
		set
		{
			if (_sessionMode == value)
				return;

			_sessionMode = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.CustomSessionStart_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Session), Order = 25,
		Description = nameof(SmartMoneyFlowResources.CustomSessionStart_Description))]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public TimeSpan CustomSessionStart
	{
		get => _customSessionStart;
		set
		{
			if (_customSessionStart == value)
				return;

			_customSessionStart = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.Sessions_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Session), Order = 30,
		Description = nameof(SmartMoneyFlowResources.Sessions_Description))]
	[Range(0, 1000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int Sessions
	{
		get => _sessions;
		set
		{
			if (_sessions == value)
				return;

			_sessions = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Properties: Filters

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterEnabled_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter1), Order = 100,
		Description = nameof(SmartMoneyFlowResources.FilterEnabled_Description))]
	public bool UseFilter1
	{
		get => _useFilter[0];
		set => SetUseFilter(0, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMinVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter1), Order = 110,
		Description = nameof(SmartMoneyFlowResources.FilterMinVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume1
	{
		get => _minVolume[0];
		set => SetVolumeRange(0, value, _maxVolume[0]);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMaxVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter1), Order = 120,
		Description = nameof(SmartMoneyFlowResources.FilterMaxVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume1
	{
		get => _maxVolume[0];
		set => SetVolumeRange(0, _minVolume[0], value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterSpreadRole_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter1), Order = 130,
		Description = nameof(SmartMoneyFlowResources.FilterSpreadRole_Description))]
	public FilterRole Role1
	{
		get => _role[0];
		set => SetRole(0, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter1), Order = 140,
		Description = nameof(SmartMoneyFlowResources.FilterColor_Description))]
	public CrossColor Color1
	{
		get => _filterSeries[0].Color;
		set => _filterSeries[0].Color = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterLineWidth_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter1), Order = 150,
		Description = nameof(SmartMoneyFlowResources.FilterLineWidth_Description))]
	[Range(1, 20)]
	public int LineWidth1
	{
		get => _filterSeries[0].Width;
		set => _filterSeries[0].Width = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterEnabled_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter2), Order = 200,
		Description = nameof(SmartMoneyFlowResources.FilterEnabled_Description))]
	public bool UseFilter2
	{
		get => _useFilter[1];
		set => SetUseFilter(1, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMinVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter2), Order = 210,
		Description = nameof(SmartMoneyFlowResources.FilterMinVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume2
	{
		get => _minVolume[1];
		set => SetVolumeRange(1, value, _maxVolume[1]);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMaxVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter2), Order = 220,
		Description = nameof(SmartMoneyFlowResources.FilterMaxVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume2
	{
		get => _maxVolume[1];
		set => SetVolumeRange(1, _minVolume[1], value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterSpreadRole_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter2), Order = 230,
		Description = nameof(SmartMoneyFlowResources.FilterSpreadRole_Description))]
	public FilterRole Role2
	{
		get => _role[1];
		set => SetRole(1, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter2), Order = 240,
		Description = nameof(SmartMoneyFlowResources.FilterColor_Description))]
	public CrossColor Color2
	{
		get => _filterSeries[1].Color;
		set => _filterSeries[1].Color = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterLineWidth_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter2), Order = 250,
		Description = nameof(SmartMoneyFlowResources.FilterLineWidth_Description))]
	[Range(1, 20)]
	public int LineWidth2
	{
		get => _filterSeries[1].Width;
		set => _filterSeries[1].Width = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterEnabled_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter3), Order = 300,
		Description = nameof(SmartMoneyFlowResources.FilterEnabled_Description))]
	public bool UseFilter3
	{
		get => _useFilter[2];
		set => SetUseFilter(2, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMinVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter3), Order = 310,
		Description = nameof(SmartMoneyFlowResources.FilterMinVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume3
	{
		get => _minVolume[2];
		set => SetVolumeRange(2, value, _maxVolume[2]);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMaxVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter3), Order = 320,
		Description = nameof(SmartMoneyFlowResources.FilterMaxVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume3
	{
		get => _maxVolume[2];
		set => SetVolumeRange(2, _minVolume[2], value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterSpreadRole_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter3), Order = 330,
		Description = nameof(SmartMoneyFlowResources.FilterSpreadRole_Description))]
	public FilterRole Role3
	{
		get => _role[2];
		set => SetRole(2, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter3), Order = 340,
		Description = nameof(SmartMoneyFlowResources.FilterColor_Description))]
	public CrossColor Color3
	{
		get => _filterSeries[2].Color;
		set => _filterSeries[2].Color = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterLineWidth_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter3), Order = 350,
		Description = nameof(SmartMoneyFlowResources.FilterLineWidth_Description))]
	[Range(1, 20)]
	public int LineWidth3
	{
		get => _filterSeries[2].Width;
		set => _filterSeries[2].Width = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterEnabled_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter4), Order = 400,
		Description = nameof(SmartMoneyFlowResources.FilterEnabled_Description))]
	public bool UseFilter4
	{
		get => _useFilter[3];
		set => SetUseFilter(3, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMinVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter4), Order = 410,
		Description = nameof(SmartMoneyFlowResources.FilterMinVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume4
	{
		get => _minVolume[3];
		set => SetVolumeRange(3, value, _maxVolume[3]);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMaxVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter4), Order = 420,
		Description = nameof(SmartMoneyFlowResources.FilterMaxVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume4
	{
		get => _maxVolume[3];
		set => SetVolumeRange(3, _minVolume[3], value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterSpreadRole_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter4), Order = 430,
		Description = nameof(SmartMoneyFlowResources.FilterSpreadRole_Description))]
	public FilterRole Role4
	{
		get => _role[3];
		set => SetRole(3, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter4), Order = 440,
		Description = nameof(SmartMoneyFlowResources.FilterColor_Description))]
	public CrossColor Color4
	{
		get => _filterSeries[3].Color;
		set => _filterSeries[3].Color = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterLineWidth_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter4), Order = 450,
		Description = nameof(SmartMoneyFlowResources.FilterLineWidth_Description))]
	[Range(1, 20)]
	public int LineWidth4
	{
		get => _filterSeries[3].Width;
		set => _filterSeries[3].Width = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterEnabled_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter5), Order = 500,
		Description = nameof(SmartMoneyFlowResources.FilterEnabled_Description))]
	public bool UseFilter5
	{
		get => _useFilter[4];
		set => SetUseFilter(4, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMinVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter5), Order = 510,
		Description = nameof(SmartMoneyFlowResources.FilterMinVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume5
	{
		get => _minVolume[4];
		set => SetVolumeRange(4, value, _maxVolume[4]);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterMaxVolume_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter5), Order = 520,
		Description = nameof(SmartMoneyFlowResources.FilterMaxVolume_Description))]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume5
	{
		get => _maxVolume[4];
		set => SetVolumeRange(4, _minVolume[4], value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterSpreadRole_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter5), Order = 530,
		Description = nameof(SmartMoneyFlowResources.FilterSpreadRole_Description))]
	public FilterRole Role5
	{
		get => _role[4];
		set => SetRole(4, value);
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter5), Order = 540,
		Description = nameof(SmartMoneyFlowResources.FilterColor_Description))]
	public CrossColor Color5
	{
		get => _filterSeries[4].Color;
		set => _filterSeries[4].Color = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.FilterLineWidth_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Filter5), Order = 550,
		Description = nameof(SmartMoneyFlowResources.FilterLineWidth_Description))]
	[Range(1, 20)]
	public int LineWidth5
	{
		get => _filterSeries[4].Width;
		set => _filterSeries[4].Width = value;
	}

	#endregion

	#region Properties: Spread

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SpreadPositiveColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1010,
		Description = nameof(SmartMoneyFlowResources.SpreadPositiveColor_Description))]
	public CrossColor SpreadPositiveColor
	{
		get => _spreadPositiveColor;
		set
		{
			_spreadPositiveColor = value;
			RefreshSpreadColors();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SpreadNegativeColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1020,
		Description = nameof(SmartMoneyFlowResources.SpreadNegativeColor_Description))]
	public CrossColor SpreadNegativeColor
	{
		get => _spreadNegativeColor;
		set
		{
			_spreadNegativeColor = value;
			RefreshSpreadColors();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.ShowSignalLine_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1030,
		Description = nameof(SmartMoneyFlowResources.ShowSignalLine_Description))]
	public bool ShowSignalLine
	{
		get => _showSignalLine;
		set
		{
			_showSignalLine = value;
			UpdateVisibility();
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SignalPeriod_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1040,
		Description = nameof(SmartMoneyFlowResources.SignalPeriod_Description))]
	[Range(2, 500)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int SignalPeriod
	{
		get => _signalPeriod;
		set
		{
			if (_signalPeriod == value)
				return;

			_signalPeriod = value;
			RefreshSpreadColors();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SignalColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1050,
		Description = nameof(SmartMoneyFlowResources.SignalColor_Description))]
	public CrossColor SignalColor
	{
		get => _signalSeries.Color;
		set => _signalSeries.Color = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.SignalWidth_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1060,
		Description = nameof(SmartMoneyFlowResources.SignalWidth_Description))]
	[Range(1, 20)]
	public int SignalWidth
	{
		get => _signalSeries.Width;
		set => _signalSeries.Width = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.UseFourColors_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1070,
		Description = nameof(SmartMoneyFlowResources.UseFourColors_Description))]
	public bool UseFourColors
	{
		get => _useFourColors;
		set
		{
			_useFourColors = value;
			RefreshSpreadColors();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.PositiveRisingColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1080,
		Description = nameof(SmartMoneyFlowResources.PositiveRisingColor_Description))]
	public CrossColor PositiveRisingColor
	{
		get => _positiveRisingColor;
		set
		{
			_positiveRisingColor = value;
			RefreshSpreadColors();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.PositiveFallingColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1090,
		Description = nameof(SmartMoneyFlowResources.PositiveFallingColor_Description))]
	public CrossColor PositiveFallingColor
	{
		get => _positiveFallingColor;
		set
		{
			_positiveFallingColor = value;
			RefreshSpreadColors();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.NegativeRisingColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1100,
		Description = nameof(SmartMoneyFlowResources.NegativeRisingColor_Description))]
	public CrossColor NegativeRisingColor
	{
		get => _negativeRisingColor;
		set
		{
			_negativeRisingColor = value;
			RefreshSpreadColors();
		}
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.NegativeFallingColor_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Spread), Order = 1110,
		Description = nameof(SmartMoneyFlowResources.NegativeFallingColor_Description))]
	public CrossColor NegativeFallingColor
	{
		get => _negativeFallingColor;
		set
		{
			_negativeFallingColor = value;
			RefreshSpreadColors();
		}
	}

	#endregion

	#region Properties: Alerts

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.AlertOnZeroCross_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Alerts), Order = 2010,
		Description = nameof(SmartMoneyFlowResources.AlertOnZeroCross_Description))]
	public bool AlertOnZeroCross
	{
		get => _alertOnZeroCross;
		set => _alertOnZeroCross = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.AlertOnSignalCross_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Alerts), Order = 2020,
		Description = nameof(SmartMoneyFlowResources.AlertOnSignalCross_Description))]
	public bool AlertOnSignalCross
	{
		get => _alertOnSignalCross;
		set => _alertOnSignalCross = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.AlertFile_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Alerts), Order = 2030,
		Description = nameof(SmartMoneyFlowResources.AlertFile_Description))]
	public string AlertFile
	{
		get => _alertFile;
		set => _alertFile = value;
	}

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.AlertCooldownSeconds_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Alerts), Order = 2040,
		Description = nameof(SmartMoneyFlowResources.AlertCooldownSeconds_Description))]
	[Range(0, 3600)]
	public int AlertCooldownSeconds
	{
		get => _alertCooldownSeconds;
		set => _alertCooldownSeconds = value;
	}

	#endregion

	#region Properties: Diagnostics

	[Display(ResourceType = typeof(SmartMoneyFlowResources), Name = nameof(SmartMoneyFlowResources.DetailedLog_DisplayName),
		GroupName = nameof(SmartMoneyFlowResources.Group_Diagnostics), Order = 3010,
		Description = nameof(SmartMoneyFlowResources.DetailedLog_Description))]
	public bool DetailedLog
	{
		get => _detailedLog;
		set => _detailedLog = value;
	}

	#endregion

	#region Ctor

	public SmartMoneyFlow()
		: base(true)
	{
		Panel = IndicatorDataProvider.NewPanel;
		DenyToChangePanel = true;

		// Default line colors and widths grow with the trade size: gray, cyan, royal blue, orange, firebrick.
		var colors = new[]
		{
			CrossColor.FromArgb(255, 128, 128, 128),
			CrossColor.FromArgb(255, 0, 255, 255),
			CrossColor.FromArgb(255, 65, 105, 225),
			CrossColor.FromArgb(255, 255, 165, 0),
			CrossColor.FromArgb(255, 178, 34, 34)
		};
		var widths = new[] { 1, 2, 2, 3, 4 };

		var names = new[]
		{
			SmartMoneyFlowResources.Series_Filter1,
			SmartMoneyFlowResources.Series_Filter2,
			SmartMoneyFlowResources.Series_Filter3,
			SmartMoneyFlowResources.Series_Filter4,
			SmartMoneyFlowResources.Series_Filter5
		};

		for (var i = 0; i < FilterCount; i++)
		{
			_filterSeries[i] = new ValueDataSeries($"Filter{i + 1}Series", names[i])
			{
				Color = colors[i],
				Width = widths[i],
				IsHidden = true,
				ShowZeroValue = false,
				UseMinimizedModeIfEnabled = true
			};
		}

		DataSeries[0] = _filterSeries[0];

		for (var i = 1; i < FilterCount; i++)
			DataSeries.Add(_filterSeries[i]);

		DataSeries.Add(_spreadSeries);
		DataSeries.Add(_signalSeries);

		UpdateVisibility();
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"SmartMoneyFlow: initialized ({typeof(SmartMoneyFlow).Assembly.GetName().Version}).");
	}

	protected override void OnRecalculate()
	{
		lock (_calcLock)
		{
			// A response to a request made before this recalculation is stale.
			_requestId = 0;
			_lastBar = -1;
			Array.Clear(_delta);
			_sessionStart.Clear();
			_historyReady = false;
			_pendingTrades.Clear();
			_pendingTicks.Clear();
			_lastTrade = null;
			_bufferStartTickTime = _lastTickTime;
			_bufferStartTickCount = _lastTickCount;
			_spreadSign = 0;
			_signalSign = 0;
			_pendingAlerts.Clear();
			_orphanUpdates = 0;
		}
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		if (bar != CurrentBar - 1)
			return;

		// A new realtime bar starts with the running values of the previous one, before its first trade.
		lock (_calcLock)
		{
			if (_historyReady)
				EnsureBar(bar);
		}
	}

	protected override void OnFinishRecalculate()
	{
		if (CurrentBar == 0)
			return;

		_firstBar = FindFirstBar();

		// From the first calculated bar to the last trade of the forming bar.
		var request = new CumulativeTradesRequest(GetCandle(_firstBar).Time, GetCandle(CurrentBar - 1).LastTime, 0, 0);

		lock (_calcLock)
			_requestId = request.RequestId;

		this.LogInfo($"SmartMoneyFlow: requesting cumulative trades from {request.BeginTime:yyyy-MM-dd HH:mm:ss} " +
			$"to {request.EndTime:yyyy-MM-dd HH:mm:ss} (bars {_firstBar}-{CurrentBar - 1}).");

		RequestForCumulativeTrades(request);
	}

	protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
	{
		lock (_calcLock)
		{
			if (request.RequestId != _requestId)
				return;
		}

		CalculateHistory(cumulativeTrades);
		ReplayPendingTrades();
	}

	protected override void OnCumulativeTrade(CumulativeTrade trade)
	{
		if (_cumulativeTrades)
			OnRealtimeTrade(trade, false);
	}

	protected override void OnUpdateCumulativeTrade(CumulativeTrade trade)
	{
		if (_cumulativeTrades)
			OnRealtimeTrade(trade, true);
	}

	protected override void OnNewTrade(MarketDataArg trade)
	{
		if (_cumulativeTrades)
			return;

		int bar;

		lock (_calcLock)
		{
			if (!_historyReady)
			{
				_pendingTicks.Add(trade);
				return;
			}

			bar = ProcessTick(trade);
			CheckCrosses();
		}

		if (bar >= 0)
			RaiseBarValueChanged(bar);

		RaisePendingAlerts();
	}

	#endregion

	#region Private Methods

	private void SetUseFilter(int filter, bool value)
	{
		_useFilter[filter] = value;
		UpdateVisibility();
		RedrawChart();
	}

	private void SetVolumeRange(int filter, decimal min, decimal max)
	{
		if (_minVolume[filter] == min && _maxVolume[filter] == max)
			return;

		_minVolume[filter] = min;
		_maxVolume[filter] = max;
		RecalculateValues();
	}

	private void SetRole(int filter, FilterRole role)
	{
		if (_role[filter] == role)
			return;

		_role[filter] = role;

		// The spread is derived from the filter lines: no recalculation needed.
		lock (_calcLock)
			UpdateSpread(_firstBar, _lastBar);

		RedrawChart();
	}

	private void RefreshSpreadColors()
	{
		lock (_calcLock)
			UpdateSpread(_firstBar, _lastBar);

		RedrawChart();
	}

	// Filters view: the enabled filter lines. Spread view: the spread histogram only.
	private void UpdateVisibility()
	{
		var filters = _viewMode == ViewMode.Filters;

		for (var i = 0; i < FilterCount; i++)
			_filterSeries[i].VisualType = filters && _useFilter[i] ? VisualMode.Line : VisualMode.Hide;

		_spreadSeries.VisualType = filters ? VisualMode.Hide : VisualMode.Histogram;
		_signalSeries.VisualType = !filters && _showSignalLine ? VisualMode.Line : VisualMode.Hide;
	}

	// Called under _calcLock. Recalculates the spread, the signal and the spread color from the
	// filter lines for the bars from..to. The signal of a bar depends on the spread of the previous
	// bars, and its color on the previous signal, so a range is always updated in ascending order
	// up to the last bar that depends on the change.
	private void UpdateSpread(int from, int to)
	{
		from = Math.Max(from, _firstBar);

		for (var bar = from; bar <= to; bar++)
		{
			decimal spread = 0;

			for (var i = 0; i < FilterCount; i++)
			{
				if (_role[i] == FilterRole.Smart)
					spread += _filterSeries[i][bar];
				else if (_role[i] == FilterRole.Dumb)
					spread -= _filterSeries[i][bar];
			}

			_spreadSeries[bar] = spread;
		}

		for (var bar = from; bar <= to; bar++)
		{
			_signalSeries[bar] = Signal(bar);
			_spreadSeries.Colors[bar] = SpreadColor(bar).Convert();
		}
	}

	// Called under _calcLock. Simple moving average of the spread over the last SignalPeriod bars,
	// without going back past the start of the session: the signal restarts with the lines.
	private decimal Signal(int bar)
	{
		decimal sum = 0;
		var count = 0;

		for (var b = bar; b >= _firstBar && count < _signalPeriod; b--)
		{
			sum += _spreadSeries[b];
			count++;

			if (b < _sessionStart.Count && _sessionStart[b])
				break;
		}

		return count > 0 ? sum / count : 0;
	}

	// Called under _calcLock. By sign only, or by sign and signal direction. At a session start
	// the signal has no previous value and counts as rising.
	private CrossColor SpreadColor(int bar)
	{
		var positive = _spreadSeries[bar] >= 0;

		if (!_useFourColors)
			return positive ? _spreadPositiveColor : _spreadNegativeColor;

		var sessionStart = bar <= _firstBar || (bar < _sessionStart.Count && _sessionStart[bar]);
		var rising = sessionStart || _signalSeries[bar] >= _signalSeries[bar - 1];

		if (positive)
			return rising ? _positiveRisingColor : _positiveFallingColor;

		return rising ? _negativeRisingColor : _negativeFallingColor;
	}

	// First bar of the oldest session calculated.
	private int FindFirstBar()
	{
		if (_sessions <= 0)
			return 0;

		var found = 0;

		for (var bar = CurrentBar - 1; bar > 0; bar--)
		{
			// Continuous mode counts the default sessions to find where to start.
			var start = _sessionMode == SessionMode.Continuous ? IsNewSession(bar) : IsSessionStart(bar);

			if (!start)
				continue;

			found++;

			if (found == _sessions)
				return bar;
		}

		return 0;
	}

	private bool IsSessionStart(int bar)
	{
		if (bar == 0)
			return true;

		return _sessionMode switch
		{
			SessionMode.Custom => CustomSessionDay(bar) != CustomSessionDay(bar - 1),
			SessionMode.Continuous => false,
			_ => IsNewSession(bar)
		};
	}

	// Day of the custom session a bar opens in: the chart date of its open time shifted back by
	// the session start. A session starts on the first bar whose day differs from the previous
	// bar's, which also finds the start after a weekend or a daily break that spans the start time,
	// and a start at 00:00.
	private DateTime CustomSessionDay(int bar)
	{
		return (ChartTime(GetCandle(bar).Time) - _customSessionStart).Date;
	}

	private DateTime ChartTime(DateTime utc)
	{
		if (InstrumentInfo is null)
			return utc;

#if ATAS_STABLE || ATAS_LATEST
		return utc.AddHours(InstrumentInfo.TimeZone);
#else
		return utc.Add(InstrumentInfo.TimeZoneOffset);
#endif
	}

	// Called under _calcLock. Records whether the bar starts a session and, if so, restarts the
	// running deltas. The first calculated bar always starts from 0.
	private bool StartBar(int bar)
	{
		var start = bar == _firstBar || IsSessionStart(bar);

		while (_sessionStart.Count <= bar)
			_sessionStart.Add(false);

		_sessionStart[bar] = start;

		if (start)
			Array.Clear(_delta);

		return start;
	}

	private bool Matches(int filter, decimal volume)
	{
		return volume >= _minVolume[filter] && (_maxVolume[filter] == 0 || volume <= _maxVolume[filter]);
	}

	// Called under _calcLock. Adds a trade to every filter whose range contains its size.
	private void AddVolume(decimal volume, TradeDirection direction)
	{
		var signed = direction == TradeDirection.Buy ? volume : -volume;

		for (var i = 0; i < FilterCount; i++)
		{
			if (Matches(i, volume))
				_delta[i] += signed;
		}
	}

	// Signed volume that a trade adds to a filter: its size when the filter's range contains it, else 0.
	private decimal Contribution(int filter, decimal volume, TradeDirection direction)
	{
		if (!Matches(filter, volume))
			return 0;

		return direction == TradeDirection.Buy ? volume : -volume;
	}

	// Called under _calcLock. Replaces what a trade added to the filters (old volume and direction,
	// 0 for a new trade) with its new volume and direction, in the bar it belongs to and in every
	// later bar, since the lines are cumulative.
	private void ApplyVolumeChange(int bar, decimal oldVolume, TradeDirection oldDirection,
		decimal newVolume, TradeDirection newDirection)
	{
		for (var i = 0; i < FilterCount; i++)
		{
			var diff = Contribution(i, newVolume, newDirection) - Contribution(i, oldVolume, oldDirection);

			if (diff == 0)
				continue;

			// The change stops at the next session start: a later session does not carry it.
			var inCurrentSession = true;

			for (var b = bar; b <= _lastBar; b++)
			{
				if (b > bar && _sessionStart[b])
				{
					inCurrentSession = false;
					break;
				}

				_filterSeries[i][b] += diff;
			}

			if (inCurrentSession)
				_delta[i] += diff;
		}

		UpdateSpread(bar, _lastBar);
	}

	// Called under _calcLock. Opens every bar after the last written one up to the given bar,
	// starting each with the running values.
	private void EnsureBar(int bar)
	{
		if (bar <= _lastBar)
			return;

		// The previous bar has closed.
		if (_detailedLog && _historyReady && _lastBar >= _firstBar)
			LogBar(_lastBar);

		for (var b = _lastBar + 1; b <= bar; b++)
		{
			if (StartBar(b) && b != _firstBar)
			{
				this.LogInfo($"SmartMoneyFlow: new session at bar {b} ({GetCandle(b).Time:yyyy-MM-dd HH:mm:ss}), lines restart from 0.");

				// A restart from 0 is not a cross.
				_spreadSign = 0;
				_signalSign = 0;
			}

			WriteBar(b);
		}

		_lastBar = bar;
	}

	// Called under _calcLock. Bar that contains a trade time: the last bar opened at or before it.
	private int BarOfTime(DateTime time)
	{
		for (var bar = CurrentBar - 1; bar > _firstBar; bar--)
		{
			if (GetCandle(bar).Time <= time)
				return bar;
		}

		return _firstBar;
	}

	private void OnRealtimeTrade(CumulativeTrade trade, bool isUpdate)
	{
		int bar;

		lock (_calcLock)
		{
			if (!_historyReady)
			{
				// Copy: the platform may keep updating the same trade object.
				_pendingTrades.Add((trade.MemberwiseClone(), isUpdate));
				return;
			}

			bar = ProcessTrade(trade, isUpdate, false);
			CheckCrosses();
		}

		if (bar >= 0)
			RaiseBarValueChanged(bar);

		RaisePendingAlerts();
	}

	// Called under _calcLock. Counts a realtime trade, or replaces the volume of the last trade
	// when it is an update of it. Returns the last bar changed, or -1.
	//
	// An update of a trade other than the last one counted is an orphan. While replaying the
	// buffer it belongs to a trade already in the history, so it is skipped; in realtime it is
	// counted as a new trade, as MultiMarketPower does.
	private int ProcessTrade(CumulativeTrade trade, bool isUpdate, bool fromBuffer)
	{
		if (CurrentBar == 0)
			return -1;

		EnsureBar(CurrentBar - 1);

		if (trade.Direction == TradeDirection.Between)
			return _lastBar;

		if (isUpdate && _lastTrade != null && _lastTrade.IsEqual(trade))
		{
			ApplyVolumeChange(_lastTradeBar, _lastTrade.Volume, _lastTrade.Direction, trade.Volume, trade.Direction);
			_lastTrade = trade.MemberwiseClone();
			return _lastBar;
		}

		if (isUpdate && fromBuffer)
			return -1;

		if (isUpdate)
			_orphanUpdates++;

		var bar = BarOfTime(trade.Time);
		ApplyVolumeChange(bar, 0, trade.Direction, trade.Volume, trade.Direction);

		_lastTrade = trade.MemberwiseClone();
		_lastTradeBar = bar;

		return _lastBar;
	}

	// Called under _calcLock, after a realtime change. Event-based: a cross is a change of sign
	// between two observations of the forming bar, not the value being on one side, so a spread
	// that stays on one side alerts once. Zero keeps the previous sign.
	private void CheckCrosses()
	{
		if (!_historyReady || _lastBar < _firstBar || _lastBar < 0)
			return;

		var spread = _spreadSeries[_lastBar];
		var signal = _signalSeries[_lastBar];
		var now = DateTime.UtcNow;

		if (DetectCross(spread, ref _spreadSign, out var above) && _alertOnZeroCross
			&& (now - _lastZeroAlertUtc).TotalSeconds >= _alertCooldownSeconds)
		{
			_lastZeroAlertUtc = now;
			_pendingAlerts.Add(($"Smart Money Flow: spread crossed {(above ? "above" : "below")} zero ({spread:0.##})", above));
		}

		if (DetectCross(spread - signal, ref _signalSign, out above) && _alertOnSignalCross
			&& (now - _lastSignalAlertUtc).TotalSeconds >= _alertCooldownSeconds)
		{
			_lastSignalAlertUtc = now;
			_pendingAlerts.Add(($"Smart Money Flow: spread crossed {(above ? "above" : "below")} the signal " +
				$"(spread {spread:0.##}, signal {signal:0.##})", above));
		}
	}

	// Whether value changed sign since the last observation; above is the new side.
	private static bool DetectCross(decimal value, ref int lastSign, out bool above)
	{
		var sign = Math.Sign(value);
		above = sign > 0;

		if (sign == 0)
			return false;

		var crossed = lastSign != 0 && sign != lastSign;
		lastSign = sign;
		return crossed;
	}

	private void RaisePendingAlerts()
	{
		List<(string Message, bool Positive)> alerts;

		lock (_calcLock)
		{
			if (_pendingAlerts.Count == 0)
				return;

			alerts = new List<(string Message, bool Positive)>(_pendingAlerts);
			_pendingAlerts.Clear();
		}

		foreach (var (message, positive) in alerts)
		{
			this.LogInfo($"SmartMoneyFlow: alert: {message}");
			AddAlert(_alertFile, InstrumentInfo?.Instrument ?? string.Empty, message,
				positive ? _spreadPositiveColor : _spreadNegativeColor, CrossColor.FromArgb(255, 255, 255, 255));
		}
	}

	// Called under _calcLock. Counts a realtime tick. Returns the last bar changed, or -1.
	private int ProcessTick(MarketDataArg tick)
	{
		if (CurrentBar == 0)
			return -1;

		EnsureBar(CurrentBar - 1);

		if (tick.Time == _lastTickTime)
			_lastTickCount++;
		else
		{
			_lastTickTime = tick.Time;
			_lastTickCount = 1;
		}

		if (tick.Direction != TradeDirection.Between)
			ApplyVolumeChange(BarOfTime(tick.Time), 0, tick.Direction, tick.Volume, tick.Direction);

		return _lastBar;
	}

	// Replays the trades buffered while the history was pending, then switches to realtime.
	// The buffer is drained in batches without holding the lock across a whole batch; the
	// switch happens under the lock once the buffer is empty, so no trade is left behind.
	private void ReplayPendingTrades()
	{
		var replayed = 0;
		var skipped = 0;
		var updates = 0;
		int boundaryTicksToSkip;

		lock (_calcLock)
		{
			boundaryTicksToSkip = _boundaryTickCount -
				(_bufferStartTickTime == _historyEndTime ? _bufferStartTickCount : 0);
		}

		while (true)
		{
			List<(CumulativeTrade Trade, bool IsUpdate)> batch;
			List<MarketDataArg> ticks;

			lock (_calcLock)
			{
				if (_pendingTrades.Count == 0 && _pendingTicks.Count == 0)
				{
					_historyReady = true;
					break;
				}

				batch = new List<(CumulativeTrade Trade, bool IsUpdate)>(_pendingTrades);
				_pendingTrades.Clear();
				ticks = new List<MarketDataArg>(_pendingTicks);
				_pendingTicks.Clear();
			}

			foreach (var tick in ticks)
			{
				lock (_calcLock)
				{
					var inHistory = tick.Time < _historyEndTime
						|| (tick.Time == _historyEndTime && boundaryTicksToSkip-- > 0);

					if (inHistory)
					{
						skipped++;
						continue;
					}

					ProcessTick(tick);
					replayed++;
				}
			}

			foreach (var (trade, isUpdate) in batch)
			{
				lock (_calcLock)
				{
					if (isUpdate && _lastTrade != null && _lastTrade.IsEqual(trade))
					{
						// Also when the trade is the last one of the history: the update replaces
						// the volume the response had for it.
						ProcessTrade(trade, true, true);
						updates++;
						continue;
					}

					// An update of another trade with the last response time: that trade was the
					// one still aggregating. It becomes the tracked last trade.
					if (isUpdate && TrackBoundaryTrade(trade))
					{
						ProcessTrade(trade, true, true);
						updates++;
						continue;
					}

					// An update of any other trade is an orphan: its trade is final in the history.
					if (isUpdate || IsInHistory(trade))
					{
						skipped++;
						continue;
					}

					ProcessTrade(trade, isUpdate, true);
					replayed++;
				}
			}
		}

		this.LogInfo($"SmartMoneyFlow: realtime started ({(_cumulativeTrades ? "cumulative trades" : "ticks")}); " +
			$"buffered events: {replayed} new trades, " +
			$"{updates} updates of the last trade, {skipped} skipped as already in the history.");

		if (CurrentBar > 0)
			RaiseBarValueChanged(CurrentBar - 1);
	}

	// Called under _calcLock. Whether a buffered trade is already counted in the history: it is
	// older than the last response trade, or it has the same time and is one of the response trades
	// at that time. Several trades can share a timestamp, so the time alone is not enough.
	private bool IsInHistory(CumulativeTrade trade)
	{
		if (trade.Time < _historyEndTime)
			return true;

		if (trade.Time > _historyEndTime)
			return false;

		foreach (var (boundary, _) in _boundaryTrades)
		{
			if (boundary.IsEqual(trade))
				return true;
		}

		return false;
	}

	// Called under _calcLock. When the trade is one of the response trades at the last response
	// time, makes it the tracked last trade, with the volume and bar the history counted for it.
	private bool TrackBoundaryTrade(CumulativeTrade trade)
	{
		for (var i = 0; i < _boundaryTrades.Count; i++)
		{
			var (boundary, bar) = _boundaryTrades[i];

			if (!boundary.IsEqual(trade))
				continue;

			// The tracked trade leaves the list with its current volume, so tracking can
			// return to it later from the right volume.
			if (_lastTrade != null)
			{
				for (var j = 0; j < _boundaryTrades.Count; j++)
				{
					if (_boundaryTrades[j].Trade.IsEqual(_lastTrade))
						_boundaryTrades[j] = (_lastTrade, _lastTradeBar);
				}
			}

			_lastTrade = boundary;
			_lastTradeBar = bar;
			return true;
		}

		return false;
	}

	// Called under _calcLock. Writes the running deltas as the values of the bar.
	private void WriteBar(int bar)
	{
		for (var i = 0; i < FilterCount; i++)
			_filterSeries[i][bar] = _delta[i];

		UpdateSpread(bar, bar);
	}

	// Rebuilds every bar from the first calculated bar with the trades of the response, or with
	// their ticks in tick mode. Items are assigned by time to the bar whose Time..LastTime contains
	// them; items before the first bar or between two bars are not counted, and neither are items
	// without direction, as in MultiMarketPower.
	private void CalculateHistory(IEnumerable<CumulativeTrade> cumulativeTrades)
	{
		int count;
		DateTime firstTime, lastTime;
		int lastBar;

		lock (_calcLock)
		{
			foreach (var series in DataSeries)
				series.Clear();

			Array.Clear(_delta);
			_sessionStart.Clear();
			lastBar = CurrentBar - 1;
			_lastTrade = null;
			_boundaryTrades.Clear();
			_boundaryTickCount = 0;

			if (_cumulativeTrades)
			{
				var trades = cumulativeTrades
					.Where(t => t.Direction != TradeDirection.Between)
					.OrderBy(t => t.Time)
					.ToList();

				_historyEndTime = trades.Count > 0 ? trades[^1].Time : DateTime.MinValue;

				FillBars(trades, t => t.Time, (trade, bar) =>
				{
					AddVolume(trade.Volume, trade.Direction);

					// The last trade of the response may still be aggregating: its later
					// updates must replace the volume counted here.
					_lastTrade = trade;
					_lastTradeBar = bar;

					if (trade.Time == _historyEndTime)
						_boundaryTrades.Add((trade.MemberwiseClone(), bar));
				}, lastBar);

				if (_lastTrade != null)
					_lastTrade = _lastTrade.MemberwiseClone();

				count = trades.Count;
				firstTime = count > 0 ? trades[0].Time : default;
				lastTime = _historyEndTime;
			}
			else
			{
				// Ticks without direction are kept for the boundary count: the realtime
				// buffer receives them too.
				var ticks = cumulativeTrades
					.Where(t => t.Ticks != null)
					.SelectMany(t => t.Ticks)
					.OrderBy(t => t.Time)
					.ToList();

				_historyEndTime = ticks.Count > 0 ? ticks[^1].Time : DateTime.MinValue;

				for (var i = ticks.Count - 1; i >= 0 && ticks[i].Time == _historyEndTime; i--)
					_boundaryTickCount++;

				FillBars(ticks, t => t.Time, (tick, _) =>
				{
					if (tick.Direction != TradeDirection.Between)
						AddVolume(tick.Volume, tick.Direction);
				}, lastBar);

				count = ticks.Count;
				firstTime = count > 0 ? ticks[0].Time : default;
				lastTime = _historyEndTime;
			}
		}

		UpdateVisibility();

		this.LogInfo($"SmartMoneyFlow: history calculated, {count} {(_cumulativeTrades ? "cumulative trades" : "ticks")}" +
			(count > 0 ? $" from {firstTime:yyyy-MM-dd HH:mm:ss.fff} to {lastTime:yyyy-MM-dd HH:mm:ss.fff}" : "") +
			$", bars {_firstBar}-{lastBar} ({_sessions} sessions, {SessionDescription()}); " +
			$"filters {string.Join(", ", Enumerable.Range(0, FilterCount).Select(FilterDescription))}; signal period {_signalPeriod}.");

		if (_detailedLog)
		{
			lock (_calcLock)
			{
				for (var bar = Math.Max(_firstBar, lastBar - LoggedHistoryBars + 1); bar <= lastBar; bar++)
					LogBar(bar);
			}
		}

		RedrawChart();
	}

	private string FilterDescription(int filter)
	{
		var role = _role[filter] switch
		{
			FilterRole.Smart => "smart",
			FilterRole.Dumb => "dumb",
			_ => "none"
		};

		var max = _maxVolume[filter] == 0 ? "+" : $"-{_maxVolume[filter]:0.##}";
		return $"F{filter + 1} {_minVolume[filter]:0.##}{max} {role}";
	}

	// Called under _calcLock. One line with the values of a bar.
	private void LogBar(int bar)
	{
		var values = string.Join(" / ", Enumerable.Range(0, FilterCount).Select(i => _filterSeries[i][bar].ToString("0.##")));
		var orphans = _orphanUpdates > 0 ? $"; {_orphanUpdates} orphan updates so far" : "";

		this.LogInfo($"SmartMoneyFlow: bar {bar} {GetCandle(bar).Time:yyyy-MM-dd HH:mm:ss}" +
			$"{(bar < _sessionStart.Count && _sessionStart[bar] ? " (session start)" : "")}: " +
			$"filters {values}; spread {_spreadSeries[bar]:0.##}; signal {_signalSeries[bar]:0.##}{orphans}");
	}

	private string SessionDescription()
	{
		return _sessionMode switch
		{
			SessionMode.Custom => $"custom start {_customSessionStart:hh\\:mm}",
			SessionMode.Continuous => "continuous",
			_ => "default session"
		};
	}

	// Called under _calcLock. Walks the bars from the first calculated bar to lastBar and counts
	// each item (sorted by time) in the bar that contains its time, then writes the bar.
	private void FillBars<T>(List<T> items, Func<T, DateTime> timeOf, Action<T, int> count, int lastBar)
	{
		var index = 0;

		for (var bar = _firstBar; bar <= lastBar; bar++)
		{
			StartBar(bar);
			var candle = GetCandle(bar);

			while (index < items.Count && timeOf(items[index]) < candle.Time)
				index++;

			while (index < items.Count && timeOf(items[index]) <= candle.LastTime)
			{
				count(items[index], bar);
				index++;
			}

			WriteBar(bar);
		}

		_lastBar = lastBar;
	}

	#endregion
}
