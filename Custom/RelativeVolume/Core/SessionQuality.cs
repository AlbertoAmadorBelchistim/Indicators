namespace ATAS.Indicators.Technical.RelativeVolumeCore;

using System;
using System.Collections.Generic;

/// <summary>
/// Which past sessions are worth comparing against.
/// </summary>
/// <remarks>
/// A half day around a holiday trades a fraction of a normal session and ends early. Left in the
/// reference it drags the middle down all afternoon, and then an ordinary Tuesday looks busy. A
/// session much shorter than the usual one is left out rather than corrected.
/// </remarks>
public static class SessionQuality
{
	/// <summary>
	/// Marks the sessions long enough to count, comparing each one against the median length.
	/// </summary>
	/// <param name="durations">Length of each session in minutes, in the order they happened.</param>
	/// <param name="minFraction">Fraction of the median length a session needs. Zero keeps them all.</param>
	public static bool[] SelectFull(IReadOnlyList<int> durations, decimal minFraction)
	{
		if (durations == null || durations.Count == 0)
			return Array.Empty<bool>();

		var kept = new bool[durations.Count];

		if (minFraction <= 0m)
		{
			for (var i = 0; i < kept.Length; i++)
				kept[i] = true;

			return kept;
		}

		var sorted = new List<int>(durations);
		sorted.Sort();

		var median = sorted[sorted.Count / 2];
		var threshold = median * minFraction;

		for (var i = 0; i < durations.Count; i++)
			kept[i] = durations[i] >= threshold;

		return kept;
	}
}
