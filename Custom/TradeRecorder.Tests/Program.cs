namespace TradeRecorder.Tests;

using System;
using System.Collections.Generic;
using ATAS.Indicators.Technical.TradeRecorderCore;

internal static class Program
{
	private static readonly DateTime T0 = new(2026, 9, 21, 14, 30, 0, DateTimeKind.Utc);

	private static int Main()
	{
		var suites = new List<(string Name, Action<Check> Run)>
		{
			("Ring", RingTests),
			("Case", CaseTests),
			("Lines", LineTests),
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

	private static Sample At(int seconds, decimal last = 100m)
		=> new(T0.AddSeconds(seconds), last, last - 0.25m, last + 0.25m, 10m, 12m, 80m, 90m, 0m);

	private static void RingTests(Check c)
	{
		var ring = new SampleRing(4);

		c.Equal(0, ring.Count, "empty");
		c.Equal(0, ring.Since(T0).Count, "nothing to give");

		for (var i = 0; i < 3; i++)
			ring.Add(At(i));

		c.Equal(3, ring.Count, "three samples");

		var all = ring.Since(T0);
		c.Equal(3, all.Count, "all of them");
		c.Equal(T0, all[0].TimeUtc, "oldest first");
		c.Equal(T0.AddSeconds(2), all[2].TimeUtc, "newest last");

		// Past the capacity the oldest is overwritten, and the order still holds.
		for (var i = 3; i < 7; i++)
			ring.Add(At(i));

		all = ring.Since(T0);
		c.Equal(4, all.Count, "only the capacity is kept");
		c.Equal(T0.AddSeconds(3), all[0].TimeUtc, "the oldest kept is the fourth");
		c.Equal(T0.AddSeconds(6), all[3].TimeUtc, "and the newest is the last one added");

		// The window only gives what falls inside it.
		var recent = ring.Since(T0.AddSeconds(5));
		c.Equal(2, recent.Count, "two samples in the last window");
		c.Equal(T0.AddSeconds(5), recent[0].TimeUtc, "from the moment asked for");

		ring.Clear();
		c.Equal(0, ring.Count, "cleared");

		ring.Resize(2);
		ring.Add(At(0));
		ring.Add(At(1));
		ring.Add(At(2));
		c.Equal(2, ring.Count, "resized to two");

		c.Equal(1, new SampleRing(0).Capacity, "a ring always holds at least one");
	}

	private static void CaseTests(Check c)
	{
		var state = new CaseState { TailSeconds = 120 };

		c.Equal(CaseEvent.None, state.Update(0m, T0), "flat and quiet");
		c.Equal(CasePhase.Idle, state.Phase, "nothing to record");

		c.Equal(CaseEvent.Opened, state.Update(2m, T0.AddSeconds(10)), "the position opens");
		c.Equal(1, state.Side, "long");
		c.Equal(2m, state.PeakVolume, "two contracts");
		c.Equal(T0.AddSeconds(10), state.OpenedUtc, "and when");

		c.Equal(CaseEvent.Changed, state.Update(4m, T0.AddSeconds(20)), "it grows");
		c.Equal(4m, state.PeakVolume, "the peak follows it");

		c.Equal(CaseEvent.Changed, state.Update(1m, T0.AddSeconds(30)), "and is reduced");
		c.Equal(4m, state.PeakVolume, "the peak does not come back down");

		c.Equal(CaseEvent.Closed, state.Update(0m, T0.AddSeconds(40)), "flat again");
		c.Equal(CasePhase.Tail, state.Phase, "but still recording");

		c.Equal(CaseEvent.None, state.Update(0m, T0.AddSeconds(100)), "a minute later, still recording");
		c.Equal(CasePhase.Tail, state.Phase, "the tail is not over");

		c.Equal(CaseEvent.Finished, state.Update(0m, T0.AddSeconds(160)), "two minutes after the exit it ends");
		c.Equal(CasePhase.Idle, state.Phase, "and goes quiet");

		// A short.
		state = new CaseState();
		state.Update(-3m, T0);
		c.Equal(-1, state.Side, "short");
		c.Equal(3m, state.PeakVolume, "size in absolute terms");

		// A new trade inside the tail closes the old one and starts another.
		state = new CaseState { TailSeconds = 120 };
		state.Update(1m, T0);
		state.Update(0m, T0.AddSeconds(10));
		c.Equal(CasePhase.Tail, state.Phase, "in the tail");
		c.Equal(CaseEvent.Opened, state.Update(-2m, T0.AddSeconds(30)), "and another trade arrives");
		c.Equal(-1, state.Side, "the new one is a short");
		c.Equal(T0.AddSeconds(30), state.OpenedUtc, "with its own opening");
		c.Equal(2m, state.PeakVolume, "and its own size");

		state.Reset();
		c.Equal(CasePhase.Idle, state.Phase, "reset");
	}

	private static void LineTests(Check c)
	{
		var id = JsonLines.CaseId("NQ Z6", T0);
		c.Equal("NQ_Z6-20260921143000000", id, "the identifier carries the instrument and the millisecond");

		var line = JsonLines.Sample(id, "NQ Z6", At(0, 24000m));

		c.True(line.StartsWith("{", StringComparison.Ordinal) && line.EndsWith("}", StringComparison.Ordinal), "one object");
		c.True(line.Contains("\"t\":\"2026-09-21T14:30:00.000Z\"", StringComparison.Ordinal), "the time to the millisecond, in UTC");
		c.True(line.Contains("\"bid\":23999.75", StringComparison.Ordinal), "the bid, with a dot for decimals");
		c.True(line.Contains("\"ask_depth\":90", StringComparison.Ordinal), "the depth of each side");
		c.True(!line.Contains("\n", StringComparison.Ordinal), "and it is one line");

		var trade = JsonLines.Trade(id, "NQ Z6", "SIM-1", 1, 3m, T0, T0.AddSeconds(45), 24000m, 24012.5m, 187.5m, 420);

		c.True(trade.Contains("\"side\":\"long\"", StringComparison.Ordinal), "the side");
		c.True(trade.Contains("\"qty\":3", StringComparison.Ordinal), "the size");
		c.True(trade.Contains("\"pnl\":187.5", StringComparison.Ordinal), "the result");
		c.True(trade.Contains("\"samples\":420", StringComparison.Ordinal), "and how many samples belong to it");

		// A quote in an account name does not break the file.
		var awkward = JsonLines.Trade(id, "NQ", "he said \"hello\"", -1, 1m, T0, T0, 1m, 1m, 0m, 1);
		c.True(awkward.Contains("\\\"hello\\\"", StringComparison.Ordinal), "quotes are escaped");
		c.True(awkward.Contains("\"side\":\"short\"", StringComparison.Ordinal), "and the short is a short");
	}

	/// <summary>A trade of forty seconds with a minute of recording before and two after.</summary>
	private static void ScenarioTests(Check c)
	{
		// Four samples a second for sixty seconds of window.
		var ring = new SampleRing(60 * 4);
		var state = new CaseState { TailSeconds = 120 };

		for (var i = 0; i < 400; i++)
			ring.Add(At(i / 4));

		var opened = T0.AddSeconds(100);
		c.Equal(CaseEvent.Opened, state.Update(2m, opened), "the trade opens");

		// What was already recorded in the minute before the entry.
		var before = ring.Since(opened.AddSeconds(-60));
		c.True(before.Count > 0, "there is a before, because it was already recording");
		c.True(before[0].TimeUtc >= opened.AddSeconds(-60), "and it starts where the window does");

		state.Update(2m, opened.AddSeconds(20));
		c.Equal(CaseEvent.Closed, state.Update(0m, opened.AddSeconds(40)), "and it closes");

		// The tail keeps going, and only then is the case finished.
		c.Equal(CaseEvent.None, state.Update(0m, opened.AddSeconds(100)), "still in the tail");
		c.Equal(CaseEvent.Finished, state.Update(0m, opened.AddSeconds(161)), "and it ends two minutes after the exit");

		var id = JsonLines.CaseId("NQ", opened);
		c.Equal("NQ-20260921143140000", id, "the case is named after its opening");
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
