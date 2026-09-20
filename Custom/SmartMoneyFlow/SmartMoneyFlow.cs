namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using ATAS.Indicators.Drawing;

using OFT.Attributes.Editors;

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

	#endregion

	#region Properties

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
		}
	}

	protected override void OnCalculate(int bar, decimal value)
	{
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

	// Called under _calcLock. Writes the running deltas as the values of the bar.
	private void WriteBar(int bar)
	{
		for (var i = 0; i < FilterCount; i++)
			_filterSeries[i][bar] = _delta[i];
	}

	// Rebuilds every bar from the first calculated bar with the trades of the response.
	// Trades are assigned by time to the bar whose Time..LastTime contains them; trades before
	// the first bar or between two bars are not counted, and neither are trades without
	// direction, as in MultiMarketPower.
	private void CalculateHistory(IEnumerable<CumulativeTrade> cumulativeTrades)
	{
		var trades = cumulativeTrades
			.Where(t => t.Direction != TradeDirection.Between)
			.OrderBy(t => t.Time)
			.ToList();

		int lastBar;

		lock (_calcLock)
		{
			foreach (var series in DataSeries)
				series.Clear();

			Array.Clear(_delta);
			lastBar = CurrentBar - 1;

			var index = 0;

			for (var bar = _firstBar; bar <= lastBar; bar++)
			{
				var candle = GetCandle(bar);

				while (index < trades.Count && trades[index].Time < candle.Time)
					index++;

				while (index < trades.Count && trades[index].Time <= candle.LastTime)
				{
					AddVolume(trades[index].Volume, trades[index].Direction);
					index++;
				}

				WriteBar(bar);
			}

			_lastBar = lastBar;
		}

		UpdateVisibility();

		this.LogInfo($"SmartMoneyFlow: history calculated, {trades.Count} trades" +
			(trades.Count > 0 ? $" from {trades[0].Time:yyyy-MM-dd HH:mm:ss.fff} to {trades[^1].Time:yyyy-MM-dd HH:mm:ss.fff}" : "") +
			$", bars {_firstBar}-{lastBar}; last values {string.Join(" / ", _delta.Select(d => d.ToString("0.##")))}.");

		RedrawChart();
	}

	#endregion
}
