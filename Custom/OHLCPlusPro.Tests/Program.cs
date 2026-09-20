namespace OHLCPlusPro.Tests;

using System;
using System.Collections.Generic;
using ATAS.Indicators.Technical.OhlcPlusPro.Core;

internal static class Program
{
	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("Step", StepTests),
			("HighNodes", HighTests),
			("LowNodes", LowTests),
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

	/// <summary>Profile from a low price upwards, one entry per step, volumes as given.</summary>
	private static List<PriceVolume> Profile(decimal low, decimal step, params decimal[] volumes)
	{
		var levels = new List<PriceVolume>();

		for (var i = 0; i < volumes.Length; i++)
		{
			if (volumes[i] >= 0m)
				levels.Add(new PriceVolume(low + i * step, volumes[i]));
		}

		return levels;
	}

	private static NodeSettings Settings(decimal high = 70m, decimal low = 20m, int gap = 0, int min = 1)
		=> new() { HighPercent = high, LowPercent = low, GapTolerance = gap, MinLevels = min };

	private static void StepTests(Check c)
	{
		c.Equal(0.25m, VolumeNodes.DetectStep(Profile(100m, 0.25m, 1, 1, 1), 0m), "the smallest gap is the step");

		// A profile grouped by the chart scale has a coarser step than the tick.
		c.Equal(5m, VolumeNodes.DetectStep(Profile(100m, 5m, 1, 1, 1), 0m), "the grouping is read from the profile");

		// Holes in the profile do not stretch the step.
		var withHole = new List<PriceVolume> { new(100m, 1m), new(100.25m, 1m), new(101m, 1m) };
		c.Equal(0.25m, VolumeNodes.DetectStep(withHole, 0m), "a hole is not the step");

		c.Equal(0.25m, VolumeNodes.DetectStep(withHole, 0.25m), "a known step wins over the profile");
		c.Equal(0.25m, VolumeNodes.DetectStep(new List<PriceVolume> { new(100m, 1m) }, 0.25m), "one price and the tick of the instrument");
		c.Equal(0m, VolumeNodes.DetectStep(new List<PriceVolume>(), 0m), "nothing at all");
	}

	private static void HighTests(Check c)
	{
		// Peak 100 at the middle, threshold 70 %.
		var levels = Profile(100m, 1m, 10, 20, 80, 100, 75, 20, 10);
		var bands = VolumeNodes.FindHigh(levels, Settings(min: 1), 1m);

		c.Equal(1, bands.Count, "one band around the peak");
		c.Equal(102m - 0.5m, bands[0].Low, "half a step below the first price of the band");
		c.Equal(104m + 0.5m, bands[0].High, "half a step above the last");
		c.Equal(3, bands[0].Levels, "80, 100 and 75");
		c.Equal(100m, bands[0].PeakVolume, "the peak inside the band");

		// Two separate nodes.
		levels = Profile(100m, 1m, 100, 10, 10, 10, 90);
		bands = VolumeNodes.FindHigh(levels, Settings(min: 1), 1m);
		c.Equal(2, bands.Count, "two nodes, one at each end");
		c.Equal(99.5m, bands[0].Low, "the lower one starts at the bottom");
		c.Equal(104.5m, bands[1].High, "the upper one ends at the top");

		// A single gap joins them when it is tolerated.
		levels = Profile(100m, 1m, 100, 10, 90);
		c.Equal(2, VolumeNodes.FindHigh(levels, Settings(min: 1), 1m).Count, "without tolerance they are two");

		bands = VolumeNodes.FindHigh(levels, Settings(gap: 1, min: 1), 1m);
		c.Equal(1, bands.Count, "one price of tolerance joins them");
		c.Equal(3, bands[0].Levels, "and the band covers the three prices");

		// The tolerance never extends the band past its last price.
		levels = Profile(100m, 1m, 100, 90, 10, 10);
		bands = VolumeNodes.FindHigh(levels, Settings(gap: 2, min: 1), 1m);
		c.Equal(1, bands.Count, "one band");
		c.Equal(101.5m, bands[0].High, "it ends at 101, not at the tolerated prices above");

		// Minimum size.
		levels = Profile(100m, 1m, 100, 10, 10, 90, 85);
		bands = VolumeNodes.FindHigh(levels, Settings(min: 2), 1m);
		c.Equal(1, bands.Count, "the single price is not a band");
		c.Equal(2, bands[0].Levels, "the pair is");

		// Nothing to find.
		c.Equal(0, VolumeNodes.FindHigh(Profile(100m, 1m, 0, 0, 0), Settings(), 1m).Count, "a profile with no volume");
		c.Equal(0, VolumeNodes.FindHigh(new List<PriceVolume>(), Settings(), 1m).Count, "an empty profile");
	}

	private static void LowTests(Check c)
	{
		// Threshold 20 % of 100: the two ends are low nodes.
		var levels = Profile(100m, 1m, 10, 15, 100, 90, 5);
		var bands = VolumeNodes.FindLow(levels, Settings(min: 1), 1m);

		c.Equal(2, bands.Count, "below and above the peak");
		c.Equal(2, bands[0].Levels, "10 and 15");
		c.Equal(1, bands[1].Levels, "and the 5 at the top");

		// A price with no trades at all is the emptiest of them: it has to be seen.
		var withHole = new List<PriceVolume> { new(100m, 100m), new(101m, 90m), new(103m, 80m) };
		bands = VolumeNodes.FindLow(withHole, Settings(min: 1), 1m);

		c.Equal(1, bands.Count, "the price that never traded is a low node");
		c.Equal(101.5m, bands[0].Low, "it covers 102 alone");
		c.Equal(102.5m, bands[0].High, "half a step on each side");

		// Several empty prices in a row are one band.
		withHole = new List<PriceVolume> { new(100m, 100m), new(104m, 90m) };
		bands = VolumeNodes.FindLow(withHole, Settings(min: 1), 1m);
		c.Equal(1, bands.Count, "one band for the whole gap");
		c.Equal(3, bands[0].Levels, "101, 102 and 103");

		// A grouped profile keeps its own step: the gap is one cell, not four ticks.
		withHole = new List<PriceVolume> { new(100m, 100m), new(105m, 10m), new(110m, 90m) };
		bands = VolumeNodes.FindLow(withHole, Settings(min: 1), 0m);
		c.Equal(1, bands.Count, "the middle cell alone");
		c.Equal(5m, bands[0].High - bands[0].Low, "the band is one cell of five points tall");
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
