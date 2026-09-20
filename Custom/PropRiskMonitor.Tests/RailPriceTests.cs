namespace PropRiskMonitor.Tests;

using ATAS.Indicators.Technical.PropRisk;

internal static class RailPriceTests
{
	public static void Run(Check check)
	{
		// ES: tick 0.25, tick cost 12.5 -> 50 per point per contract.
		// Long 2 at 5000, equity 50000: daily stop 49000 -> 1000 / (2 * 50) = 10 points -> 4990.
		var stop = RailPriceSolver.Stop(2m, 5000m, 50000m, 0.25m, 12.5m, 49000m, 48000m);
		check.True(stop is { Price: 4990m, Source: RailSource.DailyLoss }, "long stop from the daily limit");

		// Trailing stop higher than the daily one: it wins.
		stop = RailPriceSolver.Stop(2m, 5000m, 50000m, 0.25m, 12.5m, 49000m, 49500m);
		check.True(stop is { Price: 4995m, Source: RailSource.Trailing }, "trailing stop is more restrictive");

		// Rounding towards the price: 1 contract, 1010 to lose -> 20.2 points -> 4979.8 -> 4980.
		stop = RailPriceSolver.Stop(1m, 5000m, 50000m, 0.25m, 12.5m, 48990m, null);
		check.Equal(4980m, stop?.Price, "long stop rounds up (reached before the limit)");

		// Short: stop above, rounds down.
		stop = RailPriceSolver.Stop(-1m, 5000m, 50000m, 0.25m, 12.5m, 48990m, null);
		check.Equal(5020m, stop?.Price, "short stop rounds down");

		// Target: long 1, 1010 to go -> 5020.2 -> 5020.25 (reached at that price).
		var target = RailPriceSolver.Target(1m, 5000m, 50000m, 0.25m, 12.5m, 51010m);
		check.Equal(5020.25m, target?.Price, "long target rounds away from the price");
		target = RailPriceSolver.Target(-1m, 5000m, 50000m, 0.25m, 12.5m, 51010m);
		check.Equal(4979.75m, target?.Price, "short target rounds away from the price");

		check.True(RailPriceSolver.Stop(0m, 5000m, 50000m, 0.25m, 12.5m, 49000m, null) == null, "flat: no rail");
		check.True(RailPriceSolver.Stop(1m, 5000m, 48000m, 0.25m, 12.5m, 49000m, null) == null, "limit already crossed");
		check.True(RailPriceSolver.Target(1m, 5000m, 52000m, 0.25m, 12.5m, 51000m) == null, "target already reached");
		check.True(RailPriceSolver.Stop(1m, 5000m, 50000m, 0.25m, 0m, 49000m, null) == null, "no tick cost");
	}
}
