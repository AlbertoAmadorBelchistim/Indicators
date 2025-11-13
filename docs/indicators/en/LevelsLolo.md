## 🟦 LevelsLolo (Levels Text)

- **File name:** [LevelsLolo.cs](https://github.com/AlbertoAmadorBelchistim/Indicators/blob/compile/myindicators/MyIndicators/LevelsLolo.cs)
- **Indicator name:** LevelsLolo  
- **Official web:** — (custom indicator; documented in this repository)  
- **Compatibility:** ATAS stable build and above  
- **Current version:** 1.1.0 (2025-10-30)  
- **Acknowledgements:** Inspired by the original concept by **Alejandro Uriza — LevelsPro**, which introduced structured visualization of SpotGamma levels.

![Overlay of levels on the chart](../img/LevelsLolo1.png)

---

### ⚙️ Configurable parameters

#### 🖋️ Text and alignment
- **Font size**: font size (6–48 px, default 10)  
- **Right-aligned text**: align text to the right of the line (if disabled, aligns to the left)  
- **Last bar only**: extend lines only up to the **last visible bar** instead of the full chart width  
- **Offset X / Offset Y**: horizontal and vertical offset of the text (in pixels) from the anchor point

#### 📏 Thickness and transparency
- **Thick / Medium / Thin width**: line width for each thickness tier  
- **Thick / Medium / Thin alpha**: opacity for each tier (0–255)  
- **Thick / Medium max rank**: maximum *rank* threshold per category  
  (`≤ ThickMaxRank → thick`, `≤ MediumMaxRank → medium`, remaining → thin)

#### 🎨 Colors and styles
- **Pens (CO, LG, VT, CW, PW, ZG, Other)**: base color and width per level type  
- Warm colors are predefined to differentiate from other indicators:  
  - `PW` → support (green) `CW` → resistance (red)  
  - `VT` → volatility trigger (yellow) `LG` → institutional absorption (orange)  
  - `CO` → price magnet (amber) `ZG` → regime context (neutral gray)  
- **Enable 0DTE halo**: bright red glow below the main line for *0DTE* levels  
  - **0DTE halo alpha / extra width**: opacity and additional halo thickness

#### 💾 Data and visibility
- **Raw text**: manual input of levels  
- Price levels are comma-separated, multiple levels at the same price use `&`  
  Example:  
  `$SP: CO44, 7073, LG07, 7048, CO05 & LG14, 6898, VT 0DTE, 6743, LG1 0DTE, 6720`  
- **Clear text now**: manually clears the current input and chart  
- **Only visible price range**: hides levels outside the current visible range

![Indicator configuration panel](../img/LevelsLoloConfig.png)

---

### 🧭 Classification
📂 **Levels** — Indicators for visualizing external levels (text / CSV) with type and magnitude hierarchy

---

### 🧠 Typical usage
- Load **SpotGamma** levels (CO, LG, PW, CW, VT, ZG)  
- Interpret magnitudes (smaller number = stronger → LG01 > LG05 > LG15)  
- Identify areas of **institutional absorption**, **option walls**, and **volatility triggers**  
- Visualize price magnets and **points of interest (POI)** for the session

---

### 📊 Relevance level
🔟 **9 / 10**  
✅ Direct visualization of session key levels  
✅ Fine control of thickness, color, and hierarchy by rank  
⛔ Requires manual update of the SpotGamma text string

---

### 🎯 Scalping strategies where it applies
- **Controlled rebound:** test at **LG01 / LG05** with quick rejection → *fade* entry  
- **Confirmed breakout:** break and retest of **VT** → continuation  
- **Option wall fade:** proximity to **PW/CW** with exhaustion → counter-move  
- **Price magnets:** **CO** = partial-exit targets

---

### ⚙️ Optimal setup (1-min, ES Mini)
- **Font size:** 10  
- **Offset X/Y:** 6 / 6  
- **Right-aligned:** ✅  
- **Last bar only:** ✅  
- **ThickMaxRank / MediumMaxRank:** 3 / 10  
- **Enable 0DTE halo:** ✅ (Alpha ≈ 120, ExtraWidth 2)  
- **Only visible price range:** ✅  
- **Pens:** default SpotGamma colors  
- **Opacity tiers:** 255 / 210 / 160  

✅ Draws thicker and more opaque lines for **lower ranks** (1–3)  
✅ Highlights *0DTE* levels in bright red and adds dotted accent if **LG/PW/CW**  
⛔ Does not replace contextual volume analysis; acts as an **informative overlay**

---

### 🧪 Development notes
- **Inline parser:** recognizes tokens `CO`, `LG`, `VT`, `CW`, `PW`, `ZG` with *rank* and optional `0DTE` suffix  
- **Category hierarchy:** `VT > LG > PW/CW > CO > ZG > Other`  
- **Width and alpha:** determined by effective *rank* (`null / 0 → thick`)  
- **Render logic:**  
  - Halo → main line → accent (dotted 0DTE LG/PW/CW)  
  - Text aligned according to `RightAligned` and offset by `Offset X/Y`  
- **Font and recalculation:** changing font size triggers `RecalculateValues()`  
- **Modular design:** uses class structure `LabelToken`, `MergedLevel`, `Category`

---

### 🆕 Changelog
#### Version 1.1 (2025-10-30)
- **Fixed “Clear text now” button**: it now correctly clears both the chart and the level text field in the UI immediately.

#### Version 1.0 (2025-10-25)
- **Adaptive style by importance:** line **thickness** and **opacity (alpha)** are automatically derived from *rank* — more important levels (lower ranks) are drawn thicker and more opaque.  
- **Type and number hierarchy:** when several levels share the **same price**, the indicator applies the style of the **most important** one, evaluated first by *rank* (lower = stronger) and then by **type** (priority: VT > LG > PW/CW > CO > ZG > Other).  
- **New recognizable tags:** now detects suffixes and aliases such as `0DTE`, `Zero Gamma`, `LargeGamma`, `PutWall`, `CallWall`, etc., improving compatibility with diverse SpotGamma text formats.

![Call Wall visualization](../img/LevelsLolo2.png)

---

### 🛠️ Possible future improvements
- Automatic import from SpotGamma file or clipboard.
<!--stackedit_data:
eyJoaXN0b3J5IjpbMTcwMzYzMjc0M119
-->