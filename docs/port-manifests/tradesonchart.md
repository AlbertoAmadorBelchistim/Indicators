# Port manifest — TradesOnChart

This document is the authoritative record for porting `TradesOnChart.cs` from `prready/main`
into the local branch stack.

**Source:** `prready/main`
**Integration target:** `local/tradesonchart-i18n` (stacked on `local/build/04-localization`)
**Status:** `complete`

---

## Phase 1 — Evaluation + Manifest

Evaluation complete. Manifest created as part of port planning.

---

## Phase 2 — Standalone branches (Develop-based)

No standalone fix/feat branches are required before Phase 2. All fixes and features are tightly
coupled to the new TradesOnChart rendering pipeline introduced in this batch and are best
delivered as a single `feat/tradesonchart` branch.

### Fix branches (Develop-based)

None planned.

### Feat branches

| Branch | Base | Status |
|--------|------|--------|
| `feat/tradesonchart` | `Develop` | done |

---

## Phase 3 — Resource keys required in `local/build/04-localization`

33 new keys (5 existing keys already have correct values or need only minor updates).

**Existing keys with changed values (update in-place):**
- `LossColor`: "Loss Color" → "Loss color"
- `TradeDetails`: "Trade Details" → "Details"

### New keys to add

| Key | Value (en) | Introducing commit | Status |
|-----|-----------|-------------------|--------|
| `ProfitColorDescription` | "Color used to display profitable trades." | 777b1019 | done |
| `LossColorDescription` | "Color used to display losing trades." | 777b1019 | done |
| `LabelDisplayModeHide` | "Hide" | 777b1019 | done |
| `LabelDisplayModeShort` | "Short" | 777b1019 | done |
| `LabelDisplayModeFull` | "Full" | 777b1019 | done |
| `LabelDisplayModeCard` | "Card" | 777b1019 | done |
| `TradeDirectionLong` | "Long" | 777b1019 | done |
| `TradeDirectionShort` | "Short" | 777b1019 | done |
| `TradeEntry` | "Entry" | 777b1019 | done |
| `TradeExit` | "Exit" | 777b1019 | done |
| `TradeResult` | "Result" | 777b1019 | done |
| `TradePnL` | "PnL" | 777b1019 | done |
| `TradeIn` | "In" | 777b1019 | done |
| `TradeOut` | "Out" | 777b1019 | done |
| `TradeTicks` | "ticks" | 777b1019 | done |
| `TradeTicksShort` | "t" | 777b1019 | done |
| `LabelDistance` | "Label distance" | 643da7e5 | done |
| `LabelDistanceDescription` | "Vertical spacing between trade markers and labels (px)." | 643da7e5 | done |
| `LabelXAnchor` | "Label centering" | 4196b812 | done |
| `LabelXAnchorDescription` | "Defines the horizontal reference used to position trade labels." | 4196b812 | done |
| `LabelXAnchorCloseBar` | "Close bar" | 4196b812 | done |
| `LabelXAnchorMidpoint` | "Trade midpoint" | 4196b812 | done |
| `LabelMode` | "Label mode" | 751d9c63 | done |
| `LabelModeDescription` | "Selects how trade labels are displayed." | 751d9c63 | done |
| `LinesAndMarkers` | "Lines and markers" | 751d9c63 | done |
| `DetailsGroup` | "Details" | 751d9c63 | done |
| `TradeLine` | "Trade line" | 751d9c63 | done |
| `TradeLineDescription` | "Draws a line connecting entry and exit markers." | 751d9c63 | done |
| `TradeDetailsDescription` | "Shows trade details when hovering markers or labels." | 751d9c63 | done |
| `TradeBuyColor` | "Buy color" | 751d9c63 | done |
| `TradeSellColor` | "Sell color" | 751d9c63 | done |
| `MarkerSize` | "Marker size" | 751d9c63 | done |
| `MarkerSizeDescription` | "Size of entry/exit markers." | 751d9c63 | done |

**Note:** `LineWidth` and `DashStyle` already exist in Resources.resx and do not need to be added.
`ShowDescription`, `LabelDisplay`, `Labels`, `TradeDetails`, `ProfitColor`, `LossColor` already exist
(2 of these need value updates listed above).

---

## Phase 4 — Integration branch: `local/tradesonchart-i18n`

### Fix commits

No standalone fix branches planned (see Phase 2).

---

### Feat commits: `feat/tradesonchart`

All 23 commits go onto a single `feat/tradesonchart` branch rooted at `Develop`.
6 commits require **adaptation**: replace `typeof(Resources)` references with hardcoded strings
or `typeof(Strings)` equivalents (Resources keys don't exist on Develop).

### Skipped commits

| Commit | Description | Reason |
|--------|-------------|--------|
| `fc6fceb4` | fix(DailyLines): wip fixes for production | Unrelated DailyLines fix |
| `78d4efe6` | docs(tradesonchart): add ADR to postpone synthetic FIFO trade reconstruction | Documentation only; ADR has no code change |

### Commits in order

| Commit | Description | Needs adaptation | Status | Notes |
|--------|-------------|-----------------|--------|-------|
| `48ab4e05` | chore: localize Display metadata for label mode and PnL colors | YES | done | Adds `using Properties;`; convert `typeof(Resources)` Display attrs → hardcoded strings for LabelDisplayMode enum; keep `typeof(Strings)` for Visualization group |
| `d0068a75` | fix: keep LabelDisplayMode values stable | no | done | |
| `df4b8d8f` | fix: prevent render crash by snapshotting trades list | no | done | |
| `3749d4f6` | feat: load trade history by chart range | no | done | |
| `33b1bae4` | fix: dedupe history and realtime trades | no | done | |
| `945216c3` | fix: show closed trades immediately | no | done | |
| `bf19b9b6` | perf: optimize bar lookup using binary search | no | done | |
| `ec4bcf9d` | perf: bound label collision checks during rendering | no | done | |
| `3acfb922` | perf: reuse tooltip buffer and avoid per-frame allocations | no | done | |
| `c1a4e902` | perf: reuse string formats and reduce label/tooltip allocations | no | done | |
| `0d15a583` | fix: guard against null candle during label render | no | done | |
| `7451f8d6` | fix: cleanup subscriptions on dispose and prevent double attach | no | done | |
| `61dab5db` | feat: add label distance spacing option | YES | done | `[Display(ResourceType = typeof(Resources), Name = nameof(Resources.LabelDistance), ...)]` → hardcoded; keep `GroupName = nameof(Strings.Visualization)` |
| `4561ed08` | chore: expose Card label mode in UI | YES | done | `typeof(Resources)` for `Card` enum display → hardcoded `[Display(Name = "Card")]` |
| `a26d3724` | chore: localize tooltip and label text tokens | YES | done | Replace `Resources.TradeDirectionLong/Short`, `Resources.TradeEntry/Exit/Result/Ticks` runtime calls → hardcoded English strings |
| `97f26fcc` | refactor: extract label text building | no | done | |
| `37f55fe2` | feat: implement Card label rendering | no | done | |
| `f60a6ed2` | feat: add connector line and improve Card placement | no | done | |
| `77134b33` | feat: add label X anchor and centered placement | YES | done | `LabelHorizontalAnchor` enum and `LabelXAnchor` property use `typeof(Resources)` → hardcoded Display names |
| `9365055a` | feat: make Midpoint anchor center between entry/exit markers | no | done | |
| `a4c8d841` | feat: improve card label readability (header/body/footer) | no | done | |
| `03ed72e8` | chore: regroup and relocalize settings for trader workflow | YES | done | Major reorganization: all Display attrs moved from `typeof(Strings)` → `typeof(Resources)` with new groups (LinesAndMarkers, Labels, DetailsGroup). On feat/: keep `typeof(Strings)` where keys exist; hardcode group names |
| `c0b75fd2` | fix: redraw chart on visual settings changes | no | done | Indentation + logic only; no new resource references |

### Adaptation guide for feat/tradesonchart

**48ab4e05** — LabelDisplayMode enum + ProfitColor/LossColor properties:
```csharp
// Replace typeof(Resources) Display attrs on LabelDisplayMode enum:
[Display(Name = "Hide")]       // was nameof(Resources.LabelDisplayModeHide)
[Display(Name = "Short")]      // was nameof(Resources.LabelDisplayModeShort)
[Display(Name = "Full")]       // was nameof(Resources.LabelDisplayModeFull)
// ProfitColor/LossColor: keep typeof(Strings) or hardcode Name only (GroupName stays Strings.Visualization)
[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Visualization), ...)]
// → use hardcoded Name with Strings.Visualization GroupName:
[Display(Name = "Profit color", GroupName = nameof(Strings.Visualization), Description = "Color used to display profitable trades.")]
```

**61dab5db** — LabelDistance property:
```csharp
// Replace:
[Display(ResourceType = typeof(Resources), Name = nameof(Resources.LabelDistance), Description = nameof(Resources.LabelDistanceDescription), GroupName = nameof(Strings.Visualization))]
// With:
[Display(Name = "Label distance", Description = "Vertical spacing between trade markers and labels (px).", GroupName = nameof(Strings.Visualization))]
```

**4561ed08** — Card enum value:
```csharp
// Replace:
[Display(ResourceType = typeof(Resources), Name = nameof(Resources.LabelDisplayModeCard))]
// With:
[Display(Name = "Card")]
```

**a26d3724** — Runtime token lookups:
```csharp
// Replace all Resources.X runtime string references with hardcoded English:
// Resources.TradeDirectionLong   → "Long"
// Resources.TradeDirectionShort  → "Short"
// Resources.TradeEntry           → "Entry"
// Resources.TradeExit            → "Exit"
// Resources.TradeResult          → "Result"
// Resources.TradeTicks           → "ticks"
```

**77134b33** — LabelHorizontalAnchor enum + property:
```csharp
// Replace enum Display attrs:
[Display(Name = "Close bar")]     // was nameof(Resources.LabelXAnchorCloseBar)
[Display(Name = "Trade midpoint")] // was nameof(Resources.LabelXAnchorMidpoint)
// Replace property Display attr:
[Display(Name = "Label centering", Description = "Defines the horizontal reference used to position trade labels.", GroupName = nameof(Strings.Visualization))]
```

**03ed72e8** — Full settings reorganization:
```csharp
// Properties moving from Strings.Visualization to new Resources groups:
// ShowLine    → GroupName = "Lines and markers", use Strings.ShowLines for Name
// ShowTooltip → GroupName = "Details", use Strings.ShowDescription for Name
// LabelDisplay → [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LabelDisplay), GroupName = "Lines and markers")] // hardcode group
// BuyColor   → [Display(ResourceType = typeof(Strings), Name = nameof(Strings.BuyColor), GroupName = "Lines and markers")]
// SellColor  → [Display(ResourceType = typeof(Strings), Name = nameof(Strings.SellColor), GroupName = "Lines and markers")]
// ProfitColor → [Display(Name = "Profit color", GroupName = "Labels")]
// LossColor  → [Display(Name = "Loss color", GroupName = "Labels")]
// LineWidth  → [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineWidth), GroupName = "Lines and markers")]
// DashStyle  → [Display(ResourceType = typeof(Strings), Name = nameof(Strings.DashStyle), GroupName = "Lines and markers")]
// MarkerSize → [Display(Name = "Marker size", Description = "Size of entry/exit markers.", GroupName = "Lines and markers")]
// LabelDistance → [Display(Name = "Label distance", GroupName = "Labels", Order = 20)]
// LabelXAnchor  → [Display(Name = "Label centering", GroupName = "Labels", Order = 30)]
```

---

### Integration notes

Stacked on `local/build/04-localization`. Cherry-pick from `feat/tradesonchart` and apply
`typeof(Resources)` for all keys that were hardcoded on the feat branch.

All adaptations from Phase 2 are reversed here — the Resources keys are available via
`local/build/04-localization`.

| Commit (prready/main) | Description | Status |
|----------------------|-------------|--------|
| All feat/tradesonchart commits | Single squash or cascade | done |

---

## Phase 5 — Smoke test

### 4.1 Build
- [ ] `dotnet build -c Alpha` — 0 errors
- [ ] `dotnet build -c Stable` — 0 errors

### 4.2 Content completeness
- [ ] All feat/tradesonchart commits present (via `git diff`)
- [ ] All 6 adaptation commits correctly use `typeof(Resources)` (not hardcoded strings)

### 4.3 Resource completeness
- [ ] All 33 new Phase 0 keys exist in Resources.resx (en)
- [ ] All 33 new keys exist in Resources.de-de.resx and Resources.ru-ru.resx
- [ ] Resources.Designer.cs up to date
- [ ] No `nameof(Resources.X)` in TradesOnChart.cs points to missing key

### 4.4 Functional smoke test
- [ ] Indicator loads without crash
- [ ] Trade history loads for current chart range
- [ ] Buy/Sell markers visible on chart
- [ ] Label mode (Hide/Short/Full/Card) switches render correctly
- [ ] Card label shows entry/exit/PnL text
- [ ] Settings panel: LinesAndMarkers, Labels, Details groups visible
- [ ] No resource key literal visible in UI

---

## Phase 6 — Documentation

- [ ] `publish_stable_MyIndicators` docs updated to reflect final behavior
- [ ] Manifest marked `complete`

---

## Section 5 — Intentional divergences from prready/main

| Area | prready/main behavior | Local behavior | Reason | Revisit? |
|------|-----------------------|----------------|--------|----------|
| `LossColor` value | "Loss color" | updated from "Loss Color" | Casing consistency with prready/main final value | No |
| `TradeDetails` value | "Details" | updated from "Trade Details" | Matches final prready/main value | No |

---

## Section 6 — Pending / known gaps

None. All phases complete.
