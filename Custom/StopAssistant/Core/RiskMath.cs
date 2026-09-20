namespace ATAS.Indicators.Technical.StopAssistantCore;

using System;

/// <summary>Direction of the trade being sized.</summary>
public enum TradeSide
{
	Long = 1,
	Short = -1,
}

/// <summary>
/// What a stop costs and how many contracts fit behind it. Everything here is arithmetic: no
/// number in this file is a forecast, and none of them says whether the trade is a good idea.
/// </summary>
public readonly record struct RiskPlan(
	decimal Entry,
	decimal Stop,
	int Ticks,
	decimal RiskPerContract,
	int Contracts,
	decimal Risk,
	decimal BudgetUsed,
	decimal BudgetLeft,
	bool Blocked)
{
	/// <summary>The stop is valid and at least one contract fits.</summary>
	public bool IsTradable => Ticks > 0 && Contracts > 0;
}

public static class RiskMath
{
	/// <summary>Ticks between two prices, always positive. Zero when the tick size is unknown.</summary>
	public static int Ticks(decimal from, decimal to, decimal tickSize)
	{
		if (tickSize <= 0m)
			return 0;

		var ticks = Math.Abs(from - to) / tickSize;

		// Prices arrive rounded to the tick, but a level from a profile or an average price may not.
		return (int)Math.Round(ticks, MidpointRounding.AwayFromZero);
	}

	/// <summary>A stop belongs below the entry when long and above it when short.</summary>
	public static bool IsOnTheRightSide(decimal entry, decimal stop, TradeSide side)
		=> side == TradeSide.Long ? stop < entry : stop > entry;

	/// <summary>Price at a multiple of the risk taken, on the other side of the entry.</summary>
	public static decimal Target(decimal entry, decimal stop, TradeSide side, decimal r)
	{
		var distance = Math.Abs(entry - stop) * r;

		return side == TradeSide.Long ? entry + distance : entry - distance;
	}

	/// <summary>
	/// What is left of the allowance for the day. The loss so far arrives as the platform reports
	/// it, negative when money was lost, so it is added rather than subtracted.
	/// </summary>
	public static decimal BudgetLeft(decimal dailyLimit, decimal realizedToday)
	{
		if (dailyLimit <= 0m)
			return 0m;

		var left = dailyLimit + Math.Min(realizedToday, 0m);

		return left > 0m ? left : 0m;
	}

	/// <summary>
	/// The plan for one stop. The budget is what the trade may lose; when a daily allowance is
	/// given, whichever of the two is smaller decides, because the day's limit is the real wall.
	/// </summary>
	public static RiskPlan Plan(
		decimal entry,
		decimal stop,
		TradeSide side,
		decimal tickSize,
		decimal tickCost,
		decimal budget,
		decimal dailyLimit,
		decimal realizedToday,
		int maxContracts)
	{
		var ticks = Ticks(entry, stop, tickSize);
		var riskPerContract = ticks * tickCost;

		var left = dailyLimit > 0m ? BudgetLeft(dailyLimit, realizedToday) : decimal.MaxValue;
		var usable = dailyLimit > 0m ? Math.Min(budget, left) : budget;

		var blocked = dailyLimit > 0m && left <= 0m;

		if (ticks <= 0 || riskPerContract <= 0m || usable <= 0m || !IsOnTheRightSide(entry, stop, side))
			return new RiskPlan(entry, stop, ticks, riskPerContract, 0, 0m, 0m, dailyLimit > 0m ? left : 0m, blocked);

		var contracts = (int)Math.Floor(usable / riskPerContract);

		if (maxContracts > 0 && contracts > maxContracts)
			contracts = maxContracts;

		if (contracts < 0)
			contracts = 0;

		var risk = contracts * riskPerContract;

		return new RiskPlan(
			entry,
			stop,
			ticks,
			riskPerContract,
			contracts,
			risk,
			risk,
			dailyLimit > 0m ? left - risk : 0m,
			blocked);
	}
}
