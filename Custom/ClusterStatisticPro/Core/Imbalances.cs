namespace ATAS.Indicators.Technical.ClusterStatsCore;

using System;
using System.Collections.Generic;

/// <summary>Traded volume at one price of a bar.</summary>
public readonly record struct PriceLevel(decimal Price, decimal Ask, decimal Bid);

public sealed class ImbalanceSettings
{
	/// <summary>Minimum ratio between the dominant and the other volume, in percent (300 = 3 to 1).</summary>
	public decimal RatioPercent { get; set; } = 300m;

	/// <summary>Minimum volume of the dominant side.</summary>
	public decimal MinDominantVolume { get; set; } = 30m;

	/// <summary>Minimum difference between the dominant and the other volume.</summary>
	public decimal MinDifference { get; set; } = 30m;

	/// <summary>Consecutive imbalanced levels that make a stack.</summary>
	public int StackedMinLevels { get; set; } = 3;
}

/// <summary>Imbalances of one bar: levels and stacks (runs of consecutive levels) on each side.</summary>
public readonly record struct ImbalanceCounts(int Buy, int Sell, int StackedBuy, int StackedSell)
{
	public int Net => Buy - Sell;

	public int StackedNet => StackedBuy - StackedSell;
}

/// <summary>
/// Diagonal imbalances of a bar. A buy imbalance is an ask volume that dominates the bid volume one
/// level below it; a sell imbalance is a bid volume that dominates the ask volume one level above
/// it. A side with no volume at all is not compared (no ratio against zero). A stack is a run of
/// at least <see cref="ImbalanceSettings.StackedMinLevels"/> consecutive imbalanced levels of the
/// same side; a price level without trades between two levels breaks the run.
/// </summary>
public static class ImbalanceCounter
{
	public static ImbalanceCounts Count(IReadOnlyList<PriceLevel> levels, ImbalanceSettings settings)
	{
		if (levels.Count < 2)
			return default;

		var sorted = new List<PriceLevel>(levels);
		sorted.Sort((a, b) => b.Price.CompareTo(a.Price));

		// The level step of the bar: the tick size, or the cluster size when the levels are grouped.
		var step = decimal.MaxValue;

		for (var i = 1; i < sorted.Count; i++)
		{
			var gap = sorted[i - 1].Price - sorted[i].Price;

			if (gap > 0m && gap < step)
				step = gap;
		}

		var ratio = settings.RatioPercent / 100m;
		var stackMin = Math.Max(2, settings.StackedMinLevels);
		int buy = 0, sell = 0, stackedBuy = 0, stackedSell = 0, buyRun = 0, sellRun = 0;

		for (var i = 1; i < sorted.Count; i++)
		{
			var upper = sorted[i - 1];
			var lower = sorted[i];

			if (upper.Price - lower.Price != step)
			{
				// Not neighbours: a price without trades in between ends any run.
				buyRun = sellRun = 0;
				continue;
			}

			var isBuy = IsDominant(upper.Ask, lower.Bid, ratio, settings);
			var isSell = !isBuy && IsDominant(lower.Bid, upper.Ask, ratio, settings);

			if (isBuy)
			{
				buy++;
				sellRun = 0;

				if (++buyRun == stackMin)
					stackedBuy++;
			}
			else if (isSell)
			{
				sell++;
				buyRun = 0;

				if (++sellRun == stackMin)
					stackedSell++;
			}
			else
				buyRun = sellRun = 0;
		}

		return new ImbalanceCounts(buy, sell, stackedBuy, stackedSell);
	}

	private static bool IsDominant(decimal dominant, decimal other, decimal ratio, ImbalanceSettings settings)
	{
		return other > 0m
			&& dominant / other >= ratio
			&& dominant >= settings.MinDominantVolume
			&& dominant - other >= settings.MinDifference;
	}
}

/// <summary>
/// Alert when the net imbalance of a bar crosses a threshold (by absolute value), at most once per
/// bar. Live: while the bar forms, from inside to outside the threshold. Closed: once per bar,
/// when the next bar opens, if the closed bar is outside and the one before it was not.
/// </summary>
public sealed class NetImbalanceAlert
{
	private int _lastAlertBar = -1;
	private int _liveBar = -1;
	private int _previousLive;

	public void Reset()
	{
		_lastAlertBar = -1;
		_liveBar = -1;
		_previousLive = 0;
	}

	/// <summary>Live mode, called on every update of the bar in progress. True to alert.</summary>
	public bool OnLiveUpdate(int bar, int net, int threshold)
	{
		if (bar != _liveBar)
		{
			_liveBar = bar;
			_previousLive = 0;
		}

		var fire = _lastAlertBar != bar && Math.Abs(_previousLive) < threshold && Math.Abs(net) >= threshold;
		_previousLive = net;

		if (fire)
			_lastAlertBar = bar;

		return fire;
	}

	/// <summary>Closed mode, called with the last closed bar and the one before it. True to alert.</summary>
	public bool OnBarClosed(int closedBar, int net, int previousNet, int threshold)
	{
		if (closedBar < 0 || closedBar == _lastAlertBar)
			return false;

		_lastAlertBar = closedBar;
		return Math.Abs(net) >= threshold && Math.Abs(previousNet) < threshold;
	}
}
