namespace ATAS.Indicators.Technical.PropRisk;

using System;

/// <summary>Time zone in which the trading day ends.</summary>
public enum DayResetZone
{
	/// <summary>New York time, with its daylight saving changes (the usual 17:00 of futures prop firms).</summary>
	NewYork,

	/// <summary>The computer's local time.</summary>
	Local
}

/// <summary>
/// Maps instants to trading days. A trading day ends at <see cref="ResetTime"/> in the chosen time
/// zone: with 17:00 New York, Monday's trading day runs from Sunday 17:00 to Monday 17:00 New York
/// time, in winter and in summer alike. No ATAS dependency, so it can be tested on its own.
/// </summary>
public sealed class SessionClock
{
	private static readonly Lazy<TimeZoneInfo> NewYorkZone = new(ResolveNewYork);

	private readonly TimeZoneInfo _zone;

	public SessionClock(DayResetZone zone, TimeSpan resetTime)
		: this(zone == DayResetZone.NewYork ? NewYorkZone.Value : TimeZoneInfo.Local, resetTime)
	{
	}

	public SessionClock(TimeZoneInfo zone, TimeSpan resetTime)
	{
		_zone = zone ?? throw new ArgumentNullException(nameof(zone));

		if (resetTime < TimeSpan.Zero || resetTime >= TimeSpan.FromDays(1))
			throw new ArgumentOutOfRangeException(nameof(resetTime));

		ResetTime = resetTime;
	}

	public TimeSpan ResetTime { get; }

	/// <summary>Trading day (a date) that contains the UTC instant.</summary>
	public DateTime TradingDay(DateTime utc)
	{
		var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), _zone);
		var day = (local - ResetTime).Date;

		// With a reset during the day, the time after the reset belongs to the next day's session.
		return ResetTime > TimeSpan.Zero ? day.AddDays(1) : day;
	}

	/// <summary>UTC instant at which the trading day containing <paramref name="utc"/> ends.</summary>
	public DateTime DayEndUtc(DateTime utc)
	{
		var day = TradingDay(utc);
		var endLocal = ResetTime > TimeSpan.Zero ? day.Add(ResetTime) : day.AddDays(1);

		// A reset that falls in a skipped hour (daylight saving) happens at the first valid time.
		while (_zone.IsInvalidTime(endLocal))
			endLocal = endLocal.AddMinutes(1);

		return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified), _zone);
	}

	private static TimeZoneInfo ResolveNewYork()
	{
		foreach (var id in new[] { "America/New_York", "Eastern Standard Time" })
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(id);
			}
			catch (TimeZoneNotFoundException)
			{
			}
			catch (InvalidTimeZoneException)
			{
			}
		}

		// Last resort: a fixed UTC-5 zone. Loses the summer shift, but keeps the indicator working.
		return TimeZoneInfo.CreateCustomTimeZone("PropRisk-EST", TimeSpan.FromHours(-5), "EST", "EST");
	}
}
