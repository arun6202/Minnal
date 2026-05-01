# CONTEXT.md — Current Sprint State
# Updated at end of every Claude and Codex CLI session.
# This file is the live heartbeat of the project.

---

## Project State: PRE-IMPLEMENTATION
## Sprint: 0 — Foundation
## Date: 2025

---

## Locked Decisions (Do Not Re-Litigate)

See Minnal.md for full detail. Summary:

- Language: Rust (windows-rs, WinUI 3)
- License: Apache 2.0 — all deps must comply
- AI Model: gemma-4-e4b 14B (MIT license)
- AI Backend: llama.cpp FFI + DirectML (Intel Xe)
- Hook Runtime: Wasmtime (Apache 2.0, Cranelift JIT)
- Storage: SQLite + sqlx + sqlite-vec
- Machine Floor: 16GB RAM minimum — hard enforcement
- Hook execution: Pre=sync(5s timeout), Post=async(isolated)
- Model assessment: once at startup, hold + dialog on pressure
- Response bodies: SHA content-addressed deduplication
- Examples: separate Minnal-examples repo
- Distribution: Ollama sidecar if present, guided install if absent
- v1 invariant: SIGNED (see Minnal.md §1)

---

## Open Decisions

```
[ ] Cargo workspace initialised in GitHub repo?
[ ] Repo name confirmed: Minnal ?
[ ] WinUI 3 minimum Windows SDK version pinned?
[ ] gemma-4-e4b GGUF download URL finalised (HuggingFace mirror)?
[ ] llama.cpp vendored as git submodule or cargo FFI crate?
[ ] DirectML SDK version pinned?
[ ] sqlite-vec version pinned and tested on Windows?
[ ] CI: GitHub Actions — Windows runner confirmed available?
[ ] MSIX signing: personal use (no EV cert needed for now)?
```

---

## Work Completed

```
[x] Architecture grilled across 4 rounds (Claude + Arun)
[x] Minnal.md authored and signed
[x] CLAUDE.md session loader created
[x] CONTEXT.md initialised
[ ] Nothing committed to GitHub yet
```

---

## Next Task for Codex CLI

```
Crate:    workspace root
Task:     Initialise Cargo workspace

Steps:
  1. cargo new --lib Minnal-core
  2. cargo new --lib Minnal-ui
  3. cargo new --lib Minnal-ai
  4. cargo new --lib Minnal-hooks
  5. cargo new --lib Minnal-store
  6. cargo new --lib Minnal-export
  7. Create root Cargo.toml with [workspace] members
  8. Add to each crate Cargo.toml:
       [package] edition = "2021"
       license = "Apache-2.0"
  9. Create .gitignore:
       /target
       *.sqlite
       *.sqlite-wal
       *.sqlite-shm
       *.gguf
       *.ggml
       .Minnal-secrets
       Minnal-data/
  10. Create empty Minnal.md placeholder in root (real file provided)
  11. cargo build --workspace (must compile clean)
  12. Commit: "chore: initialise Minnal workspace"

Constraints:
  - Zero dependencies added yet — skeleton only
  - Every crate must compile independently
  - No unwrap() anywhere — skeleton code uses todo!() only
  - License headers in every lib.rs

Review gate: Claude reviews workspace structure before any crate work begins
```

---

## Next Task for Claude (Next Session)

```
Task:   Design Minnal-store crate
        - Full SQLite schema as Rust types (sqlx)
        - Migration V001 implementation
        - Repository trait definitions (not implementations)
        - Property-based test spec for SHA deduplication
        - sqlite-vec integration design
```

---

## Known Risks (Monitor)

```
[HIGH]   WinUI 3 + Rust: pioneering territory. No production examples.
         Mitigation: spike WinUI 3 tab + split pane in week 1 before 
         any other UI work. If spike fails, escalate to Arun immediately.

[HIGH]   llama.cpp + DirectML on Windows: build complexity.
         Mitigation: pin llama.cpp commit hash. Test DirectML backend
         separately before integrating with Minnal-ai.

[MEDIUM] sqlite-vec Windows build: check for MSVC toolchain requirements.
         Mitigation: test sqlite-vec extension load on target machine
         before committing to it in schema.

[MEDIUM] gemma-4-e4b GGUF availability: confirm MIT-licensed weights are
         available on HuggingFace without usage restrictions.
         Mitigation: Arun to verify license on HuggingFace model card.

[LOW]    OAuth 2.0 PKCE local redirect server: port conflicts.
         Mitigation: use ephemeral port, register multiple fallback ports.
```



*Update this file at the end of every session.*
*Never Settle.*
