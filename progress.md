# progress.md — Minnal maui-fsharp-spike
# Live measurements. Updated at end of each session.
# Branch: maui-fsharp-spike | Date: 2026-05-02

---

## Memory — Cold Start, No Model, No Data

All measurements taken via BFS process-tree walk from the MAUI host PID.
WebView2 child processes attributed to the app only (parent-PID chain, not all msedgewebview2 on system).

### Debug build (`dotnet run`)

| Process | MB |
|---|---|
| Minnal.Maui.exe (host) | 161.9 |
| msedgewebview2 — renderer | 125.2 |
| msedgewebview2 — browser | 83.1 |
| msedgewebview2 — GPU | 62.0 |
| msedgewebview2 — GPU-info | 35.7 |
| msedgewebview2 — crash handler | 18.4 |
| msedgewebview2 — spare renderer | 9.2 |
| **TOTAL TREE** | **495.5 MB** |

Target: 350 MB cold. **Over by 145 MB (+42%).**

### Release + PublishTrimmed (`dotnet publish -c Release -p:PublishTrimmed=true`)

| Process | MB |
|---|---|
| Minnal.Maui.exe (host) | 129.8 |
| msedgewebview2 — renderer | 127.4 |
| msedgewebview2 — browser | 81.0 |
| msedgewebview2 — GPU | 53.0 |
| msedgewebview2 — GPU-info | 36.4 |
| msedgewebview2 — crash handler | 18.4 |
| msedgewebview2 — spare renderer | 9.3 |
| **TOTAL TREE** | **455.1 MB** |

Target: 350 MB cold. **Over by 105 MB (+30%).**

### Debug → Release delta

| Component | Debug | Release | Saved |
|---|---|---|---|
| Host process | 161.9 MB | 129.8 MB | **−32.1 MB** |
| WebView2 tree (5 procs) | 333.6 MB | 325.5 MB | **−8.1 MB** |
| Total | 495.5 MB | 455.1 MB | **−40.4 MB** |

Trim helps the host significantly (−32 MB). WebView2 is unaffected by trim — it is a native Chromium process.

### Observation: PublishTrimmed startup regression

The Release + trimmed binary exhibited a slow Blazor initialisation ("Loading..." persisted past 15s).
Probable cause: IL2040 warnings from FSharp.Core — the trimmer removes metadata that FSharp.Core's
reflection-based startup path expects. Fix before next measurement:

```xml
<!-- add to Minnal.Maui.csproj -->
<TrimmerRootDescriptor Include="TrimmerRoots.xml" />
```

Or suppress trimming on FSharp.Core until the issue is diagnosed:

```xml
<TrimmerRootAssembly Include="FSharp.Core" />
```

---

## Memory Reduction Roadmap

| Lever | Est. saving | Status |
|---|---|---|
| Release + PublishTrimmed | −32 MB host | Done — measure confirmed |
| `--in-process-gpu` WebView2 flag | −50 to −65 MB | **Next** |
| `--disable-features=Translate,AutofillAssistant,MediaRouter` | −10 to −20 MB | Queued |
| Fix FSharp.Core trim annotations | 0 MB (correctness, not size) | Queued |
| NativeAOT | −20 to −40 MB host | v1.1 |

**Projected floor with GPU consolidation:** ~390 MB.
**Projected floor with GPU + feature flags:** ~370–380 MB.
The 350 MB target requires NativeAOT or a structural change (e.g. WPF host instead of MAUI).

---

## UI — Feature State (Debug build, 2026-05-02)

Screenshot taken: `Desktop/minnal_app.png`

| Surface | Status | Notes |
|---|---|---|
| Top intent bar | Rendered | Static text; `<input>` not wired yet |
| Left rail | Rendered | 3 requests from SQLite via F# domain |
| Centre request workbench | Rendered | Method/URL/headers/body/response all present |
| Right Why drawer | Rendered | Evidence, hook review, memory policy |
| Bottom status strip | Rendered | Tree/Host/WebView2/Heap live; amber gate at 350 MB |
| Playground (zero-persist) | Not started | Phase 4 M7 |
| Semantic canvas | Not started | Phase 4 M3 |
| Real HTTP execution | Not started | Requires Minnal-core integration |
| AI (gemma) | Not started | Phase 2 |

---

## Icon

Lightning bolt (`#FF6B1A` on `#151b20`) applied to:
- `Resources/AppIcon/appicon.svg` (full icon + background)
- `Resources/AppIcon/appiconfg.svg` (foreground layer for adaptive icons)
- `Resources/Splash/splash.svg` (splash screen)
- `Minnal.Maui.csproj` — adaptive icon `Color` updated from `#512BD4` → `#151b20`

---

*Update this file after every measurement session.*
*Never Settle.*
