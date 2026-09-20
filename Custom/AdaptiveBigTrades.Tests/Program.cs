namespace AdaptiveBigTrades.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using ATAS.Indicators.Technical.BigTrades;

internal static class Program
{
	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("RollingPercentile", PercentileTests),
			("BigTradeEngine", EngineTests),
			("HistoryVsRealtime", EquivalenceTests),
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

	private static readonly DateTime T0 = new(2026, 9, 21, 15, 30, 0);

	private static TradeKey Key(int i, int side = 1) => new(T0.AddMilliseconds(i * 10), 5000m + i % 7, side);

	private static void PercentileTests(Check c)
	{
		var p = new RollingPercentile();
		c.Equal(0m, p.Quantile(80), "empty");

		for (var v = 1; v <= 10; v++)
			p.Add(v);

		c.Equal(8m, p.Quantile(80), "80% of 1..10");
		c.Equal(10m, p.Quantile(100), "100%");
		c.Equal(1m, p.Quantile(1), "1% is the smallest");

		p.Remove(10);
		p.Remove(9);
		c.Equal(7m, p.Quantile(80), "80% of 1..8 (ceil 6.4 = 7th)");
		p.Remove(42);
		c.Equal(8, p.Count, "removing an absent volume is ignored");

		var q = new RollingPercentile();

		for (var i = 0; i < 90; i++)
			q.Add(1);

		for (var i = 0; i < 10; i++)
			q.Add(50);

		c.Equal(1m, q.Quantile(80), "mostly one lot: threshold 1");
		c.Equal(50m, q.Quantile(95), "95% reaches the block trades");
	}

	private static void EngineTests(Check c)
	{
		var e = new BigTradeEngine(windowTrades: 100, percentile: 80, minTrades: 10);

		for (var i = 0; i < 9; i++)
			e.Add(Key(i), 1);

		e.Add(Key(9), 100);
		c.Equal(0, e.Bubbles.Count, "no bubbles before the window has the minimum trades");
		c.True(e.CurrentThreshold.HasValue, "threshold available with 10 trades");

		e.Add(Key(10), 1);
		c.Equal(0, e.Bubbles.Count, "equal to the threshold is not big");

		e.Add(Key(11), 5);
		c.Equal(1, e.Bubbles.Count, "above the threshold");
		c.Equal(1m, e.Bubbles[0].Threshold, "compared with the threshold before joining");

		e.Add(Key(11), 7);
		c.Equal(1, e.Bubbles.Count, "the same key updates, no second bubble");
		c.Equal(7m, e.Bubbles[0].Volume, "bubble volume follows the update");

		e.Add(Key(12), 1);
		e.Update(Key(12), 3);
		c.Equal(2, e.Bubbles.Count, "a trade that grows past its threshold becomes big");

		var w = new BigTradeEngine(windowTrades: 5, percentile: 50, minTrades: 1);

		for (var i = 0; i < 20; i++)
			w.Add(Key(i), i + 1);

		c.Equal(5, w.TradesInWindow, "window keeps the last 5 trades");
		c.Equal(18m, w.CurrentThreshold ?? -1, "median of 16..20");
		w.Update(Key(0), 1000);
		c.Equal(18m, w.CurrentThreshold ?? -1, "an update of an evicted trade is ignored");

		var o = new BigTradeEngine(10, 50, 1);
		o.Add(Key(5), 1);
		o.Add(Key(6), 10);
		o.Add(Key(2), 20);
		c.True(o.Bubbles.Select(b => b.Time).SequenceEqual(o.Bubbles.Select(b => b.Time).OrderBy(t => t)), "bubbles stay in time order");
		c.Equal(1, o.FirstIndexAtOrAfter(Key(6).Time), "first bubble at or after a time");
	}

	// The same trades, once final (history) and once growing through updates (realtime), must
	// give the same bubbles.
	private static void EquivalenceTests(Check c)
	{
		var rnd = new Random(11);
		var mismatches = 0;

		for (var scenario = 0; scenario < 300; scenario++)
		{
			var count = rnd.Next(50, 800);
			var window = rnd.Next(10, 300);
			var percentile = rnd.Next(50, 99);
			var minTrades = rnd.Next(1, 60);
			var trades = new List<(TradeKey Key, decimal[] Steps)>();

			for (var i = 0; i < count; i++)
			{
				var final = rnd.Next(10) == 0 ? rnd.Next(20, 200) : rnd.Next(1, 6);
				var steps = new List<decimal>();
				var v = 0m;

				while (v < final)
				{
					v = Math.Min(final, v + rnd.Next(1, 30));
					steps.Add(v);
				}

				trades.Add((Key(i, rnd.Next(2) == 0 ? 1 : -1), steps.ToArray()));
			}

			var history = new BigTradeEngine(window, percentile, minTrades);
			var live = new BigTradeEngine(window, percentile, minTrades);

			foreach (var (key, steps) in trades)
			{
				history.Add(key, steps[^1]);
				live.Add(key, steps[0]);

				foreach (var s in steps.Skip(1))
					live.Update(key, s);
			}

			var a = string.Join(";", history.Bubbles.Select(b => $"{b.Key}:{b.Volume}:{b.Threshold}"));
			var b = string.Join(";", live.Bubbles.Select(b => $"{b.Key}:{b.Volume}:{b.Threshold}"));

			if (a != b)
				mismatches++;
		}

		c.Equal(0, mismatches, "300 scenarios, history and realtime bubbles equal");
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
