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