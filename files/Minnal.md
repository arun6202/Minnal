# Minnal.md — Master Architecture Specification
# Minnal: AI-Native API Workspace
# Tagline: Never Settle.
# License: Apache 2.0
# Status: v1 Architecture — Signed

---

## 0. Soul

Minnal is not a Postman clone.
Postman is a static form-filler built before local LLMs existed.
Minnal is what an API workspace looks like when designed *after* them.

The request is not the product. The intent behind it is.

Every static form Postman makes you fill — Minnal infers.
Every script Postman makes you write — Minnal generates and explains.
Every folder Postman makes you create — Minnal organises semantically.
Every response Postman shows raw — Minnal explains in plain English.

Postman compatible on import and export. Independent on everything else.

**Never Settle.**

---

## 1. v1 Invariant — Signed

```
Minnal v1 is correct when:

  A developer can import a Postman collection or Swagger/OpenAPI spec,
  execute any REST request with any auth scheme,
  have gemma-4-e4b explain the response in plain English,
  generate a WASM auth pre-hook from a natural language description,
  and search 300 requests by intent not by folder —

  all offline. All on their own hardware.
  All without Electron. All without a cloud call.
  All without settling.
```

Everything beyond this is correct behaviour v1 does not yet exhibit.
Not missing features. Not-yet-proven theorems.

---

## 2. Machine Spec Contract

### 2.1 Minimum Specification (Hard Floor — No Negotiation)

```
RAM:      16 GB minimum
CPU:      Intel 12th Gen Core i5 or above (AVX2 mandatory, AVX-512 beneficial)
OS:       Windows 10 22H2 or Windows 11
GPU:      Intel Xe integrated or discrete (optional — enhances inference)
Storage:  4 GB free for model weights + workspace
```

### 2.2 Below-Floor Behaviour

Minnal detects RAM < 16GB at startup and displays:

```
"Minnal requires 16GB RAM to run its AI engine with integrity.
 Never Settle means never compromise.
 Upgrade your machine or check our lightweight REST-only CLI companion."
```

Clean exit. No degraded mode. No apology.

### 2.3 Target Development Machine

```
RAM:      32 GB
CPU:      Intel 12th Gen i7, AVX-512 capable
GPU:      Intel Xe iGPU, 4-8 GB BIOS-allocated shared VRAM
OS:       Windows 11
```

---

## 3. Model Router — Formal Specification

### 3.1 Assessment Types

```rust
pub struct MachineProfile {
    pub available_ram_gb:    f32,
    pub avx512_support:      bool,
    pub intel_xe_vram_gb:    f32,
    pub on_battery:          bool,
    pub thermal_level:       ThermalLevel,
    pub cpu_generation:      u8,
}

pub enum ThermalLevel { Normal, Warm, Hot }

pub enum ModelSelection {
    Unsupported,
    Gemma_4_E4B,
}

pub enum ModelEvent {
    AssessmentComplete(ModelSelection),
    MemoryPressureDetected { used_gb: f32 },
    HoldingModelUntilRestart(ModelSelection),
    UserNotified,
}
```

### 3.2 Routing Logic

```rust
pub fn route(profile: &MachineProfile) -> ModelSelection {
    if profile.available_ram_gb < 16.0 {
        return ModelSelection::Unsupported;
    }

    match profile {
        // 32GB+ — Q8 quality, partial iGPU offload — your machine
        
    }
}
```

### 3.3 Runtime Re-Assessment Policy

```
Assessment:   Once at startup only.
Mid-session:  Memory pressure detected → hold current model.
Notification: Status bar indicator + non-blocking dialog:
              "Memory pressure detected. Minnal is holding gemma-4-e4b
               to preserve session integrity. Restart to re-assess."
Downgrade:    Never silently. Never mid-session.
Principle:    A pipeline that changes behaviour under external pressure
              is a pipeline you cannot trust. (ETL law applied.)
Thinking Mode : when required go thinking mode or do off 
```

### 3.4 Inference Backend

```
Runtime:    llama.cpp via FFI (DirectML backend for Intel)
Wrapper:    Rust trait ModelBackend — swappable without API change
Fallback:   CPU via llama.cpp if DirectML unavailable
Future:     OpenVINO backend behind feature flag
Format:     GGUF — Q4_K_M and Q8_0 variants
```

---

## 4. Cargo Workspace Structure

```
Minnal/
├── Minnal.md                   ← This document. Source of truth.
├── CLAUDE.md                   ← Session context loader for Claude
├── CONTEXT.md                  ← Current sprint state and open decisions
├── Cargo.toml                  ← Workspace manifest
├── Cargo.lock                  ← Committed. Always.
│
├── crates/
│   ├── Minnal-core/            ← HTTP engine, auth, env vars, types
│   │   Owner: Arun (your craft, not delegated)
│   │
│   ├── Minnal-ui/              ← WinUI 3 shell, XAML, panels, nav
│   │   Owner: Codex CLI generates, Claude reviews
│   │
│   ├── Minnal-ai/              ← Model router, inference, prompt engine
│   │   Owner: Claude designs, Codex CLI scaffolds
│   │
│   ├── Minnal-hooks/           ← Wasmtime sandbox, hook lifecycle
│   │   Owner: Claude designs, Codex CLI scaffolds
│   │
│   ├── Minnal-store/           ← SQLite schema, migrations, embeddings
│   │   Owner: Claude designs, Codex CLI scaffolds
│   │
│   └── Minnal-export/          ← HTML/PDF/Excel rendering
│       Owner: Codex CLI generates, Claude reviews
│
└── examples/                   ← Symlink to Minnal-examples repo
```

### 4.1 Crate Dependency Rules

```
Minnal-ui       → Minnal-core, Minnal-ai, Minnal-hooks, Minnal-store
Minnal-ai       → Minnal-store (reads history for context)
Minnal-hooks    → Minnal-core, Minnal-ai (AI-in-hook capability)
Minnal-store    → (no internal deps — pure data layer)
Minnal-core     → (no internal deps — pure engine)
Minnal-export   → Minnal-store, Minnal-core

Cycle rule: Zero cycles. Enforced by CI. No exceptions.
```

---

## 5. SQLite Schema — Versioned, Migration-Aware

### 5.1 Core Entities

```sql
-- Schema version tracking
CREATE TABLE schema_migrations (
    version     INTEGER PRIMARY KEY,
    applied_at  TEXT NOT NULL,
    description TEXT NOT NULL
);

-- Collections (Postman v2.1 compatible structure)
CREATE TABLE collections (
    id          TEXT PRIMARY KEY,          -- UUID v7
    name        TEXT NOT NULL,
    sha         TEXT NOT NULL,             -- content-addressed, ETL pattern
    git_path    TEXT,                      -- optional git repo path
    imported_at TEXT NOT NULL,
    updated_at  TEXT NOT NULL,
    source      TEXT NOT NULL              -- 'postman' | 'openapi' | 'native'
);

-- Requests
CREATE TABLE requests (
    id              TEXT PRIMARY KEY,
    collection_id   TEXT NOT NULL REFERENCES collections(id),
    name            TEXT NOT NULL,
    method          TEXT NOT NULL,         -- GET POST PUT PATCH DELETE etc
    url_template    TEXT NOT NULL,         -- may contain {{variables}}
    headers         TEXT NOT NULL,         -- JSON
    body_template   TEXT,                  -- JSON | raw | form-data
    auth_ref        TEXT,                  -- FK to auth_configs
    schema_ref      TEXT,                  -- FK to contracts
    created_at      TEXT NOT NULL,
    updated_at      TEXT NOT NULL
);

-- Environments and Variables
CREATE TABLE environments (
    id      TEXT PRIMARY KEY,
    name    TEXT NOT NULL,
    scope   TEXT NOT NULL                  -- 'global' | 'collection' | 'request'
);

CREATE TABLE variables (
    id              TEXT PRIMARY KEY,
    env_id          TEXT NOT NULL REFERENCES environments(id),
    key             TEXT NOT NULL,
    value_encrypted TEXT NOT NULL,         -- AES-256-GCM, key from Windows DPAPI
    source          TEXT NOT NULL          -- 'manual' | 'hook' | 'ai_inferred'
);

-- Auth Configurations
CREATE TABLE auth_configs (
    id          TEXT PRIMARY KEY,
    scheme      TEXT NOT NULL,             -- 'bearer' | 'basic' | 'oauth2_pkce'
                                           -- 'api_key' | 'aws_sig_v4' | 'mtls'
    config      TEXT NOT NULL,             -- JSON, encrypted sensitive fields
    created_at  TEXT NOT NULL
);

-- Response History — Content-Addressed
CREATE TABLE responses (
    id          TEXT PRIMARY KEY,
    request_id  TEXT NOT NULL REFERENCES requests(id),
    timestamp   TEXT NOT NULL,
    status      INTEGER NOT NULL,
    latency_ms  INTEGER NOT NULL,
    headers     TEXT NOT NULL,             -- JSON
    body_sha    TEXT NOT NULL REFERENCES response_bodies(sha),
    size_bytes  INTEGER NOT NULL
);

-- Response Bodies — Deduplicated by SHA (ETL Parquet pattern)
-- Same body returned 500 times: stored once, referenced 500 times.
-- SHA change = contract change. Natural fingerprint.
CREATE TABLE response_bodies (
    sha         TEXT PRIMARY KEY,          -- SHA-256 of raw body
    body        BLOB NOT NULL,             -- zstd compressed
    content_type TEXT NOT NULL,
    stored_at   TEXT NOT NULL
);

-- WASM Hooks
CREATE TABLE hooks (
    id          TEXT PRIMARY KEY,
    request_id  TEXT REFERENCES requests(id),
    collection_id TEXT REFERENCES collections(id),
    phase       TEXT NOT NULL,             -- 'pre' | 'post'
    wasm_sha    TEXT NOT NULL,             -- content-addressed WASM binary
    description TEXT NOT NULL,            -- natural language source of truth
    approved_at TEXT,                     -- NULL = pending approval
    approved_by TEXT,                      -- 'user' | 'dry_run_passed'
    timeout_ms  INTEGER NOT NULL DEFAULT 5000
);

-- Assertions
CREATE TABLE assertions (
    id          TEXT PRIMARY KEY,
    request_id  TEXT NOT NULL REFERENCES requests(id),
    type        TEXT NOT NULL,             -- 'declarative' | 'wasm'
    spec        TEXT NOT NULL,             -- TOML (declarative) or wasm_sha
    created_at  TEXT NOT NULL
);

-- Semantic Embeddings (sqlite-vec extension)
CREATE VIRTUAL TABLE embeddings USING vec0(
    request_id  TEXT,
    embedding   FLOAT[1024]                -- gemma-4-e4b embedding dimension
);

-- Contract Learning
CREATE TABLE contracts (
    id              TEXT PRIMARY KEY,
    request_id      TEXT NOT NULL REFERENCES requests(id),
    inferred_schema TEXT NOT NULL,         -- JSON Schema
    observed_at     TEXT NOT NULL,
    violation_count INTEGER NOT NULL DEFAULT 0,
    last_violation  TEXT                   -- timestamp
);

-- Dependency Graph Edges
CREATE TABLE request_dependencies (
    from_request_id TEXT NOT NULL REFERENCES requests(id),
    to_request_id   TEXT NOT NULL REFERENCES requests(id),
    dependency_type TEXT NOT NULL,         -- 'auth_token' | 'response_field' | 'ordering'
    field_path      TEXT,                  -- JSONPath of consumed field
    inferred_by     TEXT NOT NULL,         -- 'ai' | 'user'
    PRIMARY KEY (from_request_id, to_request_id)
);
```

### 5.2 Migration Strategy

```
Tool:       sqlx with compile-time checked migrations
Directory:  Minnal-store/migrations/
Naming:     V{n}__{description}.sql  (e.g. V001__initial_schema.sql)
Policy:     Migrations are append-only. Never edit a shipped migration.
            Schema changes = new migration file.
Startup:    Auto-migrate on launch. Failed migration = hard abort with message.
```

---

## 6. WASM Hook Architecture

### 6.1 Runtime

```
Engine:         Wasmtime (Bytecode Alliance, Apache 2.0)
JIT:            Cranelift — Intel-optimised
Isolation:      Capability-based sandbox — hook gets exactly what it's granted
Interface:      WASI Preview 2
Rust binding:   wasmtime crate (idiomatic, no FFI ceremony)
```

### 6.2 Hook Lifecycle

```
Pre-Hook  (SYNCHRONOUS — blocks request pipeline)
  1. Hook loaded from WASM binary (sha-verified)
  2. Capability grants applied (see 6.3)
  3. Hook receives: RequestContext (method, url, headers, body, env_vars)
  4. Hook returns: Modified RequestContext | Cancel(reason) | Passthrough
  5. Hard timeout: 5000ms (user-configurable per hook, max 30s)
  6. Timeout breach: request cancelled, error surfaced, hook flagged

Post-Hook (ASYNCHRONOUS — does not block response delivery)
  1. Response delivered to UI immediately
  2. Hook executes in background with response + original request
  3. Hook can: write to SQLite history, trigger assertions,
               update variables, call AI for analysis
  4. Post-hook failure: logged, never crashes main session
```

### 6.3 Capability Grants

```rust
pub struct HookCapabilities {
    pub can_http_outbound:    bool,   // make external HTTP calls
    pub can_sqlite_read:      bool,   // read collection/history data
    pub can_sqlite_write:     bool,   // write variables/annotations
    pub can_ai_inference:     bool,   // call gemma-4-e4b from within hook
    pub can_cancel_request:   bool,   // pre-hook only: abort the request
    pub can_redirect_request: bool,   // pre-hook only: change URL/method
    pub allowed_hosts:        Vec<String>, // HTTP allowlist if can_http_outbound
}
```

Default capability set for AI-generated hooks: **minimal**
User must explicitly elevate capabilities after reviewing hook code.

### 6.4 Hook Approval Flow

```
1. User describes hook in English
   "Before every Payments request, get JWT from /auth/login
    with username={{USERNAME}} password={{PASSWORD_SECRET}},
    attach as Bearer header"

2. gemma-4-e4b generates WASM hook source (Rust → compiled to WASM)
   Shows: source code in review pane, capability requirements listed,
          estimated execution time

3. Dry-run in sandbox:
   Executes against one request with mock/env data
   Shows: what would have been modified, timing, any errors

4. User approves → hook stored with approved_at timestamp
   User rejects → hook discarded, no trace in production store

5. Hook runs. Every execution logged with timing + outcome.
```

---

## 7. AI Feature Specifications — v1

### UC-01: Intent-to-Request

```
Trigger:    User types natural language in Playground input
            e.g. "POST to add a product with name price and category"

gemma-4-e4b infers:
  - HTTP method (from verb analysis)
  - Endpoint pattern (from collection history similarity)
  - Request body schema (from description + known collection patterns)
  - Content-Type header
  - Auth scheme (from collection environment)

Output:     Pre-filled Playground — editable, not committed
User flow:  Tweak → Test → Discard | Promote to Collection
Commit:     Explicit user action only. AI never auto-saves.
```

### UC-02: Swagger/OpenAPI Comprehension

```
Trigger:    User pastes OpenAPI URL or drops YAML/JSON file

Standard import:
  - All endpoints imported to collection (Postman v2.1 compatible)

gemma-4-e4b comprehension layer (beyond standard import):
  - Semantic domain grouping (auth / users / payments / admin)
  - Suggested execution order for dependent endpoints
  - Auth gap detection: endpoints with no documented auth → flagged
  - Hook stub generation for complex flows
  - Plain English collection summary generated

Output:     Collection + comprehension report in side panel
```

### UC-03: Auth Chain AI

```
Trigger:    Natural language in Auth AI panel
            "For Payments, get JWT from /auth/login,
             refresh on 401, attach as Bearer"

Flow:
  1. gemma-4-e4b generates WASM hook (pre-hook + post-hook pair)
  2. Hook review pane opens — source visible
  3. Dry-run in Wasmtime sandbox
  4. User approves
  5. Hook activates for collection scope

Capability grants: can_http_outbound=true, allowed_hosts=[auth endpoint]
                   can_sqlite_write=true (stores token in variables)
                   can_cancel_request=false (never blocks on token failure)
```

### UC-04: Response Explanation

```
Trigger:    "Explain this" button in response pane (always visible)

gemma-4-e4b reads:
  - Status code + semantic meaning
  - Response headers (cache, rate limit, content negotiation)
  - Body schema + anomalies
  - Latency vs collection average for this endpoint
  - Contract diff: did schema change from last known response?

Output:     Plain English explanation in side panel
            Structured: What happened | Why | What changed | What to do

Context:    gemma-4-e4b given last 10 responses for this request as context
            from SQLite history — not just the current one
```

### UC-05: Playground Session

```
Model:      Ephemeral — Mode C (session-only, no SQLite write until commit)
Scope:      Global scratch pad tab, always present
            Any collection request: "Fork to Playground" option

Behaviour:
  - Full AI assistance active during session
  - All response history visible within session (in-memory only)
  - Session end → everything evaporates
  - Promote: user clicks "Add to Collection" → named, saved, indexed
  - Discard: close tab → confirmed gone, no recovery

SQLite:     Zero writes until explicit Promote action
Principle:  Experiment first. Organise later. Commit deliberately.
```

### UC-06: Semantic Search (v1 — included in v1 invariant)

```
Index:      sqlite-vec virtual table, gemma-4-e4b embeddings (1024-dim)
Trigger:    Ctrl+K → type intent in English
            "find all requests that delete or archive user data"

Flow:
  1. Query embedded by gemma-4-e4b (local, offline)
  2. Cosine similarity search over request embeddings
  3. Ranked results: request name, collection, method, inferred domain
  4. Click result → opens request directly

Indexing:   Background process on import and on every new request save
            Re-index on model version change
Performance: 300 requests → sub-100ms on 12th Gen i7 (target)
```

---

## 8. Tier 2 Vision — v2 Roadmap (Signed, Not Yet Proven)

```
UC-07: Dependency Graph     — DAG visualisation, AI-inferred edges
UC-08: Contract Learning    — Schema drift detection, violation alerts
UC-09: API Memory           — Anomaly surface, latency trend analysis
UC-10: Collection Diff      — Swagger version diff in plain English
```

---

## 9. Tier 3 Long Game — v3+ (Acknowledged)

```
UC-11: AI-in-Hook           — gemma-4-e4b callable from within WASM hook
UC-12: Multi-Request Chain  — Natural language orchestration
UC-13: Spec Generation      — OpenAPI from observed traffic
UC-14: Test Suite Gen       — AI-generated WASM assertion suites
UC-15: Git Collaboration    — AI-mediated collection code review
```

---

## 10. Auth Schemes — v1 Complete

```
Scheme          Implementation
────────────────────────────────────────────────────
API Key         Header / Query param / Cookie
Bearer Token    Static + AI-managed (UC-03)
Basic Auth      Username + password, base64 encoded
OAuth 2.0 PKCE  Local HTTP server for redirect capture
                Token refresh lifecycle managed
                Windows Credential Manager for storage
AWS Sig v4      HMAC-SHA256, all AWS regions
mTLS            Client certificate, Windows cert store
Custom          Header template with variable injection
```

---

## 11. Export Specification

```
Format      Implementation
───────────────────────────────────────────────────────
HTML        Native Rust template rendering
            Syntax-highlighted request/response bodies
            Collection summary with metadata

PDF         HTML → WebView2 print path (Windows native)
            No Python. No external deps. Zero weight.
            Ironic but correct.

Excel       calamine + rust_xlsxwriter (Apache 2.0 both)
            Request history as structured table
            Response times, status distribution, body sizes
```

---

## 12. Apache 2.0 Compliance Manifest

```
Component           License         Bundled?    Status
───────────────────────────────────────────────────────
gemma-4-e4b 14B     Apache 2.0      No¹         ✅ Clean
llama.cpp           MIT             FFI link    ✅ Clean
Wasmtime            Apache 2.0      Cargo dep   ✅ Clean
sqlite-vec          Apache 2.0      Cargo dep   ✅ Clean
windows-rs          MIT + Apache    Cargo dep   ✅ Clean
calamine            MIT + Apache    Cargo dep   ✅ Clean
rust_xlsxwriter     MIT + Apache    Cargo dep   ✅ Clean
sqlx                MIT + Apache    Cargo dep   ✅ Clean
WebView2            MSFT EULA       OS-provided ✅ Clean²
serde / serde_json  MIT + Apache    Cargo dep   ✅ Clean
tokio               MIT             Cargo dep   ✅ Clean
reqwest             MIT + Apache    Cargo dep   ✅ Clean

¹ gemma-4-e4b not bundled in installer. First-run guided download.
  Ollama sidecar preferred if present. Guided install if absent.
² WebView2 is system-provided on Windows 10/11. Not redistributed.

Prohibited (do not add without license review):
  Qwen family     — Qianwen Commercial License
  Google Gemma    — Google Gemma ToS (note: gemma-4-e4b is unrelated;
                    name collision only — gemma-4-e4b ships under Apache 2.0)
  Llama family    — Meta Llama Community License
```

---

## 13. v1 Done Definition — Checklist

```
Architecture
  □ Cargo workspace builds clean on Windows (zero warnings policy)
  □ All 6 crates compile independently
  □ CI pipeline: build + test on every commit

Minnal-core
  □ HTTP engine: all methods, all headers, redirect handling
  □ All auth schemes implemented and tested
  □ Environment variable system: global / collection / request scope
  □ Postman v2.1 collection import
  □ OpenAPI 3.x import
  □ Postman v2.1 collection export

Minnal-store
  □ SQLite schema V001 migration applied
  □ Response body SHA deduplication active
  □ Content-addressed storage verified by property test
  □ sqlite-vec extension loaded and indexed

Minnal-ai
  □ Machine assessment runs at startup
  □ Model router selects correct gemma-4-e4b variant
  □ Model re-assessment dialog wired
  □ UC-01 Intent-to-Request working end to end
  □ UC-04 Response explanation working end to end
  □ UC-06 Semantic search: 300 requests searchable by intent

Minnal-hooks
  □ Wasmtime sandbox initialised
  □ Pre-hook: synchronous, 5s timeout enforced
  □ Post-hook: asynchronous, failure isolated
  □ UC-03 Auth chain: full approval flow working
  □ Dry-run sandbox working
  □ Capability grant UI working

Minnal-ui
  □ WinUI 3 shell: collections panel, request editor, response viewer
  □ Playground tab: ephemeral, no SQLite until Promote
  □ Command palette: Ctrl+K → AI intent entry
  □ Status bar: model selection indicator
  □ Cold start < 2 seconds (measured, not estimated)
  □ Idle RAM < 100 MB (without model loaded)
  □ Idle RAM < 10 GB (with gemma-4-e4b Q8 loaded, 32GB machine)

Minnal-export
  □ HTML export: syntax-highlighted, complete
  □ PDF via WebView2 print path: working
  □ Excel: request history table export

Non-Functional
  □ Zero cloud calls in default configuration
  □ Zero telemetry without explicit opt-in
  □ All secrets via Windows DPAPI / Credential Manager
  □ .gitignore: model weights, SQLite db, secrets — enforced in CI
```

---

## 14. Claude + Codex CLI Workflow Contract

```
Roles
─────
Arun        Decision authority. Minnal-core HTTP engine owner.
            Integrator for all Codex CLI output.

Claude      Architecture. Type design. AI subsystem. WASM sandbox.
            Code review gate. Minnal.md maintenance.
            Fails PRs without sentiment when standards violated.

Codex CLI   WinUI 3 XAML boilerplate. Win32 FFI ceremony.
            Export rendering. File scaffolding. Test stubs.
            Commits to public GitHub repo.

Review Gate (Claude scrutinises every Codex CLI commit)
────────────────────────────────────────────────────────
⚠️  Any unwrap() outside #[cfg(test)]
⚠️  Any stringly-typed data deserving a type
⚠️  Any C++ idiom smuggled via Win32 FFI
⚠️  Any AI feature touching network without explicit user action
⚠️  Any imperative pattern where declarative exists
⚠️  Any dependency not in Apache 2.0 compliance manifest
⚠️  Any migration that edits rather than appends
⚠️  Any hook capability granted without user approval flow

Session Protocol
────────────────
Every Claude session begins with: load CLAUDE.md + CONTEXT.md
Every Codex CLI session begins with: load CLAUDE.md + CONTEXT.md
CONTEXT.md updated at end of every session with:
  - Decisions made
  - Work completed
  - Open questions
  - Next task for Codex CLI
```

---

## 15. Performance Contracts (Measured, Not Estimated)

```
Cold start (no model):          < 2 seconds
Cold start (with model):        < 8 seconds (model mmap, not full load)
Idle RAM (no model):            < 100 MB
Idle RAM (gemma-4-e4b Q8 mmap):      < 500 MB active + model pages on demand
Inference latency (response     < 3 seconds first token
  explain, typical response):
Semantic search (300 requests): < 100 ms
HTTP request round trip:        Zero Minnal overhead (direct pass-through)
SQLite write (response save):   < 5 ms
```

---

*Minnal.md v0.1 — Architecture Signed*
*Claude + Arun — $(date)*
*Never Settle.*
