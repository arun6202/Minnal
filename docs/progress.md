# progress.md — Minnal maui-fsharp-spike
# Live measurements. Updated at end of each session.
# Branch: maui-fsharp-spike | Date: 2026-05-02

---

## Session 5 - 2026-05-02 - Gemma 4 E4B GGUF + AI harness

### What changed

| Item | Status |
|---|---|
| Removed older Gemma 3 download path | Done |
| Downloaded Gemma 4 E4B IT Q4_K_M GGUF | Done |
| Bundled model under app raw assets | Done |
| Preferred non-`mmproj` GGUF when scanning model roots | Done |
| Added test-only `AiModelRoot` smart constructor | Done |
| Added real AI harness test | Done |
| Ignored Rust tests for this pass | Done |

### Model

```text
Repo: ggml-org/gemma-4-E4B-it-GGUF
File: gemma-4-E4B-it-Q4_K_M.gguf
Base model: google/gemma-4-E4B-it
Local path: apps/Minnal.Maui/Resources/Raw/ai/gemma-4-E4B-it-Q4_K_M.gguf
Local size: 4.97 GB
```

The GGUF is intentionally ignored by git via `*.gguf`. It is present locally and copied into the app output, but it is not pushed as a repository blob.

### Tests

Command:

```powershell
dotnet run --project apps\Minnal.AppModel.Tests\Minnal.AppModel.Tests.fsproj --no-restore
```

Result:

```text
8 tests run
8 passed
0 ignored
0 failed
0 errored
FsCheck property: 100 cases passed
```

The AI harness loads `gemma-4-E4B-it-Q4_K_M.gguf` and produces inference through `LLamaSharp.Backend.Cpu`.

### MAUI build

Command:

```powershell
dotnet build apps\Minnal.Maui\Minnal.Maui.csproj -f net10.0-windows10.0.19041.0 --no-restore
```

Result:

```text
Build succeeded.
0 warnings
0 errors
```

### Warm memory - Debug, Gemma 4 E4B loaded

Sample after launching the MAUI app and waiting 55 seconds:

```text
OUTPUT_GGUF_COUNT=1
HOST_MB=3928.8
HOST_PLUS_NEW_WEBVIEW2_MB=4211.6
```

| Process | MB |
|---|---:|
| Minnal.Maui.exe | 3928.8 |
| msedgewebview2 renderer/browser | 155.1 |
| msedgewebview2 GPU/browser support | 65.8 |
| msedgewebview2 GPU-info | 34.0 |
| msedgewebview2 crash handler | 18.4 |
| msedgewebview2 spare renderer | 9.5 |
| **Total** | **4211.6** |

### Interpretation

Gemma 4 E4B Q4_K_M is viable on this machine using CPU and a 512-token context. The shell plus loaded model sits around 4.2 GB working set in Debug. This fits the local-first direction, but it changes the memory envelope: the previous 700 MB warm target only applies before loading a real model.

---

## Session 4 — 2026-05-02 — F# service law + bundled GGUF policy

### What changed

| Item | File(s) | Status |
|---|---|---|
| Removed C# AI service | Services/AiService.cs | Done |
| Removed C# HTTP service | Services/HttpExecutionService.cs | Done |
| F# AI service port/adapter | Minnal.AppModel/Library.fs | Done |
| F# HTTP execution port/adapter | Minnal.AppModel/Library.fs | Done |
| LLamaSharp ownership moved to F# project | Minnal.AppModel.fsproj | Done |
| MAUI host remains DI glue only | MauiProgram.cs | Done |
| Model lookup changed from AppData to bundled app content | Library.fs | Done |
| SQLite spike DB moved from AppData to app-local `state/` | Library.fs | Done |
| Bundled model asset folder | Resources/Raw/ai/ | Done |

### F# law status

Durable service logic now lives in F#:

- `IAiService`
- `LlamaAiService`
- `IHttpExecutionService`
- `HttpExecutionService`
- `HttpResultView`
- bundled GGUF path discovery

C# is back to thin MAUI glue: app startup, DI registration, and Razor rendering.

Known weakness: service-shaped interfaces and mutable model lifetime are still interop adapters. They are marked with warning comments in F# because MAUI DI, `HttpClient`, SQLite, and LLamaSharp are class/disposable APIs. The domain state remains DU/smart-constructor based.

### Bundled GGUF policy

The app no longer reads model weights from:

```text
%LOCALAPPDATA%\Minnal\models\
```

Instead, it scans the installed app content directory for `*.gguf`.

Spike source location:

```text
apps/Minnal.Maui/Resources/Raw/ai/
```

Package rule: put the GGUF under `Resources/Raw/ai/` before packaging. The model is then carried with the app content. No model download or AppData model folder is required.

Current measurement has no GGUF bundled:

```text
BUNDLED_GGUF_COUNT=0
```

### SQLite policy

The spike DB moved from `%LOCALAPPDATA%` to:

```text
<app-base>/state/maui-spike.sqlite
```

This satisfies the current "all app stuff lives with the app" spike rule.

Known weakness: installed/mobile app bundles are commonly read-only. Before store/mobile packaging, writable state needs a platform-specific decision. For now, the unpackaged Windows spike can write beside the app output.

### Memory — Debug, no bundled model

| Process | MB |
|---|---:|
| Minnal.Maui.exe (host) | 161.3 |
| msedgewebview2 — renderer/browser | 154.6 |
| msedgewebview2 — GPU/browser support | 63.5 |
| msedgewebview2 — GPU-info | 34.1 |
| msedgewebview2 — crash handler | 18.4 |
| msedgewebview2 — spare renderer | 9.6 |
| **TOTAL HOST + NEW WEBVIEW2** | **441.5 MB** |

### Memory — Release, no trim, no bundled model

| Process | MB |
|---|---:|
| Minnal.Maui.exe (host) | 152.5 |
| msedgewebview2 — renderer/browser | 154.6 |
| msedgewebview2 — GPU/browser support | 63.5 |
| msedgewebview2 — GPU-info | 34.1 |
| msedgewebview2 — crash handler | 18.4 |
| msedgewebview2 — spare renderer | 9.5 |
| **TOTAL HOST + NEW WEBVIEW2** | **432.7 MB** |

### Current AI state

AI is wired but blocked until a GGUF is bundled into app content.

```text
Off -> Loading -> Ready   (GGUF found under installed app content)
Off -> Loading -> Blocked (no bundled GGUF found)
```

No GGUF has been committed. A real model file must be supplied as a release artifact or intentionally added to the package input.

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
| LLamaSharp 0.27.0 + Backend.Cpu | Minnal.AppModel.fsproj | Done |
| AiService (graceful no-model blocked state) | Minnal.AppModel/Library.fs | Done |
| HttpExecutionService (real HttpClient) | Minnal.AppModel/Library.fs | Done |
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
Off → Loading → Ready   (GGUF found under installed app content)
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
| AI loading (LLamaSharp) | Wired, Blocked | No bundled GGUF under app content |
| Real HTTP execution | Wired | GET api.github.com/zen fires from Send |
| Playground (zero-persist) | Not started | Phase 4 M7 |
| Semantic canvas | Not started | Phase 4 M3 |
| AI explain (gemma) | Wired, blocked | Needs GGUF model to activate |

---

## AI Blocker — No Bundled GGUF Model

Current product law: model weights are bundled with the app package, not read from AppData.

To unblock AI:
1. Download a GGUF model (e.g. gemma-3-1b-it-q4_k_m.gguf or phi-3-mini-4k-instruct-q4.gguf)
2. Place it under: `apps/Minnal.Maui/Resources/Raw/ai/`
3. Rebuild/package Minnal — AI state will transition Off → Loading → Ready

Recommended for spike (fits in 4 GB RAM): **gemma-3-1b-it-q4_k_m.gguf** (~800 MB)

---

## Way Forward

### Immediate (next session)

1. **Bundle GGUF model** — unlock Explain button end-to-end
   - Download: `winget install -e --id HuggingFace.HuggingFaceCLI` then `huggingface-cli download google/gemma-3-1b-it-gguf`
   - Put the `.gguf` under `apps/Minnal.Maui/Resources/Raw/ai/`

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
