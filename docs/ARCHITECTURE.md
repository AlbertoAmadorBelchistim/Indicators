# Architecture - ATAS Indicators Repository

This document defines the **code architecture principles and recurring design patterns**
used across this repository for developing and modifying ATAS indicators.

It is intentionally based on **real, existing indicators**
(e.g. `ClusterStatistic`, `Volume`, `OHLCPlus`)
and not on abstract or idealized designs.

This document covers:

- indicator code architecture
- separation of responsibilities
- state, rendering and calculation patterns
- compatibility and adaptation rules
- localization as a presentation concern

This document does **not** define:

- branch strategy
- patch promotion workflow
- PR preparation rules
- repository integration flows

Those concerns belong in repository engineering documentation such as:

- `docs/branch-stacks.md`
- `docs/patch-registry.md`
- `CONTRIBUTING.md`

---

## 1. Architectural goals

All indicators in this repository should aim to satisfy:

- Clear separation between **calculation**, **state**, and **rendering**
- Deterministic behavior across:
  - historical bars
  - realtime bars
  - session boundaries
- Minimal implicit coupling between components
- Visual output that accurately reflects the calculated data
- Compatibility with upstream ATAS evolution
- Local compatibility adaptations that remain isolated and auditable

---

## 2. Separation of responsibilities

### 2.1 Calculation layer

Responsibilities:
- Compute values from market data
- Handle historical vs realtime logic explicitly
- Avoid direct interaction with rendering or UI state

Rules:
- No drawing logic in calculation paths
- No dependency on screen coordinates or visual properties
- Guard all divisions and derived metrics against zero or missing data

Observed patterns:
- `ClusterStatistic`: heavy calculation with multiple derived series
- `Volume`: minimal calculation, but strict dependency on bar lifecycle
- `OHLCPlus`: calculation tightly coupled to bar indexing and session logic

---

### 2.2 State management

Indicators must manage **explicit state**, not implicit assumptions.

Key distinctions:
- Historical vs realtime bars
- Session resets vs continuous accumulation
- Per-bar cached values vs recomputed values

Rules:
- State resets must be explicit and intentional
- Avoid relying on implicit ATAS behavior unless documented
- Series must be indexed consistently and predictably

Anti-patterns to avoid:
- Mixing session reset logic into rendering
- Hidden state changes triggered by visual flags
- Relying on `CurrentBar` side effects without guarding logic

---

### 2.3 Rendering layer

Responsibilities:
- Visualize already-calculated data
- Reflect sign, magnitude and hierarchy correctly
- Remain stable across zoom, scale and refresh events

Rules:
- Rendering must never change calculation results
- Rendering must tolerate missing or zero values
- Color, thickness and style must be derived from **meaning**, not convenience

Observed patterns:
- `ClusterStatistic`: color logic tied to semantic meaning
- `Volume`: minimal rendering but strict alignment with bars
- `OHLCPlus`: rendering directly mirrors OHLC semantics

---

## 3. Historical vs realtime behavior

Indicators must behave correctly in all phases:

- full historical load
- realtime bar updates
- session transitions

Rules:
- Realtime updates must not invalidate historical values
- Historical calculations must not depend on realtime-only data
- Any difference in behavior must be **explicitly documented**

Common risk:
- Indicators that look correct historically but drift or repaint in realtime

---

## 4. UI, parameters and resources

UI elements must be treated as **interfaces**, not logic drivers.

Rules:
- Parameters affect calculation configuration, not flow control
- UI changes must not alter indicator semantics unless explicitly intended
- Localization and resources (`Display`, `.resx`) must be isolated from logic

Commit discipline:
- UI/resource changes must not be mixed with calculation changes
- UI divergence from upstream must be intentional and documented

---

## 5. Compatibility and build-flavor adaptations

Compatibility adaptations must be treated as **isolated technical layers**, not as silent changes to indicator semantics.

Examples include:
- version shims
- API guards
- build-flavor conditionals
- stable/beta/alpha compatibility workarounds

Rules:
- Compatibility code must preserve functional intent whenever possible
- Version-specific behavior must be explicit and easy to locate
- Shims should isolate platform or API differences, not hide architectural problems
- Compatibility work must not leak unnecessarily into core calculation paths

Preferred pattern:
- keep core indicator logic stable
- isolate version or flavor differences behind narrow compatibility points
- document non-trivial behavior changes when exact parity is impossible

Anti-patterns to avoid:
- scattering flavor-specific conditionals throughout unrelated logic
- mixing compatibility code with unrelated refactors
- using shims to justify unclear semantics or broken abstractions

### 5.1 ATAS X compatibility

ATAS X is the cross-platform version (Windows + macOS). Key differences from classic ATAS:

**Auto-mapped — no code changes needed:**
- `System.Windows.Media.Color`, `SolidColorBrush`, and similar types from `System.Windows.Media` are automatically mapped to cross-platform equivalents at runtime

**Incompatible — requires attention:**
- Custom property editors built with WPF `UserControl` (files in `Editors/`) **will not work on ATAS X**
- Indicators that reference these editors will fail at runtime on the cross-platform version
- Before publishing for ATAS X users, audit `Editors/` and either replace with native ATAS UI controls or mark as Windows-only

Rule: Do not introduce new WPF-dependent custom editors. Use native ATAS attribute-based UI (`NumericEditor`, `ComboBoxEditor`, `[Display]`, etc.) wherever possible.

---

## 6. Modified indicators (`*Modif`)

Modified indicators are **derived variants**, not uncontrolled forks.

Rules:
- The original indicator logic should remain the conceptual reference unless an intentional divergence is documented
- Variants must preserve traceability between:
  - original implementation
  - renamed/derived artifact
  - local behavior changes
- Namespace, class name and display metadata may change as part of derivation
- Logic changes should be made in a controlled and auditable way before or during derivation, depending on the workflow in use

Guidelines:
- Avoid ad hoc manual divergence with no traceable rationale
- Keep variant-specific behavior explicit
- Do not treat `*Modif` files as a dumping ground for unrelated local experiments

The goal is controlled derivation, not permanent fork drift.

---

## 7. Acceptable patterns vs anti-patterns

### Acceptable
- Explicit state transitions
- Redundant guards for safety
- Clear naming of derived metrics
- Slight duplication to preserve clarity
- Narrow compatibility layers when required by ATAS version differences

### Anti-patterns
- Hidden coupling between render and calculation
- Implicit session assumptions
- Refactors that obscure trading meaning
- Visual tweaks that distort interpretation
- Compatibility code scattered across unrelated logic

---

## 8. Localization and resources

Localization in this repository follows ATAS conventions and is treated as a
presentation concern, not a logic driver.

### Core rules

- Indicator logic must never depend on language or culture
- All functional semantics must be invariant across locales
- The neutral resource value (`Resources.resx`) is mandatory and acts as the authoritative fallback

### Resource coverage

- Additional languages are provided on a best-effort basis, but:
  - any **new** key introduced in `Resources.resx` **must** be added to all satellite `.resx` files shipped by this repository
- Missing translations must not affect:
  - calculation
  - rendering
  - indicator behavior
- Resource completeness for existing keys is not required for architectural correctness, but PRs introducing new UI strings must not ship partial coverage

### Naming, reuse, and structure

- Prefer reusing existing keys when the meaning matches exactly
- Create new keys only when reuse would change meaning, cause ambiguity, or degrade UX clarity
- Resource keys must be stable, descriptive, and follow the existing naming style (PascalCase)
- Use `...Description` suffix for tooltip/description strings
- Do **not** include indicator names in resource key names when keys are intended to be reusable
- Technical identifiers (class names, enums, internal fields) must not be localized
- UI-facing strings must be isolated in resource files

### Comments and traceability

- Every `<data>` entry should include a `<comment>` node
- Prefer comments that identify the indicator or UI context where the key is used

### UX constraints

- Resource values should be short, scannable, and interpretable quickly under pressure
- Avoid long phrases in names shown in settings panels; prefer compact labels with unambiguous meaning

### Change isolation

Localization changes must not be mixed with:
- calculation logic
- rendering refactors
- architectural changes

If a feature requires both localization and logic:
- do localization first
- then implement the feature in separate commits

---

## 9. Recurring implementation patterns

Patterns that appear in multiple indicators and should be followed consistently.

### 9.1 Color fields: storage vs property vs per-bar assignment

ATAS uses two color types with different roles:
- `System.Drawing.Color` — internal storage (fields)
- `CrossColor` (`System.Windows.Media.Color`) — property type exposed to UI, and the type returned by `_series.Color`

Correct pattern (from `AO.cs`, `Delta.cs`, etc.):

```csharp
// Field: store as Drawing.Color
private Color _upColor = Color.Green;

// Property: expose as CrossColor, convert on get/set
public CrossColor UpColor
{
    get => _upColor.Convert();
    set { _upColor = value.Convert(); RecalculateValues(); }
}

// Per-bar assignment: Colors[bar] accepts Drawing.Color
_series.Colors[bar] = _upColor;                     // field → direct
_series.Colors[bar] = _series.Color.Convert();      // CrossColor → Drawing.Color
```

Never assign `CrossColor` directly to `Colors[bar]` — it compiles but produces a type mismatch at runtime.

### 9.2 Session-aware Welford running statistics

For dynamic thresholds that reset per session and accumulate independently per direction:

```csharp
// State
private int _n;
private double _mean, _m2;

// Update (call once per in-session bar)
private static void WelfordPush(ref int n, ref double mean, ref double m2, double x)
{
    n++;
    var delta = x - mean;
    mean += delta / n;
    m2 += delta * (x - mean);
}

private static double WelfordStd(int n, double m2)
    => n > 1 ? Math.Sqrt(m2 / (n - 1)) : 0.0;
```

Rules:
- Reset accumulators at `bar == 0` and at detected session starts
- Use `_samplesForMeanStd` as a readiness guard before applying dynamic values
- Keep separate accumulators per sign direction (positive extremes vs negative magnitude extremes)
- Cache the computed threshold for the current bar to avoid look-ahead

### 9.3 Enum display attributes: `typeof(Strings)` vs `typeof(Resources)`

- Use `typeof(Strings)` when the enum value name matches an existing upstream key (e.g., `SMA`, `EMA`, `Period`)
- Use `typeof(Resources)` for new keys that exist only in the local resource files
- Never mix the two `typeof(...)` within a single enum — prefer a consistent source per enum

```csharp
// Upstream key exists → typeof(Strings)
[Display(ResourceType = typeof(Strings), Name = nameof(Strings.SMA))]
Sma = 0,

// New local key → typeof(Resources)
[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ZeroCross))]
ZeroCross = 1,
```

### 9.4 Build-flavor guards

Prefer the minimal and future-proof form:

```csharp
// Prefer (works for any future flavor that is not Stable)
#if !ATAS_STABLE
_candleSeries.DrawCandleBorder = true;
#endif

// Avoid (must be updated each time a new flavor is added)
#if ATAS_ALPHA || ATAS_BETA || ATAS_LATEST || ATAS_X_ALPHA || ATAS_X_BETA
_candleSeries.DrawCandleBorder = true;
#endif

// Avoid (empty if-body is confusing)
#if ATAS_STABLE
#else
_candleSeries.DrawCandleBorder = true;
#endif
```

---

### 9.5 DataSeries types and configuration properties

ATAS provides several DataSeries types. Choose based on visual intent:

| Type | Use case |
|------|----------|
| `ValueDataSeries` | Scalar values — lines, histograms, dots |
| `RangeDataSeries` | Channel/band between two boundary values |
| `CandleDataSeries` | OHLC candle rendering |
| `PaintbarsDataSeries` | Per-bar color overlay (typically `IsHidden = true`) |
| `ObjectDataSeries` | Generic typed object storage, not rendered directly |

**Key `ValueDataSeries` properties:**

```csharp
_series.VisualType = VisualMode.Histogram;   // Line | Histogram | Dots | Hide | OnlyValueOnAxis | ...
_series.IsHidden = true;                      // Hide from data series list in UI
_series.ShowCurrentValue = false;             // Show/hide current value on price axis
_series.ShowZeroValue = false;                // Whether zero is rendered or treated as empty
_series.IgnoredByAlerts = true;               // Exclude from alert evaluation
_series.ResetAlertsOnNewBar = true;           // Reset alert state on each new bar
_series.UseMinimizedModeIfEnabled = true;     // Collapse in minimized panel mode
_series.ScaleIt = false;                      // Whether this series participates in auto-scaling
```

**`RangeDataSeries`** — sync colors via `PropertyChanged`:

```csharp
_upperBand.PropertyChanged += (s, e) => {
    if (e.PropertyName == nameof(_upperBand.Color))
        _lowerBand.Color = _upperBand.Color;
};
```

---

### 9.6 Indicator lifecycle methods

Override in this order of responsibility:

```csharp
// Called once during construction — set panel, series, static config
public MyIndicator() : base(true)
{
    Panel = IndicatorDataProvider.NewPanel;
    DenyToChangePanel = true;
    EnableCustomDrawing = true;
    SubscribeToDrawingEvents(DrawingLayouts.Final | DrawingLayouts.LatestBar);
    DataSeries[0] = _mainSeries;
}

// Called when indicator is added to chart — subscribe to platform events
protected override void OnInitialize()
{
    _provider = TradingStatisticsProvider;
    _provider.TradesUpdated += OnTradesUpdated;
    TradingManager.PortfolioSelected += OnPortfolioSelected;
}

// Called on dispose — always unsubscribe
protected override void OnDispose()
{
    if (_provider != null) _provider.TradesUpdated -= OnTradesUpdated;
    TradingManager.PortfolioSelected -= OnPortfolioSelected;
}

// Called before first calculation — initialize color fields from defaults
protected override void OnApplyDefaultColors()
{
    _upColor = DefaultColors.Green.Convert();   // Drawing.Color from CrossColor default
}

// Called when RecalculateValues() is triggered — clear series, reset state
protected override void OnRecalculate()
{
    // Series cleared automatically by base; reset accumulators here if needed
}
```

Rules:
- Platform event subscriptions belong in `OnInitialize`, never in the constructor
- Every subscription in `OnInitialize` must have a matching unsubscription in `OnDispose`
- `OnApplyDefaultColors` is for setting `Drawing.Color` field defaults, not CrossColor properties
- `DenyToChangePanel = true` prevents users from dragging the indicator to a different panel

---

### 9.7 Custom rendering (OnRender)

```csharp
// Constructor
EnableCustomDrawing = true;
SubscribeToDrawingEvents(DrawingLayouts.Final);
DataSeries[0].VisualType = VisualMode.Hide;  // hide series when only custom rendering is used

// Fields — allocate once, not per render call
private readonly RenderFont _font = new("Arial", 10);
private readonly RenderStringFormat _format = new() { Alignment = StringAlignment.Center };

protected override void OnRender(RenderContext context, DrawingLayouts layout)
{
    // Only render bars that are on screen
    for (var bar = FirstVisibleBarNumber; bar <= LastVisibleBarNumber; bar++)
    {
        var x = ChartInfo.GetXByBar(bar);
        var y = ChartInfo.GetYByPrice((double)_series[bar]);
        context.DrawString("label", _font, Color.White, x, y, _format);
        context.DrawLine(pen, x1, y1, x2, y2);
    }
}
```

Rules:
- `DrawingLayouts.Final` — renders after all series are drawn; use for overlays
- `DrawingLayouts.LatestBar` — renders only on the last bar; efficient for real-time annotations
- `DrawingLayouts.Historical` — renders during backtesting passes
- Store `RenderFont`, `Pen`, `RenderStringFormat` as class fields — never allocate per render call
- Always guard with `FirstVisibleBarNumber` / `LastVisibleBarNumber` — never iterate all bars in OnRender
- Use `ChartInfo.GetXByBar(bar)` and `ChartInfo.GetYByPrice(price)` for coordinate conversion
- Use `ChartInfo.ColorsStore` for platform-consistent color theming when appropriate
- Rendering must never write to DataSeries or change calculation state

---

### 9.8 Parameter and property attribute pattern

```csharp
[Parameter]
[Range(1, 10000)]
[Display(
    ResourceType = typeof(Resources),
    Name = nameof(Resources.Period),
    GroupName = nameof(Resources.Settings),
    Description = nameof(Resources.PeriodDescription),
    Order = 100)]
public int Period
{
    get => _period;
    set { _period = value; RecalculateValues(); }
}
```

**Order convention:** use multiples of 10 within a group; space groups by 100 (Settings: 100–190, Visualization: 200–290, Alerts: 300–390).

**Property change triggers:**
- Call `RecalculateValues()` when the change requires a full recalculation pass
- Call `RaisePropertyChanged(nameof(Prop))` only when one property change should update another UI property without triggering recalc

**`Filter` types** — for parameters with an enable/disable toggle:

```csharp
private FilterInt _levelFilter = new() { Value = 5, Enabled = true };

[Display(...)]
public FilterInt LevelFilter
{
    get => _levelFilter;
    set { _levelFilter = value; RecalculateValues(); }
}
// In OnCalculate: if (_levelFilter.Enabled) { use _levelFilter.Value }
```

Use `Filter<T>` for decimal, `FilterInt` for int, `FilterString` for string. All implement `INotifyPropertyChanged` — assign the whole object in the setter so the UI updates correctly.

---

### 9.9 Session and bar lifecycle

```csharp
protected override void OnCalculate(int bar, decimal value)
{
    // ── bar == 0: full reset ──────────────────────────────────────────────
    if (bar == 0)
    {
        DataSeries.ForEach(x => x.Clear());
        _sessionHigh = decimal.MinValue;
        _n = 0; _mean = 0; _m2 = 0;   // Welford accumulators
        return;
    }

    // ── session boundary ─────────────────────────────────────────────────
    if (IsNewSession(bar))
    {
        _sessionHigh = decimal.MinValue;
        _n = 0; _mean = 0; _m2 = 0;
    }

    // ── main calculation ─────────────────────────────────────────────────
    var candle = GetCandle(bar);
    // ...

    // ── realtime-only logic (alerts, UI notifications) ───────────────────
    if (bar == CurrentBar - 1 && bar != _lastAlertBar)
    {
        _lastAlertBar = bar;
        AddAlert(_alertFile, InstrumentInfo.Instrument, "message", Color.White, Color.Black);
    }
}
```

Rules:
- `bar == 0` resets everything — series, accumulators, session state
- `bar == CurrentBar - 1` identifies the live bar; use `_lastBar` / `_lastAlertBar` guards to prevent duplicate alerts on re-ticks
- `IsNewSession(bar)` detects session boundaries; also available: `IsNewWeek(bar)`, `IsNewMonth(bar)`
- `SetPointOfEndLine(bar)` / `RemovePointOfEndLine(bar)` — shift displaced/lagging lines to correct end position

---

### 9.10 Alerts and trading statistics

**Alerts:**

```csharp
// Trigger at bar == CurrentBar - 1 with dedup guard
if (bar == CurrentBar - 1 && !_alreadyAlerted)
{
    _alreadyAlerted = true;
    AddAlert("alert.wav", InstrumentInfo.Instrument, $"Signal: {_value:F2}", Color.White, Color.DarkGreen);
}
// Reset guard on new bar
if (bar != _lastBar) { _lastBar = bar; _alreadyAlerted = false; }
```

Series alert flags:
- `_series.IgnoredByAlerts = true` — exclude auxiliary/visual series from alert evaluation
- `_series.ResetAlertsOnNewBar = true` — reset alert state automatically on each new bar

**Trading statistics** (read trade history for display):

```csharp
protected override void OnInitialize()
{
    _stats = TradingStatisticsProvider;
    _stats.TradesUpdated += OnTradesUpdated;
    TradingManager.PortfolioSelected += OnPortfolioSelected;
}

protected override void OnDispose()
{
    _stats.TradesUpdated -= OnTradesUpdated;
    TradingManager.PortfolioSelected -= OnPortfolioSelected;
}

private void OnTradesUpdated(object sender, EventArgs e) => RedrawChart();
```

Use `InstrumentInfo.TickSize` for price-relative thresholds (e.g., alert when move > N ticks).

---

### 9.11 Real-time event hooks (ExtendedIndicator)

Indicators that react to live market events override these methods from `ExtendedIndicator`.
**All subscriptions from `OnInitialize` must be unsubscribed in `OnDispose`.**
Any UI update from these callbacks must go through `DoActionInGuiThread(() => { ... })`.

**Trade and tape events:**
```csharp
protected override void OnNewTrade(MarketDataArg trade) { }         // single tick
protected override void OnNewTrades(MarketDataArg[] trades) { }     // batch of ticks
protected override void OnCumulativeTrade(CumulativeTrade trade) { }
protected override void OnUpdateCumulativeTrade(CumulativeTrade trade) { }
```

**Order book events:**
```csharp
protected override void OnBestBidAskChanged(MarketDataArg arg) { }
protected override void MarketDepthChanged(MarketDepth depth) { }
protected override void MarketDepthsChanged(MarketDepth[] depths) { }
```

**Order and position events** (use when indicator also manages orders):
```csharp
protected override void OnNewOrder(Order order) { }
protected override void OnOrderChanged(Order order) { }
protected override void OnNewMyTrade(MyTrade trade) { }
protected override void OnOrderRegisterFailed(Order order) { }
protected override void OnPortfolioChanged(Portfolio portfolio) { }
protected override void OnPositionChanged(Position position) { }
```

**Timer-based updates:**
```csharp
// In constructor or OnInitialize:
SubscribeToTimer(TimeSpan.FromSeconds(1));

protected override void OnTimer() { DoActionInGuiThread(() => RedrawChart()); }

protected override void OnDispose() { UnsubscribeFromTimer(); }
```

**GUI thread safety:** event callbacks execute on a background thread. Any DataSeries write
or chart state mutation must be wrapped in `DoActionInGuiThread(() => { ... })`.

**Snapshot access:**
```csharp
var snapshot = GetMarketDepthSnapshot();    // current order book
var bids = CumulativeDomBids;               // aggregated DOM bids
var asks = CumulativeDomAsks;               // aggregated DOM asks
```

---

### 9.12 Additional Indicator properties and utilities

```csharp
// Chart state
int first = FirstVisibleBarNumber;
int last  = LastVisibleBarNumber;
int count = VisibleBarsCount;

// Text annotations (render-independent, persisted on chart)
AddText("id", bar, price, "label", Color.White, Color.Transparent, Color.White,
        new RenderFont("Arial", 9), StringAlignment.Center, false, false);

// Platform color theming
var palette = ChartInfo.ColorsStore;   // use for platform-consistent colors

// DOM snapshot
var snapshot = GetMarketDepthSnapshot();
```

`VisibleBarsCount`, `FirstVisibleBarNumber`, and `LastVisibleBarNumber` are the same
properties used in `OnRender` guards — always prefer them over iterating `CurrentBar`.

---

## 11. Architecture as a living document

This document is:
- authoritative
- versioned
- intentionally conservative

It should be updated when:
- a new recurring pattern emerges
- an existing rule is proven insufficient
- a compatibility pattern becomes common enough to deserve explicit guidance

Architecture changes must be justified by **real code**, not preference.

---

## 12. Architecture Decision Records (ADR)

Non-trivial technical or UX decisions should be documented using an ADR,
especially when they affect:

- multiple indicators
- compatibility strategy
- rendering semantics
- long-term repository conventions

See:

`docs/decisions/ADR-TEMPLATE.md`

