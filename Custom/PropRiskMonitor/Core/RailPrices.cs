namespace ATAS.Indicators.Technical.PropRisk;

using System;

public enum RailSource
{
	None,
	DailyLoss,
	Trailing,
	DailyTarget
}

public readonly record struct RailPrice(decimal Price, RailSource Source, decimal Equity);

/// <summary>
/// Prices at which the open position of the chart instrument takes the account equity to a limit.
/// The equity moves linearly with the price from the current one: equity(p) = equity + quantity *
/// (p - price) * tick cost / tick size. Other positions of the account are assumed not to move.
/// </summary>
public static class RailPriceSolver
{
	/// <summary>
	/// Stop price for the most restrictive of the daily stop and the trailing stop (the higher
	/// equity). Rounded to the tick towards the current price, so the rail is reached before the
	/// limit. Null when flat, with no limit, or when the limit is already crossed.
	/// </summary>
	public static RailPrice? Stop(decimal quantity, decimal price, decimal equity, decimal tickSize, decimal tickCost,
		decimal? dailyStopEquity, decimal? trailingStopEquity)
	{
		RailSource source;
		decimal floor;

		if (dailyStopEquity is { } daily && (trailingStopEquity is not { } t || daily >= t))
		{
			floor = daily;
			source = RailSource.DailyLoss;
		}
		else if (trailingStopEquity is { } trailing)
		{
			floor = trailing;
			source = RailSource.Trailing;
		}
		else
			return null;

		if (equity <= floor)
			return null;

		var p = Solve(quantity, price, equity, tickSize, tickCost, floor, towardsPrice: true);
		return p is { } stop ? new RailPrice(stop, source, floor) : null;
	}

	/// <summary>Price at which the equity reaches the target, rounded so that it is reached at that price.</summary>
	public static RailPrice? Target(decimal quantity, decimal price, decimal equity, decimal tickSize, decimal tickCost, decimal? targetEquity)
	{
		if (targetEquity is not { } target || equity >= target)
			return null;

		var p = Solve(quantity, price, equity, tickSize, tickCost, target, towardsPrice: false);
		return p is { } value ? new RailPrice(value, RailSource.DailyTarget, target) : null;
	}

	private static decimal? Solve(decimal quantity, decimal price, decimal equity, decimal tickSize, decimal tickCost, decimal level, bool towardsPrice)
	{
		if (quantity == 0m || tickSize <= 0m || tickCost <= 0m)
			return null;

		var exact = price + (level - equity) / (quantity * tickCost / tickSize);

		if (exact <= 0m)
			return null;

		var ticks = exact / tickSize;

		// Towards the price: a long's stop (below) rounds up, a short's stop (above) rounds down.
		// Away from the price: a long's target (above) rounds up, a short's target (below) rounds down.
		var roundUp = towardsPrice ? exact < price : exact > price;
		var rounded = (roundUp ? Math.Ceiling(ticks) : Math.Floor(ticks)) * tickSize;
		return rounded;
	}
}
