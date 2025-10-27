## 🟦 LevelsLolo (Levels Text)

- **File name:** `LevelsLolo.cs`  
- **Indicator name:** LevelsLolo  
- **Official web:** - (custom indicator; documentation in this repository)  
- **Credits:** Inspired by the original idea of **Alejandro Uriza - LevelsPro**, who introduced the structured visualization concept for SpotGamma levels.

![Level overlay on chart](../img/LevelsLolo1.png)

---

### ⚙️ Configurable Parameters

#### 🖋️ Text and Alignment
- **Font size:** text size (6–48 px, default 10).  
- **Right-aligned text:** align text to the right of the line (if disabled, text is left-aligned).  
- **Last bar only:** extend lines only up to the **last visible bar** instead of the full right edge.  
- **Offset X / Offset Y:** horizontal and vertical text offset in pixels from the base point.

#### 📏 Width and Transparency
- **Thick / Medium / Thin width:** line width per thickness category.  
- **Thick / Medium / Thin alpha:** line opacity per category (0–255).  
- **Thick / Medium max rank:** maximum *rank* allowed for each tier  
  (`≤ ThickMaxRank → thick`, `≤ MediumMaxRank → medium`, higher → thin).

#### 🎨 Colors and Styles
- **Pens (CO, LG, VT, CW, PW, ZG, Other):** base color and width per type.  
- Warm tones are used to visually separate this indicator from others:  
  - `PW` → support (green) `CW` → resistance (red)  
  - `VT` → volatility trigger (yellow) `LG` → institutional absorption (orange)  
  - `CO` → price magnet (amber) `ZG` → regime context (neutral gray)  
- **Enable 0DTE halo:** bright red stroke drawn below the main line for *0DTE* levels.  
  - **0DTE halo alpha / extra width:** opacity and width of the halo accent.

#### 💾 Data and Visibility
- **Raw text:** input field containing level text.  
- Price levels are separated by commas, and multiple levels at the same price are joined with `&`.  
  Example:  
  `$SP: CO44, 7073, LG07, 7048, CO05 & LG14, 6898, VT 0DTE, 6743, LG1 0DTE, 6720`
- **Clear text now:** manually clears the input box.  
- **Only visible price range:** skip levels outside the visible Y-range.

![LevelsLolo configuration panel](../img/LevelsLoloConfig.png)

---

### 🧭 Classification
📂 **Levels** - External level visualization indicators (text / CSV) with hierarchical ordering by type and magnitude.

---

### 🧠 Typical Use Cases
- Load **SpotGamma** levels (CO, LG, PW, CW, VT, ZG).  
- Interpret magnitude (lower = more important → LG01 > LG05 > LG15).  
- Identify zones of **institutional absorption**, **option walls**, and **volatility triggers**.  
- Visualize price magnets and other **Points of Interest (POIs)** relevant for the session.

---

### 📊 Relevance Level
🔟 **9 / 10**  
✅ Direct visualization of key session levels  
✅ Fine control over line width, color, and hierarchy  
⛔ Requires keeping the SpotGamma text string up to date

---

### 🎯 Scalping Strategies Where It Excels
- **Controlled bounce:** test at **LG01 / LG05** with clear rejection → *fade* entry.  
- **Confirmed breakout:** cross and retest of **VT** → continuation.  
- **Option wall fade:** approach to **PW/CW** with exhaustion → counter move.  
- **Price magnets:** **CO** = partial take-profit targets.

---

### ⚙️ Optimal Configuration (1-min, ES Mini)
- **Font size:** 10  
- **Offset X/Y:** 6 / 6  
- **Right-aligned:** ✅  
- **Last bar only:** ✅  
- **ThickMaxRank / MediumMaxRank:** 3 / 10  
- **Enable 0DTE halo:** ✅ (Alpha ≈ 120, ExtraWidth 2)  
- **Only visible price range:** ✅  
- **Pens:** default SpotGamma colors  
- **Opacity tiers:** 255 / 210 / 160  

✅ Draws thicker, more opaque lines for **lower ranks (1–3)**.  
✅ Highlights *0DTE* levels with a bright red halo and dotted accent for **LG/PW/CW**.  
⛔ Informational overlay - does not replace contextual volume analysis.

---

### 🧪 Development Notes
- **Inline parsing:** recognizes tokens `CO`, `LG`, `VT`, `CW`, `PW`, `ZG` with *rank* and `0DTE` suffix.  
- **Category priority:** `VT > LG > PW/CW > CO > ZG > Other`.  
- **Line width and alpha:** computed dynamically by effective *rank* (`null / 0 → thick`).  
- **Rendering logic:**  
  - Draw order → halo → main line → accent (0DTE dotted).  
  - Text aligned per `RightAligned` and offset by `Offset X/Y`.  
- **Font and recalculation:** changing font size triggers `RecalculateValues()`.  
- **Modular design:** uses classes `LabelToken`, `MergedLevel`, `Category`.

---

### 🆕 Key Improvements Over Previous Versions
- **Style adapts to level importance:** both **line width** and **opacity (alpha)** are now determined automatically by the *rank* - lower ranks are drawn thicker and more opaque.  
- **Merged levels prioritization:** when multiple levels exist at the **same price**, the visual style is based on the **most important one**, ranked first by *rank* (lower = stronger) and then by **type** (priority: VT > LG > PW/CW > CO > ZG > Other).  
- **New detectable labels:** added support for variants like `0DTE`, `Zero Gamma`, `LargeGamma`, `PutWall`, `CallWall`, and others - expanding compatibility with different SpotGamma formats.

![Call Wall visualization](../img/LevelsLolo2.png)

---

### 🛠️ Potential Future Improvements
- Automatic import from SpotGamma file or clipboard.  

---
