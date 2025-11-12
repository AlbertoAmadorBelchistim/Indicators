| File `.cs`               | Indicator´s name              | Category       | Scalping score           | Version               | Gemini's Verdict | Answer|
|-----------------------------|-----------------------------------|----------------------------|-------------------------------|------------|-----------|----|
| `ADR.cs` [(ES)](indicators/ADR.es.md)                    | Average Daily Range                                | Volatility          | 7/10          | Stable | Keep (Utility)| Has the market already made its expected move of the day?|
| `ADX.cs` [(ES)](indicators/ADX.es.md)| ADX                                | Trend        | 6/10          | Stable | **Keep**| Is the market in a strong trend (either up or down), or is it just 'chopping' sideways?|
| `ATR.cs` [(ES)](indicators/ATR.es.md)                     | ATR                          | Volatility| 8/10                      | Stable| **Keep and Improve**|What has been the _average true size_ (including gaps) of each bar over the last X periods? |
| `AccountInfoDisplay.cs` [(ES)](indicators/AccountInfoDisplay.es.md)                  | Account Info Display               | Visualization                   | 7/10                | Latest| **Keep**|What is my account's real-time status (Balance, PnL, Margin) right now, without me having to look away from the chart? |
| `ActiveVolume.cs` [(ES)](indicators/ActiveVolume.es.md)                  | Active Volume              | Order Flow                   | 8/10|Stable|**Keep and Improve**| Filtering out all the small 'noise' trades, where is the _significant_, _aggressive_ buying and selling volume actually showing up on the price ladder?|
<!--stackedit_data:
eyJoaXN0b3J5IjpbNzg5MjY2NjhdfQ==
-->