namespace ATAS.Indicators.Technical.DomPressureCore;

using System;
using System.Collections.Generic;

/// <summary>Which value of the DOM power a bar shows.</summary>
public enum PowerSampling
{
	/// <summary>The last value seen while the bar was open.</summary>
	BarClose,

	/// <summary>The average of every value seen while the bar was open.</summary>
	BarAverage
}

/// <summary>DOM power values seen while one bar was open.</summary>
public struct PowerSample
{
	public decimal Last;

	public decimal Sum;

	public int Count;

	public void Add(decimal value)
	{
		Last = value;
		Sum += value;
		Count++;
	}

	public decimal ValueOf(PowerSampling sampling)
	{
		if (Count == 0)
			return 0m;

		return sampling == PowerSampling.BarAverage ? Sum / Count : Last;
	}
}

/// <summary>
/// DOM power of each bar by bar open time. The market depth has no history, so these values only
/// exist for the bars that were open while the chart was running; keeping them by time lets them
/// survive a recalculation of the chart. Not synchronized: the indicator guards it.
/// </summary>
public sealed class PowerStore
{
	private readonly Dictionary<DateTime, PowerSample> _samples = new();

	public int Count => _samples.Count;

	public void Clear() => _samples.Clear();

	public PowerSample Add(DateTime barTime, decimal value)
	{
		_samples.TryGetValue(barTime, out var sample);
		sample.Add(value);
		_samples[barTime] = sample;
		return sample;
	}

	public bool TryGet(DateTime barTime, out PowerSample sample) => _samples.TryGetValue(barTime, out sample);
}

public static class AbsorptionRule
{
	/// <summary>
	/// Aggression against the book: the bar delta and the DOM power have opposite signs, and the
	/// delta is at least <paramref name="thresholdPercent"/> of the power.
	/// </summary>
	public static bool IsAbsorption(decimal power, decimal delta, decimal thresholdPercent)
	{
		if (power == 0m || delta == 0m || Math.Sign(power) == Math.Sign(delta))
			return false;

		return Math.Abs(delta) >= Math.Abs(power) * thresholdPercent / 100m;
	}
}
