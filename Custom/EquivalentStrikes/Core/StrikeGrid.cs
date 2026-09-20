namespace ATAS.Indicators.Technical.EquivalentStrikesCore;

using System;
using System.Collections.Generic;

/// <summary>One line of the grid: a strike of the underlying and the chart price it maps to.</summary>
public readonly record struct StrikeLine(decimal Strike, decimal Price, bool IsMajor);

/// <summary>
/// Affine map between an underlying (QQQ, NDX, SPY, SPX) and the instrument of the chart:
/// <c>price = strike * Factor + Basis</c>. The same map converts a chart price back into a
/// price of the underlying, and enumerates the strikes that fall inside a price range.
/// </summary>
public sealed class StrikeGrid
{
	/// <summary>Multipliers applied to the strike spacing when the range holds too many lines.</summary>
	private static readonly decimal[] Ladder = { 1m, 2m, 5m, 10m, 20m, 50m, 100m, 200m, 500m, 1000m };

	private decimal _factor = 1m;
	private decimal _spacing = 1m;
	private decimal _majorSpacing;
	private int _maxLines = 100;

	/// <summary>Chart points per point of the underlying. Must be positive.</summary>
	public decimal Factor
	{
		get => _factor;
		set => _factor = value;
	}

	/// <summary>Added after the factor: the premium of the future over the index, in chart points.</summary>
	public decimal Basis { get; set; }

	/// <summary>Distance between strikes, in points of the underlying.</summary>
	public decimal Spacing
	{
		get => _spacing;
		set => _spacing = value;
	}

	/// <summary>Strikes that are a multiple of this value are major. Zero disables the distinction.</summary>
	public decimal MajorSpacing
	{
		get => _majorSpacing;
		set => _majorSpacing = value;
	}

	/// <summary>Lines the grid may produce before the spacing is coarsened.</summary>
	public int MaxLines
	{
		get => _maxLines;
		set => _maxLines = value < 1 ? 1 : value;
	}

	public bool IsValid => _factor > 0 && _spacing > 0;

	/// <summary>Chart price of a strike.</summary>
	public decimal ToPrice(decimal strike) => strike * _factor + Basis;

	/// <summary>Price of the underlying equivalent to a chart price.</summary>
	public decimal ToStrike(decimal price) => _factor <= 0 ? 0m : (price - Basis) / _factor;

	/// <summary>Basis that maps <paramref name="underlying"/> onto <paramref name="chartPrice"/> with a known factor.</summary>
	public static decimal BasisFrom(decimal chartPrice, decimal underlying, decimal factor)
		=> chartPrice - underlying * factor;

	/// <summary>Factor that maps <paramref name="underlying"/> onto <paramref name="chartPrice"/> with a known basis.</summary>
	public static decimal FactorFrom(decimal chartPrice, decimal underlying, decimal basis)
		=> underlying == 0m ? 0m : (chartPrice - basis) / underlying;

	/// <summary>Strike of the grid closest to a chart price.</summary>
	public decimal NearestStrike(decimal price)
	{
		if (!IsValid)
			return 0m;

		return Math.Round(ToStrike(price) / _spacing, MidpointRounding.AwayFromZero) * _spacing;
	}

	/// <summary>
	/// Fills <paramref name="into"/> with the strikes between two chart prices, from the lowest up.
	/// Returns the spacing actually used, which is a multiple of <see cref="Spacing"/> when the
	/// range holds more than <see cref="MaxLines"/> strikes, or zero when nothing is drawn.
	/// </summary>
	public decimal Build(decimal priceLow, decimal priceHigh, List<StrikeLine> into)
	{
		into.Clear();

		if (!IsValid)
			return 0m;

		if (priceLow > priceHigh)
			(priceLow, priceHigh) = (priceHigh, priceLow);

		// The factor is positive, so the price range maps to a range of the underlying with the same order.
		var low = ToStrike(priceLow);
		var high = ToStrike(priceHigh);

		var step = _spacing;

		foreach (var multiplier in Ladder)
		{
			step = _spacing * multiplier;

			if (Count(low, high, step) <= _maxLines)
				break;
		}

		var first = Math.Ceiling(low / step);
		var last = Math.Floor(high / step);

		for (var i = first; i <= last && into.Count < _maxLines; i++)
		{
			var strike = i * step;
			into.Add(new StrikeLine(strike, ToPrice(strike), IsMajor(strike, step)));
		}

		return into.Count == 0 ? 0m : step;
	}

	private bool IsMajor(decimal strike, decimal step)
	{
		if (_majorSpacing <= 0m)
			return false;

		// Once the grid is coarser than the major spacing every line it keeps is at least as meaningful.
		return step >= _majorSpacing || strike % _majorSpacing == 0m;
	}

	private static decimal Count(decimal low, decimal high, decimal step)
	{
		var count = Math.Floor(high / step) - Math.Ceiling(low / step) + 1m;
		return count < 0m ? 0m : count;
	}
}
