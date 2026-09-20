namespace EquivalentStrikes.Tests;

using System;
using System.Collections.Generic;
using ATAS.Indicators.Technical.EquivalentStrikesCore;

internal static class Program
{
	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("Map", MapTests),
			("Grid", GridTests),
			("Anchor", AnchorTests),
			("Presets", PresetTests),
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

	private static StrikeGrid Grid(decimal factor, decimal basis, decimal spacing, decimal major = 0m, int max = 100)
		=> new() { Factor = factor, Basis = basis, Spacing = spacing, MajorSpacing = major, MaxLines = max };

	private static void MapTests(Check c)
	{
		var g = Grid(41m, 150m, 1m);

		c.Equal(20650m, g.ToPrice(500m), "500 * 41 + 150");
		c.Equal(500m, g.ToStrike(20650m), "back to the underlying");
		c.Equal(150m, g.ToPrice(0m), "the basis is the price of strike zero");

		g.Factor = 1m;
		g.Basis = -120m;
		c.Equal(24380m, g.ToPrice(24500m), "a negative basis is a future below the index");

		c.True(!Grid(0m, 0m, 1m).IsValid, "a factor of zero is not a map");
		c.True(!Grid(-1m, 0m, 1m).IsValid, "a negative factor is not a map");
		c.True(!Grid(41m, 0m, 0m).IsValid, "a spacing of zero has no grid");
		c.Equal(0m, Grid(0m, 0m, 1m).ToStrike(100m), "no map, no equivalence");

		g = Grid(41m, 150m, 5m);
		c.Equal(500m, g.NearestStrike(20650m), "the strike itself");
		c.Equal(500m, g.NearestStrike(20700m), "just above 500, still 500");
		c.Equal(505m, g.NearestStrike(20800m), "closer to 505");
	}

	private static void GridTests(Check c)
	{
		var lines = new List<StrikeLine>();
		var g = Grid(41m, 150m, 1m);

		// 500 .. 510 of the underlying.
		var step = g.Build(g.ToPrice(500m), g.ToPrice(510m), lines);
		c.Equal(1m, step, "the spacing fits");
		c.Equal(11, lines.Count, "500 to 510 inclusive");
		c.Equal(500m, lines[0].Strike, "lowest first");
		c.Equal(510m, lines[10].Strike, "up to the top");
		c.Equal(g.ToPrice(505m), lines[5].Price, "the price of each line comes from the map");

		// Ends that fall between strikes are not rounded outwards.
		step = g.Build(g.ToPrice(500.4m), g.ToPrice(510.6m), lines);
		c.Equal(10, lines.Count, "501 to 510, and nothing below 501");
		c.Equal(501m, lines[0].Strike, "the first strike inside the range");
		c.Equal(510m, lines[lines.Count - 1].Strike, "the last strike inside the range");
		c.Equal(1m, step, "still the base spacing");

		// The order of the two prices does not matter.
		g.Build(g.ToPrice(510m), g.ToPrice(500m), lines);
		c.Equal(11, lines.Count, "the range is read low to high either way");
		c.Equal(500m, lines[0].Strike, "still from the bottom");

		// Above the limit the spacing is coarsened instead of dropping lines.
		g = Grid(41m, 150m, 1m, 0m, 20);
		step = g.Build(g.ToPrice(500m), g.ToPrice(600m), lines);
		c.Equal(10m, step, "1, 2 and 5 give 101, 51 and 21 lines; 10 gives 11");
		c.Equal(11, lines.Count, "500 to 600 every 10");
		c.Equal(500m, lines[0].Strike, "the coarse grid stays on the round strikes");

		g = Grid(1m, 0m, 25m, 0m, 10);
		step = g.Build(0m, 100000m, lines);
		c.Equal(12500m, step, "the ladder goes up to 500 times the spacing");

		// Nothing to draw.
		g = Grid(41m, 150m, 5m);
		c.Equal(0m, g.Build(g.ToPrice(500.2m), g.ToPrice(500.4m), lines), "no strike inside the range");
		c.Equal(0, lines.Count, "and no lines");
		c.Equal(0m, Grid(0m, 0m, 1m).Build(100m, 200m, lines), "no map, no grid");
		c.Equal(0, lines.Count, "the list is cleared even when nothing is built");

		// Major lines.
		g = Grid(41m, 150m, 1m, 5m);
		g.Build(g.ToPrice(500m), g.ToPrice(510m), lines);
		c.True(lines[0].IsMajor, "500 is a multiple of 5");
		c.True(!lines[1].IsMajor, "501 is not");
		c.True(lines[5].IsMajor, "505 is");
		c.Equal(3, lines.FindAll(l => l.IsMajor).Count, "500, 505 and 510");

		g = Grid(41m, 150m, 1m, 0m);
		g.Build(g.ToPrice(500m), g.ToPrice(510m), lines);
		c.Equal(0, lines.FindAll(l => l.IsMajor).Count, "no major spacing, no major lines");

		// When the grid ends up coarser than the major spacing every line is major.
		g = Grid(41m, 150m, 1m, 5m, 20);
		g.Build(g.ToPrice(500m), g.ToPrice(700m), lines);
		c.Equal(lines.Count, lines.FindAll(l => l.IsMajor).Count, "a grid of 10 is coarser than 5");

		// Negative prices of the underlying are not invented for an instrument below the basis.
		g = Grid(1m, 20000m, 5m);
		g.Build(20001m, 20010m, lines);
		c.Equal(2, lines.Count, "strikes 5 and 10 above zero");
		c.Equal(5m, lines[0].Strike, "the first strike above the basis");
	}

	private static void AnchorTests(Check c)
	{
		// Both quotes: the ratio is the factor and what is left is the premium of the future.
		var r = StrikeAnchor.Solve(24600m, 600m, 24450m, 41m, 0m, AnchorTarget.Basis);
		c.True(r.Ok, "two quotes are enough");
		c.Equal(40.75m, r.Factor, "24450 / 600");
		c.Equal(150m, r.Basis, "24600 - 24450");

		// The index is what decides, whatever the target says.
		r = StrikeAnchor.Solve(24600m, 600m, 24450m, 41m, 0m, AnchorTarget.Factor);
		c.Equal(40.75m, r.Factor, "the pair wins over the target");
		c.Equal(150m, r.Basis, "and gives the basis too");

		// One quote, known factor: the basis moves.
		r = StrikeAnchor.Solve(24600m, 600m, 0m, 41m, 999m, AnchorTarget.Basis);
		c.True(r.Ok, "a factor and a quote give a basis");
		c.Equal(41m, r.Factor, "the factor is kept");
		c.Equal(-0m, r.Basis, "24600 - 600 * 41");

		// One quote, known basis: the factor stretches.
		r = StrikeAnchor.Solve(24600m, 600m, 0m, 1m, 150m, AnchorTarget.Factor);
		c.True(r.Ok, "a basis and a quote give a factor");
		c.Equal(40.75m, r.Factor, "(24600 - 150) / 600");
		c.Equal(150m, r.Basis, "the basis is kept");

		// An index and a future of the same index: the factor is one and the basis is the premium.
		r = StrikeAnchor.Solve(24600m, 24450m, 0m, 1m, 0m, AnchorTarget.Basis);
		c.Equal(1m, r.Factor, "same scale");
		c.Equal(150m, r.Basis, "the premium");

		// Quotes that cannot produce a map leave the settings alone.
		r = StrikeAnchor.Solve(24600m, 0m, 0m, 41m, 7m, AnchorTarget.Basis);
		c.True(!r.Ok, "no quote of the underlying");
		c.Equal(41m, r.Factor, "nothing changes");
		c.Equal(7m, r.Basis, "nothing changes");

		c.True(!StrikeAnchor.Solve(0m, 600m, 0m, 41m, 0m, AnchorTarget.Basis).Ok, "no price on the chart yet");
		c.True(!StrikeAnchor.Solve(24600m, 600m, 0m, 0m, 0m, AnchorTarget.Basis).Ok, "no factor to keep");
		c.True(!StrikeAnchor.Solve(100m, 600m, 0m, 1m, 150m, AnchorTarget.Factor).Ok, "a basis above the price would give a negative factor");

		// Ratios that do not divide exactly are rounded, not truncated.
		r = StrikeAnchor.Solve(24600m, 596.8m, 24450m, 41m, 0m, AnchorTarget.Basis);
		c.Equal(40.968499m, r.Factor, "24450 / 596.8 to six decimals");
	}

	private static void PresetTests(Check c)
	{
		c.True(!StrikePresets.TryGet(StrikePreset.Custom, out _), "Custom fills nothing");

		StrikePresets.TryGet(StrikePreset.QqqOnNq, out var v);
		c.Equal(41m, v.Factor, "QQQ is about a forty-first of NDX");
		c.Equal(1m, v.Spacing, "QQQ strikes go one by one");
		c.Equal("QQQ", v.Label, "the label of the pair");

		StrikePresets.TryGet(StrikePreset.NdxOnNq, out v);
		c.Equal(1m, v.Factor, "NDX and NQ share the scale");
		c.Equal(25m, v.Spacing, "NDX strikes go 25 by 25");

		StrikePresets.TryGet(StrikePreset.SpyOnEs, out v);
		c.Equal(10m, v.Factor, "SPY is about a tenth of SPX");

		StrikePresets.TryGet(StrikePreset.SpxOnEs, out v);
		c.Equal(5m, v.Spacing, "SPX strikes go 5 by 5");
		c.Equal(25m, v.MajorSpacing, "and the round ones every 25");
	}

	private static void ScenarioTests(Check c)
	{
		// NQ at 24600.25 with NDX at 24450.10 and QQQ at 596.80.
		var anchor = StrikeAnchor.Solve(24600.25m, 596.80m, 24450.10m, 41m, 0m, AnchorTarget.Basis);
		var g = Grid(anchor.Factor, anchor.Basis, 1m, 5m, 200);

		c.True(anchor.Ok, "the anchor holds");
		c.Near(596.80m, g.ToStrike(24600.25m), 0.001m, "the chart price reads back as the quote of QQQ");
		c.Near(24450.10m, g.ToPrice(596.80m) - anchor.Basis, 0.01m, "and without the premium it is the index");

		var lines = new List<StrikeLine>();
		g.Build(24400m, 24800m, lines);

		c.True(lines.Count > 5, "a visible range of 400 points holds several QQQ strikes");

		foreach (var line in lines)
		{
			c.True(line.Price >= 24400m && line.Price <= 24800m, $"strike {line.Strike} inside the range");
			c.Near(line.Strike, g.ToStrike(line.Price), 0.0001m, $"strike {line.Strike} maps both ways");
		}

		// The equivalence of a round QQQ strike is the price a trader would watch.
		var strike600 = g.ToPrice(600m);
		c.Near(24731.35m, strike600, 0.5m, "QQQ 600 is around NQ 24731");
		c.True(g.NearestStrike(strike600 + 10m) == 600m, "ten points above is still the same strike");
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
