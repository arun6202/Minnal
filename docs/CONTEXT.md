# CONTEXT.md - Current Sprint State
# Updated at end of every Claude and Codex CLI session.
# This file is the live heartbeat of the project.

---

## Project State: PHASE 0 COMPLETE - PHASE 0.5 PARTIAL
## Sprint: 0.5 - Spike Week
## Date: 2026-05-02

---

## Locked Decisions (Do Not Re-Litigate)

See Minnal.md and plan.md for full detail. Summary:

- Language: Rust (WinUI 3 chrome + WebView2 content for UI)
- License: Apache 2.0 - all deps must comply
- AI Model: gemma-4-e4b 14B (Apache 2.0, not bundled)
- AI Backend: llama.cpp FFI; `llama-cpp-2` DirectML path blocked pending custom build decision
- Hook Runtime: Wasmtime (Apache 2.0, Cranelift JIT)
- Storage: SQLite + sqlx + sqlite-vec, with hnsw_rs fallback if Windows spike fails
- Machine Floor: 16GB RAM minimum - hard enforcement
- Hook execution: Pre=sync(5s timeout), Post=async(isolated)
- Model assessment: once at startup, hold + dialog on pressure
- Response bodies: SHA content-addressed deduplication
- Examples: separate Minnal-examples repo
- Distribution: Ollama sidecar if present, guided install if absent
- v1 invariant: SIGNED (see Minnal.md section 1)

---

## Open Decisions

```
[x] Cargo workspace initialised in GitHub repo? - Phase 0 scaffold in progress
[x] Repo name confirmed: Minnal
[x] WinUI 3 minimum Windows SDK version pinned? - Path B locked: WinUI 3 chrome + WebView2 content
[x] gemma-4-e4b GGUF download URL finalised? - model locked; exact HF revision SHA still needed before download code
[x] llama.cpp vendored as git submodule or cargo FFI crate? - `llama-cpp-2` 0.1.146, CPU-only for Spike B; DirectML deferred to Phase 2 M5
[x] DirectML SDK version pinned? - DirectML 1.15.x target retained for Phase 2; CPU-only for Spike B
[x] sqlite-vec version pinned and tested on Windows? - `sqlite-vec` 0.1.9 confirmed on Windows MSVC
[x] CI: GitHub Actions Windows runner confirmed available? - windows-2022
[x] MSIX signing: personal use? - self-signed for dev/internal
```

---

## Work Completed

```
[x] Architecture grilled across 4 rounds (Claude + Arun)
[x] Minnal.md authored and signed
[x] CLAUDE.md session loader created
[x] CONTEXT.md initialised
[x] Git repository initialised locally
[x] GitHub remote configured over SSH: git@github.com:arun6202/Minnal.git
[x] Initial docs commit pushed to origin/main
[x] plan.md implementation plan authored
[x] design/Minnal-store/V001.md design pack authored
[x] phase-0-and-0.5.md task script authored
[x] Minnal-ui V001 design direction authored: execution-first workbench, contextual Why drawer, semantic canvas as mode
[x] Phase 0 Cargo workspace scaffold committed and pushed
[x] Phase 0 scaffold re-verified: `cargo fmt --all -- --check` and `cargo build --workspace --all-targets`
[x] Spike A: Path B locked (2026-05-02). No usable `windows-app-sdk` Rust crate found; installed Windows App Runtime is 1.2, below 1.6 target.
[ ] Spike B: CPU path decided (2026-05-02). `llama-cpp-2` 0.1.146 CPU-only. Spike code rewritten for full one-token inference + n_embd reporting. Blocked on: (1) install LLVM (`winget install LLVM.LLVM`), (2) run `.\download_model.ps1` from spikes/spike-b-llama/. DirectML deferred to Phase 2 M5.
[x] Spike C: sqlite-vec confirmed on Windows (2026-05-02). `sqlite-vec` 0.1.9 + `rusqlite` 0.31 compiled, loaded via `sqlite3_auto_extension`, and returned `[(1, 0.0), (2, 0.800000011920929)]`.
```

---

## Next Task for Codex CLI

```
Task:   Close Spike B — one-token smoke test

Steps:
  1. Run (admin terminal): winget install LLVM.LLVM
  2. Set env var: $env:LIBCLANG_PATH = "C:\Program Files\LLVM\bin"
  3. cd spikes/spike-b-llama
  4. Run: .\download_model.ps1
  5. Run: cargo run
  6. Paste the "=== SPIKE B RESULT ===" block into CONTEXT.md under Work Completed.
  7. If n_embd ≠ 1024, update V001 schema FLOAT[] before Phase 1 begins.

Constraints:
  - Spike runs CPU-only; DirectML deferred to Phase 2 M5.
  - No production crate work before spike outcomes are recorded.

Review gate: Claude reviews n_embd result and schema decision before Phase 1 M1 begins.
```

---

## Next Task for Claude (Next Session)

```
Task: Review Phase 0 scaffold, Minnal-ui V001 direction, and prepare Phase 0.5 spike review checklist.
```

---

## Known Risks (Monitor)

```
[LOW]    WinUI 3 + Rust: Path A blocked; Path B locked.
         Mitigation: WebView2 content hosted by native WinUI 3 chrome.

[HIGH]   llama.cpp + DirectML on Windows: deferred.
         Mitigation: Spike B runs CPU-only (llama-cpp-2 0.1.146); DirectML tackled in Phase 2 M5.

[MEDIUM] sqlite-vec Windows build fragility.
         Mitigation: Spike C passed on Windows MSVC; keep hnsw_rs fallback behind EmbeddingIndex.

[MEDIUM] Embedding dimension mismatch with V001 FLOAT[1024].
         Mitigation: Spike B records actual gemma-4-e4b embedding dimension before V001 ships.

[LOW]    OAuth 2.0 PKCE local redirect server: port conflicts.
         Mitigation: use ephemeral port, register multiple fallback ports.
```

---

*Update this file at the end of every session.*
*Never Settle.*




