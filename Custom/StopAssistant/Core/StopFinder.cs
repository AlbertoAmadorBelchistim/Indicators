namespace ATAS.Indicators.Technical.StopAssistantCore;

using System;
using System.Collections.Generic;

/// <summary>Where a stop candidate comes from.</summary>
public enum StopKind
{
	/// <summary>A multiple of the average true range.</summary>
	Atr,

	/// <summary>Beyond the last swing of the price.</summary>
	Swing,

	/// <summary>Beyond a level of the session profile: POC, value area high or low.</summary>
	Level,

	/// <summary>A price written by hand.</summary>
	Manual,
}

/// <summary>One possible stop, already on the right side of the entry.</summary>
public readonly record struct StopCandidate(StopKind Kind, string Label, decimal Price, int Ticks);

/// <summary>One bar, as far as the stop candidates care.</summary>
public readonly record struct Bar(decimal High, decimal Low, decimal Close);

public static class StopFinder
{
	/// <summary>
	/// Average true range over the last <paramref name="period"/> bars, as a plain mean of the true
	/// ranges. The bars arrive oldest first and the last one is the most recent.
	/// </summary>
	public static decimal Atr(IReadOnlyList<Bar> bars, int period)
	{
		if (bars == null || bars.Count < 2 || period < 1)
			return 0m;

		var count = 0;
		var sum = 0m;

		for (var i = bars.Count - 1; i > 0 && count < period; i--)
		{
			var bar = bars[i];
			var previousClose = bars[i - 1].Close;

			var range = Math.Max(bar.High, previousClose) - Math.Min(bar.Low, previousClose);

			sum += range;
			count++;
		}

		return count == 0 ? 0m : sum / count;
	}

	/// <summary>
	/// Lowest low, or highest high, of the last <paramref name="lookback"/> bars: the swing a stop
	/// hides behind. Nothing here guesses where the swing "should" be.
	/// </summary>
	public static decimal Swing(IReadOnlyList<Bar> bars, int lookback, TradeSide side)
	{
		if (bars == null || bars.Count == 0 || lookback < 1)
			return 0m;

		var from = Math.Max(0, bars.Count - lookback);
		var extreme = side == TradeSide.Long ? decimal.MaxValue : decimal.MinValue;

		for (var i = from; i < bars.Count; i++)
		{
			if (side == TradeSide.Long)
			{
				if (bars[i].Low < extreme)
					extreme = bars[i].Low;
			}
			else if (bars[i].High > extreme)
			{
				extreme = bars[i].High;
			}
		}

		return extreme == decimal.MaxValue || extreme == decimal.MinValue ? 0m : extreme;
	}

	/// <summary>
	/// Every candidate that sits on the right side of the entry, nearest first. A candidate on the
	/// wrong side is dropped rather than flipped: a stop above the entry of a long is not a stop.
	/// </summary>
	public static List<StopCandidate> Build(
		decimal entry,
		TradeSide side,
		decimal tickSize,
		decimal atr,
		decimal atrMultiplier,
		decimal swing,
		int paddingTicks,
		IReadOnlyList<(string Label, decimal Price)> levels,
		decimal manual)
	{
		var candidates = new List<StopCandidate>();
		var padding = Math.Max(paddingTicks, 0) * tickSize;
		var sign = side == TradeSide.Long ? -1m : 1m;

		if (atr > 0m && atrMultiplier > 0m)
			Add(candidates, StopKind.Atr, $"{atrMultiplier:0.##} x ATR", entry + sign * atr * atrMultiplier, entry, side, tickSize);

		if (swing > 0m)
			Add(candidates, StopKind.Swing, "Swing", swing + sign * padding, entry, side, tickSize);

		if (levels != null)
		{
			foreach (var (label, price) in levels)
			{
				if (price > 0m)
					Add(candidates, StopKind.Level, label, price + sign * padding, entry, side, tickSize);
			}
		}

		if (manual > 0m)
			Add(candidates, StopKind.Manual, "Manual", manual, entry, side, tickSize);

		candidates.Sort(static (a, b) => a.Ticks.CompareTo(b.Ticks));

		return candidates;
	}

	/// <summary>The candidate a choice points at, or the nearest one when it is not there.</summary>
	public static bool TryPick(IReadOnlyList<StopCandidate> candidates, StopKind kind, out StopCandidate picked)
	{
		picked = default;

		if (candidates == null || candidates.Count == 0)
			return false;

		foreach (var candidate in candidates)
		{
			if (candidate.Kind == kind)
			{
				picked = candidate;
				return true;
			}
		}

		picked = candidates[0];
		return true;
	}

	private static void Add(List<StopCandidate> into, StopKind kind, string label, decimal price, decimal entry, TradeSide side, decimal tickSize)
	{
		if (price <= 0m || !RiskMath.IsOnTheRightSide(entry, price, side))
			return;

		into.Add(new StopCandidate(kind, label, price, RiskMath.Ticks(entry, price, tickSize)));
	}
}
