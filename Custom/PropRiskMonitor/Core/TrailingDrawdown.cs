namespace ATAS.Indicators.Technical.PropRisk;

using System;

/// <summary>When the trailing peak follows the equity.</summary>
public enum TrailingPeakMode
{
	/// <summary>On every update: the peak follows the intraday high of the equity (open PnL included).</summary>
	Realtime,

	/// <summary>Once per trading day, with the equity at the end of the previous day.</summary>
	EndOfDay
}

/// <summary>How the trailing drawdown starts.</summary>
public enum TrailingInitMode
{
	/// <summary>From the equity when it starts: the stop is that equity minus the maximum drawdown.</summary>
	CurrentEquity,

	/// <summary>From a stop equity given by hand (for example the one the prop firm shows).</summary>
	ManualStop
}

public sealed class TrailingSettings
{
	public bool Enabled { get; set; }

	public decimal MaxDrawdown { get; set; }

	public TrailingInitMode InitMode { get; set; }

	public decimal ManualStopEquity { get; set; }

	public TrailingPeakMode PeakMode { get; set; }

	/// <summary>The stop stops trailing once it reaches the start equity plus <see cref="LockOffset"/>.</summary>
	public bool LockEnabled { get; set; }

	public decimal LockOffset { get; set; }

	public bool MonthlyReset { get; set; }

	/// <summary>Day of the month of the reset; the first trading day on or after it resets.</summary>
	public int MonthlyResetDay { get; set; } = 1;
}

/// <summary>Persisted state of the trailing drawdown of one account.</summary>
public sealed class TrailingState
{
	public bool Initialized { get; set; }

	public decimal StartEquity { get; set; }

	public decimal PeakEquity { get; set; }

	public bool Breached { get; set; }

	/// <summary>Trading day and equity of the last update, to apply the end of day after a restart.</summary>
	public DateTime LastDay { get; set; }

	public decimal LastEquity { get; set; }

	/// <summary>Year * 12 + month of the last monthly reset (or of the start).</summary>
	public int LastMonthlyReset { get; set; }

	public TrailingState Clone() => (TrailingState)MemberwiseClone();
}

public readonly record struct TrailingResult(
	bool Active,
	decimal StartEquity,
	decimal PeakEquity,
	decimal StopEquity,
	decimal Remaining,
	decimal CurrentDrawdown,
	bool Locked,
	bool Breached);

/// <summary>
/// Trailing drawdown: the stop equity is the peak minus the maximum drawdown, the peak only ever
/// goes up, and optionally the stop locks at the start equity plus an offset (Topstep locks at the
/// start, Apex at the start plus 100). A breach is latched until the state is reset.
/// </summary>
public static class TrailingDrawdownEngine
{
	public static TrailingResult Update(TrailingState state, TrailingSettings settings, decimal equity, DateTime tradingDay)
	{
		if (!settings.Enabled || settings.MaxDrawdown <= 0m)
			return default;

		if (!state.Initialized)
			Initialize(state, settings, equity, tradingDay);
		else
		{
			if (tradingDay > state.LastDay)
			{
				// End of day: the previous day's last equity can raise the peak. After a restart on a
				// later day this also applies the close that was persisted.
				if (settings.PeakMode == TrailingPeakMode.EndOfDay)
					state.PeakEquity = Math.Max(state.PeakEquity, state.LastEquity);

				if (settings.MonthlyReset && IsMonthlyResetDue(state, settings, tradingDay))
					Initialize(state, settings, equity, tradingDay, TrailingInitMode.CurrentEquity);
			}

			if (settings.PeakMode == TrailingPeakMode.Realtime)
				state.PeakEquity = Math.Max(state.PeakEquity, equity);
		}

		state.LastDay = tradingDay > state.LastDay ? tradingDay : state.LastDay;
		state.LastEquity = equity;

		var stop = state.PeakEquity - settings.MaxDrawdown;
		var locked = false;

		if (settings.LockEnabled && stop >= state.StartEquity + settings.LockOffset)
		{
			stop = state.StartEquity + settings.LockOffset;
			locked = true;
		}

		if (equity <= stop)
			state.Breached = true;

		return new TrailingResult(true, state.StartEquity, state.PeakEquity, stop, equity - stop, state.PeakEquity - equity, locked, state.Breached);
	}

	/// <summary>Starts again from the current equity (or the manual stop), clearing a breach.</summary>
	public static void Reset(TrailingState state)
	{
		state.Initialized = false;
		state.Breached = false;
	}

	private static void Initialize(TrailingState state, TrailingSettings settings, decimal equity, DateTime tradingDay, TrailingInitMode? mode = null)
	{
		var peak = (mode ?? settings.InitMode) == TrailingInitMode.ManualStop && settings.ManualStopEquity > 0m
			? settings.ManualStopEquity + settings.MaxDrawdown
			: equity;

		state.Initialized = true;
		state.StartEquity = peak;
		state.PeakEquity = peak;
		state.Breached = false;
		state.LastDay = tradingDay;
		state.LastEquity = equity;

		var month = tradingDay.Year * 12 + tradingDay.Month - 1;
		state.LastMonthlyReset = tradingDay.Day >= ResetDayOf(settings, tradingDay) ? month : month - 1;
	}

	private static bool IsMonthlyResetDue(TrailingState state, TrailingSettings settings, DateTime tradingDay)
	{
		var month = tradingDay.Year * 12 + tradingDay.Month - 1;
		return month > state.LastMonthlyReset && tradingDay.Day >= ResetDayOf(settings, tradingDay);
	}

	private static int ResetDayOf(TrailingSettings settings, DateTime day)
	{
		return Math.Clamp(settings.MonthlyResetDay, 1, DateTime.DaysInMonth(day.Year, day.Month));
	}
}
