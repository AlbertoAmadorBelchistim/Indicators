namespace PropRiskMonitor.Tests;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ATAS.Indicators.Technical.PropRisk;

internal static class StateStoreTests
{
	public static void Run(Check check)
	{
		var dir = Path.Combine(Path.GetTempPath(), "prm-tests-" + Guid.NewGuid().ToString("N"));
		var store = new StateStore(dir);
		var day = new DateTime(2026, 9, 14);

		try
		{
			check.Equal("APEX_12345-01.state.json", StateStore.FileNameOf("APEX/12345-01"), "file name is sanitized");

			// Round trip.
			var s = new AccountState();
			s.Trailing = new TrailingState { Initialized = true, StartEquity = 50000m, PeakEquity = 51000m, LastDay = day, LastEquity = 50900m };
			s.Daily = new DailyState { Initialized = true, Day = day, StartEquity = 50000m, PeakEquity = 51000m, LowEquity = 49800m, ClosedPnlBaseline = 10m };
			s.Ledgers["ES"] = new LedgerState { Day = day, ClosedToday = { 100m, -50m } };
			store.Save("A1", s, new[] { "ES" });
			var back = store.Load("A1");
			check.True(back.Trailing.PeakEquity == 51000m && back.Daily.ClosedPnlBaseline == 10m && back.Ledgers["ES"].ClosedToday.SequenceEqual(new[] { 100m, -50m }), "round trip");

			// Two charts on the same account: peaks merge, each keeps its instrument's trades.
			var nq = back.Clone();
			nq.Ledgers.Remove("ES");
			nq.Ledgers["NQ"] = new LedgerState { Day = day, ClosedToday = { 20m } };
			nq.Trailing.PeakEquity = 50500m; // an older view of the peak
			nq.Daily.StopHit = true;
			var merged = store.Save("A1", nq, new[] { "NQ" });
			check.True(merged.Trailing.PeakEquity == 51000m, "peak never goes down in a merge");
			check.True(merged.Daily.StopHit, "hits are kept");
			check.True(merged.Ledgers.ContainsKey("ES") && merged.Ledgers.ContainsKey("NQ"), "both instruments kept");

			// A later day on disk wins over a stale chart.
			var stale = store.Load("A1");
			stale.Daily.Day = day.AddDays(-1);
			merged = store.Save("A1", stale, new[] { "NQ" });
			check.Equal(day, merged.Daily.Day, "later day on disk wins");

			// Corrupt file: quarantined, backup restored, never replaced with an empty state.
			var path = store.PathOf("A1");
			File.WriteAllText(path, "{ not json");
			var restored = store.Load("A1");
			check.True(restored.Trailing.Initialized, "restored from the backup");
			check.True(Directory.GetFiles(dir, "*.corrupt-*").Length == 1, "corrupt file kept aside");

			// No backup either: a new state, and the unreadable file is kept.
			var lone = new StateStore(Path.Combine(dir, "lone"));
			Directory.CreateDirectory(Path.Combine(dir, "lone"));
			File.WriteAllText(lone.PathOf("B"), "garbage");
			check.True(!lone.Load("B").Trailing.Initialized, "unreadable without backup starts new");
			check.True(Directory.GetFiles(Path.Combine(dir, "lone"), "*.corrupt-*").Length == 1, "unreadable file kept aside");

			// Concurrent writers from several threads: the file stays valid and the highest peak survives.
			var tasks = Enumerable.Range(0, 8).Select(i => Task.Run(() =>
			{
				for (var k = 0; k < 20; k++)
				{
					var mine = store.Load("C");
					mine.Trailing = new TrailingState { Initialized = true, StartEquity = 1000m, PeakEquity = 1000m + i * 100 + k, LastDay = day };
					store.Save("C", mine, new[] { "I" + i });
				}
			})).ToArray();
			Task.WaitAll(tasks);
			var final = store.Load("C");
			check.Equal(1719m, final.Trailing.PeakEquity, "concurrent saves keep the highest peak");
			check.True(Directory.GetFiles(dir, "*.tmp").Length == 0, "no temporary files left");
		}
		finally
		{
			try { Directory.Delete(dir, true); } catch (IOException) { }
		}
	}
}
