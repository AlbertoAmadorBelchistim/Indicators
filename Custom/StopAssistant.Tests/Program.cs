namespace StopAssistant.Tests;

using System;
using System.Collections.Generic;
using ATAS.Indicators.Technical.StopAssistantCore;

internal static class Program
{
	/// <summary>NQ: quarter-point tick worth five dollars.</summary>
	private const decimal Tick = 0.25m;

	private const decimal TickCost = 5m;

	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("Ticks", TickTests),
			("Sizing", SizingTests),
			("DailyLimit", LimitTests),
			("Targets", TargetTests),
			("Atr", AtrTests),
			("Candidates", CandidateTests),
			("Scenario", ScenarioTests),
		};

		var failed = 0;

		foreach (var (name, run) in suites)
		{
			var check = new Check(name);
			run(check);
			Console.WriteLine($"{name}: {check.Passed} passed, {check.Failed} failed");
			failed += check.Failed;
		}

		return failed == 0 ? 0 : 1;
	}

	private static void TickTests(Check c)
	{
		c.Equal(40, RiskMath.Ticks(24000m, 23990m, Tick), "ten points are forty ticks");
		c.Equal(40, RiskMath.Ticks(23990m, 24000m, Tick), "and the other way round the same");
		c.Equal(0, RiskMath.Ticks(24000m, 24000m, Tick), "no distance");
		c.Equal(0, RiskMath.Ticks(24000m, 23990m, 0m), "without a tick size there is no count");

		// A level from a profile does not always land on a tick.
		c.Equal(41, RiskMath.Ticks(24000m, 23989.8m, Tick), "a level between ticks rounds to the nearest");

		c.True(RiskMath.IsOnTheRightSide(24000m, 23990m, TradeSide.Long), "a long stops below");
		c.True(!RiskMath.IsOnTheRightSide(24000m, 24010m, TradeSide.Long), "not above");
		c.True(RiskMath.IsOnTheRightSide(24000m, 24010m, TradeSide.Short), "a short stops above");
		c.True(!RiskMath.IsOnTheRightSide(24000m, 24000m, TradeSide.Short), "and never at the entry itself");
	}

	private static void SizingTests(Check c)
	{
		// 40 ticks at five dollars is 200 per contract; 500 of budget fits two.
		var plan = RiskMath.Plan(24000m, 23990m, TradeSide.Long, Tick, TickCost, 500m, 0m, 0m, 0);

		c.Equal(40, plan.Ticks, "forty ticks of stop");
		c.Equal(200m, plan.RiskPerContract, "two hundred per contract");
		c.Equal(2, plan.Contracts, "two contracts fit in five hundred");
		c.Equal(400m, plan.Risk, "and they risk four hundred");
		c.True(plan.IsTradable, "the trade stands");

		// Never rounded up: 2.5 contracts is two.
		plan = RiskMath.Plan(24000m, 23990m, TradeSide.Long, Tick, TickCost, 500m, 0m, 0m, 0);
		c.Equal(2, plan.Contracts, "half a contract does not exist");

		// A stop so wide that not even one fits.
		plan = RiskMath.Plan(24000m, 23900m, TradeSide.Long, Tick, TickCost, 500m, 0m, 0m, 0);
		c.Equal(400, plan.Ticks, "a hundred points");
		c.Equal(0, plan.Contracts, "two thousand per contract does not fit in five hundred");
		c.True(!plan.IsTradable, "so there is no trade");

		// The ceiling of contracts wins when it is lower.
		plan = RiskMath.Plan(24000m, 23990m, TradeSide.Long, Tick, TickCost, 5000m, 0m, 0m, 3);
		c.Equal(3, plan.Contracts, "the maximum caps it");
		c.Equal(600m, plan.Risk, "and the risk with it");

		// A stop on the wrong side is not a stop.
		plan = RiskMath.Plan(24000m, 24010m, TradeSide.Long, Tick, TickCost, 500m, 0m, 0m, 0);
		c.Equal(0, plan.Contracts, "a long does not stop above its entry");

		plan = RiskMath.Plan(24000m, 24010m, TradeSide.Short, Tick, TickCost, 500m, 0m, 0m, 0);
		c.Equal(2, plan.Contracts, "the short with the same stop does");
	}

	private static void LimitTests(Check c)
	{
		// Nothing lost yet: the whole allowance is there.
		c.Equal(1000m, RiskMath.BudgetLeft(1000m, 0m), "the day starts whole");
		c.Equal(400m, RiskMath.BudgetLeft(1000m, -600m), "six hundred lost, four hundred left");
		c.Equal(0m, RiskMath.BudgetLeft(1000m, -1000m), "the limit reached");
		c.Equal(0m, RiskMath.BudgetLeft(1000m, -1500m), "and past it there is nothing, not a negative");
		c.Equal(1000m, RiskMath.BudgetLeft(1000m, 800m), "a winning day does not widen the allowance");

		// The day's wall is lower than the trade's budget: the day decides.
		var plan = RiskMath.Plan(24000m, 23990m, TradeSide.Long, Tick, TickCost, 500m, 1000m, -700m, 0);
		c.Equal(1, plan.Contracts, "only three hundred left: one contract");
		c.Equal(300m, plan.BudgetLeft + plan.Risk, "and the account of what is left adds up");
		c.True(!plan.Blocked, "there is still room");

		// The limit is spent.
		plan = RiskMath.Plan(24000m, 23990m, TradeSide.Long, Tick, TickCost, 500m, 1000m, -1000m, 0);
		c.Equal(0, plan.Contracts, "no allowance, no contracts");
		c.True(plan.Blocked, "and it says why");

		// Without a daily limit the budget of the trade is the only wall.
		plan = RiskMath.Plan(24000m, 23990m, TradeSide.Long, Tick, TickCost, 500m, 0m, -5000m, 0);
		c.Equal(2, plan.Contracts, "no limit set: the loss of the day does not size the trade");
		c.True(!plan.Blocked, "nothing blocks it");
	}

	private static void TargetTests(Check c)
	{
		c.Equal(24010m, RiskMath.Target(24000m, 23990m, TradeSide.Long, 1m), "one R above");
		c.Equal(24020m, RiskMath.Target(24000m, 23990m, TradeSide.Long, 2m), "two R");
		c.Equal(23990m, RiskMath.Target(24000m, 24010m, TradeSide.Short, 1m), "and a short goes down");
		c.Equal(24005m, RiskMath.Target(24000m, 23990m, TradeSide.Long, 0.5m), "half an R");
	}

	private static void AtrTests(Check c)
	{
		var bars = new List<Bar>
		{
			new(100m, 90m, 95m),
			new(105m, 95m, 100m),
			new(110m, 100m, 105m),
			new(115m, 105m, 110m),
		};

		c.Equal(10m, StopFinder.Atr(bars, 3), "three ranges of ten");

		// A gap counts from the previous close, not from the bar alone.
		bars.Add(new Bar(130m, 125m, 128m));
		c.Equal(20m, StopFinder.Atr(bars, 1), "the gap is part of the range");

		c.Equal(0m, StopFinder.Atr(null, 5), "no bars");
		c.Equal(0m, StopFinder.Atr(new List<Bar> { new(1m, 1m, 1m) }, 5), "one bar has no true range");

		c.Equal(95m, StopFinder.Swing(bars, 4, TradeSide.Long), "the lowest low of the last four bars, not of every bar");
		c.Equal(130m, StopFinder.Swing(bars, 4, TradeSide.Short), "and the highest high");
		c.Equal(105m, StopFinder.Swing(bars, 2, TradeSide.Long), "a shorter look back sees less");
	}

	private static void CandidateTests(Check c)
	{
		var levels = new List<(string, decimal)> { ("VAL", 23985m), ("POC", 23995m), ("VAH", 24020m) };

		var candidates = StopFinder.Build(
			entry: 24000m,
			side: TradeSide.Long,
			tickSize: Tick,
			atr: 12m,
			atrMultiplier: 1m,
			swing: 23980m,
			paddingTicks: 4,
			levels: levels,
			manual: 0m);

		// The value area high is above a long's entry: it is not a stop.
		c.Equal(4, candidates.Count, "ATR, swing, POC and VAL");
		c.Equal(StopKind.Level, candidates[0].Kind, "the nearest is the POC");
		c.Equal(23994m, candidates[0].Price, "one point under the POC, four ticks of padding");

		// Sorted by distance, nearest first.
		for (var i = 1; i < candidates.Count; i++)
			c.True(candidates[i].Ticks >= candidates[i - 1].Ticks, "sorted from the nearest");

		c.Equal(23988m, candidates[1].Price, "the ATR at twelve points");
		c.Equal(23979m, candidates[3].Price, "the swing with its padding");

		// The choice picks its kind, and falls back to the nearest when that kind is not there.
		c.True(StopFinder.TryPick(candidates, StopKind.Swing, out var picked), "the swing is there");
		c.Equal(23979m, picked.Price, "and it is the one picked");

		StopFinder.TryPick(candidates, StopKind.Manual, out picked);
		c.Equal(23994m, picked.Price, "no manual stop: the nearest one instead");

		c.True(!StopFinder.TryPick(new List<StopCandidate>(), StopKind.Atr, out _), "nothing to pick");

		// A short mirrors it.
		candidates = StopFinder.Build(24000m, TradeSide.Short, Tick, 12m, 1m, 24020m, 4, levels, 0m);
		c.Equal(3, candidates.Count, "ATR, swing and the value area high");
		c.Equal(24012m, candidates[0].Price, "the ATR above the entry");
	}

	/// <summary>A long on NQ with a thousand of allowance, six hundred already lost.</summary>
	private static void ScenarioTests(Check c)
	{
		var bars = new List<Bar>();

		for (var i = 0; i < 20; i++)
			bars.Add(new Bar(24000m + i, 23990m + i, 23995m + i));

		var atr = StopFinder.Atr(bars, 14);
		var swing = StopFinder.Swing(bars, 20, TradeSide.Long);

		c.Equal(10m, atr, "ten points of range");
		c.Equal(23990m, swing, "the low of the twenty bars on the chart");

		var candidates = StopFinder.Build(24019m, TradeSide.Long, Tick, atr, 1.5m, swing, 4, null, 0m);
		c.Equal(2, candidates.Count, "ATR and swing");

		StopFinder.TryPick(candidates, StopKind.Swing, out var stop);
		c.Equal(23989m, stop.Price, "the swing with four ticks under it");
		c.Equal(120, stop.Ticks, "thirty points away");

		// 84 ticks at five dollars: 420 per contract. Four hundred left of the day.
		var plan = RiskMath.Plan(24019m, stop.Price, TradeSide.Long, Tick, TickCost, 500m, 1000m, -600m, 0);
		c.Equal(600m, plan.RiskPerContract, "six hundred per contract");
		c.Equal(0, plan.Contracts, "which does not fit in the four hundred left of the day");
		c.True(!plan.Blocked, "the day is not over, the stop is simply too wide for what is left");

		// The nearer stop does fit.
		StopFinder.TryPick(candidates, StopKind.Atr, out var tighter);
		plan = RiskMath.Plan(24019m, tighter.Price, TradeSide.Long, Tick, TickCost, 500m, 1000m, -600m, 0);
		c.Equal(60, plan.Ticks, "fifteen points");
		c.Equal(300m, plan.RiskPerContract, "three hundred per contract");
		c.Equal(1, plan.Contracts, "one contract");
		c.Equal(100m, plan.BudgetLeft, "and a hundred of the day left after it");

		c.Equal(24034m, RiskMath.Target(24019m, tighter.Price, TradeSide.Long, 1m), "one R up");
		c.Equal(24049m, RiskMath.Target(24019m, tighter.Price, TradeSide.Long, 2m), "two R");
	}
}

internal sealed class Check
{
	private readonly string _suite;

	public Check(string suite) => _suite = suite;

	public int Passed { get; private set; }

	public int Failed { get; private set; }

	public void True(bool condition, string what)
	{
		if (condition)
		{
			Passed++;
			return;
		}

		Failed++;
		Console.WriteLine($"  FAIL [{_suite}] {what}");
	}

	public void Equal<T>(T expected, T actual, string what)
	{
		True(EqualityComparer<T>.Default.Equals(expected, actual), $"{what}: expected {expected}, got {actual}");
	}
}
