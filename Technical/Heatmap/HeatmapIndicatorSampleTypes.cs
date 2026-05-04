#nullable enable

namespace ATAS.Indicators.Technical.Heatmap;

public sealed record HeatmapPriceLineSample
{
	public decimal Price { get; init; }
}

public readonly record struct HeatmapValueAreaSample
{
	public decimal Poc { get; init; }
	public decimal ValueAreaHigh { get; init; }
	public decimal ValueAreaLow { get; init; }
}

public sealed record HeatmapPriceChangeSample
{
	public float Value { get; init; }
}

public sealed record HeatmapPressureSample
{
	public float BuyNormalized { get; init; }
	public float SellNormalized { get; init; }
}
