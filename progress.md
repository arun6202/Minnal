# progress.md — Minnal maui-fsharp-spike
# Live measurements. Updated at end of each session.
# Branch: maui-fsharp-spike | Date: 2026-05-02

---

## Session 3 — 2026-05-02 — WebView2 memory flags + trim investigation

### What changed

| Item | File(s) | Status |
|---|---|---|
| WebView2 browser flags | MauiProgram.cs | Done |
| `--in-process-gpu` | MauiProgram.cs | Done |
| Disable Translate / AutofillAssistant / MediaRouter | MauiProgram.cs | Done |
| FSharp.Core trim root | Minnal.Maui.csproj | Added, insufficient |
| Explicit linker roots for List<T> / WinRT startup | TrimmerRoots.xml | Added, insufficient |
| Debug build | dotnet build | Passed |
| Release no-trim publish | dotnet publish | Passed |
| Release trimmed publish | dotnet publish | Publishes, crashes at startup |

### WebView2 flags applied

```csharp
WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS =
  --in-process-gpu --disable-features=Translate,AutofillAssistant,MediaRouter
```

The flag is set before MAUI creates the `BlazorWebView`, scoped to Windows only.

### Memory — Debug, cold, no model loaded, WebView2 flags enabled

Measurement taken from the app host plus newly spawned `msedgewebview2` processes after 18 seconds.

| Process | MB |
|---|---:|
| Minnal.Maui.exe (host) | 159.2 |
| msedgewebview2 — renderer/browser | 154.5 |
| msedgewebview2 — GPU/browser support | 63.5 |
| msedgewebview2 — GPU-info | 33.9 |
| msedgewebview2 — crash handler | 18.3 |
| msedgewebview2 — spare renderer | 9.3 |
| **TOTAL HOST + NEW WEBVIEW2** | **438.7 MB** |

Compared to Session 2 debug baseline (487.4 MB): **−48.7 MB**.

### Memory — Release, no trim, no model loaded, WebView2 flags enabled

| Process | MB |
|---|---:|
| Minnal.Maui.exe (host) | 155.1 |
| msedgewebview2 — renderer/browser | 160.3 |
| msedgewebview2 — GPU/browser support | 64.0 |
| msedgewebview2 — GPU-info | 35.9 |
| msedgewebview2 — crash handler | 18.3 |
| msedgewebview2 — spare renderer | 9.3 |
| **TOTAL HOST + NEW WEBVIEW2** | **443.0 MB** |

Release without trimming is stable, but it is not materially lower than Debug after WebView2 flags.

### Trim blocker

`dotnet publish -c Release -p:PublishTrimmed=true` completes, but the published executable crashes at startup:

```text
System.TypeInitializationException: WinRT.TypeNameSupport
System.TypeLoadException: Could not load type
'System.Collections.Generic.List`1' from assembly 'System.Collections'
```

Tried:

- `TrimmerRootAssembly Include="FSharp.Core"`
- `TrimmerRootAssembly Include="System.Collections"`
- `TrimmerRootAssembly Include="System.Collections.Concurrent"`
- `TrimmerRootAssembly Include="System.Collections.NonGeneric"`
- `TrimmerRootAssembly Include="WinRT.Runtime"`
- explicit `TrimmerRoots.xml` for `List<T>`, `WinRT.TypeNameSupport`, and `WinRT.ProjectionInitializer`
- `TrimMode=partial`

Result: still crashes with the same WinRT projection startup error.

Current position: **do not use trimmed Release for Windows MAUI yet**. Use Debug or normal Release for the spike. Treat MAUI/WinRT trimming as a separate packaging investigation.

---

## Session 2 — 2026-05-02 — AI wiring + HTTP execution

### What we built

| Item | File(s) | Status |
|---|---|---|
| LLamaSharp 0.27.0 + Backend.Cpu | Minnal.Maui.csproj | Done |
| AiService (graceful no-model blocked state) | Services/AiService.cs | Done |
| HttpExecutionService (real HttpClient) | Services/HttpExecutionService.cs | Done |
| DI registration of both services | MauiProgram.cs | Done |
| Home.razor — interactive buttons wired | Components/Pages/Home.razor | Done |
| Active request → GET https://api.github.com/zen | Library.fs + SQLite seed | Done |
| Build: 0 errors, 0 warnings | Debug build | Done |
| Debug tree (cold, no model) | Process tree | 487.4 MB |

### Home.razor interactivity added

| Button | Action |
|---|---|
| Send | HttpExecutionService.SendAsync → live response panel |
| Explain | LlamaAiService.ExplainAsync → Why drawer |
| Run | Send + Explain chained |

### AI state machine

```
Off → Loading → Ready   (GGUF found at %LOCALAPPDATA%\Minnal\models\)
Off → Loading → Blocked (no GGUF found)
```

Status text and state shown in left-rail Model panel and status bar.
AI loads in background on first render (OnAfterRender, fire-and-forget Task.Run).

### Memory — Debug, cold, no model loaded

| Process | MB |
|---|---|
| Minnal.Maui.exe (host) | 159.2 |
| msedgewebview2 — renderer | 120.3 |
| msedgewebview2 — browser | 83.1 |
| msedgewebview2 — GPU | 63.5 |
| msedgewebview2 — GPU-info | 33.7 |
| msedgewebview2 — crash handler | 18.4 |
| msedgewebview2 — spare renderer | 9.2 |
| **TOTAL TREE** | **487.4 MB** |

Compared to Session 1 debug baseline (495.5 MB): **−8.1 MB**.
LLamaSharp DLLs are loaded but idle (no model); overhead negligible.

---

## Session 1 — 2026-05-02 — Baseline measurements + icon

### Memory — Cold Start, No Model, No Data

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
<TrimmerRootDescriptor Include="TrimmerRoots.xml" />
```

Or suppress trimming on FSharp.Core:

```xml
<TrimmerRootAssembly Include="FSharp.Core" />
```

### Icon

Lightning bolt (`#FF6B1A` on `#151b20`) applied to:
- `Resources/AppIcon/appicon.svg` (full icon + background)
- `Resources/AppIcon/appiconfg.svg` (foreground layer for adaptive icons)
- `Resources/Splash/splash.svg` (splash screen)
- `Minnal.Maui.csproj` — adaptive icon `Color` updated from `#512BD4` → `#151b20`

---

## Memory Reduction Roadmap

| Lever | Est. saving | Status |
|---|---|---|
| Release + PublishTrimmed | −32 MB host | Done — measure confirmed |
| `--in-process-gpu` WebView2 flag | −48.7 MB measured debug | Done |
| `--disable-features=Translate,AutofillAssistant,MediaRouter` | included above | Done |
| Fix FSharp.Core trim annotations | 0 MB (correctness, not size) | Blocked — WinRT trim crash remains |
| NativeAOT | −20 to −40 MB host | v1.1 |

**Measured floor with WebView2 flags:** ~439 MB debug, ~443 MB normal Release.
The 350 MB target requires NativeAOT or a structural change (e.g. WPF host instead of MAUI).

---

## UI — Feature State (Debug build, 2026-05-02, Session 2)

| Surface | Status | Notes |
|---|---|---|
| Top intent bar | Rendered | Static text; intent input not wired yet |
| Left rail | Rendered | 3 requests from SQLite; AI state shown live |
| Centre request workbench | Rendered | Send wired → real HTTP; live response shown |
| Right Why drawer | Rendered | Shows AI explanation after Explain click |
| Bottom status strip | Rendered | Tree/Host/WebView2/Heap live; AI state live |
| AI loading (LLamaSharp) | Wired, Blocked | No GGUF at %LOCALAPPDATA%\Minnal\models\ |
| Real HTTP execution | Wired | GET api.github.com/zen fires from Send |
| Playground (zero-persist) | Not started | Phase 4 M7 |
| Semantic canvas | Not started | Phase 4 M3 |
| AI explain (gemma) | Wired, blocked | Needs GGUF model to activate |

---

## AI Blocker — No GGUF Model

Architecture law: model weights must NOT be bundled in repo or installer.

To unblock AI:
1. Download a GGUF model (e.g. gemma-3-1b-it-q4_k_m.gguf or phi-3-mini-4k-instruct-q4.gguf)
2. Place at: `%LOCALAPPDATA%\Minnal\models\<any-name>.gguf`
3. Restart Minnal — AI state will transition Off → Loading → Ready

Recommended for spike (fits in 4 GB RAM): **gemma-3-1b-it-q4_k_m.gguf** (~800 MB)

---

## Way Forward

### Immediate (next session)

1. **Place GGUF model** — unlock Explain button end-to-end
   - Download: `winget install -e --id HuggingFace.HuggingFaceCLI` then `huggingface-cli download google/gemma-3-1b-it-gguf`
   - Or direct download from HuggingFace to `%LOCALAPPDATA%\Minnal\models\`

2. **Test Send + Explain live** — GET api.github.com/zen → AI explains the zen quote
   - Measure process tree with model loaded (warm baseline)
   - Record in progress.md: warm tree MB

3. **Investigate WebView2 process model further**
   - `--in-process-gpu` helped but did not remove all GPU/browser support cost
   - Next measurement should compare UI correctness and process tree with/without each flag

4. **Trimmed Release investigation**
   - Current trimmed Release crashes in WinRT projection startup
   - Keep normal Release as the runnable optimized build until this is isolated

### Medium term

5. **Intent bar wired** — input `<input>` fires AI search over request list
6. **Request selection** — clicking left-rail items loads that request into workbench
7. **Postman collection import** — Phase 1 M2 (import *.json, seed SQLite)
8. **Auth token storage** — Windows DPAPI / Credential Manager for Bearer tokens

### v1 gap check

| v1 invariant | Status |
|---|---|
| Import Postman / Swagger | Not started |
| Execute any REST request | Partially done (GET, no auth) |
| gemma-4-e4b explains response | Wired, blocked on GGUF |
| WASM auth pre-hook | Not started |
| Search 300 requests by intent | Not started |
| All offline | Architecture ready |
| No Electron, no cloud call | Confirmed |

---

*Update this file after every measurement session.*
*Never Settle.*
