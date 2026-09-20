namespace PropRiskMonitor.Tests;

using System;

using ATAS.Indicators.Technical.PropRisk;

internal static class TradeLedgerTests
{
	public static void Run(Check check)
	{
		var day = new DateTime(2026, 9, 14);
		var t = new DateTime(2026, 9, 14, 14, 0, 0, DateTimeKind.Utc);
		var s = new LedgerState();
		PositionSnapshot P(decimal vol, decimal unr, decimal real) => new(vol, 5000m, unr, real);

		// Long 1, peak +300, dip -100, add 1, partial close +200, close with +150 more.
		check.True(TradeLedger.Update(s, P(1, 0, 0), day, t) == null && s.Current?.Side == 1, "open long");
		TradeLedger.Update(s, P(1, 300, 0), day, t);
		TradeLedger.Update(s, P(1, -100, 0), day, t);
		TradeLedger.Update(s, P(2, -50, 0), day, t);
		TradeLedger.Update(s, P(1, 100, 200), day, t);
		var closed = TradeLedger.Update(s, P(0, 0, 350), day, t);
		check.True(closed != null && closed.Pnl == 350m, "trade PnL includes the partial close");
		check.Equal(300m, closed?.MaxOpenPnl, "MFE");
		check.Equal(-100m, closed?.MinOpenPnl, "MAE");
		check.Equal(2, closed?.MaxQuantity, "max quantity");

		// Reversal: short 2 from long 1 closes the long (realized -80) and opens a short.
		TradeLedger.Update(s, P(1, 0, 350), day, t);
		closed = TradeLedger.Update(s, P(-1, 0, 270), day, t);
		check.True(closed != null && closed.Pnl == -80m && s.Current?.Side == -1, "reversal closes and opens");

		// Close the short at -40, but the realized PnL arrives after the flat volume.
		closed = TradeLedger.Update(s, P(0, 0, 270), day, t);
		check.Equal(0m, closed?.Pnl, "flat with the realized not yet updated");
		TradeLedger.Update(s, P(0, 0, 230), day, t);
		check.Equal(-40m, s.Last?.Pnl, "late realized update corrects the last trade");

		var stats = TradeLedger.Stats(s, day);
		check.Equal(3, stats.Trades, "trades today");
		check.Equal(1, stats.Wins, "wins");
		check.Equal(2, stats.Losses, "losses");
		check.Equal(-2, stats.Streak, "two losses in a row");
		check.Equal(230m, stats.RealizedPnl, "realized of the trades");

		// A new trading day starts the counts again; a persisted clone keeps them.
		var restored = s.Clone();
		check.Equal(3, TradeLedger.Stats(restored, day).Trades, "restored state keeps today's trades");
		TradeLedger.Update(s, P(0, 0, 230), day.AddDays(1), t.AddDays(1));
		check.Equal(0, TradeLedger.Stats(s, day.AddDays(1)).Trades, "new day");
		check.Equal(0, TradeLedger.Stats(restored, day.AddDays(1)).Trades, "stats of a later day are empty");
	}
}
