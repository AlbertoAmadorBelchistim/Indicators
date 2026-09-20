namespace RelativeVolume.Tests;

using System;
using System.Collections.Generic;
using ATAS.Indicators.Technical.RelativeVolumeCore;

internal static class Program
{
	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("Reference", ReferenceTests),
			("Sample", SampleTests),
			("Ratio", RatioTests),
			("Prorate", ProrateTests),
			("Sessions", SessionTests),
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

	private static RvolEngine Engine(int minSessions = 3, int maxSessions = 20)
		=> new() { MinSessions = minSessions, MaxSessions = maxSessions };

	/// <summary>Adds one session whose bar at every minute carries the given volume.</summary>
	private static void AddSession(RvolEngine engine, decimal volumePerBar, int minutes = 3)
	{
		var cumulative = 0m;

		for (var minute = 0; minute < minutes; minute++)
		{
			cumulative += volumePerBar;
			engine.Add(minute, cumulative, volumePerBar, volumePerBar / 10m);
		}
	}

	private static void ReferenceTests(Check c)
	{
		var e = Engine();

		AddSession(e, 100m);
		AddSession(e, 200m);
		AddSession(e, 300m);

		var bar = e.Get(RvolMetric.BarVolume, 0);
		c.Equal(200m, bar.Median, "the middle session of three");
		c.Equal(3, bar.Sessions, "three sessions behind it");

		var cum = e.Get(RvolMetric.CumulativeVolume, 2);
		c.Equal(600m, cum.Median, "three bars of 200 accumulated");

		var range = e.Get(RvolMetric.BarRange, 1);
		c.Equal(20m, range.Median, "the range travels on its own");

		// Percentiles by nearest rank: every number is one a session really had.
		AddSession(e, 400m);
		bar = e.Get(RvolMetric.BarVolume, 0);
		c.Equal(200m, bar.Median, "four sessions: 100, 200, 300, 400");
		c.Equal(100m, bar.Low, "the 25th percentile");
		c.Equal(300m, bar.High, "the 75th");

		// A moment of the day nobody reached has nothing to say.
		c.True(e.Get(RvolMetric.BarVolume, 99).IsEmpty, "a minute that never traded");
	}

	private static void SampleTests(Check c)
	{
		var e = Engine(minSessions: 5);

		AddSession(e, 100m);
		AddSession(e, 200m);
		AddSession(e, 300m);
		AddSession(e, 400m);

		c.True(e.Get(RvolMetric.BarVolume, 0).IsEmpty, "four sessions are not five: nothing is said");

		AddSession(e, 500m);
		var bar = e.Get(RvolMetric.BarVolume, 0);
		c.True(!bar.IsEmpty, "the fifth one opens it");
		c.Equal(5, bar.Sessions, "and it says how many");

		// Only the most recent sessions are kept.
		e = Engine(minSessions: 1, maxSessions: 3);
		AddSession(e, 10m);
		AddSession(e, 20m);
		AddSession(e, 30m);
		AddSession(e, 40m);

		bar = e.Get(RvolMetric.BarVolume, 0);
		c.Equal(3, bar.Sessions, "the oldest one drops out");
		c.Equal(30m, bar.Median, "20, 30, 40");

		e.Clear();
		c.Equal(0, e.Buckets, "cleared");
	}

	private static void RatioTests(Check c)
	{
		c.Equal(2m, RvolEngine.Ratio(200m, 100m), "twice the usual");
		c.Equal(0.5m, RvolEngine.Ratio(50m, 100m), "half");
		c.Equal(1m, RvolEngine.Ratio(100m, 100m), "an ordinary moment");

		// No reference is not the same as no activity.
		c.Equal(0m, RvolEngine.Ratio(500m, 0m), "without a reference there is no ratio");
		c.Equal(0m, RvolEngine.Ratio(0m, 100m), "nothing traded");
	}

	private static void ProrateTests(Check c)
	{
		var stats = new Stats(100m, 60m, 140m, 12);

		// A bar a quarter of the way through has traded a quarter of what it will.
		var quarter = stats.Prorate(0.25m);
		c.Equal(25m, quarter.Median, "a quarter of the reference");
		c.Equal(15m, quarter.Low, "and of the band");
		c.Equal(12, quarter.Sessions, "the sample does not change");

		c.Equal(100m, stats.Prorate(1m).Median, "a finished bar is the whole reference");
		c.Equal(100m, stats.Prorate(2m).Median, "and never more than the whole");
		c.Equal(0m, stats.Prorate(0m).Median, "a bar that just opened");
		c.Equal(0m, stats.Prorate(-1m).Median, "nor less than nothing");
	}

	private static void SessionTests(Check c)
	{
		// Four normal days and a half day: the short one is left out.
		var kept = SessionQuality.SelectFull(new[] { 405, 405, 210, 405, 405 }, 0.6m);
		c.Equal(5, kept.Length, "one answer per session");
		c.True(kept[0] && kept[1] && kept[3] && kept[4], "the full days count");
		c.True(!kept[2], "the half day does not");

		// A session slightly shorter is not a half day.
		kept = SessionQuality.SelectFull(new[] { 405, 405, 380, 405 }, 0.6m);
		c.True(kept[2], "twenty minutes short is still a session");

		kept = SessionQuality.SelectFull(new[] { 405, 210, 405 }, 0m);
		c.True(kept[1], "without a filter everything counts");

		c.Equal(0, SessionQuality.SelectFull(Array.Empty<int>(), 0.6m).Length, "no sessions");
		c.Equal(0, SessionQuality.SelectFull(null, 0.6m).Length, "nothing at all");
	}

	/// <summary>A morning that starts quiet and turns busy, against ten ordinary sessions.</summary>
	private static void ScenarioTests(Check c)
	{
		var e = Engine(minSessions: 10, maxSessions: 20);

		// Ten past sessions: 1000 lots in the first minute, 500 in each of the next two.
		for (var session = 0; session < 10; session++)
		{
			e.Add(0, 1000m, 1000m, 10m);
			e.Add(1, 1500m, 500m, 6m);
			e.Add(2, 2000m, 500m, 6m);
		}

		var open = e.Get(RvolMetric.BarVolume, 0);
		c.Equal(1000m, open.Median, "the open is always busy");
		c.Equal(10, open.Sessions, "ten sessions");

		// Today: an ordinary open and then a minute with three times the usual.
		c.Equal(1m, RvolEngine.Ratio(1000m, open.Median), "the open is ordinary");

		var second = e.Get(RvolMetric.BarVolume, 1);
		c.Equal(3m, RvolEngine.Ratio(1500m, second.Median), "and the next minute is three times the usual");

		// Cumulative volume smooths it: 2500 against 1500 is far less dramatic.
		var cum = e.Get(RvolMetric.CumulativeVolume, 1);
		c.Equal(1500m, cum.Median, "the usual by that minute");
		c.Near(1.67m, RvolEngine.Ratio(2500m, cum.Median), 0.01m, "the session as a whole is busy, not wild");

		// Half way through the second bar, the comparison is against half the reference.
		var half = second.Prorate(0.5m);
		c.Equal(250m, half.Median, "half a reference bar");
		c.Equal(2m, RvolEngine.Ratio(500m, half.Median), "500 traded in half a bar is twice the usual pace");
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

	public void Near(decimal expected, decimal actual, decimal tolerance, string what)
	{
		True(Math.Abs(expected - actual) <= tolerance, $"{what}: expected {expected} +- {tolerance}, got {actual}");
	}
}
