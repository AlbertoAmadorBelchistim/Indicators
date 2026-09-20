namespace ATAS.Indicators.Technical.PropRisk;

using System;
using System.Collections.Generic;

public enum RiskStatus
{
	Ok,
	Caution,
	Stop
}

public enum RiskReason
{
	TrailingBreached,
	TrailingNearStop,
	DailyStop,
	DailyStopHitEarlier,
	DailyTarget,
	MaxTrades,
	NearMaxTrades,
	MaxLosses,
	OneLossAway,
	NearDailyStop,
	LossFromStart,
	Giveback,
	ConsistencyStop,
	ConsistencyCaution
}

/// <summary>One rule that applies, with the value that triggered it and its limit.</summary>
public readonly record struct RiskFinding(RiskStatus Severity, RiskReason Reason, decimal Value, decimal Limit);

/// <summary>Settings of the suggested status. Amounts are account currency, percentages 0-100; 0 turns a rule off.</summary>
public sealed class RecommendationSettings
{
	public bool Enabled { get; set; }

	public int MaxTradesPerDay { get; set; }

	public decimal CautionTradesPercent { get; set; }

	public int MaxConsecutiveLosses { get; set; }

	public decimal TrailingCautionAmount { get; set; }

	public decimal CautionRemainingLossPercent { get; set; }

	public decimal CautionLossFromStart { get; set; }

	public decimal GivebackAmount { get; set; }

	public decimal GivebackPercentOfTarget { get; set; }

	public decimal PayoutObjective { get; set; }

	public decimal ConsistencyCautionPercent { get; set; }

	public decimal ConsistencyStopPercent { get; set; }
}

public readonly record struct RecommendationInput(
	TrailingResult Trailing,
	DailyResult Daily,
	LedgerStats Trades,
	decimal DailyLossLimit,
	decimal DailyProfitTarget);

/// <summary>
/// Suggested trading status: every rule is evaluated and all the ones that apply are returned, so a
/// daily stop hit earlier does not hide a later limit. The status is the most severe of them.
/// </summary>
public static class RecommendationRules
{
	public static RiskStatus StatusOf(IReadOnlyList<RiskFinding> findings)
	{
		var status = RiskStatus.Ok;

		foreach (var finding in findings)
		{
			if (finding.Severity > status)
				status = finding.Severity;
		}

		return status;
	}

	public static List<RiskFinding> Evaluate(RecommendationInput input, RecommendationSettings settings)
	{
		var findings = new List<RiskFinding>();

		if (!settings.Enabled)
			return findings;

		var trailing = input.Trailing;
		var daily = input.Daily;

		if (trailing.Active)
		{
			if (trailing.Breached)
				findings.Add(new(RiskStatus.Stop, RiskReason.TrailingBreached, trailing.Remaining, 0m));
			else if (settings.TrailingCautionAmount > 0m && trailing.Remaining <= settings.TrailingCautionAmount)
				findings.Add(new(RiskStatus.Caution, RiskReason.TrailingNearStop, trailing.Remaining, settings.TrailingCautionAmount));
		}

		if (daily.AtStop)
			findings.Add(new(RiskStatus.Stop, RiskReason.DailyStop, daily.DayPnl, -input.DailyLossLimit));
		else if (daily.StopHitToday)
			findings.Add(new(RiskStatus.Caution, RiskReason.DailyStopHitEarlier, daily.DayPnl, -input.DailyLossLimit));
		else if (settings.CautionRemainingLossPercent > 0m && input.DailyLossLimit > 0m && daily.RemainingLoss is { } remaining)
		{
			var threshold = input.DailyLossLimit * settings.CautionRemainingLossPercent / 100m;

			if (remaining <= threshold)
				findings.Add(new(RiskStatus.Caution, RiskReason.NearDailyStop, remaining, threshold));
		}

		if (daily.TargetHitToday)
			findings.Add(new(RiskStatus.Stop, RiskReason.DailyTarget, daily.DayPnl, input.DailyProfitTarget));

		var trades = input.Trades;

		if (settings.MaxTradesPerDay > 0)
		{
			var cautionAt = (int)Math.Ceiling(settings.MaxTradesPerDay * settings.CautionTradesPercent / 100m);

			if (trades.Trades >= settings.MaxTradesPerDay)
				findings.Add(new(RiskStatus.Stop, RiskReason.MaxTrades, trades.Trades, settings.MaxTradesPerDay));
			else if (settings.CautionTradesPercent > 0m && cautionAt > 0 && trades.Trades >= cautionAt)
				findings.Add(new(RiskStatus.Caution, RiskReason.NearMaxTrades, trades.Trades, settings.MaxTradesPerDay));
		}

		if (settings.MaxConsecutiveLosses > 0)
		{
			var losing = Math.Max(0, -trades.Streak);

			if (losing >= settings.MaxConsecutiveLosses)
				findings.Add(new(RiskStatus.Stop, RiskReason.MaxLosses, losing, settings.MaxConsecutiveLosses));
			else if (settings.MaxConsecutiveLosses >= 2 && losing == settings.MaxConsecutiveLosses - 1)
				findings.Add(new(RiskStatus.Caution, RiskReason.OneLossAway, losing, settings.MaxConsecutiveLosses));
		}

		if (settings.CautionLossFromStart > 0m && -daily.DayPnl >= settings.CautionLossFromStart)
			findings.Add(new(RiskStatus.Caution, RiskReason.LossFromStart, daily.DayPnl, -settings.CautionLossFromStart));

		var peakPnl = daily.PeakEquity - daily.StartEquity;

		if (peakPnl > 0m)
		{
			var threshold = settings.GivebackAmount > 0m
				? settings.GivebackAmount
				: settings.GivebackPercentOfTarget > 0m && input.DailyProfitTarget > 0m
					? input.DailyProfitTarget * settings.GivebackPercentOfTarget / 100m
					: 0m;

			var giveback = peakPnl - daily.DayPnl;

			if (threshold > 0m && giveback >= threshold)
				findings.Add(new(RiskStatus.Caution, RiskReason.Giveback, giveback, threshold));
		}

		if (settings.PayoutObjective > 0m)
		{
			var realized = daily.RealizedToday;
			var stopAt = settings.PayoutObjective * settings.ConsistencyStopPercent / 100m;
			var cautionAt = settings.PayoutObjective * settings.ConsistencyCautionPercent / 100m;

			if (settings.ConsistencyStopPercent > 0m && realized >= stopAt)
				findings.Add(new(RiskStatus.Stop, RiskReason.ConsistencyStop, realized, stopAt));
			else if (settings.ConsistencyCautionPercent > 0m && realized >= cautionAt)
				findings.Add(new(RiskStatus.Caution, RiskReason.ConsistencyCaution, realized, cautionAt));
		}

		return findings;
	}
}
