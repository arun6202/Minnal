# CONTEXT.md - Current Sprint State
# Updated at end of every Claude and Codex CLI session.
# This file is the live heartbeat of the project.

---

## Project State: PHASE 0 IN PROGRESS
## Sprint: 0 - Foundation
## Date: 2026-05-01

---

## Locked Decisions (Do Not Re-Litigate)

See Minnal.md and plan.md for full detail. Summary:

- Language: Rust (windows-rs, WinUI 3)
- License: Apache 2.0 - all deps must comply
- AI Model: gemma-4-e4b 14B (Apache 2.0, not bundled)
- AI Backend: llama.cpp FFI via llama-cpp-2 + DirectML where available
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
[x] WinUI 3 minimum Windows SDK version pinned? - Windows App SDK 1.6 GA, min Win10 22H2
[x] gemma-4-e4b GGUF download URL finalised? - model locked; exact HF revision SHA still needed before download code
[x] llama.cpp vendored as git submodule or cargo FFI crate? - llama-cpp-2, revisit only if DirectML unavailable
[x] DirectML SDK version pinned? - DirectML 1.15.x
[x] sqlite-vec version pinned and tested on Windows? - pin 0.1.x; Phase 0.5 spike decides sqlite-vec vs hnsw_rs
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
[x] Phase 0 Cargo workspace scaffold verified locally; commit in progress
```

---

## Next Task for Codex CLI

```
Crate:    workspace root
Task:     Complete Phase 0 foundation scaffold

Steps:
  1. Verify Cargo workspace builds clean.
  2. Verify each crate builds independently.
  3. Verify CI workflow is present.
  4. Commit: "chore: initialise Minnal workspace"
  5. Push origin/main.

Constraints:
  - Skeleton only; no feature implementation yet.
  - minnal-core and minnal-store have no internal deps.
  - Cargo.lock is committed.
  - No model weights, SQLite DBs, secrets, or session transcripts committed.
  - License headers in every lib.rs.

Review gate: Claude reviews workspace structure before any crate feature work begins.
```

---

## Next Task for Claude (Next Session)

```
Task: Review Phase 0 scaffold, Minnal-ui V001 direction, and prepare Phase 0.5 spike review checklist.
```

---

## Known Risks (Monitor)

```
[HIGH]   WinUI 3 + Rust: pioneering territory.
         Mitigation: Phase 0.5 Spike A before any UI feature work.

[HIGH]   llama.cpp + DirectML on Windows: build complexity.
         Mitigation: Phase 0.5 Spike B before Minnal-ai implementation.

[MEDIUM] sqlite-vec Windows build fragility.
         Mitigation: Phase 0.5 Spike C; hnsw_rs fallback behind EmbeddingIndex.

[MEDIUM] Embedding dimension mismatch with V001 FLOAT[1024].
         Mitigation: Spike B records actual gemma-4-e4b embedding dimension before V001 ships.

[LOW]    OAuth 2.0 PKCE local redirect server: port conflicts.
         Mitigation: use ephemeral port, register multiple fallback ports.
```

---

*Update this file at the end of every session.*
*Never Settle.*



