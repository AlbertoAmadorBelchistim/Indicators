namespace ATAS.Indicators.Technical.EquivalentStrikesCore;

using System.ComponentModel.DataAnnotations;

using Res = ATAS.Indicators.Technical.EquivalentStrikesResources;

/// <summary>Instrument pairs with known strike ladders.</summary>
public enum StrikePreset
{
	/// <summary>Nothing is filled in: the map and the spacing are the ones already set.</summary>
	[Display(ResourceType = typeof(Res), Name = nameof(Res.Custom))]
	Custom,

	/// <summary>QQQ strikes on a Nasdaq 100 future (NQ, MNQ).</summary>
	[Display(ResourceType = typeof(Res), Name = nameof(Res.QqqOnNq))]
	QqqOnNq,

	/// <summary>NDX strikes on a Nasdaq 100 future (NQ, MNQ).</summary>
	[Display(ResourceType = typeof(Res), Name = nameof(Res.NdxOnNq))]
	NdxOnNq,

	/// <summary>SPY strikes on an S&amp;P 500 future (ES, MES).</summary>
	[Display(ResourceType = typeof(Res), Name = nameof(Res.SpyOnEs))]
	SpyOnEs,

	/// <summary>SPX strikes on an S&amp;P 500 future (ES, MES).</summary>
	[Display(ResourceType = typeof(Res), Name = nameof(Res.SpxOnEs))]
	SpxOnEs,
}

/// <summary>Defaults a preset writes into the settings. The basis always comes from the anchor.</summary>
public readonly record struct PresetValues(decimal Factor, decimal Spacing, decimal MajorSpacing, string Label);

public static class StrikePresets
{
	/// <summary>
	/// Values of a preset. The factors are the usual ratios of each pair (an ETF tracks a tenth or a
	/// forty-first of its index and both drift with dividends), so they are a starting point: the
	/// anchor with the quote of the index computes the exact one.
	/// </summary>
	public static bool TryGet(StrikePreset preset, out PresetValues values)
	{
		switch (preset)
		{
			case StrikePreset.QqqOnNq:
				values = new PresetValues(41m, 1m, 5m, "QQQ");
				return true;

			case StrikePreset.NdxOnNq:
				values = new PresetValues(1m, 25m, 100m, "NDX");
				return true;

			case StrikePreset.SpyOnEs:
				values = new PresetValues(10m, 1m, 5m, "SPY");
				return true;

			case StrikePreset.SpxOnEs:
				values = new PresetValues(1m, 5m, 25m, "SPX");
				return true;

			default:
				values = default;
				return false;
		}
	}
}
