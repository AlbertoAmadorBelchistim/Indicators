namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

using OFT.Attributes.Editors;

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

	private static readonly ImbalanceLevel[] NoImbalances = Array.Empty<ImbalanceLevel>();

	// Imbalances per bar index; null until the bar has been calculated as a closed bar.
	private readonly List<ImbalanceLevel[]> _barImbalances = new();

	private readonly PriceVolumeInfo _levelCache = new();

	private decimal _imbalanceRatio = 3m;
	private decimal _minDominantVolume = 20m;

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
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"DiagonalImbalance: initialized ({typeof(DiagonalImbalance).Assembly.GetName().Version}).");
	}

	protected override void OnRecalculate()
	{
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
		{
			CalculateClosedBar(bar - 1);

			if (_historyLoaded)
				LogBar(bar - 1);
		}
	}

	protected override void OnFinishRecalculate()
	{
		_historyLoaded = true;

		var lastClosed = CurrentBar - 2;

		this.LogInfo($"DiagonalImbalance: history calculated, {lastClosed + 1} closed bars, " +
			$"{_historyBuyCount} buy / {_historySellCount} sell imbalances " +
			$"(ratio {_imbalanceRatio}, min dominant volume {_minDominantVolume}).");

		for (var bar = Math.Max(0, lastClosed - LoggedHistoryBars + 1); bar <= lastClosed; bar++)
			LogBar(bar);
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

		while (_barImbalances.Count <= bar)
			_barImbalances.Add(null);

		_barImbalances[bar] = levels;

		foreach (var level in levels)
		{
			if (level.IsBuy)
				_historyBuyCount++;
			else
				_historySellCount++;
		}
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

	// A passive side of zero is not evaluated yet: those levels are skipped.
	private bool IsImbalance(decimal dominant, decimal passive)
	{
		return passive > 0
			&& dominant >= _minDominantVolume
			&& dominant >= passive * _imbalanceRatio;
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
