namespace PropRiskMonitor.Tests;

using System;

using ATAS.Indicators.Technical.PropRisk;

internal static class SessionClockTests
{
	public static void Run(Check check)
	{
		var ny = new SessionClock(DayResetZone.NewYork, new TimeSpan(17, 0, 0));
		static DateTime Utc(int y, int mo, int d, int h, int mi = 0) => new(y, mo, d, h, mi, 0, DateTimeKind.Utc);

		// Winter (EST, UTC-5): 17:00 New York = 22:00 UTC.
		check.Equal(new DateTime(2026, 1, 13), ny.TradingDay(Utc(2026, 1, 13, 21, 59)), "winter before the reset");
		check.Equal(new DateTime(2026, 1, 14), ny.TradingDay(Utc(2026, 1, 13, 22, 0)), "winter at the reset");

		// Summer (EDT, UTC-4): 17:00 New York = 21:00 UTC. A fixed offset would get this wrong.
		check.Equal(new DateTime(2026, 7, 14), ny.TradingDay(Utc(2026, 7, 14, 20, 59)), "summer before the reset");
		check.Equal(new DateTime(2026, 7, 15), ny.TradingDay(Utc(2026, 7, 14, 21, 0)), "summer at the reset");

		// Sunday evening opening belongs to Monday.
		check.Equal(new DateTime(2026, 9, 21), ny.TradingDay(Utc(2026, 9, 20, 22, 0)), "Sunday 18:00 NY is Monday");

		// Day ends across the daylight saving changes (8 March and 1 November 2026).
		check.Equal(Utc(2026, 3, 6, 22), ny.DayEndUtc(Utc(2026, 3, 6, 15)), "Friday before DST ends at 22:00 UTC");
		check.Equal(Utc(2026, 3, 9, 21), ny.DayEndUtc(Utc(2026, 3, 9, 15)), "Monday after DST ends at 21:00 UTC");
		check.Equal(Utc(2026, 11, 2, 22), ny.DayEndUtc(Utc(2026, 11, 2, 15)), "Monday after DST end ends at 22:00 UTC");

		// Every instant is before the end of its own day, and the end starts the next day.
		var t = Utc(2026, 1, 1, 0);

		for (var i = 0; i < 24 * 400; i++, t = t.AddMinutes(61))
		{
			var end = ny.DayEndUtc(t);
			check.True(end > t && ny.TradingDay(end) == ny.TradingDay(t).AddDays(1), $"end of the day of {t:u}");
		}

		// Midnight reset: the trading day is the calendar date.
		var utcMidnight = new SessionClock(TimeZoneInfo.Utc, TimeSpan.Zero);
		check.Equal(new DateTime(2026, 5, 4), utcMidnight.TradingDay(Utc(2026, 5, 4, 23, 59)), "midnight reset keeps the date");
		check.Equal(Utc(2026, 5, 5, 0), utcMidnight.DayEndUtc(Utc(2026, 5, 4, 10)), "midnight reset ends at midnight");
	}
}
