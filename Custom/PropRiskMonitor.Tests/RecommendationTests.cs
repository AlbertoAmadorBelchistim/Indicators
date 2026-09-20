namespace PropRiskMonitor.Tests;

using System.Linq;

using ATAS.Indicators.Technical.PropRisk;

internal static class RecommendationTests
{
	public static void Run(Check check)
	{
		var cfg = new RecommendationSettings
		{
			Enabled = true, MaxTradesPerDay = 4, CautionTradesPercent = 75m, MaxConsecutiveLosses = 2,
			CautionRemainingLossPercent = 20m, GivebackPercentOfTarget = 30m, PayoutObjective = 3000m,
			ConsistencyCautionPercent = 30m, ConsistencyStopPercent = 40m, TrailingCautionAmount = 300m
		};

		DailyResult Day(decimal start, decimal peak, decimal equity, decimal realized = 0m, bool stopHit = false, bool target = false, decimal limit = 1000m)
			=> new(true, start, peak, equity, equity - start, realized, start - limit, equity - (start - limit), start + 1500m, start + 1500m - equity,
				equity <= start - limit, stopHit || equity <= start - limit, equity >= start + 1500m, target, false);

		var quiet = new RecommendationInput(default, Day(50000m, 50000m, 50100m), default, 1000m, 1500m);
		check.Equal(RiskStatus.Ok, RecommendationRules.StatusOf(RecommendationRules.Evaluate(quiet, cfg)), "quiet day is OK");

		// Trades: 3 of 4 -> caution (ceil(4 * 75%) = 3), 4 -> stop.
		var r = RecommendationRules.Evaluate(quiet with { Trades = new LedgerStats(3, 2, 1, -1, 100m) }, cfg);
		check.True(r.Any(f => f.Reason == RiskReason.NearMaxTrades && f.Severity == RiskStatus.Caution), "near max trades");
		check.True(r.Any(f => f.Reason == RiskReason.OneLossAway), "one loss away from 2");
		r = RecommendationRules.Evaluate(quiet with { Trades = new LedgerStats(4, 2, 2, -2, 0m) }, cfg);
		check.Equal(RiskStatus.Stop, RecommendationRules.StatusOf(r), "max trades and losses stop");
		check.True(r.Count(f => f.Severity == RiskStatus.Stop) == 2, "both stop reasons are listed");

		// With MaxConsecutiveLosses = 1 there is no permanent caution.
		var one = new RecommendationSettings { Enabled = true, MaxConsecutiveLosses = 1 };
		check.Equal(RiskStatus.Ok, RecommendationRules.StatusOf(RecommendationRules.Evaluate(quiet, one)), "one allowed loss: no caution at 0 losses");

		// Daily stop hit earlier and now recovered: caution, but a later max-trades stop still shows.
		r = RecommendationRules.Evaluate(new RecommendationInput(default, Day(50000m, 50000m, 49500m, stopHit: true), new LedgerStats(4, 0, 4, -4, -500m), 1000m, 1500m), cfg);
		check.True(r.Any(f => f.Reason == RiskReason.DailyStopHitEarlier) && RecommendationRules.StatusOf(r) == RiskStatus.Stop, "earlier stop does not hide later limits");

		// Remaining daily loss within 20% of the limit: caution.
		r = RecommendationRules.Evaluate(new RecommendationInput(default, Day(50000m, 50000m, 49150m), default, 1000m, 1500m), cfg);
		check.True(r.Any(f => f.Reason == RiskReason.NearDailyStop), "near daily stop");

		// Giveback of 30% of the target (450) from the day's peak.
		r = RecommendationRules.Evaluate(new RecommendationInput(default, Day(50000m, 51000m, 50500m), default, 1000m, 1500m), cfg);
		check.True(r.Any(f => f.Reason == RiskReason.Giveback), "giveback");

		// Consistency: realized 1200 = 40% of 3000 -> stop.
		r = RecommendationRules.Evaluate(new RecommendationInput(default, Day(50000m, 51200m, 51200m, 1200m), default, 1000m, 1500m), cfg);
		check.True(r.Any(f => f.Reason == RiskReason.ConsistencyStop) && !r.Any(f => f.Reason == RiskReason.ConsistencyCaution), "consistency stop only");

		// Trailing near the stop and breached.
		var near = new TrailingResult(true, 50000m, 50000m, 47500m, 250m, 2250m, false, false);
		r = RecommendationRules.Evaluate(quiet with { Trailing = near }, cfg);
		check.True(r.Any(f => f.Reason == RiskReason.TrailingNearStop), "trailing near stop");
		r = RecommendationRules.Evaluate(quiet with { Trailing = near with { Breached = true } }, cfg);
		check.Equal(RiskStatus.Stop, RecommendationRules.StatusOf(r), "trailing breached");

		// Target hit: stop trading.
		r = RecommendationRules.Evaluate(new RecommendationInput(default, Day(50000m, 51600m, 51600m, target: true), default, 1000m, 1500m), cfg);
		check.True(r.Any(f => f.Reason == RiskReason.DailyTarget && f.Severity == RiskStatus.Stop), "target reached");

		check.Equal(0, RecommendationRules.Evaluate(quiet, new RecommendationSettings()).Count, "disabled");
	}
}
