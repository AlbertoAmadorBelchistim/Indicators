#nullable enable

namespace ATAS.Indicators.Technical.Heatmap;

using OFT.Rendering.Heatmap;
using OFT.Rendering.Settings;

/// <summary>
/// Single OHLC Plus level entry inside <see cref="HeatmapOhlcPlusSettings"/>.
/// </summary>
public readonly record struct HeatmapOhlcPlusLevelSettings(
	string Id,
	HeatmapProfilePeriod Period,
	HeatmapOhlcPlusLevelKind Kind,
	string Label,
	LevelSettings Settings);
