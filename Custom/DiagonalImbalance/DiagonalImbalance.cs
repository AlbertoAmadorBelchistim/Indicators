namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text;

using OFT.Attributes.Editors;
using OFT.Rendering.Context;

using Utils.Common.Logging;

[DisplayName("Diagonal Imbalance")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Description("Detects diagonal bid/ask imbalances in the footprint and stacked imbalance zones.")]
public class DiagonalImbalance : Indicator
{
	#region Nested Types: Engine

	// One diagonal imbalance found in a bar.
	// Buy:  Ask at Price against Bid one tick below.
	// Sell: Bid at Price against Ask one tick above.
	internal readonly struct ImbalanceLevel
	{
		public ImbalanceLevel(decimal price, bool isBuy, decimal dominant, decimal passive)
		{
			Price = price;
			IsBuy = isBuy;
			Dominant = dominant;
			Passive = passive;
		}

		public decimal Price { get; }

		public bool IsBuy { get; }

		public decimal Dominant { get; }

		public decimal Passive { get; }
	}

	#endregion

	#region Fields

	// Number of most recent closed bars whose imbalances are written to the log after a recalculation.
	private const int LoggedHistoryBars = 20;

	// Maximum number of levels listed per logged bar.
	private const int LoggedLevelsPerBar = 12;

	// Minimum bar width in pixels to draw buy and sell marks on separate halves of the bar.
	private const int MinSplitBarWidth = 6;

	private static readonly ImbalanceLevel[] NoImbalances = Array.Empty<ImbalanceLevel>();

	// Imbalances per bar index; null until the bar has been calculated as a closed bar.
	private readonly List<ImbalanceLevel[]> _barImbalances = new();

	// Guards _barImbalances: written by the calculation thread, read by the render thread.
	private readonly object _barsLock = new();

	// Visible bars copied under the lock by OnRender, reused between frames.
	private readonly List<(int Bar, ImbalanceLevel[] Levels)> _renderBars = new();

	private readonly PriceVolumeInfo _levelCache = new();

	private decimal _imbalanceRatio = 3m;
	private decimal _minDominantVolume = 20m;
	private bool _ignoreZeroLevels;
	private decimal _minVolumeDifference;

	private bool _showMarks = true;
	private Color _buyColor = Color.FromArgb(140, 0, 200, 83);
	private Color _sellColor = Color.FromArgb(140, 229, 57, 53);

	private bool _historyLoaded;
	private int _historyBuyCount;
	private int _historySellCount;

	#endregion

	#region Properties

	[Display(Name = "Imbalance ratio", GroupName = "Calculation", Order = 100,
		Description = "Minimum ratio between the dominant side and the diagonal passive side, e.g. 3 = 300%.")]
	[Range(1, 100)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal ImbalanceRatio
	{
		get => _imbalanceRatio;
		set
		{
			if (_imbalanceRatio == value)
				return;

			_imbalanceRatio = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Minimum dominant volume", GroupName = "Calculation", Order = 110,
		Description = "Minimum volume on the dominant side for a level to count as an imbalance.")]
	[Range(0, 1000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinDominantVolume
	{
		get => _minDominantVolume;
		set
		{
			if (_minDominantVolume == value)
				return;

			_minDominantVolume = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Minimum volume difference", GroupName = "Calculation", Order = 115,
		Description = "Minimum difference between the dominant side and the diagonal passive side. 0 disables the filter.")]
	[Range(0, 1000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolumeDifference
	{
		get => _minVolumeDifference;
		set
		{
			if (_minVolumeDifference == value)
				return;

			_minVolumeDifference = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Ignore zero levels", GroupName = "Calculation", Order = 120,
		Description = "When enabled, a level whose diagonal passive side has no volume is never an imbalance. " +
			"When disabled, it counts as an infinite ratio and only the minimum dominant volume applies.")]
	public bool IgnoreZeroLevels
	{
		get => _ignoreZeroLevels;
		set
		{
			if (_ignoreZeroLevels == value)
				return;

			_ignoreZeroLevels = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Properties: Visuals

	[Display(Name = "Show imbalance marks", GroupName = "Visuals", Order = 200,
		Description = "Highlights every level with a diagonal imbalance.")]
	public bool ShowMarks
	{
		get => _showMarks;
		set
		{
			_showMarks = value;
			RedrawChart();
		}
	}

	[Display(Name = "Buy imbalance color", GroupName = "Visuals", Order = 210,
		Description = "Color of buy imbalances (Ask against the Bid one tick below).")]
	public CrossColor BuyColor
	{
		get => _buyColor.Convert();
		set
		{
			_buyColor = value.Convert();
			RedrawChart();
		}
	}

	[Display(Name = "Sell imbalance color", GroupName = "Visuals", Order = 220,
		Description = "Color of sell imbalances (Bid against the Ask one tick above).")]
	public CrossColor SellColor
	{
		get => _sellColor.Convert();
		set
		{
			_sellColor = value.Convert();
			RedrawChart();
		}
	}

	#endregion

	#region Ctor

	public DiagonalImbalance()
		: base(true)
	{
		DenyToChangePanel = true;

		// The indicator draws on the price panel; the default series stays hidden
		// so it does not show up as an empty line in the chart or the Drawing panel.
		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"DiagonalImbalance: initialized ({typeof(DiagonalImbalance).Assembly.GetName().Version}).");
	}

	protected override void OnRecalculate()
	{
		lock (_barsLock)
			_barImbalances.Clear();

		_historyLoaded = false;
		_historyBuyCount = 0;
		_historySellCount = 0;
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		var lastBar = CurrentBar - 1;

		if (bar < lastBar)
		{
			// Closed bar (history).
			CalculateClosedBar(bar);
			return;
		}

		// Forming bar: it is evaluated once it closes (the realtime evaluation of the
		// forming bar comes later). A new forming bar means the previous one just closed.
		if (bar > 0 && !IsCalculated(bar - 1))
			CalculateClosedBar(bar - 1);
	}

	protected override void OnFinishRecalculate()
	{
		_historyLoaded = true;

		var lastClosed = CurrentBar - 2;

		this.LogInfo($"DiagonalImbalance: history calculated, {lastClosed + 1} closed bars, " +
			$"{_historyBuyCount} buy / {_historySellCount} sell imbalances " +
			$"(ratio {_imbalanceRatio}, min dominant volume {_minDominantVolume}, " +
			$"min volume difference {_minVolumeDifference}, ignore zero levels {_ignoreZeroLevels}).");

		for (var bar = Math.Max(0, lastClosed - LoggedHistoryBars + 1); bar <= lastClosed; bar++)
			LogBar(bar);
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (!_showMarks || ChartInfo is null || InstrumentInfo is null)
			return;

		var tickSize = InstrumentInfo.TickSize;

		if (tickSize <= 0)
			return;

		var container = ChartInfo.PriceChartContainer;
		var barWidth = Math.Max(1, (int)container.BarsWidth);

		// Wide enough bars split like a Bid x Ask footprint: sell imbalances (Bid) on the left
		// half, buy imbalances (Ask) on the right half. Narrow bars use the full width.
		var split = barWidth >= MinSplitBarWidth;
		var leftWidth = split ? barWidth / 2 : barWidth;
		var rightWidth = split ? barWidth - leftWidth : barWidth;

		_renderBars.Clear();

		lock (_barsLock)
		{
			var firstBar = Math.Max(0, FirstVisibleBarNumber);
			var lastBar = Math.Min(LastVisibleBarNumber, _barImbalances.Count - 1);

			for (var bar = firstBar; bar <= lastBar; bar++)
			{
				var barLevels = _barImbalances[bar];

				if (barLevels is { Length: > 0 })
					_renderBars.Add((bar, barLevels));
			}
		}

		foreach (var (bar, levels) in _renderBars)
		{
			var x = ChartInfo.GetXByBar(bar);

			foreach (var level in levels)
			{
				// GetYByPrice(price, true) is the top edge of the price row.
				var top = ChartInfo.GetYByPrice(level.Price, true);
				var bottom = ChartInfo.GetYByPrice(level.Price - tickSize, true);
				var height = Math.Max(1, bottom - top);

				var rect = level.IsBuy
					? new Rectangle(split ? x + leftWidth : x, top, rightWidth, height)
					: new Rectangle(x, top, leftWidth, height);

				context.FillRectangle(level.IsBuy ? _buyColor : _sellColor, rect);
			}
		}
	}

	#endregion

	#region Private Methods: Engine

	private bool IsCalculated(int bar)
	{
		return bar < _barImbalances.Count && _barImbalances[bar] != null;
	}

	private void CalculateClosedBar(int bar)
	{
		var levels = FindImbalances(bar);

		lock (_barsLock)
		{
			while (_barImbalances.Count <= bar)
				_barImbalances.Add(null);

			_barImbalances[bar] = levels;
		}

		foreach (var level in levels)
		{
			if (level.IsBuy)
				_historyBuyCount++;
			else
				_historySellCount++;
		}

		// After the history, a bar only reaches this point when it closes (in realtime or replay,
		// possibly several at once). Bars without imbalances are not logged.
		if (_historyLoaded && levels.Length > 0)
			LogBar(bar);
	}

	// Walks the bar from Low to High. At each price P the Ask of P and the Bid of P - 1 tick
	// are known, which is exactly the pair needed for a buy imbalance at P and for a sell
	// imbalance at P - 1 tick (Bid of P - 1 tick against Ask of P).
	private ImbalanceLevel[] FindImbalances(int bar)
	{
		var tickSize = InstrumentInfo?.TickSize ?? 0m;

		if (tickSize <= 0)
			return NoImbalances;

		var candle = GetCandle(bar);
		List<ImbalanceLevel> found = null;

		var belowBid = 0m;

		for (var price = candle.Low; price <= candle.High; price += tickSize)
		{
			var info = candle.GetPriceVolumeInfo(price, _levelCache);
			var ask = info?.Ask ?? 0m;
			var bid = info?.Bid ?? 0m;

			var belowPrice = price - tickSize;

			// Buy imbalance at P: Ask(P) against Bid(P - 1 tick).
			if (price > candle.Low && IsImbalance(ask, belowBid))
				(found ??= new()).Add(new ImbalanceLevel(price, true, ask, belowBid));

			// Sell imbalance at P - 1 tick: Bid(P - 1 tick) against Ask(P).
			if (price > candle.Low && IsImbalance(belowBid, ask))
				(found ??= new()).Add(new ImbalanceLevel(belowPrice, false, belowBid, ask));

			belowBid = bid;
		}

		return found?.ToArray() ?? NoImbalances;
	}

	// Both levels of a comparison are always inside the bar (Low..High): the Low has no
	// level below and the High none above, so the bar edges are never compared against
	// prices that did not trade. Inside the bar, a price without trades reads as zero.
	private bool IsImbalance(decimal dominant, decimal passive)
	{
		if (dominant <= 0 || dominant < _minDominantVolume)
			return false;

		// Absolute filter: a high ratio between two small numbers (e.g. 9 vs 2) is not significant.
		if (dominant - passive < _minVolumeDifference)
			return false;

		// Zero passive side: infinite ratio, unless zero levels are ignored.
		if (passive == 0)
			return !_ignoreZeroLevels;

		return dominant >= passive * _imbalanceRatio;
	}

	#endregion

	#region Private Methods: Diagnostics

	private void LogBar(int bar)
	{
		if (!IsCalculated(bar))
			return;

		var levels = _barImbalances[bar];
		var candle = GetCandle(bar);

		var buy = 0;
		var sell = 0;

		foreach (var level in levels)
		{
			if (level.IsBuy)
				buy++;
			else
				sell++;
		}

		var sb = new StringBuilder();
		sb.Append($"DiagonalImbalance: bar {bar} {candle.Time:yyyy-MM-dd HH:mm:ss} " +
			$"[{candle.Low}-{candle.High}]: {buy} buy / {sell} sell");

		for (var i = 0; i < levels.Length && i < LoggedLevelsPerBar; i++)
		{
			var level = levels[i];
			sb.Append(level.IsBuy
				? $"; B@{level.Price} ask {level.Dominant} vs bid {level.Passive}"
				: $"; S@{level.Price} bid {level.Dominant} vs ask {level.Passive}");
		}

		if (levels.Length > LoggedLevelsPerBar)
			sb.Append($"; +{levels.Length - LoggedLevelsPerBar} more");

		this.LogInfo(sb.ToString());
	}

	#endregion
}
