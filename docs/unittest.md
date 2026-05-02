# Unit Test Report

Date: 2026-05-02  
Branch: `maui-fsharp-spike`

## Summary

| Area | Result | Notes |
|---|---:|---|
| Bundled Gemma/GGUF check | Blocked | No `.gguf` file exists in repo or app output |
| F# AppModel tests | Passed | 7 Expecto tests, including 100 FsCheck property cases |
| MAUI Windows build | Passed | 0 warnings, 0 errors |
| Rust workspace tests | Passed | All crates compile; no Rust tests currently defined |
| Live REST probe | Passed | `GET https://api.github.com/zen` returned successfully |
| Markdown consolidation | Done | Markdown files moved under `docs/` |

## Gemma / GGUF Status

Gemma is not downloaded or bundled.

Checked:

```powershell
Get-ChildItem -Path E:\Arun\Workspace\Minnal -Recurse -Filter *.gguf
```

Result:

```text
No .gguf files found.
```

Current app policy is bundled model only. The app scans its installed content directory for `*.gguf`; it does not load weights from `%LOCALAPPDATA%`.

Expected source location before packaging:

```text
apps/Minnal.Maui/Resources/Raw/ai/<model>.gguf
```

## F# AppModel Tests

Command:

```powershell
dotnet run --project apps\Minnal.AppModel.Tests\Minnal.AppModel.Tests.fsproj --no-restore
```

Result:

```text
7 tests run
7 passed
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
| `HttpExecutionService` | reports invalid URL as effect error |

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

## Rust Workspace Tests

Command:

```powershell
cargo test --workspace
```

Result:

```text
Finished test profile successfully.
All Rust crates compiled.
0 Rust unit tests are currently defined.
0 Rust doc tests are currently defined.
```

Crates checked:

| Crate | Status |
|---|---|
| `minnal-ai` | Passed, 0 tests |
| `minnal-core` | Passed, 0 tests |
| `minnal-export` | Passed, 0 tests |
| `minnal-hooks` | Passed, 0 tests |
| `minnal-store` | Passed, 0 tests |
| `minnal-ui` | Passed, 0 tests |

## Known Gaps

No bundled GGUF exists yet, so real local inference has not been tested.

No end-to-end MAUI UI automation exists yet. Current verification is service-level tests plus app build.

The live REST probe is a network smoke test, not a deterministic unit test. The deterministic HTTP unit test covers the local error path only.

Rust crates still have no behavior tests.
