namespace PropRiskMonitor.Tests;

using System;
using System.Collections.Generic;

internal static class Program
{
	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("SessionClock", SessionClockTests.Run),
			("TrailingDrawdown", TrailingDrawdownTests.Run),
			("DailyRails", DailyRailsTests.Run),
			("TradeLedger", TradeLedgerTests.Run),
			("Recommendations", RecommendationTests.Run),
			("RailPrices", RailPriceTests.Run),
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
}

/// <summary>Minimal assertion collector, so the tests need no test framework package.</summary>
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
