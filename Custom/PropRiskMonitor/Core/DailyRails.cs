namespace ATAS.Indicators.Technical.PropRisk;

using System;

/// <summary>Where the daily loss limit is measured from.</summary>
public enum DailyLossMode
{
	/// <summary>From the start equity of the day: stop = start - limit.</summary>
	FromStart,

	/// <summary>From the highest equity of the day: stop = day's peak - limit (a daily trailing loss).</summary>
	FromPeak
}

/// <summary>Start equity of a trading day.</summary>
public enum DailyBaseMode
{
	/// <summary>The last equity of the previous trading day (the usual end-of-day balance), when known.</summary>
	PreviousDayClose,

	/// <summary>The first equity seen in the day.</summary>
	FirstEquityOfDay
}

public sealed class DailySettings
{
	public bool Enabled { get; set; }

	public decimal LossLimit { get; set; }

	public DailyLossMode LossMode { get; set; }

	public decimal ProfitTarget { get; set; }

	public DailyBaseMode BaseMode { get; set; }
}

/// <summary>Persisted state of the current trading day of one account.</summary>
public sealed class DailyState
{
	public DateTime Day { get; set; }

	public bool Initialized { get; set; }

	public decimal StartEquity { get; set; }

	public decimal PeakEquity { get; set; }

	public decimal LowEquity { get; set; }

	/// <summary>Closed PnL of the account at the start of the day; realized today = closed PnL - baseline.</summary>
	public decimal ClosedPnlBaseline { get; set; }

	public bool StopHit { get; set; }

	public bool TargetHit { get; set; }

	public decimal LastEquity { get; set; }

	public decimal LastClosedPnl { get; set; }

	public DailyState Clone() => (DailyState)MemberwiseClone();
}

public readonly record struct DailyResult(
	bool LimitsActive,
	decimal StartEquity,
	decimal PeakEquity,
	decimal LowEquity,
	decimal DayPnl,
	decimal RealizedToday,
	decimal? StopEquity,
	decimal? RemainingLoss,
	decimal? TargetEquity,
	decimal? RemainingTarget,
	bool AtStop,
	bool StopHitToday,
	bool AtTarget,
	bool TargetHitToday,
	bool NewDay);

/// <summary>
/// Statistics and limits of the current trading day: start, peak and low equity, day PnL and
/// realized PnL, and optionally a loss limit (from the start or from the day's peak) and a profit
/// target. Hitting a limit is latched for the rest of the day.
/// </summary>
public static class DailyRailsEngine
{
	public static DailyResult Update(DailyState state, DailySettings settings, decimal equity, decimal closedPnl, DateTime tradingDay)
	{
		var newDay = !state.Initialized || tradingDay > state.Day;

		if (newDay)
		{
			var continuesPrevious = state.Initialized && tradingDay > state.Day;

			var start = continuesPrevious && settings.BaseMode == DailyBaseMode.PreviousDayClose
				? state.LastEquity
				: equity;

			state.ClosedPnlBaseline = continuesPrevious ? state.LastClosedPnl : closedPnl;
			state.Day = tradingDay;
			state.Initialized = true;
			state.StartEquity = start;
			state.PeakEquity = Math.Max(start, equity);
			state.LowEquity = Math.Min(start, equity);
			state.StopHit = false;
			state.TargetHit = false;
		}
		else
		{
			state.PeakEquity = Math.Max(state.PeakEquity, equity);
			state.LowEquity = Math.Min(state.LowEquity, equity);
		}

		state.LastEquity = equity;
		state.LastClosedPnl = closedPnl;

		decimal? stop = null, remainingLoss = null, target = null, remainingTarget = null;
		var atStop = false;
		var atTarget = false;

		if (settings.Enabled && settings.LossLimit > 0m)
		{
			stop = (settings.LossMode == DailyLossMode.FromPeak ? state.PeakEquity : state.StartEquity) - settings.LossLimit;
			remainingLoss = equity - stop.Value;
			atStop = equity <= stop.Value;
			state.StopHit |= atStop;
		}

		if (settings.Enabled && settings.ProfitTarget > 0m)
		{
			target = state.StartEquity + settings.ProfitTarget;
			remainingTarget = target.Value - equity;
			atTarget = equity >= target.Value;
			state.TargetHit |= atTarget;
		}

		return new DailyResult(
			settings.Enabled,
			state.StartEquity,
			state.PeakEquity,
			state.LowEquity,
			equity - state.StartEquity,
			closedPnl - state.ClosedPnlBaseline,
			stop,
			remainingLoss,
			target,
			remainingTarget,
			atStop,
			state.StopHit,
			atTarget,
			state.TargetHit,
			newDay);
	}
}
