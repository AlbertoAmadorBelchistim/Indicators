namespace ATAS.Indicators.Technical.DomPressureCore;

using System;
using System.Collections.Generic;

/// <summary>
/// Resting volume of the order book by price, both sides. Not synchronized: the indicator guards
/// it with its own lock. No ATAS dependency, so it can be tested on its own.
/// </summary>
public sealed class DepthBook
{
	private readonly SortedList<decimal, decimal> _asks = new();
	private readonly SortedList<decimal, decimal> _bids = new();

	public int AskLevels => _asks.Count;

	public int BidLevels => _bids.Count;

	public void Clear()
	{
		_asks.Clear();
		_bids.Clear();
	}

	/// <summary>Sets the volume of one level; zero or less removes it.</summary>
	public void Apply(bool isAsk, decimal price, decimal volume)
	{
		var side = isAsk ? _asks : _bids;

		if (volume <= 0m)
			side.Remove(price);
		else
			side[price] = volume;
	}

	/// <summary>
	/// Volume resting on the <paramref name="levels"/> levels of each side nearest to the market
	/// (lowest asks, highest bids); 0 or less takes the whole book. False while a side is empty.
	/// </summary>
	public bool TryGetSums(int levels, out decimal bidVolume, out decimal askVolume)
	{
		bidVolume = 0m;
		askVolume = 0m;

		if (_asks.Count == 0 || _bids.Count == 0)
			return false;

		var askCount = levels <= 0 ? _asks.Count : Math.Min(levels, _asks.Count);
		var askValues = _asks.Values;

		for (var i = 0; i < askCount; i++)
			askVolume += askValues[i];

		var bidCount = levels <= 0 ? _bids.Count : Math.Min(levels, _bids.Count);
		var bidValues = _bids.Values;
		var last = bidValues.Count - 1;

		for (var i = 0; i < bidCount; i++)
			bidVolume += bidValues[last - i];

		return true;
	}
}
