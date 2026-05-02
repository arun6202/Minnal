# Minnal

Minnal is an AI-native API workspace for Windows.

Tagline: **Never Settle.**

## Status

Phase 0 scaffold is complete. The repository currently contains:

- Rust Cargo workspace with six crates under `crates/`
- Signed architecture spec in `Minnal.md`
- Implementation plan in `plan.md`
- Phase 0 / 0.5 execution script in `phase-0-and-0.5.md`
- Store V001 design pack in `design/Minnal-store/V001.md`
- UI V001 direction in `design/Minnal-ui/V001.md`
- Windows GitHub Actions CI

Next phase: Phase 0.5 spikes.

## Workspace Crates

- `minnal-core`: HTTP engine, auth schemes, environment variables, core types
- `minnal-store`: SQLite persistence, content-addressed response bodies, vector index
- `minnal-ai`: model router, inference backend, prompt engine
- `minnal-hooks`: Wasmtime sandbox, hook lifecycle, capability grants
- `minnal-export`: HTML, PDF, and Excel exports
- `minnal-ui`: WinUI 3 shell

## V1 Invariant

A developer can import a Postman collection or Swagger/OpenAPI spec, execute REST requests with auth, explain responses locally with `gemma-4-e4b`, generate a WASM auth pre-hook from natural language, and search requests by intent.

All offline. All local. No Electron. No cloud call.

## Verify

```powershell
cargo fmt --all -- --check
cargo clippy --workspace --all-targets -- -D warnings
cargo build --workspace --all-targets
cargo test --workspace
```

## Phase 0.5 Spikes

Before feature implementation, prove the risky platform paths:

1. WinUI 3 + Rust hello-window
2. `llama.cpp` / `llama-cpp-2` + DirectML smoke test
3. `sqlite-vec` load/query on Windows

Record outcomes in `CONTEXT.md` before Phase 1 begins.

## License

Apache-2.0. See `LICENSE`.
