namespace ATAS.Indicators.Technical.EquivalentStrikesCore;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Res = ATAS.Indicators.Technical.EquivalentStrikesResources;

/// <summary>Where the equivalence of the last price is written.</summary>
public enum ReadoutPosition
{
	/// <summary>Next to the price scale, at the height of the last price.</summary>
	[Display(ResourceType = typeof(Res), Name = nameof(Res.AtPrice))]
	AtPrice,

	[Display(ResourceType = typeof(Res), Name = nameof(Res.TopLeft))]
	TopLeft,

	[Display(ResourceType = typeof(Res), Name = nameof(Res.TopRight))]
	TopRight,

	[Display(ResourceType = typeof(Res), Name = nameof(Res.BottomLeft))]
	BottomLeft,

	[Display(ResourceType = typeof(Res), Name = nameof(Res.BottomRight))]
	BottomRight,
}

/// <summary>Defaults of the second layer: the index behind an ETF pair.</summary>
public readonly record struct IndexLayerValues(string Label, decimal Spacing);

public static class StrikeCompanion
{
	/// <summary>
	/// The index whose strikes go with an ETF preset: NDX for QQQ, SPX for SPY. The index layer uses
	/// a factor of one and the same basis as the ETF, which is exact when the anchor had both quotes.
	/// </summary>
	public static bool TryGet(StrikePreset primary, out IndexLayerValues values)
	{
		switch (primary)
		{
			case StrikePreset.QqqOnNq:
				values = new IndexLayerValues("NDX", 100m);
				return true;

			case StrikePreset.SpyOnEs:
				values = new IndexLayerValues("SPX", 25m);
				return true;

			default:
				values = default;
				return false;
		}
	}
}

/// <summary>
/// One line to draw once both layers are combined. A line can carry a strike of the primary layer,
/// of the index layer, or of both when they fall on the same spot of the chart.
/// </summary>
public readonly record struct MergedLine(decimal Price, StrikeLine? Primary, StrikeLine? Index)
{
	public bool IsShared => Primary.HasValue && Index.HasValue;
}

public static class StrikeMerge
{
	/// <summary>
	/// Combines the lines of both layers in price order. An index line closer than
	/// <paramref name="tolerance"/> (chart points) to a primary line is folded into the nearest
	/// primary line that has no index strike yet, so two strikes on one spot become one line with
	/// one label instead of two lines drawn over each other.
	/// </summary>
	public static void Merge(IReadOnlyList<StrikeLine> primary, IReadOnlyList<StrikeLine> index, decimal tolerance, List<MergedLine> into)
	{
		into.Clear();

		// Every close pair, nearest first, so each primary line takes the index strike closest to it.
		var pairs = new List<(int Primary, int Index, decimal Distance)>();

		for (var j = 0; j < index.Count; j++)
		{
			for (var i = 0; i < primary.Count; i++)
			{
				var distance = Math.Abs(primary[i].Price - index[j].Price);

				if (distance <= tolerance)
					pairs.Add((i, j, distance));
			}
		}

		pairs.Sort((a, b) => a.Distance.CompareTo(b.Distance));

		var partner = new int[primary.Count];
		var joined = new bool[index.Count];

		for (var i = 0; i < partner.Length; i++)
			partner[i] = -1;

		foreach (var (i, j, _) in pairs)
		{
			if (partner[i] >= 0 || joined[j])
				continue;

			partner[i] = j;
			joined[j] = true;
		}

		for (var i = 0; i < primary.Count; i++)
			into.Add(new MergedLine(primary[i].Price, primary[i], partner[i] >= 0 ? index[partner[i]] : null));

		for (var j = 0; j < index.Count; j++)
		{
			if (!joined[j])
				into.Add(new MergedLine(index[j].Price, null, index[j]));
		}

		into.Sort((a, b) => a.Price.CompareTo(b.Price));
	}
}
