# Modified indicators index

- ClusterStatisticModif [(EN)](indicators/ClusterStatisticModif.md) / [(ES)](indicators/ClusterStatisticModif.es.md)
- DeltaaModif [(EN)](indicators/DeltaaModif.md) / [(ES)](indicators/DeltaaModif.es.md)
- LevelsLolo [(EN)](indicators/LevelsLolo.md) / [(ES)](indicators/LevelsLolo.es.md)

# Other repository indicators

| File `.cs`               | Indicator´s name              | Category       | Scalping score           | Version               | Gemini's Verdict | Answer|
|-----------------------------|-----------------------------------|----------------------------|-------------------------------|------------|-----------|----|
| `ACBW.cs` [(ES)](indicators/ACBW.es.md)            | Bill Williams AC                  | Momentum         | 4/10          | Stable| Discard| Is the current trend's momentum accelerating or hitting the brakes?|
| `ACDC.cs` [(ES)](indicators/ACDC.es.md)                  | AC DC Histogram          | Momentum          | 2/10          | Stable| Discard| What is the smoothed, lagging direction of the market's acceleration?|
| `ACR.cs` [(ES)](indicators/ACR.es.md)                  | Average Candle Range            | Volatility        | 5/10         | Stable | Discard (or Improve) | What is the average size of a single candle so far today?|
| `AD.cs` [(ES)](indicators/AD.es.md)                     | Accumulation/Distribution (A/D)         | Volume         | 2/10          | Stable | Discard| Is the cumulative volume flow confirming the price trend, or is it showing a divergence?|
| `ADF.cs` [(ES)](indicators/ADF.es.md)                   | Accumulation/Distribution Flow    | Volume         | 6/10          | Stable | Discard (or Improve) | Where is high 'Effort' (Volume) meeting low 'Result' (Range), signaling a potential absorption or climax?|
| `ADR.cs` [(ES)](indicators/ADR.es.md)                    | Average Daily Range                                | Volatility          | 7/10          | Stable | Keep (Utility)| Has the market already made its expected move of the day?|
| `ADX.cs` [(ES)](indicators/ADX.es.md)                    | ADX                                | Trend        | 6/10          | Stable | Discard| Is the market in a strong trend (either up or down), or is it just 'chopping' sideways?|
| `ADXR.cs` [(ES)](indicators/ADXR.es.md)                 | ADXR                               | Trend         | 2/10         | Stable | Discard| What is the smoothed-out, stable strength of the trend, ignoring short-term noise from the ADX itself?|
| `AMA.cs` [(ES)](indicators/AMA.es.md)                   | AMA                          | Trend                   | 7/10          | Stable | Discard| How can I get a smooth moving average that doesn't lag during a strong breakout but does filter out the 'chop' in a sideways market?|
| `AO.cs` [(ES)](indicators/AO.es.md)                  | Awesome Oscillator                           | Momentum| 4/10         | Stable| Discard|Is the market's recent, short-term momentum (5-bar) currently winning against the longer-term trend's momentum (34-bar) |
| `ATR.cs` [(ES)](indicators/ATR.es.md)                     | ATR                          | Volatility| 6/10                      | Stable| **Keep and Improve**|What has been the _average true size_ (including gaps) of each bar over the last X periods? |
| `ATRN.cs` [(ES)](indicators/ATRN.es.md)                  | ATR Normalized               | Volatility| 3/10                | Stable| Discard| What is the instrument's volatility (ATR) _as a percentage of its current price_? |
| AccountInfoDisplay.cs                  | Account Info Display               | Visualization                   | 7/10                | Beta?| **Keep**|What is my account's real-time status (Balance, PnL, Margin) right now, without me having to look away from the chart? |
| ActiveVolume.cs                  | Active Volume              | Order Flow                   | 8/10|Beta?|**Keep and Improve**| Filtering out all the small 'noise' trades, where is the _significant_, _aggressive_ buying and selling volume actually showing up on the price ladder?|
| AdaptiveBinaryWaveMA.cs                  | Adaptive Binary Wave               | Trend|6/10| Stable| **Keep** |Has the adaptive moving average (AMA) broken out of its recent 'crawl' by a statistically significant amount?|
| AdaptiveRsiAverage.cs                  | Adaptive RSI Moving Average               | Trend| 6/10                | Stable| Discard|How can I get a moving average that automatically _slows down_ when the market is undecided (RSI near 50) and _speeds up_ to catch trends when momentum is strong (RSI near 0 or 100)? |
| `Alligator.cs` [(ES)](indicators/Alligator.es.md)           | Alligator                    | Trend| 6/10        | Stable |Discard|Is the market 'sleeping' (in a range, with intertwined MAs) or is it 'awake and eating' (in a trend, with the MAs spread open)?|
| Angle.cs                 | Study Angle| Trend| 2/10         | Stable| Discard|What is the literal geometric angle (in degrees) of the price trend over the last X bars? |
| AroonIndicator.cs        | Aroon Indicator| Momentum                   | 3/10| Stable| Discard| Is the market's strength coming from _recently_ making new highs, or from _recently_ making new lows?|
| AroonOscillator.cs       | Aroon Oscillator             | Technical                   | Tendencia                        | Stable| Discard| |
| AskBidBars.cs            | Ask/Bid Bars                 | VolumeOrderFlow             | Order flow / Bid-Ask             | Stable| Keep (with caveats)| |
| AverageDelta.cs          | Average Delta                | VolumeOrderFlow             | Delta / Promedio                 | Stable| No| |
<!--stackedit_data:
eyJoaXN0b3J5IjpbNTIyNjk1NjExLDcxOTY0OTU0Niw0Mjg3OD
g3NTMsLTkyNjk0NjU4OCw5OTkxNjMxOTMsLTIxNDY4Mzg1NTQs
MjA2NzEyNTM5OSwzOTg3NTcyNTAsLTE5MzIzMTMyNDUsMTMyMz
M3Mzk2NCwtMzgzOTYyMTc0XX0=
-->