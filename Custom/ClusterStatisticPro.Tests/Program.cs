namespace ClusterStatisticPro.Tests;

using System;
using System.Collections.Generic;
using ATAS.Indicators.Technical.ClusterStatsCore;

internal static partial class Program
{
	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("Imbalances", ImbalanceTests),
			("NetImbalanceAlert", AlertTests),
		};

		AddSuites(suites);

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

	static partial void AddSuites(List<(string Name, Action<Check> Run)> suites);

	private static readonly ImbalanceSettings Default = new() { RatioPercent = 300, MinDominantVolume = 30, MinDifference = 30, StackedMinLevels = 3 };

	// Levels from the top price down, as (ask, bid) pairs one tick apart.
	private static List<PriceLevel> Levels(decimal top, decimal tick, params (decimal Ask, decimal Bid)[] rows)
	{
		var list = new List<PriceLevel>();

		for (var i = 0; i < rows.Length; i++)
			list.Add(new PriceLevel(top - i * tick, rows[i].Ask, rows[i].Bid));

		return list;
	}

	private static void ImbalanceTests(Check c)
	{
		// Upper ask 90 against lower bid 30: 300%, dominant 90 >= 30, difference 60 >= 30.
		var r = ImbalanceCounter.Count(Levels(100, 1, (90, 5), (10, 30)), Default);
		c.Equal(1, r.Buy, "buy imbalance at exactly 300%");
		c.Equal(0, r.Sell, "no sell");

		r = ImbalanceCounter.Count(Levels(100, 1, (89, 5), (10, 30)), Default);
		c.Equal(0, r.Buy, "below the ratio");

		r = ImbalanceCounter.Count(Levels(100, 1, (10, 5), (1, 40)), Default);
		c.Equal(1, r.Sell, "lower bid 40 against upper ask 10: sell");

		r = ImbalanceCounter.Count(Levels(100, 1, (29, 5), (10, 9)), Default);
		c.Equal(0, r.Buy, "dominant below the minimum volume");

		r = ImbalanceCounter.Count(Levels(100, 1, (45, 5), (10, 15)), new ImbalanceSettings { RatioPercent = 300, MinDominantVolume = 30, MinDifference = 31, StackedMinLevels = 3 });
		c.Equal(0, r.Buy, "difference below the minimum");

		r = ImbalanceCounter.Count(Levels(100, 1, (500, 5), (10, 0)), Default);
		c.Equal(0, r.Buy, "no ratio against a zero bid");

		// Three consecutive buy imbalances make one stack; a fourth one is the same stack.
		var run = Levels(100, 1, (90, 0), (90, 30), (90, 30), (90, 30), (0, 30));
		r = ImbalanceCounter.Count(run, Default);
		c.Equal(4, r.Buy, "four buy levels");
		c.Equal(1, r.StackedBuy, "one stack");

		// Two runs of three separated by a neutral pair: two stacks.
		var two = Levels(100, 1, (90, 0), (90, 30), (90, 30), (1, 30), (0, 1), (90, 0), (90, 30), (90, 30), (0, 30));
		r = ImbalanceCounter.Count(two, Default);
		c.Equal(2, r.StackedBuy, "two stacks");
		c.Equal(6, r.Buy, "six buy levels");

		// A price without trades between levels breaks the run.
		var gap = new List<PriceLevel> { new(100, 90, 0), new(99, 90, 30), new(98, 90, 30), new(96, 90, 30), new(95, 0, 30) };
		r = ImbalanceCounter.Count(gap, Default);
		c.Equal(3, r.Buy, "levels next to each other only");
		c.Equal(0, r.StackedBuy, "the gap breaks the stack");

		// Grouped levels (4 ticks of 0.25 = 1.00 apart) are neighbours of each other.
		var grouped = Levels(5000, 1m, (90, 0), (90, 30), (90, 30), (0, 30));
		r = ImbalanceCounter.Count(grouped, Default);
		c.Equal(3, r.Buy, "grouped levels");
		c.Equal(1, r.StackedBuy, "grouped stack");

		// Order of the input does not matter.
		var shuffled = new List<PriceLevel>(run);
		shuffled.Reverse();
		c.Equal(ImbalanceCounter.Count(run, Default), ImbalanceCounter.Count(shuffled, Default), "input order");

		// Net values.
		var mixed = Levels(100, 1, (90, 0), (90, 30), (1, 30), (1, 40), (0, 40));
		r = ImbalanceCounter.Count(mixed, Default);
		c.Equal(2, r.Buy, "mixed buy");
		c.Equal(2, r.Sell, "mixed sell");
		c.Equal(0, r.Net, "net 0");

		c.Equal(default(ImbalanceCounts), ImbalanceCounter.Count(Levels(100, 1, (90, 30)), Default), "single level");
	}

	private static void AlertTests(Check c)
	{
		var live = new NetImbalanceAlert();
		c.True(!live.OnLiveUpdate(10, 3, 5), "inside");
		c.True(live.OnLiveUpdate(10, 5, 5), "crosses out");
		c.True(!live.OnLiveUpdate(10, 2, 5), "back inside");
		c.True(!live.OnLiveUpdate(10, -6, 5), "once per bar");
		c.True(live.OnLiveUpdate(11, -6, 5), "a new bar starts from zero: reaching -6 at once is a crossing");
		c.True(!live.OnLiveUpdate(12, 1, 5), "inside on bar 12");
		c.True(live.OnLiveUpdate(12, -7, 5), "sellers cross out on bar 12");

		var closed = new NetImbalanceAlert();
		c.True(closed.OnBarClosed(20, 6, 2, 5), "closed bar outside, previous inside");
		c.True(!closed.OnBarClosed(20, 6, 2, 5), "same bar only once");
		c.True(!closed.OnBarClosed(21, -8, 6, 5), "previous already outside");
		c.True(!closed.OnBarClosed(22, 4, -8, 5), "inside");
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
