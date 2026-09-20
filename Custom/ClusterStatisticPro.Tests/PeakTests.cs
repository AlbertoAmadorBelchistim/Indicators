namespace ClusterStatisticPro.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using ATAS.Indicators.Technical.ClusterStatsCore;

internal static partial class Program
{
	static partial void AddSuites(List<(string Name, Action<Check> Run)> suites)
	{
		suites.Add(("PeakRates", PeakTests));
		suites.Add(("PeakMean", MeanTests));
	}

	private static readonly DateTime Start = new(2026, 9, 21, 15, 30, 0);

	private static void PeakTests(Check c)
	{
		var e = new PeakRateEngine(windowSeconds: 5, minVolume: 10);

		c.True(!e.Add(0, new Tick(Start, 5, 1)), "below the minimum volume");
		c.True(e.Add(0, new Tick(Start.AddSeconds(1), 5, -1)), "10 in the window: 2/sec");
		e.TryGet(0, out var p);
		c.Equal(2m, p.VolumePerSecond, "10 / 5 s");
		c.Equal(0m, p.DeltaPerSecond, "5 buy - 5 sell");

		e.Add(0, new Tick(Start.AddSeconds(2), 20, 1));
		e.TryGet(0, out p);
		c.Equal(6m, p.VolumePerSecond, "30 / 5 s");
		c.Equal(4m, p.DeltaPerSecond, "20 / 5 s");
		c.Equal(4m / 6m, p.DeltaPerVolume, "delta share at the peak");

		// 6 s later the first two prints have left the window; the window carries into bar 1.
		c.True(e.Add(1, new Tick(Start.AddSeconds(6), 1, -1)), "bar 1 peak: 21 in the window");
		e.TryGet(1, out var p1);
		c.Equal(21m / 5m, p1.VolumePerSecond, "window across the bar boundary");
		c.True(!e.Add(1, new Tick(Start.AddSeconds(8), 1, -1)), "at 8 s the 20 of 2 s has left: 2 in the window, below the minimum");
		e.TryGet(0, out p);
		c.Equal(6m, p.VolumePerSecond, "bar 0 keeps its peak");

		// Same prints in two passes give the same peaks.
		var rnd = new Random(5);
		var mismatches = 0;

		for (var scenario = 0; scenario < 200; scenario++)
		{
			var ticks = new List<(int Bar, Tick Tick)>();
			var t = Start;

			for (var i = 0; i < 500; i++)
			{
				t = t.AddMilliseconds(rnd.Next(0, 3000));
				ticks.Add(((int)((t - Start).TotalSeconds / 60), new Tick(t, rnd.Next(1, 50), rnd.Next(3) - 1)));
			}

			var a = new PeakRateEngine(rnd.Next(1, 20), rnd.Next(0, 200));
			var b = new PeakRateEngine(a.WindowSeconds, a.MinVolume);

			foreach (var (bar, tick) in ticks)
				a.Add(bar, tick);

			// The second pass stops half way and resumes: as a history followed by realtime.
			foreach (var (bar, tick) in ticks.Take(250))
				b.Add(bar, tick);

			foreach (var (bar, tick) in ticks.Skip(250))
				b.Add(bar, tick);

			for (var bar = 0; bar <= ticks[^1].Bar; bar++)
			{
				var ha = a.TryGet(bar, out var pa);
				var hb = b.TryGet(bar, out var pb);

				if (ha != hb || pa != pb)
					mismatches++;
			}
		}

		c.Equal(0, mismatches, "200 scenarios, same peaks");
	}

	private static void MeanTests(Check c)
	{
		var sma = new PeakMean(3, ema: false);
		c.Equal(0m, sma.Current, "empty");
		sma.AddClosedBar(0, 10);
		sma.AddClosedBar(1, -20);
		sma.AddClosedBar(2, 30);
		c.Equal(20m, sma.Current, "SMA of |10|, |-20|, 30");
		sma.AddClosedBar(3, 40);
		c.Equal(30m, sma.Current, "SMA of the last 3");
		c.Equal(20m, sma.MeanFor(3), "bar 3 compares with the mean before it");
		c.Equal(0m, sma.MeanFor(0), "bar 0 has no mean before it");
		c.Equal(30m, sma.MeanFor(4), "bar in progress: current mean");
		sma.AddClosedBar(3, 1000);
		c.Equal(30m, sma.Current, "a bar is added once");

		var ema = new PeakMean(3, ema: true);
		ema.AddClosedBar(0, 10);
		ema.AddClosedBar(1, 20);
		c.Equal(15m, ema.Current, "EMA alpha 0.5");
	}
}
