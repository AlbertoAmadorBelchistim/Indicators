namespace ATAS.Indicators.Technical.EquivalentStrikesCore;

using System;
using System.ComponentModel.DataAnnotations;

using Res = ATAS.Indicators.Technical.EquivalentStrikesResources;

/// <summary>What a single quote of the underlying is used to compute.</summary>
public enum AnchorTarget
{
	/// <summary>Keep the factor and move the grid: the basis absorbs the premium of the future.</summary>
	[Display(ResourceType = typeof(Res), Name = nameof(Res.Basis))]
	Basis,

	/// <summary>Keep the basis and stretch the grid.</summary>
	[Display(ResourceType = typeof(Res), Name = nameof(Res.Factor))]
	Factor,
}

/// <summary>Result of an anchor: the map it produces, or <see cref="Ok"/> false when the quotes cannot produce one.</summary>
public readonly record struct AnchorResult(decimal Factor, decimal Basis, bool Ok);

/// <summary>
/// Turns quotes taken at one moment into the map of <see cref="StrikeGrid"/>.
/// </summary>
/// <remarks>
/// With the quote of the index as well as the quote of the underlying the two parameters separate
/// exactly: the factor is the ratio between the index and the underlying (NDX / QQQ), and the basis
/// is what the future adds over the index. With only the underlying one of the two has to be known.
/// </remarks>
public static class StrikeAnchor
{
	public static AnchorResult Solve(
		decimal chartPrice,
		decimal underlyingPrice,
		decimal indexPrice,
		decimal factor,
		decimal basis,
		AnchorTarget target)
	{
		if (underlyingPrice <= 0m || chartPrice <= 0m)
			return new AnchorResult(factor, basis, false);

		if (indexPrice > 0m)
		{
			var pairFactor = Math.Round(indexPrice / underlyingPrice, 6, MidpointRounding.AwayFromZero);
			return new AnchorResult(pairFactor, Round(chartPrice - indexPrice), true);
		}

		if (target == AnchorTarget.Basis)
		{
			return factor <= 0m
				? new AnchorResult(factor, basis, false)
				: new AnchorResult(factor, Round(chartPrice - underlyingPrice * factor), true);
		}

		var solved = Math.Round((chartPrice - basis) / underlyingPrice, 6, MidpointRounding.AwayFromZero);

		return solved <= 0m
			? new AnchorResult(factor, basis, false)
			: new AnchorResult(solved, basis, true);
	}

	private static decimal Round(decimal value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);
}
