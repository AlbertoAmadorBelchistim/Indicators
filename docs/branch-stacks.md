# Local build stack

```text
develop
└─ local/build/01-base
   └─ local/build/02-multiversion
      └─ local/build/03-version-shims
         └─ local/build/04-localization
```

| Branch | Parent | Status | Notes |
|---|---|---|---|
| `local/build/01-base` | `develop` | PR open | Base local build alignment: `$(ATAS_BASE)`, `net8.0-windows` / `net8.0`, Cross platform exclusions |
| `local/build/02-multiversion` | `local/build/01-base` | local-only | Multi-flavor build configs: Alpha / Beta / Latest / Stable / ATAS_X_Alpha / ATAS_X_Beta |
| `local/build/03-version-shims` | `local/build/02-multiversion` | local-only | Version compatibility shims + TabAttribute stub for pre-release OFT.Attributes |
| `local/build/04-localization` | `local/build/03-version-shims` | local-only | New .resx keys for Volume + Delta features (67 keys total, all 6 locales) |

## Delta branches

```text
develop
├─ feat/delta-threshold-lines       (local)
│  └─ feat/delta-price-signals      (local)
│     └─ feat/delta-threshold-sel   (local)
│        └─ feat/delta-dyn-thresh   (local)
│           └─ feat/delta-audio     (local)
│              └─ feat/delta-avg    (local)
└─ local/build/04-localization
   └─ local/delta-i18n              (local)
```

| Branch | Parent | Status | Notes |
|---|---|---|---|
| `feat/delta-threshold-lines` | `develop` | local-only | Fixed threshold line series (up/down major/minor) |
| `feat/delta-price-signals` | `feat/delta-threshold-lines` | local-only | Price-panel triangle signals (visual alerts) |
| `feat/delta-threshold-sel` | `feat/delta-price-signals` | local-only | ThresholdLevel enum: per-side major/minor selection for visual alerts |
| `feat/delta-dyn-thresh` | `feat/delta-threshold-sel` | local-only | Session-anchored dynamic thresholds via Welford running statistics |
| `feat/delta-audio` | `feat/delta-dyn-thresh` | local-only | Audio alerts with cooldown, bar-close policy, and per-side threshold selection |
| `feat/delta-avg` | `feat/delta-audio` | local-only | Average delta line: SMA/EMA, three color modes (Fixed/ZeroCross/Slope) |
| `local/delta-i18n` | `local/build/04-localization` | local-only | Full i18n of Delta.cs: all new display strings via `typeof(Resources)` |

### Pending Delta branches (from prready/main, not yet extracted)

| Planned branch | Commit | Description |
|---|---|---|
| `fix/delta-marker-clamp` | `64ed7f58` | Clamp price-panel triangle markers to visible region |
| `chore/delta-ui-polish` | `3e420e5c`, `811000d6`, `5fe49e01`, `5f78e83f`, `5e336b28`, `eff7ee3f`, `0d087ebd` | UI group reorganization, Drawing group cleanup, label polish |

## Volume branches

```text
develop
├─ fix/volume-alert-label          (PR-ready)
│  └─ fix/volume-max-follow-input  (PR-ready)
│     └─ feature/volume-thresholds (local)
└─ local/build/04-localization
   └─ local/volume-i18n            (local)
```

| Branch | Parent | Status | Notes |
|---|---|---|---|
| `fix/volume-alert-label` | `develop` | PR-ready | Alert messages now reflect selected input type via `GetInputLabel()` |
| `fix/volume-max-follow-input` | `fix/volume-alert-label` | PR-ready | MaxFollowInput: highest-vol lookback now uses `val` (current input) not `candle.Volume` |
| `feature/volume-thresholds` | `fix/volume-max-follow-input` | local-only | Fixed and dynamic (Welford) volume threshold lines with session-aware reset |
| `local/volume-i18n` | `local/build/04-localization` | local-only | Full i18n of Volume.cs: all display strings via Resources, bugs fixed (double-paren, color setters) |

## Patch branches

```text
develop
├─ fix/domstrength-bid-sum-source
├─ fix/domstrength-leveldepth-off-by-one
└─ fix/dompower-leveldepth-state-reset
```

| Branch | Parent | Status | Notes |
|---|---|---|---|
| `fix/domstrength-bid-sum-source` | `develop` | PR-ready | Copy-paste bug: bid aggregation was reading `_mDepthAsk.Values` instead of `_mDepthBid.Values` |
| `fix/domstrength-leveldepth-off-by-one` | `develop` | PR-ready | Depth iteration was `<= LevelDepth.Value` (one extra level); fixed to `< LevelDepth.Value` |
| `fix/dompower-leveldepth-state-reset` | `develop` | PR-ready | LevelDepth setter now unsubscribes stale events and fully resets series + internal state |
