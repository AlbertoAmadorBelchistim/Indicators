namespace ATAS.Indicators.Technical.OhlcPlusPro.Core;

using System;
using System.Collections.Generic;

/// <summary>Traded volume at one price of a profile.</summary>
public readonly record struct PriceVolume(decimal Price, decimal Volume);

/// <summary>A run of prices that share a condition, as a band of the chart.</summary>
public readonly record struct Band(decimal Low, decimal High, int Levels, decimal PeakVolume);

public sealed class NodeSettings
{
	/// <summary>Volume of a price, as a percentage of the largest one, to count as a high node.</summary>
	public decimal HighPercent { get; set; } = 70m;

	/// <summary>Volume of a price, as a percentage of the largest one, to count as a low node.</summary>
	public decimal LowPercent { get; set; } = 20m;

	/// <summary>Prices that break the run but do not end it.</summary>
	public int GapTolerance { get; set; }

	/// <summary>Prices a run needs before it becomes a band.</summary>
	public int MinLevels { get; set; } = 2;
}

/// <summary>
/// High and low volume nodes of a profile. Both are read from the full grid of prices between the
/// lowest and the highest of the profile, so a price with no trades at all is a low node rather
/// than a hole the scan walks over.
/// </summary>
public static class VolumeNodes
{
	/// <summary>Grid cells a profile may have before it is read as traded prices only.</summary>
	private const int MaxCells = 20000;

	/// <summary>
	/// Distance between two prices of the profile. The caller knows it when the profile keeps tick
	/// prices, and passes zero when the profile is grouped by the scale of the chart: then it is
	/// the smallest gap between two prices, which cannot be smaller than the grouping.
	/// </summary>
	public static decimal DetectStep(IReadOnlyList<PriceVolume> levels, decimal knownStep)
	{
		if (knownStep > 0m)
			return knownStep;

		if (levels == null || levels.Count < 2)
			return 0m;

		var sorted = Sorted(levels);
		var step = 0m;

		for (var i = 1; i < sorted.Count; i++)
		{
			var gap = sorted[i].Price - sorted[i - 1].Price;

			if (gap > 0m && (step == 0m || gap < step))
				step = gap;
		}

		return step;
	}

	/// <summary>Bands of prices traded at or above <see cref="NodeSettings.HighPercent"/> of the peak.</summary>
	public static List<Band> FindHigh(IReadOnlyList<PriceVolume> levels, NodeSettings settings, decimal knownStep)
		=> Find(levels, settings, knownStep, high: true);

	/// <summary>Bands of prices traded at or below <see cref="NodeSettings.LowPercent"/> of the peak.</summary>
	public static List<Band> FindLow(IReadOnlyList<PriceVolume> levels, NodeSettings settings, decimal knownStep)
		=> Find(levels, settings, knownStep, high: false);

	private static List<Band> Find(IReadOnlyList<PriceVolume> levels, NodeSettings settings, decimal knownStep, bool high)
	{
		var bands = new List<Band>();

		if (levels == null || levels.Count == 0 || settings == null)
			return bands;

		var step = DetectStep(levels, knownStep);

		if (step <= 0m)
			return bands;

		var grid = BuildGrid(levels, step, out var first);

		if (grid.Length == 0)
			return bands;

		var peak = 0m;

		foreach (var volume in grid)
		{
			if (volume > peak)
				peak = volume;
		}

		if (peak <= 0m)
			return bands;

		var threshold = peak * (high ? settings.HighPercent : settings.LowPercent) / 100m;
		var tolerance = Math.Max(settings.GapTolerance, 0);
		var minLevels = Math.Max(settings.MinLevels, 1);

		var runStart = -1;
		var runEnd = -1;
		var runPeak = 0m;
		var skipped = 0;

		for (var i = 0; i < grid.Length; i++)
		{
			var volume = grid[i];
			var selected = high ? volume >= threshold : volume <= threshold;

			if (selected)
			{
				if (runStart < 0)
				{
					runStart = i;
					runPeak = 0m;
				}

				runEnd = i;
				skipped = 0;

				if (volume > runPeak)
					runPeak = volume;

				continue;
			}

			if (runStart < 0)
				continue;

			// A tolerated price keeps the run alive but never becomes its end, so a band never
			// reaches beyond the last price that actually belongs to it.
			if (++skipped <= tolerance)
				continue;

			Close(bands, runStart, runEnd, runPeak, first, step, minLevels);
			runStart = -1;
			skipped = 0;
		}

		Close(bands, runStart, runEnd, runPeak, first, step, minLevels);

		return bands;
	}

	private static void Close(List<Band> bands, int runStart, int runEnd, decimal runPeak, decimal first, decimal step, int minLevels)
	{
		if (runStart < 0 || runEnd < runStart)
			return;

		var count = runEnd - runStart + 1;

		if (count < minLevels)
			return;

		// The band covers whole prices: half a step below the first and half a step above the last.
		var low = first + runStart * step - step / 2m;
		var high = first + runEnd * step + step / 2m;

		bands.Add(new Band(low, high, count, runPeak));
	}

	/// <summary>
	/// Volume of every price between the lowest and the highest of the profile, zero included.
	/// A profile so wide that the grid would not be worth building is read as it came.
	/// </summary>
	private static decimal[] BuildGrid(IReadOnlyList<PriceVolume> levels, decimal step, out decimal first)
	{
		var sorted = Sorted(levels);
		first = sorted[0].Price;
		var last = sorted[sorted.Count - 1].Price;

		var cells = (last - first) / step;

		if (cells < 0m || cells > MaxCells)
		{
			var traded = new decimal[sorted.Count];

			for (var i = 0; i < sorted.Count; i++)
				traded[i] = sorted[i].Volume;

			return traded;
		}

		var grid = new decimal[(int)Math.Floor(cells) + 1];

		foreach (var level in sorted)
		{
			var index = (int)Math.Round((level.Price - first) / step, MidpointRounding.AwayFromZero);

			if (index >= 0 && index < grid.Length)
				grid[index] += level.Volume;
		}

		return grid;
	}

	private static List<PriceVolume> Sorted(IReadOnlyList<PriceVolume> levels)
	{
		var sorted = new List<PriceVolume>(levels);
		sorted.Sort(static (a, b) => a.Price.CompareTo(b.Price));
		return sorted;
	}
}
