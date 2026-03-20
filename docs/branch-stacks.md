# Local build stack

```text
develop  
└─ local/build/01-base  
   └─ local/build/02-multiversion  
      └─ local/build/03-version-shims 
``` 

| Branch | Parent | Status | Notes |
|---|---|---|---|
| `local/build/01-base` | `develop` | PR open | Base local build alignment. Tracks `origin/local/build-base` temporarily because PR is already open |
| `local/build/02-multiversion` | `local/build/01-base` | local-only | Changes solution layout for multiple versions |
| `local/build/03-version-shims` | `local/build/02-multiversion` | local-only | Adds version compatibility shims |

## Patch branches

```text
develop
├─ fix/domstrength-bid-sum-source
└─ fix/domstrength-leveldepth-off-by-one
``` 

| Branch | Parent | Status | Notes |
|---|---|---|---|
| `fix/domstrength-bid-sum-source` | `develop` | PR-ready | Fixes incorrect bid aggregation when `_mDepthBid.Count <= LevelDepth.Value` by summing `_mDepthBid.Values` instead of `_mDepthAsk.Values` |
| `fix/domstrength-leveldepth-off-by-one` | `develop` | PR-ready | Changes depth iteration bounds from `<= LevelDepth.Value` to `< LevelDepth.Value` |