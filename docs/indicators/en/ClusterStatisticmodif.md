## 🟦 Cluster Statistic

- **File name:** [ClusterStatisticModif.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/ClusterStatisticModif.cs)  
- **Indicator name:** Cluster Statistic Modif  
- **Official web:** [ATAS — Cluster Statistic](https://help.atas.net/en/support/solutions/articles/72000602624-cluster-statistics)  
- **Compatibility:** ATAS stable version and above  
- **Current version:** 1.1.0 (2025-10-01)  
*(Extended and optimized version by Alberto Amador Belchistim)*

![Full statistics table](../img/ClusterStatistics.png)

---

### ⚙️ Configurable Parameters

![Row selection panel](../img/ClusterStatisticRawsConfig.png)

#### 🧱 Rows
Choose which data rows are displayed in the cluster statistics table.

- **Trades** – total number of trades per bar.  
- **Height** – vertical height of the bar (in ticks).  
- **Time** – bar start timestamp.  
- **Duration (sec)** – total bar duration in seconds.  
- **Volume** – total traded volume in the bar.  
- **Volume/second** – average execution speed (volume per second).  
- **Show asks / bids** – show ask and bid volumes separately.  
- **Delta** – difference between aggressive buy and sell volume.  
- **Delta/sec** – average delta per second (flow velocity).  
- **Show Delta/Volume** – delta normalized by total volume.  
- **Max / Min Delta** – extreme delta values among bar clusters.  
- **Delta Change** – delta variation vs. the previous bar.  
- **Max Vol/sec (peak)** – highest volume-per-second inside the bar.  
- **Delta at peak** – delta at the moment of the volume peak.  
- **Delta/Vol at peak** – delta-to-volume ratio at the peak moment.  
- **Buy / Sell / Net Imbalances** – per-side and net footprint imbalances.  
- **Stacked Imbalances** – imbalances across consecutive clusters.  
- **Session Volume / Session Delta / Session DeltaVolume** – accumulated values since session start.

---

#### ⚡ Max vol/sec
Parameters for peak speed (volume-per-second) calculation.

![Peak speed configuration](../img/ClusterStatisticPeakConfig.png)

- **Time Window (sec)** – size of the moving window used for the calculation.  
  In the example, it compares cumulative volume in each 5-second interval within the bar.  
- **Min Volume per Window** – minimum volume within the window to include it.  
- **Use Auto Filter** – table colors adapt dynamically using a recent-mean filter.  
- **Auto Filter Period** – number of bars used by the auto filter.  
- **Auto Filter = EMA (off = SMA)** – moving average type (EMA or SMA).

---

#### ⚖️ Imbalance
Imbalance thresholds and filters.

![Imbalance configuration](../img/ClusterStatisticImbalanceConfig.png)

- **Imbalance Threshold (%)** – minimum difference between a level’s ask and the next lower level’s bid to highlight an imbalance.  
- **Imbalance Volume Filter** – minimum volume required to consider the imbalance.

---

#### 🎨 Visualization
General appearance of the panel.

![Visualization configuration](../img/ClusterStatisticVisualizationConfig.png)

- **Background** – panel background color.  
- **Transparency** – background opacity (0–255).  
- **1. Grid** – gridline color.  
- **Gradient display** – enables gradient shading across cells.  
- **Volume / Ask / Bid color** – base colors for each data type.

---

#### ✏️ Text

![Text configuration](../img/ClusterStatisticVisualizationConfig.png)
- **Color** – text color.  
- **Font** – typeface and size (e.g., Arial 9 px).  
- **Centered alignment** – center the text inside cells.  
- **Ratios as percent** – display ratios in percentage format.

---

#### 🧩 Headers
![Headers configuration](../img/ClusterStatisticHeadersConfig.png)
- **Color** – header background color.  
- **Hide headers** – hide the table’s title row.

---

#### 🔔 Volume / Delta / Net Imbalance Alerts
![Alerts configuration](../img/ClusterStatisticAlertsConfig.png)
- **Enabled** – activate the alert.  
- **Filter** – threshold value to trigger the alert.  
- **Alert File** – sound file name (e.g., `alert1`).  
- **Use closed candle** (Net Imbalance only) – evaluate the condition at bar close only.

---

#### ⚙️ Misc.
- **Show description** – display an internal descriptive text below the panel.

---

### 🧭 Classification
📂 **VolumeOrderFlow** — Indicators based on bar-grouped statistics (volume, delta, number of trades, etc.).

---

### 🧠 Typical usage (original version)

- Show a **per-bar summary** of key variables such as Volume, Delta, Trades.  
- Detect bars with **extreme delta** (buy/sell aggression).  
- Confirm **momentum** when volume and delta support a breakout.

---

### ✨ Enhancements in Alberto Amador Belchistim’s version

This modified version preserves the original base but adds advanced metrics oriented to professional scalping.

**Visuals / UI**

- Reordered interface fields to improve readability and speed of interpretation.  
- Better contrast and color differentiation for key metrics to surface relevant signals faster.

**Functional / additional metrics**

- Added **Delta per second (Delta/sec)** to measure net aggression normalized by bar time.  
- Introduced **PeakVolPerSec** and **PeakDeltaPerSec** series to capture intra-bar max speed.  
- Added **PeakDeltaPerVol** (Delta/Vol at the max volume/sec moment) to assess impulse efficiency vs. volume.  
- Integrated mean-based thresholds (EMA/SMA) for peak-speed metrics to highlight exceptional events relative to recent history.  
- Implemented footprint imbalances (buy/sell/net) with volume filter and optional alerts when thresholds are exceeded.  
- Fixed `maxBid` accumulation and `_maxDeltaPerVol` updates for more accurate summaries.

**Practical value**

- Lets the trader see **not only what accumulated**, but **how fast** (vol/sec, delta/sec), improving detection of true impulses vs. noise.  
- Peak metrics (Vol/sec, Delta/sec) help **anticipate or confirm breakouts** before total volume makes it obvious.  
- Imbalances assist in spotting **institutional activity or significant order-book skew** in real time.  
- Visual refinements make the panel **more legible** on fast timeframes (scalping) and constrained layouts.

---

### 📊 Relevance
🔟 **8 / 10**

✅ Extends an already relevant tool in the order-flow ecosystem.  
✅ Adds metrics that improve detection and reaction speed.  
⛔ Requires good command of volume/delta context and some panel space.

---

### 🎯 Scalping strategies

- **Conviction breakout:** high Volume + high Delta/sec + PeakVol/sec above threshold → continuation signal.  
- **Market absorption:** high Volume + low/negative Delta/sec + strong footprint imbalance near S/R → likely rejection.  
- **False breakouts:** high total Volume but low Delta/sec + low PeakDeltaPerVol → lack of impulse, possible pullback.  
- **Sustained impulse sequence:** several consecutive bars with increasing Trades, Vol/sec, Delta/sec → confirm continuation.

---

### ⚙️ Recommended setup for scalping (1M, S&P 500)

- **Active rows:**  
  - ✅ Delta  
  - ✅ Delta/sec  
  - ✅ Delta/Volume  
  - ✅ Delta Change  
  - ✅ Delta/Vol at peak  
  - ✅ Net Imbalances  
  - ✅ Stacked Imbalances  

- **Speed window (Max vol/sec):**  
  - **Time Window (sec):** `5`  
  - **Min Volume per Window:** `150`  
  - **Use Auto Filter:** ✅  
  - **Auto Filter Period:** `3`  
  - **Auto Filter = EMA (off = SMA):** ✅

- **Imbalance thresholds:**  
  - **Imbalance Threshold (%):** `300`  
  - **Imbalance Volume Filter:** `30`

- **Visualization:**  
  - **Background:** `#80000000` (translucent gray)  
  - **Transparency:** `10`  
  - **Grid:** `White`  
  - **Gradient display:** ❌  
  - **Ask / Bid color:** `#FF059E05` (green) / `Red`  
  - **Volume / text color:** `White`

- **Text:**  
  - **Font:** `Arial; 9px`  
  - **Ratios as percent:** ✅  
  - **Centered alignment:** ✅

- **Headers:**  
  - **Color:** `#FF787B86`  
  - **Hide headers:** ❌

✅ Balanced reading between **flow intensity** and **net imbalance**, helping you spot:  
- Abrupt intra-bar delta shifts (fatigue or absorption).  
- Areas with dominant *stacked* or *net* imbalance.  
- Acceleration moments (Delta/sec or Delta/Vol at peak).

⛔ Avoid overloading the table with redundant metrics (e.g., Volume or Max/Min Delta) to keep clarity on 1-minute charts.

---

### 🧪 Development notes

- Based on the standard ATAS implementation, extended with new metrics for richer order-flow reading.  
- Original per-bar accumulation logic is preserved; an internal time window was added to compute Vol/sec, Delta/sec, per-second peaks and dynamic thresholds.  
- The interface was tuned for scalping with improved alignment, more metrics, and clearer visuals.

---

### 🛠️ Future improvements

- Add **automatic alerts** when Delta/sec or PeakDeltaPerVol exceed a session-based dynamic threshold.
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTE4NDc2OTk1NzBdfQ==
-->