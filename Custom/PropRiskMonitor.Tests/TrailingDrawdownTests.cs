namespace PropRiskMonitor.Tests;

using System;

using ATAS.Indicators.Technical.PropRisk;

internal static class TrailingDrawdownTests
{
	public static void Run(Check check)
	{
		var d1 = new DateTime(2026, 9, 14);
		var d2 = d1.AddDays(1);
		var d3 = d1.AddDays(2);

		// Realtime: the peak follows the intraday high and never goes down.
		var rt = new TrailingSettings { Enabled = true, MaxDrawdown = 2500m, PeakMode = TrailingPeakMode.Realtime };
		var s = new TrailingState();
		var r = TrailingDrawdownEngine.Update(s, rt, 50000m, d1);
		check.Equal(47500m, r.StopEquity, "realtime start stop");
		r = TrailingDrawdownEngine.Update(s, rt, 51000m, d1);
		check.Equal(48500m, r.StopEquity, "realtime stop follows the high");
		r = TrailingDrawdownEngine.Update(s, rt, 49000m, d1);
		check.Equal(48500m, r.StopEquity, "realtime stop does not go down");
		check.Equal(500m, r.Remaining, "remaining = equity - stop");
		check.Equal(2000m, r.CurrentDrawdown, "current drawdown from the peak");

		// End of day: intraday highs do not move the stop; the previous close does, at the next day.
		var eod = new TrailingSettings { Enabled = true, MaxDrawdown = 2500m, PeakMode = TrailingPeakMode.EndOfDay };
		s = new TrailingState();
		TrailingDrawdownEngine.Update(s, eod, 50000m, d1);
		r = TrailingDrawdownEngine.Update(s, eod, 52000m, d1);
		check.Equal(47500m, r.StopEquity, "EOD ignores the intraday high");
		r = TrailingDrawdownEngine.Update(s, eod, 51000m, d1);
		r = TrailingDrawdownEngine.Update(s, eod, 51200m, d2);
		check.Equal(48500m, r.StopEquity, "EOD takes the previous day's last equity (51000)");

		// A losing day does not lower the peak.
		TrailingDrawdownEngine.Update(s, eod, 49500m, d2);
		r = TrailingDrawdownEngine.Update(s, eod, 49600m, d3);
		check.Equal(48500m, r.StopEquity, "EOD peak does not go down after a losing day");

		// Restart on a later day: the persisted last equity is applied as the close.
		var persisted = new TrailingState { Initialized = true, StartEquity = 50000m, PeakEquity = 50000m, LastDay = d1, LastEquity = 50800m, LastMonthlyReset = 2026 * 12 + 8 };
		r = TrailingDrawdownEngine.Update(persisted, eod, 50700m, d3);
		check.Equal(48300m, r.StopEquity, "EOD catch-up after a restart");

		// Lock: the stop stops at the start equity (+ offset).
		var locked = new TrailingSettings { Enabled = true, MaxDrawdown = 2500m, LockEnabled = true, LockOffset = 100m };
		s = new TrailingState();
		TrailingDrawdownEngine.Update(s, locked, 50000m, d1);
		r = TrailingDrawdownEngine.Update(s, locked, 53000m, d1);
		check.Equal(50100m, r.StopEquity, "stop locked at start + 100");
		check.True(r.Locked, "locked flag");
		r = TrailingDrawdownEngine.Update(s, locked, 52500m, d1);
		check.True(r.Locked && r.StopEquity == 50100m, "stays locked");

		// Manual stop: the peak is the stop plus the drawdown.
		var manual = new TrailingSettings { Enabled = true, MaxDrawdown = 2000m, InitMode = TrailingInitMode.ManualStop, ManualStopEquity = 48700m };
		s = new TrailingState();
		r = TrailingDrawdownEngine.Update(s, manual, 49500m, d1);
		check.Equal(48700m, r.StopEquity, "manual stop");
		check.Equal(800m, r.Remaining, "manual remaining");

		// Breach is latched until a reset.
		s = new TrailingState();
		TrailingDrawdownEngine.Update(s, rt, 50000m, d1);
		r = TrailingDrawdownEngine.Update(s, rt, 47400m, d1);
		check.True(r.Breached, "breach");
		r = TrailingDrawdownEngine.Update(s, rt, 49000m, d1);
		check.True(r.Breached, "breach stays after recovery");
		TrailingDrawdownEngine.Reset(s);
		r = TrailingDrawdownEngine.Update(s, rt, 49000m, d1);
		check.True(!r.Breached && r.StopEquity == 46500m, "reset starts from the current equity");

		// Monthly reset: first trading day on or after the reset day, once per month.
		var monthly = new TrailingSettings { Enabled = true, MaxDrawdown = 1000m, MonthlyReset = true, MonthlyResetDay = 1 };
		s = new TrailingState();
		TrailingDrawdownEngine.Update(s, monthly, 10000m, new DateTime(2026, 9, 29));
		TrailingDrawdownEngine.Update(s, monthly, 12000m, new DateTime(2026, 9, 30));
		r = TrailingDrawdownEngine.Update(s, monthly, 9000m, new DateTime(2026, 10, 2));
		check.True(r.StartEquity == 9000m && r.StopEquity == 8000m && !r.Breached, "monthly reset on the first trading day of the month");
		r = TrailingDrawdownEngine.Update(s, monthly, 9500m, new DateTime(2026, 10, 5));
		check.Equal(9000m, r.StartEquity, "monthly reset only once per month");

		// Disabled or no drawdown: inactive.
		check.True(!TrailingDrawdownEngine.Update(new TrailingState(), new TrailingSettings(), 1m, d1).Active, "disabled");
	}
}
