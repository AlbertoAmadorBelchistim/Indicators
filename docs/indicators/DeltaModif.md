## 🟥 Delta (Modified Version)

- **File Name:** [DeltaModif.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/DeltaModif.cs)
- **Indicator Name:** Delta Modif
- **Official Website:** [ATAS — Delta](https://help.atas.net/en/support/solutions/articles/72000602362-delta)
- **Compatibility:** ATAS beta versions and higher.
For backward compatibility, the "stable" build of the indicators must be used.
- **Current Version:** 1.3.0 (6/11/2025) *(Extended and improved version by Alberto Amador Belchistim over the official ATAS beta)*
- **Acknowledgments:** To **LoloTrader** and **Nick** for their suggestions and ideas to improve this indicator.

![Delta](../img/Delta.png)

---

### ⚙️ Configurable Parameters

#### 📊 Visualization
- **Mode**:
	- `Candles`: standard format (body + wicks).
	- `High-Low`: Wicks are rendered as white "bodies".
	- `Histogram`: Bodies without wicks.
	- `Bars`: only vertical lines (bar format).
- **Minimized Mode**: Displays absolute delta, only changing the candle color (green/red).
- **Show Current Value**: displays the delta value on the Y-axis.
- **Show Threshold Lines**: Displays the 4 threshold lines.
- **Threshold Source**: `Fixed / DynamicSigned`.
	- `Fixed`: uses fixed levels configured in Fixed Threshold (Upper/Lower Major/Minor).
	- `DynamicSigned`: calculates mean and standard deviation per sign (Welford) anchored to the current session.

![Different types of visualization](../img/DeltaVisualization.png)

#### 🟦 Dynamic Threshold
- **Session Window Mode**: time window for dynamic calculation. Can be:
	- `RTH`: regular trading hours.
	- `Full24h`: full day.
- **RTH Start / End (HH:mm)**: session boundaries (default 09:30–16:00).
- **Std Multiplier (k)**: number of standard deviations to define the Major levels (default `1.0`).

![Differences Threshold](../img/DeltaThreshold.png)

---

#### 🧰 Filters
Hides bars based on criteria of price candle direction, delta type, and delta value:
- **Bar Direction (Price):** `Any / Bullish / Bearish`
- **Delta Type:** `Any / Positive / Negative`
- **Filter**: minimum |delta| amount per bar.

![Filters](../img/DeltaFilters.png)

NOTE: These filters affect the display of divergences, absorptions, and visual signals on the price, as for practical purposes, the bars that do not meet the criteria **do not exist**.

---
#### 🔀 Divergences
Detects when price and delta move in opposite directions.
* **ShowDivergence**: Shows the classic divergence dots (circles) on the **price chart**.
* **DivergenceBarsFilter**: Allows coloring the **candles/bars of the Delta indicator itself** that show divergence. You can enable/disable and choose the color.

![Divergences](../img/DeltaDivergencias.png)

---

#### 🧲 Absorption:
Looks for bars where the delta shows a significant reversal from its high/low to the close.
* **Absorption**: Enables and defines the threshold to detect absorption. Draws a small dot in the Delta panel when it detects a significant "tail" in the Delta candle.

![Absorption](../img/DeltaAbsorption.png)

---

#### 🔢 Volume Label (Delta Panel)
Displays the numerical Delta value over each bar in the indicator panel.
NOTE: It only appears if the price chart is in Footprint format to avoid display issues.
* **Show**: Enables/Disables this numerical label.
* **Color**: Text color.
* **Location**: Position of the label (`Up / Middle / Down` of the bar).
* **Font**: Font type and size.

![Volume Label](../img/DeltaVolumeLabel.png)

---

#### 📍 Visual Signals in Price Panel (Triangles)
Draws triangles on the **price chart** to flag bars whose delta exceeds the marked threshold.
IMPORTANT: This logic is independent of audio alerts.

* **Price signal Offset ticks**: Distance (in ticks) to separate the triangle from the candle (High/Low).
* **Price Signal Size**: Size in pixels of the triangle.
* **Price Signal Up Color / Price Signal Down Color**: Colors for positive and negative delta triangles.
* **Show Visual Alerts**: Enables or disables these triangles.
* **Visual Up Threshold / Visual Down Threshold**: Defines which threshold (`Major` or `Minor`) the Delta must exceed for the triangle to be drawn.

![Visual Signals](../img/DeltaVisualAlerts.png)

---

#### 🔔 Alerts
Configures audio alerts.
* **Alert File**: Name of the sound file (e.g., "alert1").
* **Font Color / Background**: Colors of the alert pop-up.
* **Audio Alerts**: Enables/Disables audio alerts.
* **Audio Up Threshold / Audio Down Threshold**: Defines which threshold (`Major` or `Minor`) the Delta must exceed to trigger the audio alert.
* **Audio At Bar Close Only**:
	- `Enabled (true)`: The alert will only sound when the candle **closes** above/below the threshold (avoids intra-bar false alarms).
	- `Disabled (false)`: Will sound in real-time as soon as the delta crosses the threshold.

---

### ✨ Improvements Introduced in the Official ATAS Beta Version

1.  **Divergence Coloring on Delta Candles**
    * In addition to the classic dots on the price, the Delta indicator's own candles/bars can now be colored when there is a divergence.
    * The color is controlled from the UI and adapts to any visual mode (Candles, Histogram, etc.).

2.  **Absorption UI Improvements**
    * An `Absorption` group has been created that unifies the control (enable/disable) and the threshold value.
    * Any change to the absorption threshold updates the drawing instantly, without reloading the indicator.

3.  **Visual Finish**
    * The border from the Delta candles has been removed for a cleaner, more modern look.

---

### ✨ Improvements Added by Alberto Amador Belchistim

#### 1) Price Signals (Signals on the Price Chart)
* **What it is**: Visual markers (triangles) that appear on the **price panel** (above/below the candles) when the bar's Delta exceeds an extreme threshold.
* **What it's for**: It allows you to identify peaks of aggression (start of momentum or climax) immediately, without having to look at the lower panel. Ideal for fast scalping.
* **Logic**: The triangles are triggered using the thresholds defined in `VisualUpLevel` and `VisualDownLevel` (you can choose "Major" or "Minor"). These, in turn, use the data source you have chosen (`Fixed` or `DynamicSigned`). This logic is **independent** of audio alerts.

#### 2) Threshold Lines
* **What it is**: Four horizontal lines (`UpMajor`, `UpMinor`, `DnMinor`, `DnMajor`) in the Delta panel.
* **What it's for**: They are the visual reference for your thresholds. They allow you to see at a glance if the current Delta is "normal", "strong" (minor), or "extreme" (major), and they act as the visual reference for the "Price Signals".
* **Logic**: They are drawn automatically based on the `Fixed` or `DynamicSigned` values.

#### 3) UI and Calculation Adjustments
* **UI Reorganization**: The parameters are grouped more logically.
* **No-Repainting Calculation**: The `DynamicSigned` mode calculates its values using the previous (closed) bar, ensuring that signals do not disappear or change in real-time ("non-repainting").

---

### 🧭 Classification
📂 **VolumeOrderFlow** — Indicators of flow and delta aggression per bar or accumulated.

---

### 🧠 Most Frequent Use
* Detecting **momentum changes** or exhaustion through visual divergences (dots on price or colored Delta bars).
* Identifying **absorptions** (aggressions with reversal) with the absorption dots.
* Detecting **extreme delta peaks** with **Price Signals** (triangles) directly on the price panel.
* Using the **Threshold Lines** (fixed or dynamic) to calibrate the level of "significant" delta in each asset and timeframe.

---
 
### 📊 Relevance Level
🔟 **9.0 / 10**

✅ **Focus on Price:** Allows trading with only the price chart, freeing up screen real estate by making the lower panel optional.
✅ **High Value for Scalping:** The on-price signals and the dynamic threshold mode are crucial changes for fast intraday trading.
⛔ **Requires Calibration:** It is still necessary to calibrate the thresholds (Fixed or the Dynamic multiplier) for each asset and timeframe.

---

### 🎯 Scalping Strategies Where It Applies
* **Extreme Delta on Breakout**: `PriceSignalUp` triangle + Delta exceeding `UpMajorLevel` → entry with momentum confirmation.
* **Absorption at Resistance**: `PriceSignalDown` triangle in an upper zone with bearish divergence → possible rejection.
* **Failed Breakout**: Bullish divergence + weak delta → warning of exhaustion.
* **Progressive Flow Change**: Sequence of bars with absorption and smaller delta magnitude → probable reversal.

---

### ⚙️ Parameters I Currently Use (1M, S&P 500)

| **Parameter** | **Recommended Value** | **Comment** |
| :--- | :--- | :--- |
| **Show Threshold Lines** | ✅ | Active visual references |
| **Threshold Source** | `Dynamic` | Allows adapting to the day if it's very volatile or atypical. |
| **Upper major / Lower major** | `500 / -500` | Captures only climax/ignition peaks. Adjust if there's too much noise. |
| **Upper minor / Lower minor** | `250 / -250` | Level of "interest" or average aggression. |
| **Session Window Mode** | `RTH` | Anchors the mean and deviation calculation to the session. |
| **RTH Start** | `09:30:00` | US session open. |
| **RTH End** | `16:00:00` | US session close. |
| **Std Multiplier (k)** | `1.0` | Standard level to define Major/Minor (adds 1 std dev) |
| **Divergence Bars** | ✅ | Add divergence coloring on bars|
| **Absorption Value** | `250` | I look to detect sharp changes from the high/low. |
| **Price Signal Offset Ticks** | `2` | Visibility without hiding the price |
| **Price Signal Size** | `10` | Balanced size |
| **Show visual alerts** | ✅ | **Enable triangles on price** |
| **Visual Up Threshold** | `Major` | **Enable triangles when crossing the upper major threshold** |
| **Visual Down Threshold** | `Major` | **Enable triangles when crossing the lower major threshold** |


✅ This configuration displays delta peaks, divergences, and absorptions in an integrated way, with consistency between the Delta panel and the price chart.

---

### 🧪 Development Notes
* Added new `ValueDataSeries` for Price Signals and Threshold Lines.
* Reorganized the UI (groups *Visualization*, *Drawing*, *Absorption*).
* Implemented conditional logic between alerts, signals, and guide lines.
* Normalized comments to English and cleaned up redundant code.

### 🛠️ Future Improvement Proposals
* Allow selection of marker shape (triangle, diamond, arrow).