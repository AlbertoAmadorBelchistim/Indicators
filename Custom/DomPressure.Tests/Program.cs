namespace DomPressure.Tests;

using System;
using System.Collections.Generic;
using ATAS.Indicators.Technical.DomPressureCore;

internal static class Program
{
	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("DepthBook", DepthBookTests),
			("PowerStore", PowerStoreTests),
			("Absorption", AbsorptionTests),
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

	private static void DepthBookTests(Check c)
	{
		var book = new DepthBook();
		c.True(!book.TryGetSums(5, out _, out _), "empty book has no sums");

		// Asks 101..105 with volume 1..5, bids 100..96 with volume 10..50.
		for (var i = 0; i < 5; i++)
		{
			book.Apply(true, 101 + i, 1 + i);
			book.Apply(false, 100 - i, 10 * (1 + i));
		}

		c.True(book.TryGetSums(2, out var bid, out var ask), "sums with both sides");
		c.Equal(3m, ask, "two nearest asks = 1 + 2");
		c.Equal(30m, bid, "two nearest bids = 10 + 20");

		book.TryGetSums(0, out bid, out ask);
		c.Equal(15m, ask, "0 levels takes every ask");
		c.Equal(150m, bid, "0 levels takes every bid");

		book.TryGetSums(50, out bid, out ask);
		c.Equal(15m, ask, "more levels than the book takes every ask");
		c.Equal(150m, bid, "more levels than the book takes every bid");

		book.Apply(true, 101, 0);
		book.TryGetSums(2, out _, out ask);
		c.Equal(5m, ask, "removed best ask: 2 + 3");

		book.Apply(false, 100, 7);
		book.TryGetSums(1, out bid, out _);
		c.Equal(7m, bid, "updated best bid");

		book.Apply(false, 100, -1);
		book.TryGetSums(1, out bid, out _);
		c.Equal(20m, bid, "negative volume removes the level");

		for (var i = 0; i < 5; i++)
			book.Apply(true, 101 + i, 0);

		c.True(!book.TryGetSums(5, out _, out _), "no sums with an empty side");
		c.Equal(0, book.AskLevels, "asks empty");

		book.Clear();
		c.Equal(0, book.BidLevels, "cleared");
	}

	private static void PowerStoreTests(Check c)
	{
		var store = new PowerStore();
		var t0 = new DateTime(2026, 9, 20, 10, 0, 0);
		var t1 = t0.AddMinutes(1);

		c.True(!store.TryGet(t0, out _), "nothing stored");

		store.Add(t0, 10);
		store.Add(t0, -20);
		var s = store.Add(t0, 40);

		c.Equal(40m, s.ValueOf(PowerSampling.BarClose), "close is the last value");
		c.Equal(10m, s.ValueOf(PowerSampling.BarAverage), "average of 10, -20, 40");
		c.Equal(3, s.Count, "three samples");

		store.Add(t1, 5);
		c.True(store.TryGet(t0, out var again) && again.Count == 3, "other bars untouched");
		c.Equal(2, store.Count, "two bars");
		c.Equal(0m, default(PowerSample).ValueOf(PowerSampling.BarAverage), "empty sample is 0");

		store.Clear();
		c.Equal(0, store.Count, "cleared");
	}

	private static void AbsorptionTests(Check c)
	{
		c.True(AbsorptionRule.IsAbsorption(1000, -150, 15), "sellers hit a bid wall at 15%");
		c.True(!AbsorptionRule.IsAbsorption(1000, -149, 15), "below the threshold");
		c.True(AbsorptionRule.IsAbsorption(-500, 100, 15), "buyers lift an ask wall");
		c.True(!AbsorptionRule.IsAbsorption(1000, 500, 15), "same sign is not absorption");
		c.True(!AbsorptionRule.IsAbsorption(0, -500, 15), "no power");
		c.True(!AbsorptionRule.IsAbsorption(1000, 0, 15), "no delta");
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
