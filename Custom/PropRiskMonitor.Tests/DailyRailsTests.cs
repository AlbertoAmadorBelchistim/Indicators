namespace PropRiskMonitor.Tests;

using System;

using ATAS.Indicators.Technical.PropRisk;

internal static class DailyRailsTests
{
	public static void Run(Check check)
	{
		var d1 = new DateTime(2026, 9, 14);
		var d2 = d1.AddDays(1);
		var cfg = new DailySettings { Enabled = true, LossLimit = 1000m, ProfitTarget = 1500m, BaseMode = DailyBaseMode.PreviousDayClose };

		var s = new DailyState();
		var r = DailyRailsEngine.Update(s, cfg, 50000m, 200m, d1);
		check.True(r.NewDay && r.StartEquity == 50000m, "first update starts the day at the current equity");
		check.Equal(49000m, r.StopEquity, "stop from start");
		check.Equal(51500m, r.TargetEquity, "target from start");
		check.Equal(0m, r.RealizedToday, "realized starts at 0");

		r = DailyRailsEngine.Update(s, cfg, 50600m, 500m, d1);
		check.Equal(600m, r.DayPnl, "day PnL includes open PnL");
		check.Equal(300m, r.RealizedToday, "realized = closed PnL - baseline");
		check.Equal(1600m, r.RemainingLoss, "remaining loss = equity - stop");

		r = DailyRailsEngine.Update(s, cfg, 48950m, 500m, d1);
		check.True(r.AtStop && r.StopHitToday, "stop hit");
		r = DailyRailsEngine.Update(s, cfg, 49500m, 500m, d1);
		check.True(!r.AtStop && r.StopHitToday, "stop hit is latched for the day");

		// Next day: start = previous day's last equity; baseline = previous day's last closed PnL.
		r = DailyRailsEngine.Update(s, cfg, 49600m, 520m, d2);
		check.True(r.NewDay && r.StartEquity == 49500m && !r.StopHitToday, "new day from the previous close, latch cleared");
		check.Equal(20m, r.RealizedToday, "realized of the new day");

		// First-equity base.
		var first = new DailySettings { Enabled = true, LossLimit = 1000m, BaseMode = DailyBaseMode.FirstEquityOfDay };
		s = new DailyState();
		DailyRailsEngine.Update(s, first, 50000m, 0m, d1);
		r = DailyRailsEngine.Update(s, first, 50300m, 0m, d2);
		check.Equal(50300m, r.StartEquity, "first equity of the day");

		// From peak: the stop trails the day's high.
		var peak = new DailySettings { Enabled = true, LossLimit = 1000m, LossMode = DailyLossMode.FromPeak };
		s = new DailyState();
		DailyRailsEngine.Update(s, peak, 50000m, 0m, d1);
		DailyRailsEngine.Update(s, peak, 51200m, 0m, d1);
		r = DailyRailsEngine.Update(s, peak, 50800m, 0m, d1);
		check.Equal(50200m, r.StopEquity, "stop from the day's peak");

		// Restart the same day with the persisted state: nothing is reset.
		var persisted = s.Clone();
		r = DailyRailsEngine.Update(persisted, peak, 50700m, 0m, d1);
		check.True(!r.NewDay && r.PeakEquity == 51200m, "same-day restart keeps the day");

		// Target hit latched.
		s = new DailyState();
		DailyRailsEngine.Update(s, cfg, 50000m, 0m, d1);
		r = DailyRailsEngine.Update(s, cfg, 51600m, 0m, d1);
		check.True(r.AtTarget && r.TargetHitToday, "target hit");
		r = DailyRailsEngine.Update(s, cfg, 51000m, 0m, d1);
		check.True(!r.AtTarget && r.TargetHitToday, "target hit latched");

		// Limits off: statistics still computed, no stop or target.
		s = new DailyState();
		r = DailyRailsEngine.Update(s, new DailySettings(), 100m, 0m, d1);
		check.True(!r.LimitsActive && r.StopEquity == null && r.TargetEquity == null, "limits off");
	}
}
