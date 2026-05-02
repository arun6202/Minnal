# plan.md — Minnal Implementation Plan
# Companion to Minnal.md (architecture) and CONTEXT.md (live state).
# Status: v0.1 — drafted pre-implementation.
# Tagline: Never Settle.

---

## 0. How To Read This Plan

This document has three layers stacked on top of each other:

1. **Resolved decisions** (§1): the 9 open items in CONTEXT.md, each closed with a chosen path and a one-line rationale. Treat as locked unless flagged 🔍 (verify) or ⚠️ (revisit on spike result).
2. **Phase map** (§2–§3): a lean, risk-first phased view. No dates. Phases are gated by acceptance criteria, not calendar.
3. **Per-crate plans** (§5): inside each phase, every crate has its own milestone ladder. Read §5 when you sit down to actually build a thing; read §2 to know what comes next.

Owners are assigned per Minnal.md §14:
- **Arun** — decision authority, owns Minnal-core craft, integrates all output.
- **Claude (design)** — architecture, types, AI subsystem, WASM sandbox, review gate.
- **Codex CLI (scaffold)** — XAML boilerplate, FFI ceremony, exports, test stubs.

When a milestone says "Claude designs → Codex scaffolds → Arun integrates", that is the canonical handoff. Skipping the design step on a Codex-owned crate is a review-gate failure.

---

## 1. Resolved Decisions (the 9 unblockers)

| # | Question | Decision | Rationale |
|---|----------|----------|-----------|
| 1 | Cargo workspace initialised in GitHub repo? | **Phase 0 task.** Repo created first on GitHub; workspace inited locally; first commit pushed in same session. | Empty-repo-on-GitHub is cheaper than discovering a name conflict after writing code. |
| 2 | Repo name confirmed: `Minnal`? | **Yes.** `minnal` (lowercase) for the GitHub URL; `Minnal` for product branding & docs. Tagline `Never Settle` in repo description. | URL hygiene + brand consistency; matches Tier-3 plan to publish publicly. |
| 3 | WinUI 3 minimum Windows SDK version pinned? | **Windows App SDK 1.6 GA.** Min target: Win10 22H2 (10.0.19045). Recommended: Win11 23H2+. | 1.6 is the most recent stable WinAppSDK with shipping WinUI 3 controls; Win10 22H2 is the lowest version still receiving security updates (matches §2.1). |
| 4 | gemma-4-e4b GGUF download URL finalised? | **gemma-4-e4b 14B, Apache 2.0.** Confirmed by Arun: Apache-aligned, no namespace conflict with Google's Gemma. Pin exact HF revision SHA, never `latest`. Default `Q4_K_M` for 16GB tier; `Q8_0` for 32GB+. First-run guided download; Ollama sidecar preferred if present. | Apache 2.0 model in an Apache 2.0 project — clean. Pinned SHA prevents silent upstream changes. Minnal.md §12 manifest updated to clarify the namespace (gemma-4-e4b is unrelated to Google Gemma family). |
| 5 | llama.cpp vendored as git submodule or cargo FFI crate? | **Open after Spike B partial.** `llama-cpp-2` 0.1.146 exists, but exposes no DirectML feature; build also requires `libclang`. | DirectML trigger fired on 2026-05-02. Decide between CPU/Vulkan/dynamic-backends via crate or llama.cpp git submodule + custom DirectML `build.rs` before Phase 2. |
| 6 | DirectML SDK version pinned? | **DirectML 1.15.x via NuGet redist + `windows-rs` bindings.** | 1.15 ships current ML operator set required by gemma-4-e4b inference; `windows-rs` already covers DirectML.h surface. |
| 7 | sqlite-vec version pinned and tested on Windows? | **Pin `sqlite-vec 0.1.x` (latest).** Phase 0 spike must load it on the target machine before any schema work begins. **Fallback**: `hnsw_rs` (Apache 2.0) over plain `BLOB` columns if extension load fails. | sqlite-vec is young; Windows MSVC linkage is the failure mode worth proving early, not on the day you ship UC-06. |
| 8 | CI: GitHub Actions Windows runner confirmed? | **Yes — `windows-2022` runner.** Cache `~/.cargo` and `target/` per workspace; install Windows App SDK in a setup step (no built-in action exists, so a one-shot PowerShell installer is committed under `.github/actions/setup-winappsdk/`). | Pinning to `windows-latest` is fragile; `windows-2022` is stable through 2026. WinAppSDK install adds ~90s to cold CI runs — accept it. |
| 9 | MSIX signing: personal use — no EV cert? | **Self-signed for dev + internal.** Document SmartScreen bypass in README. EV cert is a v1.1 concern, gated on first external installer download. | EV certs are €300+/yr and pointless before a real release; SmartScreen warning is acceptable for dogfood. |

🔍 = blocking on Arun confirmation.
⚠️ = locked-for-now, but earmarked for re-evaluation on a specific signal.

---

## 2. Phase Map (Lean, Risk-First)

```
Phase 0      Foundation                     ← prove the toolchain
Phase 0.5    Spike Week                     ← prove the two HIGH risks
Phase 1      Data + Engine Bedrock          ← Minnal-store, Minnal-core
Phase 2      AI Subsystem                   ← Minnal-ai
Phase 3      Hook Sandbox                   ← Minnal-hooks
Phase 4      Reimagined UI                  ← Minnal-ui  (the new blood)
Phase 5      Export + v1 Polish             ← Minnal-export, perf, install
Phase 6      v1 Cut                         ← gates from Minnal.md §13
```

Each phase has an **entry gate** (must be true before starting) and an **exit gate** (acceptance criteria, all green before next phase). Skipping a gate is a review-gate failure.

### 2.1 Phase summaries

**Phase 0 — Foundation.**
Initialise repo, Cargo workspace, CI, gitignore, licence headers. Nothing fancy. Outcome: `cargo build --workspace` clean, CI green.

**Phase 0.5 — Spike Week.**
Two parallel proofs:
- **Spike A**: WinUI 3 + Rust shell — minimum viable window with a tab and a split pane. If this fails, the whole UI strategy escalates immediately to Arun.
- **Spike B**: llama.cpp FFI + DirectML — load any small GGUF, run a single inference, verify DirectML backend selection on Intel Xe. If DirectML fails, downgrade to CPU with a flag and document.

Spike outcomes determine fallback paths in Phases 4 and 2 respectively.

**Phase 1 — Data + Engine Bedrock.**
Minnal-store (V001 migration, content-addressed bodies, sqlite-vec wired) and Minnal-core (HTTP engine, all auth schemes, Postman/OpenAPI import). These two are the load-bearing floor; everything else rests on them. Both must be exhaustively property-tested before AI lands on top.

**Phase 2 — AI Subsystem.**
Minnal-ai. Machine assessment → model router → llama.cpp wrapper → prompt engine. End of phase: UC-01 (intent-to-request) and UC-04 (response explanation) work end-to-end through a CLI test harness, no UI yet.

**Phase 3 — Hook Sandbox.**
Minnal-hooks. Wasmtime initialised, capability grants enforced, dry-run pipeline, AI hook generation (UC-03). Pre/post-hook timing contracts (5s sync, isolated async) verified by integration test.

**Phase 4 — Reimagined UI.**
Minnal-ui. The "new blood" phase — see §4. WinUI 3 shell, intent-first command surface, semantic canvas instead of folder tree, ambient explainability, ephemeral playground. Exit gate: a developer can complete the v1 invariant story (import → execute → explain → search → hook) without seeing a single thing that looks like Postman.

**Phase 5 — Export + v1 Polish.**
Minnal-export (HTML/PDF/Excel), cold-start budget enforcement, idle-RAM measurement, MSIX packaging, first-run model download flow, README + install docs.

**Phase 6 — v1 Cut.**
Walk Minnal.md §13 checklist. Tag `v0.1.0`. Cut a self-signed MSIX. Run the v1 invariant scenario on a fresh Win11 VM with 16GB and again with 32GB.

---

## 3. Risk-First Ordering Rationale

The default temptation is to build UI first because it is visible and motivating. We do not. Here is why:

1. **The two HIGH risks (WinUI 3 + Rust, llama.cpp + DirectML) are existential.** If either fails, the whole architecture pivots. A spike week before any feature work means a pivot is cheap. A pivot in Phase 4 is not.
2. **Minnal-store and Minnal-core are the only crates with no internal dependencies.** They can be built and tested in isolation. Building them first means the rest of the system has a typed, tested foundation to lean against.
3. **AI before UI.** The UI is shaped by what the AI can actually do, not by what it might do. Building Minnal-ai before Minnal-ui means the UI grows around real capabilities, not aspirations. (This is the discipline that prevents a Postman-clone slip — see §4.)
4. **Hooks before UI.** Hook approval flow needs UI surface; designing the surface before knowing the dry-run output shape produces wrong UI. Build the hook engine first, then design the approval pane around its actual output.
5. **Export and polish last.** Both are well-understood; both can be parallelised against test/polish work.

---

## 4. Reimagined UI/UX — The "New Blood" Section

> "Postman is a static form-filler built before local LLMs existed.
> Minnal is what an API workspace looks like when designed *after* them."
> — Minnal.md §0

This phase deserves its own section because the UI is where Minnal either fulfils the soul or quietly settles.

### 4.1 Anti-patterns (do not do these)

- **Folder tree as primary navigation.** Folders are filing-cabinet thinking. Replace with a semantic canvas (see §4.2).
- **Modal dialogs for auth setup.** Auth is a *flow*, not a config form. Render as a small composable graph.
- **"Save before run" friction.** Playground is the default surface; commits are deliberate, not accidental.
- **Tabs that survive restart by default.** Tabs are ephemeral; collections are persistent. Conflating them is Postman's mistake.
- **Raw response JSON as the primary view.** JSON is the *fallback*, not the headline. The headline is the AI explanation.
- **A separate "test" tab.** Assertions are inline with the request, not exiled to a sub-tab no one opens.
- **"Send" button.** Replace with `Run` (verb-noun parity with `Promote`, `Discard`, `Explain`, `Diff`).

### 4.2 The five surfaces (replace "tabs and forms" model)

1. **Intent Bar (top, always visible).**
   `Ctrl+K` opens it from anywhere. Accepts: natural-language intent ("POST a user with email and role"), search query ("anything that deletes user data"), command (`:run`, `:explain`, `:diff`). One input, three modes — disambiguated by gemma's intent classifier, not by tab switching.

2. **Semantic Canvas (left, persistent).**
   Replaces the folder tree. Requests are grouped by *inferred domain* (auth, payments, admin) — clusters are computed from embeddings, not declared by the user. User can pin clusters, rename them, or revert to flat list. Folders from Postman import are preserved as *one* possible view, not the only one.

3. **Request Surface (centre, contextual).**
   Method + URL + body + headers in a single dense pane — but with three innovations:
   - **Inline contract diff badges**: if the last response's schema differs from the established contract, the affected fields are highlighted in the body editor *before* you re-run.
   - **Inline auth chip**: clicking the chip opens the auth flow graph in a side-popover, not a modal.
   - **Variable provenance hover**: hovering `{{TOKEN}}` shows where it came from (env var, hook output, manual). Postman makes you guess; Minnal shows you.

4. **Response Conversation (right, threaded).**
   Not a "response panel" — a *thread*. Each run appends a new entry. Each entry has: status + latency + body + AI explanation. Clicking "Why" on any entry expands the gemma-generated reasoning. Clicking "Diff" against any prior entry shows the schema delta in plain English. Threads survive in SQLite, scoped per request.

5. **Ambient Status Strip (bottom).**
   Persistent: model name + quantisation + offline confirmation + memory pressure indicator. This is Minnal's *integrity signal* — it is never hidden, never collapsed. When the user can see "gemma-4-e4b · Q8 · offline · 7.2GB" at all times, the soul is communicated without a marketing line.

### 4.3 The Playground (deserves its own paragraph)

A persistent global tab, always present, *zero SQLite writes until Promote*. It looks like a request surface but with a tinted background and a soft prompt at the top: "Experiment first. Promote to commit." Forking any collection request to playground is one keystroke. Closing the playground tab is a confirmed-discard. This is the surface where the AI-assisted iteration happens; the canvas is the surface where the considered work lives.

### 4.4 The "Why" everywhere principle

Every AI-derived value in the UI has a "Why" affordance — a small icon, never noisy, that opens the model's reasoning. Auto-grouped clusters: Why. Auto-generated hooks: Why. Auto-inferred contracts: Why. This is what separates an AI-native tool from an AI-decorated one. The user is never asked to trust the model on faith.

### 4.5 Tone

Dark theme first, system-theme respected. Monospace for all data (URLs, JSON, headers). Sans-serif only for AI explanations and chrome. No skeuomorphic icons. No emoji in chrome. Accent colour: a single warm orange (`#FF6B1A` candidate — Arun decides) — used only for Promote actions and the Never-Settle status badge. Everything else is grayscale.

### 4.6 Keyboard-first

Every action reachable in two keystrokes from the intent bar. Mouse is permitted; not required. This is a developer tool; developers live on the keyboard.

---

## 5. Per-Crate Implementation Plans

The build order inside Phase 1+ is the order below. Where a crate has milestones (M1, M2…), they may be parallelised within the crate but not across crates without explicit Arun approval.

---

### 5.1 Minnal-store

**Owner**: Claude designs → Codex scaffolds → Arun integrates.
**Phase**: 1.
**Internal deps**: none (per architecture invariant).
**External deps (proposed)**: `sqlx` (with `sqlite` feature), `sqlite-vec`, `serde`, `serde_json`, `uuid` (v7), `sha2`, `zstd`, `time`, `thiserror`.

#### Modules

```
Minnal-store/
├── src/
│   ├── lib.rs                  ← public façade, re-exports
│   ├── error.rs                ← StoreError (thiserror)
│   ├── ids.rs                  ← typed UUID-v7 newtypes (CollectionId, RequestId, …)
│   ├── migrations/
│   │   └── V001__initial_schema.sql
│   ├── schema/                 ← Rust types mirroring tables
│   │   ├── collection.rs
│   │   ├── request.rs
│   │   ├── environment.rs
│   │   ├── auth.rs
│   │   ├── response.rs
│   │   ├── hook.rs
│   │   ├── assertion.rs
│   │   ├── contract.rs
│   │   └── dependency.rs
│   ├── repo/                   ← Repository traits (one per aggregate root)
│   │   ├── mod.rs
│   │   ├── collections.rs
│   │   ├── requests.rs
│   │   ├── responses.rs
│   │   ├── bodies.rs           ← content-addressed body store
│   │   ├── embeddings.rs
│   │   └── …
│   ├── content_addressed.rs    ← SHA-256 + zstd helpers, the ETL pattern
│   └── vec_index.rs            ← sqlite-vec wrapper (sqlite-vec OR hnsw_rs fallback)
└── tests/
    ├── property/               ← proptest-based round-trip + invariants
    └── integration/            ← real SQLite file, real migrations
```

#### Public API surface (sketch — Claude finalises in Phase 1 design doc)

```rust
pub trait CollectionRepo {
    async fn insert(&self, c: NewCollection) -> Result<CollectionId, StoreError>;
    async fn get(&self, id: CollectionId) -> Result<Option<Collection>, StoreError>;
    async fn list(&self) -> Result<Vec<Collection>, StoreError>;
    async fn delete(&self, id: CollectionId) -> Result<(), StoreError>;
}

pub trait BodyStore {
    /// Returns the SHA either inserted or already-present (idempotent).
    async fn put(&self, body: &[u8], content_type: &str) -> Result<BodySha, StoreError>;
    async fn get(&self, sha: BodySha) -> Result<Option<StoredBody>, StoreError>;
}
```

**Discriminated unions everywhere.** No `String` for `method`, `phase`, `scheme`, `source` — those become Rust enums with `serde` `rename_all = "snake_case"` and explicit `TryFrom<&str>` for SQLite reads.

#### Milestones

- **M1.** SQLite schema written as `V001__initial_schema.sql`, exact match to Minnal.md §5.1, executed by `sqlx::migrate!()` at crate init. Test: open in-memory DB, run migration, assert `schema_migrations` table contains v1.
- **M2.** Typed newtype layer (`ids.rs`) and discriminated-union enums for every stringly-typed column. Property test: round-trip `Method::POST` → SQL string → `Method::POST` is identity.
- **M3.** Schema structs with `sqlx::FromRow`. Compile-time-checked queries via `query_as!`. Test: every aggregate root has insert + select + delete coverage.
- **M4.** Content-addressed body store (`bodies.rs` + `content_addressed.rs`). Property test: storing the same body N times produces N=1 row in `response_bodies` and N rows in `responses`. Storing a different body produces a different SHA.
- **M5.** Repository trait definitions for every aggregate root. **Implementations come in Phase 1 step 2** (after design review).
- **M6.** Repository implementations (one per trait), all async, all `Result<_, StoreError>`-returning, no `unwrap()` outside tests.
- **M7.** sqlite-vec integration via `vec_index.rs`. Test: insert 100 random embeddings, query top-k, assert ordering matches naive cosine. **Branch on Phase 0.5 spike outcome**: if sqlite-vec fails on Windows, swap implementation to `hnsw_rs` *behind the same trait* — no API change.
- **M8.** Migration safety: integration test asserts that re-running migrations on an existing DB is idempotent and adds no new rows.

#### Test strategy

- Property-based via `proptest` for: SHA dedupe, JSON round-trip, enum round-trip, UUID-v7 monotonicity.
- Integration via `sqlx::SqlitePool::connect("sqlite::memory:")` for fast unit-style tests.
- Real-file integration test against a temp file to catch WAL/synchronous-mode behaviour.

#### Done definition (mapped from Minnal.md §13)

- [ ] V001 migration applied successfully on fresh and existing DB.
- [ ] Response body SHA dedup verified by property test (100+ cases).
- [ ] Content-addressed storage round-trip via integration test.
- [ ] sqlite-vec extension loads & queries on target machine.

#### Open questions specific to this crate

- Should `value_encrypted` columns be wrapped in a `Sealed<T>` newtype that *requires* a DPAPI handle to read? (Strongly suggest yes — leaks a `String` otherwise.)
- Naming: `request_dependencies` table — better named `request_edges`? (Codex chooses; Claude reviews.)

---

### 5.2 Minnal-core

**Owner**: **Arun.** Do not generate code here without explicit request.
**Phase**: 1 (in parallel with later milestones of Minnal-store, since core has no internal deps).
**Internal deps**: none.
**External deps (suggested — Arun decides)**: `reqwest` (with `rustls-tls`, `gzip`, `brotli`, `cookies`), `tokio`, `http`, `serde`, `serde_json`, `serde_yaml` (for OpenAPI), `url`, `base64`, `hmac`, `sha2`, `windows` (for DPAPI / Credential Manager), `time`.

The plan below is **suggestion only** — Arun's craft, Arun's call. Claude will review what is built; Claude will not propose internal module structure.

#### Surface area to cover (per Minnal.md §13)

- HTTP engine: all methods, all headers, redirect handling, streaming bodies.
- Auth schemes: API Key (header/query/cookie), Bearer (static + AI-managed), Basic, OAuth 2.0 PKCE (with local redirect server, refresh, Credential Manager integration), AWS Sig v4, mTLS (Windows cert store).
- Environment variable system: global / collection / request scope, with explicit precedence rules.
- Postman v2.1 collection import + export.
- OpenAPI 3.x import.

#### Milestones (suggestion)

- **M1.** HTTP engine with method/header/body/redirect coverage; integration tested against `httpbin.org` mirror.
- **M2.** Auth scheme trait, API Key + Bearer + Basic implementations.
- **M3.** OAuth 2.0 PKCE with local redirect server (ephemeral port + fallback list).
- **M4.** AWS Sig v4 (against `aws-sdk-*` test vectors).
- **M5.** mTLS via Windows cert store.
- **M6.** Variable scope resolver with precedence rules + property test.
- **M7.** Postman v2.1 importer + exporter (round-trip property test on real-world collections).
- **M8.** OpenAPI 3.x importer (round-trip against the OpenAPI spec's own example set).

#### Done definition

Per Minnal.md §13 Minnal-core checklist.

#### Claude's role here

Review only. PR-fail any:
- `unwrap()` outside `#[cfg(test)]`.
- Stringly-typed `method`/`scheme`.
- Imperative loop where iterator chain exists.
- Missing property test on a data transformation.
- C++ idiom in Win32 FFI (e.g. raw HRESULT comparison instead of `windows::core::Result`).

---

### 5.3 Minnal-ai

**Owner**: Claude designs → Codex scaffolds → Arun integrates.
**Phase**: 2.
**Internal deps**: Minnal-store (read-only, for response history context).
**External deps (proposed)**: `llama-cpp-2` (FFI binding), `tokio`, `serde`, `serde_json`, `windows` (for hardware probe + DirectML), `sysinfo` (for RAM/thermal probing), `thiserror`.

#### Modules

```
Minnal-ai/
├── src/
│   ├── lib.rs
│   ├── error.rs                ← AiError
│   ├── machine/                ← hardware assessment
│   │   ├── mod.rs
│   │   ├── profile.rs          ← MachineProfile struct (per Minnal.md §3.1)
│   │   ├── probe.rs            ← Windows API calls: RAM, AVX flags, GPU, battery
│   │   └── thermal.rs          ← thermal level inference
│   ├── router.rs               ← ModelSelection + routing logic (§3.2)
│   ├── backend/                ← inference backend trait + impls
│   │   ├── mod.rs              ← ModelBackend trait
│   │   ├── llama_cpp.rs        ← llama.cpp via llama-cpp-2
│   │   └── directml.rs         ← DirectML feature detection
│   ├── prompts/                ← prompt templates as typed structs
│   │   ├── intent_to_request.rs
│   │   ├── explain_response.rs
│   │   ├── generate_hook.rs
│   │   └── embedding.rs
│   ├── session.rs              ← model lifecycle (load once, hold until restart)
│   └── events.rs               ← ModelEvent stream (per §3.1)
└── tests/
    ├── machine_probe.rs
    ├── router.rs               ← table-driven for every (profile → selection) case
    └── integration/            ← actual model load (gated behind `--features real-model`)
```

#### Public API surface (sketch)

```rust
pub trait ModelBackend: Send + Sync {
    async fn complete(&self, prompt: Prompt) -> Result<Completion, AiError>;
    async fn embed(&self, text: &str) -> Result<Embedding, AiError>;
}

pub struct Session {
    backend: Arc<dyn ModelBackend>,
    selection: ModelSelection,
    loaded_at: SystemTime,
}

impl Session {
    pub async fn assess_and_load(profile: MachineProfile) -> Result<Self, AiError>;
    pub fn selection(&self) -> ModelSelection;
    pub fn events(&self) -> impl Stream<Item = ModelEvent>;
    /// Per §3.3 — never silently downgrades. Emits HoldingModelUntilRestart event.
    pub async fn handle_memory_pressure(&self, used_gb: f32);
}
```

#### Milestones

- **M1.** `MachineProfile` struct + `probe()` returning a real profile on the dev machine. Print it. Eyeball it. Property test: probe twice in 100ms, results identical.
- **M2.** Router (`route()` from §3.2) — table-driven test for every documented case in Minnal.md, plus boundary cases (15.9GB → Unsupported, 16.0GB → supported).
- **M3.** `ModelBackend` trait + `MockBackend` (returns canned completions) for downstream development without real model.
- **M4.** `LlamaCppBackend` over `llama-cpp-2`. Verify single inference works against a small test GGUF. **Branch on Phase 0.5 Spike B outcome.**
- **M5.** DirectML feature detection: if available, load with DirectML; else CPU. Status surfaced via `Session::selection()`.
- **M6.** Prompt template structs — one per AI feature (UC-01, UC-04, UC-06 indexing). Each is a typed builder, not a `format!()` call. Test: snapshot test on rendered prompt for representative inputs.
- **M7.** UC-01 end-to-end: take an English sentence, return a `RequestDraft` struct that can be inserted into Minnal-store via the playground path. CLI test harness.
- **M8.** UC-04 end-to-end: take a stored `Response` + last 10 stored responses for the same `RequestId`, return an `Explanation` struct. CLI test harness.
- **M9.** Embedding pipeline for UC-06: text → 1024-dim vec, written to Minnal-store's vec index. Bench: 300 requests indexed in < 30s on dev machine.
- **M10.** Memory pressure handler: on `MemoryPressureDetected`, hold (do not unload) and emit dialog event. Integration test by simulated event injection.

#### Test strategy

- Mock backend lets every prompt template be tested without GPU/CPU cost.
- Real-model tests gated behind `--features real-model` and `--ignored` so CI does not need GGUF weights.
- Snapshot testing for prompt rendering — change-detection without LLM-output flakiness.
- The router is fully pure — exhaustively table-driven.

#### Done definition

Per Minnal.md §13 Minnal-ai checklist.

#### Open questions specific to this crate

- Embedding dimension: §5.1 schema declares `FLOAT[1024]`. Spike B must verify gemma-4-e4b's embedding dimension matches before Phase 1 closes; if it differs, V001 is updated *before* it ships (migrations are append-only post-ship).
- Backend abstraction: keep `ModelBackend` trait model-agnostic regardless of the locked default — swappability is an architectural invariant, not just defensive design.
- Prompt cache: should completed prompts cache their KV-state on disk to speed second-run latency? (Suggestion: defer to v1.1 — adds complexity, not needed for v1 budget.)

---

### 5.4 Minnal-hooks

**Owner**: Claude designs → Codex scaffolds → Arun integrates.
**Phase**: 3.
**Internal deps**: Minnal-core (for `RequestContext`), Minnal-ai (for hook generation, AI-in-hook capability).
**External deps (proposed)**: `wasmtime` (with `cranelift`), `wasmtime-wasi`, `wat`, `serde`, `tokio`, `thiserror`.

#### Modules

```
Minnal-hooks/
├── src/
│   ├── lib.rs
│   ├── error.rs                ← HookError
│   ├── runtime.rs              ← Wasmtime engine wrapper
│   ├── capabilities.rs         ← HookCapabilities (§6.3)
│   ├── lifecycle/
│   │   ├── pre.rs              ← synchronous, 5s hard timeout
│   │   └── post.rs             ← async, isolated failure
│   ├── context.rs              ← RequestContext / ResponseContext (passed to hooks)
│   ├── approval/
│   │   ├── dry_run.rs          ← sandboxed test execution
│   │   ├── review.rs           ← review payload (source, capabilities, est. timing)
│   │   └── store.rs            ← persist approval state via Minnal-store
│   ├── codegen/                ← AI-generated hook source → WASM
│   │   ├── generator.rs        ← calls Minnal-ai for source
│   │   └── compiler.rs         ← rustc wasm32-wasi pipeline (or wat assembler MVP)
│   └── audit.rs                ← every execution logged with timing + outcome
└── tests/
    ├── pre_timeout.rs
    ├── post_isolation.rs
    ├── capability_enforcement.rs
    └── approval_flow.rs
```

#### Public API surface (sketch)

```rust
pub struct HookEngine {
    wasmtime: wasmtime::Engine,
    capabilities_default: HookCapabilities,  // minimal
}

impl HookEngine {
    pub async fn run_pre(&self, hook: &ApprovedHook, ctx: RequestContext)
        -> Result<PreHookOutcome, HookError>;
    pub fn run_post(&self, hook: &ApprovedHook, req: RequestContext, resp: ResponseContext);
    /// Synchronous; returns review payload to surface in UI.
    pub async fn dry_run(&self, draft: &DraftHook, ctx: RequestContext)
        -> Result<DryRunReport, HookError>;
}

pub enum PreHookOutcome {
    Modified(RequestContext),
    Cancelled { reason: String },
    Passthrough,
}
```

#### Milestones

- **M1.** Wasmtime engine init + run a hello-world WASM module. Verify Cranelift JIT enabled.
- **M2.** `HookCapabilities` struct + capability-store wiring. Default = minimal (everything `false`, empty allowlist).
- **M3.** Pre-hook synchronous executor with **hard 5000ms timeout** enforced via `wasmtime::Store` epoch interruption. Test: a hook that loops forever is killed within 5050ms.
- **M4.** Post-hook async executor with isolated failure (post-hook panic must not affect main session). Test: post-hook that panics does not propagate.
- **M5.** Capability enforcement: a hook without `can_http_outbound=true` cannot make HTTP calls (verified by hostile-test hook). A hook with `allowed_hosts=["auth.example.com"]` cannot reach `evil.example.com`.
- **M6.** Dry-run pipeline: takes a `DraftHook`, executes against mock context, returns `DryRunReport { modifications, timing_ms, errors }`.
- **M7.** Approval flow integration with Minnal-store: approved hook → `hooks` table row with `approved_at` set; rejected → no trace.
- **M8.** AI hook generation (UC-03): English description → Rust source → WASM binary → review payload. **MVP path**: use `wat` (WebAssembly text format) directly for simple hook patterns; full `rustc wasm32-wasi` pipeline deferred to v1.1 if compile time is prohibitive.
- **M9.** Audit log: every execution writes to a `hook_executions` log table (add to V001 if not present, else V002 migration).

#### Test strategy

- Hostile test hooks for capability enforcement (one per granted capability — does it leak when not granted?).
- Integration test for the full UC-03 flow: English → generated hook → dry-run → approval → execution.
- Property test: pre-hook timeout invariant — for any timeout ≤ 5000ms and any hook duration > timeout, the hook is killed within timeout + 50ms.

#### Done definition

Per Minnal.md §13 Minnal-hooks checklist.

#### Open questions specific to this crate

- The MVP for codegen: emit `.wat` directly for known patterns (e.g. "fetch token, attach as Bearer" is a templated pattern), vs full `rustc wasm32-wasi` compilation. Suggest: templated `.wat` for v1 (faster, smaller surface, easier review); `rustc` pipeline for v1.1 when we want general-purpose generation.
- Should `can_ai_inference` capability allow recursive hook spawning? (Strongly suggest no — explicit Tier 3 / v3 feature, see Minnal.md §9 UC-11.)

---

### 5.5 Minnal-export

**Owner**: Codex CLI generates, Claude reviews.
**Phase**: 5.
**Internal deps**: Minnal-store, Minnal-core.
**External deps (proposed)**: `askama` or `tera` for HTML templates, `webview2-com` or shell-out to MS Edge for PDF, `rust_xlsxwriter`, `calamine` (only if we ever import Excel — not in v1).

#### Modules

```
Minnal-export/
├── src/
│   ├── lib.rs
│   ├── html/                   ← native template rendering
│   ├── pdf/                    ← HTML → WebView2 print path
│   ├── excel/                  ← rust_xlsxwriter
│   └── error.rs
└── templates/
    ├── collection.html
    └── request.html
```

#### Milestones

- **M1.** HTML export of a single collection — syntax-highlighted bodies (server-side via `syntect`), collection metadata header, request/response pairs.
- **M2.** PDF export via WebView2 print-to-PDF API. **Spike A outcome may affect this** — if WebView2 integration is fragile, fall back to a headless print invocation.
- **M3.** Excel export of request history: one sheet for requests, one for response history with status/latency/size columns.

#### Test strategy

- Snapshot test of rendered HTML against known-good fixtures.
- PDF byte-comparison is unreliable — assert PDF generated, file > 0 bytes, magic bytes match.
- Excel: open generated file with `calamine` in test, assert sheet structure.

#### Done definition

Per Minnal.md §13 Minnal-export checklist.

---

### 5.6 Minnal-ui

**Owner**: Codex CLI generates, Claude reviews. **UX vision authored by Claude** (this plan, §4).
**Phase**: 4.
**Internal deps**: Minnal-core, Minnal-ai, Minnal-hooks, Minnal-store.
**External deps (proposed)**: `windows` crate for native chrome, WebView2 runtime/control, `tokio`, a small localhost HTTP layer for UI assets/events.

This is the most uncertain crate because WinUI 3 + Rust is pioneering territory (Minnal.md CONTEXT.md HIGH risk #1). Phase 0.5 Spike A determines whether the plan below holds or whether we pivot.

#### Two possible architectures (decision after Spike A)

**Decision (2026-05-02): Path B locked.** Spike A found no usable `windows-app-sdk` Rust crate and the dev machine only has Windows App Runtime 1.2 installed, below the 1.6 target. Pure Rust/WinUI remains too fragile for v1.

**Path A — Pure WinUI 3 + Rust (preferred).**
XAML files compiled to WinRT IDL, Rust code-behind via `windows` crate. Build pipeline includes a custom `build.rs` that runs `xamlcompiler.exe`.

**Path B — WinUI 3 chrome + WebView2 content (fallback).**
XAML for window chrome only; main content rendered as a single WebView2 hosting a Rust-served localhost UI (TS+HTMX or Yew or Leptos). Heavier, but de-risked.

If Path A succeeds in spike: proceed with §5.6 modules below.
If Path A fails: pivot. New §5.6 plan authored against Path B before any Phase 4 work begins.

#### Legacy Modules (Path A, superseded by Path B decision)

```
Minnal-ui/
├── src/
│   ├── lib.rs                  ← App entry
│   ├── shell/                  ← MainWindow, layout, theme
│   ├── intent_bar/             ← Ctrl+K command surface (§4.2 #1)
│   ├── canvas/                 ← Semantic canvas (§4.2 #2)
│   ├── request_surface/        ← Centre pane (§4.2 #3)
│   ├── response_thread/        ← Right pane (§4.2 #4)
│   ├── status_strip/           ← Bottom bar (§4.2 #5)
│   ├── playground/             ← Ephemeral tab (§4.3)
│   ├── auth_flow/              ← Auth-as-graph popover
│   ├── hook_review/            ← Approval UI (per §6.4)
│   ├── why_panel/              ← AI reasoning surface (§4.4)
│   └── theme/                  ← Dark-first, monospace data, sans chrome
├── xaml/                       ← all XAML files, one per surface
└── assets/
    ├── icons/
    └── fonts/                  ← Cascadia Code / Inter (or system)
```

#### Modules (Path B)

```
Minnal-ui/
  src/
    lib.rs          <- App entry
    shell/          <- native window, WebView2 host, theme bridge
    bridge/         <- Rust <-> WebView2 message contract
    server/         <- localhost static assets + command/event endpoints
    state/          <- UI session state projected from core/store/ai
    assets.rs       <- embedded asset lookup/versioning
  web/
    app/            <- intent bar, canvas, request surface, response thread
    components/     <- shared workbench controls
    styles/         <- dark-first, monospace data, sans chrome
    contracts/      <- typed DTOs shared with bridge
  xaml/             <- minimal native chrome only
  assets/
    icons/
    fonts/
```

#### Milestones

- **M1.** Native MainWindow hosts WebView2 and serves the static workbench from localhost. Cold-start measured.
- **M2.** Bridge contract wired: WebView2 can invoke a Rust stub and receive structured responses.
- **M3.** Semantic Canvas reading collections from Minnal-store, rendering by *folder structure* first (parity with Postman import), then iteratively replaced by embedding-based clusters when Minnal-ai is available.
- **M4.** Request Surface with method/URL/body/headers — submitting calls Minnal-core.
- **M5.** Response Thread with run-history per request from Minnal-store, "Why" affordance calling Minnal-ai UC-04.
- **M6.** Status Strip with model selection + offline indicator + memory pressure subscription to Minnal-ai's event stream.
- **M7.** Playground tab — ephemeral, with explicit Promote action; verifies zero SQLite writes until Promote (integration test).
- **M8.** Auth Flow popover for the seven schemes.
- **M9.** Hook Review pane: source code, capability list, dry-run output, approve/reject buttons.
- **M10.** "Why" panel pluggable across surfaces.
- **M11.** Cold-start budget (`< 2s no model`, `< 8s with model mmap`) measured & enforced as CI assertion.
- **M12.** Idle-RAM budget (`< 100MB no model`, `< 10GB with Q8 loaded on 32GB machine`) measured & documented.

#### Test strategy

- Manual UI verification — automate where reasonable (WinAppDriver if it works in Rust context).
- Integration tests for the *non-visual* contracts: cold-start timer, idle-RAM budget, Playground-no-SQLite-writes property.
- Snapshot tests on XAML rendered output where possible.

#### Done definition

Per Minnal.md §13 Minnal-ui checklist.

#### Open questions specific to this crate

- WinAppDriver + Rust: is automated UI testing tractable? (Spike A may answer.)
- Cluster naming: when embeddings group requests, who names the cluster? Suggestion: gemma summarises the cluster ("Auth", "User Management"); user can rename and the rename persists per-user.
- Theme: confirm accent colour (`#FF6B1A` proposal). Arun decides.

---

## 6. Phase Acceptance Gates

Phase exits only when **every** box is green. No exceptions; this is the review gate.

### Phase 0 exit
- [ ] Repo created on GitHub, name confirmed.
- [ ] Cargo workspace builds clean: `cargo build --workspace` zero warnings.
- [ ] Each crate compiles independently.
- [ ] CI runs on Windows, green on first push.
- [ ] `.gitignore` enforced (model weights, SQLite files, secrets).
- [ ] Apache 2.0 licence header in every `lib.rs`.

### Phase 0.5 exit
- [ ] Spike A: WinUI 3 window with tab + split pane runs on dev machine. Decision A or B locked in writing.
- [ ] Spike B: llama.cpp loads a small GGUF, runs one inference, DirectML detection logged. Embedding dimension confirmed against Minnal-store schema.
- [ ] sqlite-vec extension loads on dev machine, OR fallback (`hnsw_rs`) selected.
- [ ] Both spike outcomes recorded in CONTEXT.md with date.

### Phase 1 exit
- [ ] Minnal-store: every M1–M8 milestone closed.
- [ ] Minnal-core: every Minnal.md §13 core checkbox closed.
- [ ] Property tests passing in CI.
- [ ] Coverage of every auth scheme demonstrated end-to-end against test endpoints.

### Phase 2 exit
- [ ] Minnal-ai M1–M10 closed.
- [ ] UC-01 and UC-04 demonstrated via CLI test harness.
- [ ] UC-06 indexing pipeline runs in budget (<30s for 300 requests).
- [ ] Memory pressure dialog verified (synthetic event).

### Phase 3 exit
- [ ] Minnal-hooks M1–M9 closed.
- [ ] UC-03 demonstrated end-to-end through CLI/test harness.
- [ ] Capability enforcement verified by hostile tests.
- [ ] Pre-hook timeout invariant property test passing.

### Phase 4 exit
- [ ] Minnal-ui M1–M12 closed.
- [ ] v1 invariant scenario walkable in the UI.
- [ ] Cold-start and idle-RAM budgets measured and within target on dev machine.
- [ ] Five surfaces (§4.2) all functional; no Postman-clone affordances present.

### Phase 5 exit
- [ ] Minnal-export M1–M3 closed.
- [ ] MSIX produced, self-signed, installs on fresh Win11 VM.
- [ ] First-run model download flow works.
- [ ] README + install docs complete.

### Phase 6 exit
- [ ] Every box in Minnal.md §13 ticked.
- [ ] v1 invariant scenario passes on 16GB Win11 VM and 32GB dev machine.
- [ ] `v0.1.0` tagged.

---

## 7. Workflow Cadence

### Per-task ritual
1. **Claude design pass** (when crate is Claude-designed): produces a short `design/<crate>/<feature>.md` with API surface, types, test strategy. Reviewed by Arun.
2. **Codex scaffold pass**: produces code matching the design. PR opened.
3. **Claude review gate**: PR-fail on any §14 violation (no `unwrap()`, no stringly-typed, etc.).
4. **Arun integrates**: fixes review feedback or escalates.
5. **CI green** + **manual exercise** (where UX is involved) before merge.

### Session protocol (per CLAUDE.md)
- Start: load CLAUDE.md + CONTEXT.md.
- End: append Session Summary block to CONTEXT.md.

### Update plan.md when
- A spike outcome flips a decision branch.
- A milestone discovers a missing module that affects another crate.
- An open question (🔍) is resolved.

### Do not update plan.md when
- A milestone slips (that's CONTEXT.md).
- A bug is found and fixed (that's the commit).
- A small refactor happens (that's the diff).

---

## 8. Risk Register (extends CONTEXT.md)

```
[HIGH]    WinUI 3 + Rust pioneering risk.
          Mitigation: Phase 0.5 Spike A. Path-B fallback documented (§5.6).
          Trigger to escalate: spike fails after 2 days of effort.

[HIGH]    llama.cpp + DirectML on Windows.
          Mitigation: Phase 0.5 Spike B. CPU fallback documented in router.
          Trigger to escalate: DirectML backend exposes no Intel Xe path
                               in llama-cpp-2 binding.

[MEDIUM]  Embedding dimension mismatch with V001 schema (FLOAT[1024]).
          Mitigation: Phase 0.5 Spike B measures gemma-4-e4b embedding
                      dimension first; V001 is updated before it ships
                      if the dim differs.
          Trigger to escalate: dim differs *and* downstream consumers
                               (sqlite-vec / hnsw_rs) cannot tolerate the
                               new dim without index rebuild cost.

[MEDIUM]  sqlite-vec Windows build fragility.
          Mitigation: Phase 0.5 sqlite-vec spike. hnsw_rs fallback behind
                      same trait — no API change.
          Trigger to escalate: sqlite-vec fails to load *and* hnsw_rs
                               cannot match recall on representative data.

[MEDIUM]  Wasmtime AI-generated hook compile time (rustc wasm32-wasi).
          Mitigation: §5.4 M8 — templated .wat for v1; rustc deferred.
          Trigger to escalate: templated .wat patterns insufficient for
                               UC-03 demonstrated cases.

[MEDIUM]  Cold-start budget < 2s on 16GB floor machine.
          Mitigation: measure early in Phase 4, optimise lazy load paths.
          Trigger to escalate: Phase 4 M11 measurement exceeds 3s.

[LOW]     OAuth 2.0 PKCE local redirect server port conflicts.
          Mitigation: ephemeral port + fallback list (Minnal.md CONTEXT.md).

[LOW]     MSIX self-signed SmartScreen warning friction.
          Mitigation: README install bypass docs; EV cert deferred.
```

---

## 9. What This Plan Does Not Cover

Out of scope for v1 (acknowledged, not forgotten):

- **Tier 2 features** (UC-07 dependency graph, UC-08 contract learning, UC-09 API memory, UC-10 collection diff). Schema already supports these; UI surface is the next iteration.
- **Tier 3 features** (UC-11 AI-in-hook, UC-12 multi-request chains, UC-13 spec generation, UC-14 test gen, UC-15 git collaboration). Architectural hooks left in (capability flag for AI-in-hook exists), but no implementation.
- **Cross-platform.** v1 is Windows-only by deliberate choice. macOS/Linux is a v2+ conversation.
- **Telemetry / usage analytics.** Banned by §13. Even opt-in is deferred to v1.1.
- **Cloud sync.** Banned by the v1 invariant. May be considered v3+ as an opt-in BYOC (bring your own cloud) feature.

---

## 10. Handover Notes (for the next AI / engineer continuing this plan)

- Source of truth is **Minnal.md**. This plan refines but never overrules it.
- 🔍 marked decisions are **blockers** — clear them with Arun before relevant phase begins.
- ⚠️ marked decisions are **revisit triggers** — re-evaluate when the trigger fires.
- Per-crate plans assume crate ownership per Minnal.md §14. **Do not generate code in Minnal-core** without explicit Arun request.
- The "new blood" UI section (§4) is the most subjective. Discuss with Arun before deviating.
- Token economy: this plan was written under a Pro-tier weekly token constraint. Future sessions should batch updates rather than micro-edit.

---

*plan.md v0.1 — Implementation plan signed.*
*Companion to Minnal.md (architecture) and CONTEXT.md (state).*
*Never Settle.*
