namespace ATAS.Indicators.Technical.RelativeVolumeCore;

using System;
using System.Collections.Generic;

/// <summary>
/// What the reference says about one moment of the day: the middle of the past sessions, the two
/// percentiles around it, and how many sessions that came from.
/// </summary>
public readonly record struct Stats(decimal Median, decimal Low, decimal High, int Sessions)
{
	public bool IsEmpty => Sessions == 0;

	/// <summary>The reference scaled to part of a bar, for a bar that is still forming.</summary>
	public Stats Prorate(decimal fraction)
	{
		if (fraction <= 0m)
			return new Stats(0m, 0m, 0m, Sessions);

		if (fraction >= 1m)
			return this;

		return new Stats(Median * fraction, Low * fraction, High * fraction, Sessions);
	}
}

/// <summary>What is being compared against the past sessions.</summary>
public enum RvolMetric
{
	/// <summary>Volume traded since the session opened.</summary>
	CumulativeVolume,

	/// <summary>Volume of the bar.</summary>
	BarVolume,

	/// <summary>Range of the bar, high to low.</summary>
	BarRange,
}

/// <summary>
/// Today against the same time of day of the past sessions.
/// </summary>
/// <remarks>
/// The moment of the day is counted in minutes since the session opened, not as a clock time and
/// not as a bar index. That way the reference survives a change of daylight saving, a session that
/// starts late, and a chart whose bars are not minutes at all.
/// </remarks>
public sealed class RvolEngine
{
	private readonly Dictionary<int, List<decimal>> _cumulative = new();
	private readonly Dictionary<int, List<decimal>> _barVolume = new();
	private readonly Dictionary<int, List<decimal>> _barRange = new();

	private bool _built;

	/// <summary>Sessions a moment of the day needs before its reference is worth anything.</summary>
	public int MinSessions { get; set; } = 10;

	/// <summary>Sessions kept: the most recent ones, because the market of six months ago is another market.</summary>
	public int MaxSessions { get; set; } = 20;

	public decimal LowPercentile { get; set; } = 25m;

	public decimal HighPercentile { get; set; } = 75m;

	/// <summary>Moments of the day the reference knows about.</summary>
	public int Buckets => _cumulative.Count;

	/// <summary>
	/// Adds one bar of a past session. The bars have to arrive in the order they happened, because
	/// only the last <see cref="MaxSessions"/> are kept.
	/// </summary>
	public void Add(int minutesFromOpen, decimal cumulativeVolume, decimal barVolume, decimal barRange)
	{
		if (minutesFromOpen < 0)
			return;

		_built = false;

		Append(_cumulative, minutesFromOpen, cumulativeVolume);
		Append(_barVolume, minutesFromOpen, barVolume);
		Append(_barRange, minutesFromOpen, barRange);
	}

	public void Clear()
	{
		_cumulative.Clear();
		_barVolume.Clear();
		_barRange.Clear();
		_built = false;
	}

	/// <summary>Sorts every moment of the day so the percentiles can be read off it.</summary>
	public void Build()
	{
		if (_built)
			return;

		Sort(_cumulative);
		Sort(_barVolume);
		Sort(_barRange);
		_built = true;
	}

	public Stats Get(RvolMetric metric, int minutesFromOpen)
	{
		Build();

		var source = metric switch
		{
			RvolMetric.BarVolume => _barVolume,
			RvolMetric.BarRange => _barRange,
			_ => _cumulative,
		};

		if (!source.TryGetValue(minutesFromOpen, out var values) || values.Count < MinSessions)
			return default;

		return new Stats(
			Percentile(values, 50m),
			Percentile(values, LowPercentile),
			Percentile(values, HighPercentile),
			values.Count);
	}

	/// <summary>
	/// Today over the reference. One is an ordinary moment; two is twice the usual. Zero means the
	/// reference has nothing to say, which is not the same as saying the activity is zero.
	/// </summary>
	public static decimal Ratio(decimal value, decimal reference)
		=> reference > 0m ? value / reference : 0m;

	private void Append(Dictionary<int, List<decimal>> target, int bucket, decimal value)
	{
		if (!target.TryGetValue(bucket, out var values))
		{
			values = new List<decimal>(MaxSessions);
			target[bucket] = values;
		}

		values.Add(value);

		// The oldest session of this moment of the day drops out.
		if (values.Count > MaxSessions)
			values.RemoveAt(0);
	}

	private static void Sort(Dictionary<int, List<decimal>> target)
	{
		foreach (var values in target.Values)
			values.Sort();
	}

	/// <summary>Nearest rank on the sorted values: no interpolation, so every number is one a session really had.</summary>
	private static decimal Percentile(List<decimal> sorted, decimal percentile)
	{
		if (sorted.Count == 0)
			return 0m;

		if (sorted.Count == 1)
			return sorted[0];

		var rank = (int)Math.Ceiling((double)percentile / 100d * sorted.Count) - 1;

		if (rank < 0)
			rank = 0;

		if (rank >= sorted.Count)
			rank = sorted.Count - 1;

		return sorted[rank];
	}
}
