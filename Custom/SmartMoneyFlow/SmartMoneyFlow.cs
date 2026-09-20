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
[Description("Cumulative delta of five trade-size filters, with the smart money spread and its signal line.")]
public class SmartMoneyFlow : Indicator
{
	#region Fields

	// Number of trade-size filters.
	private const int FilterCount = 5;

	// One cumulative delta line per trade-size filter. Filter 1 replaces the default series.
	private readonly ValueDataSeries[] _filterSeries = new ValueDataSeries[FilterCount];

	// Smart money minus dumb money, drawn as a histogram.
	private readonly ValueDataSeries _spreadSeries = new("SpreadSeries", "Smart Money Spread")
	{
		VisualType = VisualMode.Hide,
		Width = 3,
		ShowZeroValue = true,
		UseMinimizedModeIfEnabled = true
	};

	// Simple moving average of the spread.
	private readonly ValueDataSeries _signalSeries = new("SignalSeries", "Signal")
	{
		Color = CrossColor.FromArgb(255, 255, 255, 255),
		VisualType = VisualMode.Hide,
		Width = 2,
		UseMinimizedModeIfEnabled = true
	};

	// Trade-size range of each filter: a trade matches when Min <= volume <= Max; Max 0 means no maximum.
	// Defaults are the MultiMarketPower ranges.
	private readonly decimal[] _minVolume = { 0, 6, 11, 21, 41 };
	private readonly decimal[] _maxVolume = { 5, 10, 20, 40, 0 };
	private readonly bool[] _useFilter = { true, true, true, true, true };

	// Cumulative trades (default) or individual ticks.
	private bool _cumulativeTrades = true;

	// Running signed volume (buy +, sell -) of each filter since the first calculated bar.
	private readonly decimal[] _delta = new decimal[FilterCount];

	// Guards the calculation state: the history response, realtime trades and the
	// calculation thread can all update it.
	private readonly object _calcLock = new();

	// First bar of the calculation and last bar written.
	private int _firstBar;
	private int _lastBar = -1;

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

	#endregion

	#region Properties

	[Display(Name = "Cumulative trades", GroupName = "Calculation",
		Description = "Counts cumulative trades (all the ticks of one aggressive order as one trade). Off: counts individual ticks.",
		Order = 10)]
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

	[Display(Name = "Enabled", GroupName = "Filter 1", Description = "Shows the line of this filter.", Order = 100)]
	public bool UseFilter1
	{
		get => _useFilter[0];
		set => SetUseFilter(0, value);
	}

	[Display(Name = "Minimum volume", GroupName = "Filter 1", Description = "Smallest trade size counted by this filter.", Order = 110)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume1
	{
		get => _minVolume[0];
		set => SetVolumeRange(0, value, _maxVolume[0]);
	}

	[Display(Name = "Maximum volume", GroupName = "Filter 1", Description = "Largest trade size counted by this filter. 0 = no maximum.", Order = 120)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume1
	{
		get => _maxVolume[0];
		set => SetVolumeRange(0, _minVolume[0], value);
	}

	[Display(Name = "Color", GroupName = "Filter 1", Description = "Line color of this filter.", Order = 140)]
	public CrossColor Color1
	{
		get => _filterSeries[0].Color;
		set => _filterSeries[0].Color = value;
	}

	[Display(Name = "Line width", GroupName = "Filter 1", Description = "Line width of this filter.", Order = 150)]
	[Range(1, 20)]
	public int LineWidth1
	{
		get => _filterSeries[0].Width;
		set => _filterSeries[0].Width = value;
	}

	[Display(Name = "Enabled", GroupName = "Filter 2", Description = "Shows the line of this filter.", Order = 200)]
	public bool UseFilter2
	{
		get => _useFilter[1];
		set => SetUseFilter(1, value);
	}

	[Display(Name = "Minimum volume", GroupName = "Filter 2", Description = "Smallest trade size counted by this filter.", Order = 210)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume2
	{
		get => _minVolume[1];
		set => SetVolumeRange(1, value, _maxVolume[1]);
	}

	[Display(Name = "Maximum volume", GroupName = "Filter 2", Description = "Largest trade size counted by this filter. 0 = no maximum.", Order = 220)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume2
	{
		get => _maxVolume[1];
		set => SetVolumeRange(1, _minVolume[1], value);
	}

	[Display(Name = "Color", GroupName = "Filter 2", Description = "Line color of this filter.", Order = 240)]
	public CrossColor Color2
	{
		get => _filterSeries[1].Color;
		set => _filterSeries[1].Color = value;
	}

	[Display(Name = "Line width", GroupName = "Filter 2", Description = "Line width of this filter.", Order = 250)]
	[Range(1, 20)]
	public int LineWidth2
	{
		get => _filterSeries[1].Width;
		set => _filterSeries[1].Width = value;
	}

	[Display(Name = "Enabled", GroupName = "Filter 3", Description = "Shows the line of this filter.", Order = 300)]
	public bool UseFilter3
	{
		get => _useFilter[2];
		set => SetUseFilter(2, value);
	}

	[Display(Name = "Minimum volume", GroupName = "Filter 3", Description = "Smallest trade size counted by this filter.", Order = 310)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume3
	{
		get => _minVolume[2];
		set => SetVolumeRange(2, value, _maxVolume[2]);
	}

	[Display(Name = "Maximum volume", GroupName = "Filter 3", Description = "Largest trade size counted by this filter. 0 = no maximum.", Order = 320)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume3
	{
		get => _maxVolume[2];
		set => SetVolumeRange(2, _minVolume[2], value);
	}

	[Display(Name = "Color", GroupName = "Filter 3", Description = "Line color of this filter.", Order = 340)]
	public CrossColor Color3
	{
		get => _filterSeries[2].Color;
		set => _filterSeries[2].Color = value;
	}

	[Display(Name = "Line width", GroupName = "Filter 3", Description = "Line width of this filter.", Order = 350)]
	[Range(1, 20)]
	public int LineWidth3
	{
		get => _filterSeries[2].Width;
		set => _filterSeries[2].Width = value;
	}

	[Display(Name = "Enabled", GroupName = "Filter 4", Description = "Shows the line of this filter.", Order = 400)]
	public bool UseFilter4
	{
		get => _useFilter[3];
		set => SetUseFilter(3, value);
	}

	[Display(Name = "Minimum volume", GroupName = "Filter 4", Description = "Smallest trade size counted by this filter.", Order = 410)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume4
	{
		get => _minVolume[3];
		set => SetVolumeRange(3, value, _maxVolume[3]);
	}

	[Display(Name = "Maximum volume", GroupName = "Filter 4", Description = "Largest trade size counted by this filter. 0 = no maximum.", Order = 420)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume4
	{
		get => _maxVolume[3];
		set => SetVolumeRange(3, _minVolume[3], value);
	}

	[Display(Name = "Color", GroupName = "Filter 4", Description = "Line color of this filter.", Order = 440)]
	public CrossColor Color4
	{
		get => _filterSeries[3].Color;
		set => _filterSeries[3].Color = value;
	}

	[Display(Name = "Line width", GroupName = "Filter 4", Description = "Line width of this filter.", Order = 450)]
	[Range(1, 20)]
	public int LineWidth4
	{
		get => _filterSeries[3].Width;
		set => _filterSeries[3].Width = value;
	}

	[Display(Name = "Enabled", GroupName = "Filter 5", Description = "Shows the line of this filter.", Order = 500)]
	public bool UseFilter5
	{
		get => _useFilter[4];
		set => SetUseFilter(4, value);
	}

	[Display(Name = "Minimum volume", GroupName = "Filter 5", Description = "Smallest trade size counted by this filter.", Order = 510)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolume5
	{
		get => _minVolume[4];
		set => SetVolumeRange(4, value, _maxVolume[4]);
	}

	[Display(Name = "Maximum volume", GroupName = "Filter 5", Description = "Largest trade size counted by this filter. 0 = no maximum.", Order = 520)]
	[Range(0, 100000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MaxVolume5
	{
		get => _maxVolume[4];
		set => SetVolumeRange(4, _minVolume[4], value);
	}

	[Display(Name = "Color", GroupName = "Filter 5", Description = "Line color of this filter.", Order = 540)]
	public CrossColor Color5
	{
		get => _filterSeries[4].Color;
		set => _filterSeries[4].Color = value;
	}

	[Display(Name = "Line width", GroupName = "Filter 5", Description = "Line width of this filter.", Order = 550)]
	[Range(1, 20)]
	public int LineWidth5
	{
		get => _filterSeries[4].Width;
		set => _filterSeries[4].Width = value;
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

		for (var i = 0; i < FilterCount; i++)
		{
			_filterSeries[i] = new ValueDataSeries($"Filter{i + 1}Series", $"Filter {i + 1}")
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
			_historyReady = false;
			_pendingTrades.Clear();
			_pendingTicks.Clear();
			_lastTrade = null;
			_bufferStartTickTime = _lastTickTime;
			_bufferStartTickCount = _lastTickCount;
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
		}

		if (bar >= 0)
			RaiseBarValueChanged(bar);
	}

	#endregion

	#region Private Methods

	private void SetUseFilter(int filter, bool value)
	{
		_useFilter[filter] = value;
		UpdateVisibility();
	}

	private void SetVolumeRange(int filter, decimal min, decimal max)
	{
		if (_minVolume[filter] == min && _maxVolume[filter] == max)
			return;

		_minVolume[filter] = min;
		_maxVolume[filter] = max;
		RecalculateValues();
	}

	private void UpdateVisibility()
	{
		for (var i = 0; i < FilterCount; i++)
			_filterSeries[i].VisualType = _useFilter[i] ? VisualMode.Line : VisualMode.Hide;
	}

	// First bar of the current session.
	private int FindFirstBar()
	{
		for (var bar = CurrentBar - 1; bar > 0; bar--)
		{
			if (IsNewSession(bar))
				return bar;
		}

		return 0;
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

			_delta[i] += diff;

			for (var b = bar; b <= _lastBar; b++)
				_filterSeries[i][b] += diff;
		}
	}

	// Called under _calcLock. Opens every bar after the last written one up to the given bar,
	// starting each with the running values.
	private void EnsureBar(int bar)
	{
		if (bar <= _lastBar)
			return;

		for (var b = _lastBar + 1; b <= bar; b++)
			WriteBar(b);

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
		}

		if (bar >= 0)
			RaiseBarValueChanged(bar);
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

		var bar = BarOfTime(trade.Time);
		ApplyVolumeChange(bar, 0, trade.Direction, trade.Volume, trade.Direction);

		_lastTrade = trade.MemberwiseClone();
		_lastTradeBar = bar;

		return _lastBar;
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
			$", bars {_firstBar}-{lastBar}; last values {string.Join(" / ", _delta.Select(d => d.ToString("0.##")))}.");

		RedrawChart();
	}

	// Called under _calcLock. Walks the bars from the first calculated bar to lastBar and counts
	// each item (sorted by time) in the bar that contains its time, then writes the bar.
	private void FillBars<T>(List<T> items, Func<T, DateTime> timeOf, Action<T, int> count, int lastBar)
	{
		var index = 0;

		for (var bar = _firstBar; bar <= lastBar; bar++)
		{
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
