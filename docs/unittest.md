# Unit Test Report

Date: 2026-05-02  
Branch: `maui-fsharp-spike`

## Summary

| Area | Result | Notes |
|---|---:|---|
| Bundled Gemma/GGUF check | Passed | `gemma-4-E4B-it-Q4_K_M.gguf` exists locally in bundled app assets |
| F# AppModel tests | Passed | 8 Expecto tests, including 100 FsCheck property cases |
| Real AI harness | Passed | Gemma 4 E4B Q4_K_M loaded and produced inference |
| MAUI Windows build | Passed | 0 warnings, 0 errors |
| Rust workspace tests | Skipped | Intentionally ignored for this pass |
| Live REST probe | Passed | `GET https://api.github.com/zen` returned successfully |
| Markdown consolidation | Done | Markdown files moved under `docs/` |

## Gemma / GGUF Status

Gemma is downloaded and locally bundled in the app asset tree.

Checked:

```powershell
Get-ChildItem -Path E:\Arun\Workspace\Minnal -Recurse -Filter *.gguf
```

Result:

```text
apps\Minnal.Maui\Resources\Raw\ai\gemma-4-E4B-it-Q4_K_M.gguf
apps\Minnal.Maui\bin\Debug\net10.0-windows10.0.19041.0\win-x64\ai\gemma-4-E4B-it-Q4_K_M.gguf
```

Current app policy is bundled model only. The app scans its installed content directory for `*.gguf`; it does not load weights from `%LOCALAPPDATA%`.

Expected source location before packaging:

```text
apps/Minnal.Maui/Resources/Raw/ai/<model>.gguf
```

The GGUF is ignored by git via `*.gguf`, so it is a local app/package asset, not a committed repository blob.

## F# AppModel Tests

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

Coverage added:

| Feature | Test |
|---|---|
| `NonEmptyText` smart constructor | accepts exactly non-whitespace text |
| `NonNegativeInt` smart constructor | rejects negative integers |
| `HttpStatusCode` smart constructor | rejects values outside 100-599 |
| `WorkbenchSnapshotFactory` | creates GitHub Zen active request |
| `WorkbenchService` | seeds SQLite-backed request/body counts |
| `LlamaAiService` | blocks cleanly when no bundled GGUF exists |
| `LlamaAiService` | loads bundled Gemma 4 E4B and produces inference |
| `HttpExecutionService` | reports invalid URL as effect error |

AI harness model:

```text
gemma-4-E4B-it-Q4_K_M.gguf
Source: ggml-org/gemma-4-E4B-it-GGUF
Base model: google/gemma-4-E4B-it
Local size: 4.97 GB
Backend: LLamaSharp.Backend.Cpu
Context: 512
Max output tokens: 64
```

## REST API Probe

Command:

```powershell
Invoke-RestMethod -Uri https://api.github.com/zen
```

Result:

```text
STATUS=OK
DURATION_MS=493
BODY=It's not fully shipped until it's fast.
```

This verifies the machine can reach the live REST API used by the spike request.

## MAUI Build

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

## Warm MAUI Memory With Gemma 4 E4B

Command shape:

```powershell
Launch apps\Minnal.Maui\bin\Debug\net10.0-windows10.0.19041.0\win-x64\Minnal.Maui.exe,
wait 55 seconds, then sample host plus newly spawned WebView2 processes.
```

Result:

```text
OUTPUT_GGUF_COUNT=1
HOST_MB=3928.8
HOST_PLUS_NEW_WEBVIEW2_MB=4211.6
```

Process rows:

| Process | MB |
|---|---:|
| Minnal.Maui.exe | 3928.8 |
| msedgewebview2 renderer/browser | 155.1 |
| msedgewebview2 GPU/browser support | 65.8 |
| msedgewebview2 GPU-info | 34.0 |
| msedgewebview2 crash handler | 18.4 |
| msedgewebview2 spare renderer | 9.5 |
| **Total** | **4211.6** |

## Known Gaps

No end-to-end MAUI UI automation exists yet. Current verification is service-level tests plus app build.

The live REST probe is a network smoke test, not a deterministic unit test. The deterministic HTTP unit test covers the local error path only.

Rust was intentionally ignored in this pass.

The GGUF is present locally and copied into app output, but it is not pushed to git because `.gguf` files are ignored. A release packaging step must attach or copy the model artifact.
