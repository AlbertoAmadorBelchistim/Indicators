namespace ATAS.Indicators.Technical.BigTrades;

using System;
using System.Collections.Generic;

/// <summary>Identity of a cumulative trade: its start time, first price and side (1 buy, -1 sell).</summary>
public readonly record struct TradeKey(DateTime Time, decimal Price, int Side);

/// <summary>A trade larger than the threshold in force when it started.</summary>
public sealed class Bubble
{
	public TradeKey Key { get; init; }

	public decimal Volume { get; set; }

	/// <summary>Threshold the trade was compared with, for scaling the bubble.</summary>
	public decimal Threshold { get; init; }

	public DateTime Time => Key.Time;

	public decimal Price => Key.Price;

	public int Side => Key.Side;
}

/// <summary>
/// Volume percentile of a multiset of trade volumes that changes by single additions and removals.
/// The distinct volumes of a market are few, so the walk over the sorted counts is short.
/// </summary>
public sealed class RollingPercentile
{
	private readonly SortedDictionary<decimal, int> _counts = new();

	public int Count { get; private set; }

	public void Add(decimal volume)
	{
		_counts.TryGetValue(volume, out var count);
		_counts[volume] = count + 1;
		Count++;
	}

	public void Remove(decimal volume)
	{
		if (!_counts.TryGetValue(volume, out var count))
			return;

		if (count == 1)
			_counts.Remove(volume);
		else
			_counts[volume] = count - 1;

		Count--;
	}

	public void Clear()
	{
		_counts.Clear();
		Count = 0;
	}

	/// <summary>The smallest volume with at least <paramref name="percent"/> % of the trades at or below it.</summary>
	public decimal Quantile(decimal percent)
	{
		if (Count == 0)
			return 0m;

		var cutoff = Math.Max(1, (int)Math.Ceiling(Count * percent / 100m));
		var seen = 0;
		var value = 0m;

		foreach (var (volume, count) in _counts)
		{
			seen += count;
			value = volume;

			if (seen >= cutoff)
				break;
		}

		return value;
	}
}

/// <summary>
/// Finds the trades that are large for the current market. The threshold is the volume percentile
/// of the last <see cref="WindowTrades"/> trades, and each trade is compared with the threshold
/// in force when it started, before it joins the window. The same trades give the same bubbles
/// whether they arrive final (history) or growing through updates (realtime). Not synchronized.
/// </summary>
public sealed class BigTradeEngine
{
	private sealed class Entry
	{
		public decimal Volume;
		public decimal? Threshold;
		public Bubble Bubble;
	}

	private readonly RollingPercentile _window = new();
	private readonly Queue<TradeKey> _order = new();
	private readonly Dictionary<TradeKey, Entry> _entries = new();
	private readonly List<Bubble> _bubbles = new();

	public BigTradeEngine(int windowTrades, decimal percentile, int minTrades)
	{
		WindowTrades = Math.Max(1, windowTrades);
		Percentile = percentile;
		MinTrades = Math.Max(1, minTrades);
	}

	public int WindowTrades { get; }

	/// <summary>Percentile of the trade volumes, 0-100: 80 marks roughly the largest 20% of the trades.</summary>
	public decimal Percentile { get; }

	/// <summary>Trades needed in the window before any trade is marked.</summary>
	public int MinTrades { get; }

	/// <summary>Bubbles in time order.</summary>
	public IReadOnlyList<Bubble> Bubbles => _bubbles;

	/// <summary>The threshold for the next trade, or null while the window has too few trades.</summary>
	public decimal? CurrentThreshold => _window.Count >= MinTrades ? _window.Quantile(Percentile) : null;

	public int TradesInWindow => _window.Count;

	public void Clear()
	{
		_window.Clear();
		_order.Clear();
		_entries.Clear();
		_bubbles.Clear();
	}

	/// <summary>Adds a trade, or updates it if the key is already known (a growing realtime trade).</summary>
	public void Add(TradeKey key, decimal volume)
	{
		if (_entries.ContainsKey(key))
		{
			Update(key, volume);
			return;
		}

		var entry = new Entry { Volume = volume, Threshold = CurrentThreshold };
		_entries[key] = entry;
		_order.Enqueue(key);
		_window.Add(volume);
		Evaluate(key, entry);

		while (_window.Count > WindowTrades && _order.Count > 0)
		{
			var old = _order.Dequeue();

			if (_entries.Remove(old, out var removed))
				_window.Remove(removed.Volume);
		}
	}

	/// <summary>The trade's volume grew. Ignored once the trade has left the window.</summary>
	public void Update(TradeKey key, decimal volume)
	{
		if (!_entries.TryGetValue(key, out var entry) || entry.Volume == volume)
			return;

		_window.Remove(entry.Volume);
		_window.Add(volume);
		entry.Volume = volume;
		Evaluate(key, entry);
	}

	private void Evaluate(TradeKey key, Entry entry)
	{
		var qualifies = entry.Threshold is { } threshold && entry.Volume > threshold;

		if (qualifies && entry.Bubble == null)
		{
			entry.Bubble = new Bubble { Key = key, Volume = entry.Volume, Threshold = entry.Threshold.Value };
			Insert(entry.Bubble);
		}
		else if (qualifies)
			entry.Bubble.Volume = entry.Volume;
		else if (entry.Bubble != null)
		{
			_bubbles.Remove(entry.Bubble);
			entry.Bubble = null;
		}
	}

	private void Insert(Bubble bubble)
	{
		// Almost always the newest: append; otherwise keep the time order.
		var index = _bubbles.Count;

		while (index > 0 && _bubbles[index - 1].Time > bubble.Time)
			index--;

		_bubbles.Insert(index, bubble);
	}

	/// <summary>Index of the first bubble at or after <paramref name="time"/>.</summary>
	public int FirstIndexAtOrAfter(DateTime time)
	{
		int lo = 0, hi = _bubbles.Count;

		while (lo < hi)
		{
			var mid = (lo + hi) / 2;

			if (_bubbles[mid].Time < time)
				lo = mid + 1;
			else
				hi = mid;
		}

		return lo;
	}
}
