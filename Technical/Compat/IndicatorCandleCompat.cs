using System;
using ATAS.Indicators;


internal static class IndicatorCandleCompat
{
    public static decimal GetVwapCompat(this IndicatorCandle candle)
    {
        if (candle == null)
            throw new ArgumentNullException(nameof(candle));

        #if ATAS_STABLE
        return CalculateVwapFromPriceLevels(candle);
        #else
        return candle.VWAP;
        #endif
    }

    #if ATAS_STABLE
    private static decimal CalculateVwapFromPriceLevels(IndicatorCandle candle)
    {
        var priceLevels = candle.GetAllPriceLevels();
        if (priceLevels == null)
            return 0m;

        decimal totalPriceVolume = 0m;
        decimal totalVolume = 0m;

        foreach (var level in priceLevels)
        {
            if (level == null)
                continue;

            var volume = level.Volume;
            if (volume <= 0m)
                continue;

            totalPriceVolume += level.Price * volume;
            totalVolume += volume;
        }

        return totalVolume > 0m ? totalPriceVolume / totalVolume : 0m;
    }
    #endif
}