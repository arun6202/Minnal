# phase-0-and-0.5.md
# Concrete task scripts for the Foundation + Spike phases.
# Audience: Codex CLI (or Arun) executing on Windows.
# Authority: plan.md §2 (Phase Map) · plan.md §6 (Phase Exit Gates).
# Working directory: E:\Arun\Workspace\Minnal\

---

## 0. Prerequisites — verify before running anything

```powershell
# All four MUST print version output. If any fails, STOP and install.
git --version
gh --version                   # GitHub CLI; must be authenticated: gh auth status
rustc --version                # 1.80+ recommended
cargo --version
```

Optional (used in spikes only):

```powershell
winget list --id Microsoft.WindowsAppSDK     # Windows App SDK 1.6+
```

If `gh auth status` is unauthenticated:

```powershell
gh auth login --web --git-protocol https
```

---

## Phase 0 — Foundation

Goal: empty workspace that builds clean, pushed to a fresh GitHub repo with green CI.

### Step 1 — Create the GitHub repo

```powershell
gh repo create minnal `
  --description "Minnal — AI-native API workspace. Never Settle." `
  --public `
  --license apache-2.0 `
  --gitignore Rust
```

This creates the remote with a starter `.gitignore`, `LICENSE`, and `README.md`. We will overwrite the gitignore in Step 4.

### Step 2 — Clone into the working directory

```powershell
# Working directory is E:\Arun\Workspace\Minnal\ — clone INTO it.
# (The 'files' folder stays as-is alongside the cloned repo, OR move spec docs in.)
gh repo clone minnal E:\Arun\Workspace\Minnal\minnal
```

**Decision point**: do you want the spec docs (`Minnal.md`, `CLAUDE.md`, `CONTEXT.md`, `plan.md`, `design/`) committed into the repo at the root? **Strongly suggested yes** — they are the source of truth and need to ship with the code. Suggested move:

```powershell
# Move spec docs into the cloned repo root.
Move-Item E:\Arun\Workspace\Minnal\files\Minnal.md   E:\Arun\Workspace\Minnal\minnal\Minnal.md
Move-Item E:\Arun\Workspace\Minnal\files\CLAUDE.md   E:\Arun\Workspace\Minnal\minnal\CLAUDE.md
Move-Item E:\Arun\Workspace\Minnal\files\CONTEXT.md  E:\Arun\Workspace\Minnal\minnal\CONTEXT.md
Move-Item E:\Arun\Workspace\Minnal\files\plan.md     E:\Arun\Workspace\Minnal\minnal\plan.md
Move-Item E:\Arun\Workspace\Minnal\files\design      E:\Arun\Workspace\Minnal\minnal\design
Move-Item E:\Arun\Workspace\Minnal\files\phase-0-and-0.5.md E:\Arun\Workspace\Minnal\minnal\design\phase-0-and-0.5.md
```

From here all paths are relative to `E:\Arun\Workspace\Minnal\minnal\`.

### Step 3 — Create the six crates

```powershell
cd E:\Arun\Workspace\Minnal\minnal
New-Item -ItemType Directory -Path crates -Force | Out-Null

cargo new --lib crates/minnal-core
cargo new --lib crates/minnal-store
cargo new --lib crates/minnal-ai
cargo new --lib crates/minnal-hooks
cargo new --lib crates/minnal-export
cargo new --lib crates/minnal-ui
```

Note: cargo lowercases names per Rust convention. Repo brand stays `Minnal`; package names are `minnal-*`.

### Step 4 — Workspace `Cargo.toml`

Replace any cargo-generated content with:

```toml
# E:\Arun\Workspace\Minnal\minnal\Cargo.toml
[workspace]
resolver = "2"
members  = [
    "crates/minnal-core",
    "crates/minnal-store",
    "crates/minnal-ai",
    "crates/minnal-hooks",
    "crates/minnal-export",
    "crates/minnal-ui",
]

[workspace.package]
version       = "0.1.0"
edition       = "2021"
license       = "Apache-2.0"
authors       = ["Arun"]
repository    = "https://github.com/<your-gh-user>/minnal"
rust-version  = "1.80"

[workspace.lints.rust]
unsafe_code = "deny"            # exception: minnal-ui FFI, opt-in per crate
unused_must_use = "deny"

[workspace.lints.clippy]
unwrap_used  = "deny"           # the review gate, enforced by lint
expect_used  = "deny"
panic        = "deny"
todo         = "warn"           # allowed during scaffolding; CI fails on todo at PR
dbg_macro    = "deny"
```

### Step 5 — Per-crate `Cargo.toml`

Each `crates/minnal-X/Cargo.toml` becomes:

```toml
[package]
name        = "minnal-X"            # replace X
version.workspace      = true
edition.workspace      = true
license.workspace      = true
authors.workspace      = true
repository.workspace   = true
rust-version.workspace = true
description = "Minnal — <one-line>"
```

`description` strings:
- `minnal-core`: "HTTP engine, auth schemes, environment variables, types"
- `minnal-store`: "Persistence layer — content-addressed SQLite + vec index"
- `minnal-ai`: "Model router, inference backend, prompt engine"
- `minnal-hooks`: "Wasmtime sandbox, hook lifecycle, capability grants"
- `minnal-export`: "HTML, PDF, Excel exports"
- `minnal-ui`: "WinUI 3 shell"

Crates with internal deps already known (per Minnal.md §4.1) — add now:

```toml
# crates/minnal-ai/Cargo.toml — adds:
[dependencies]
minnal-store = { path = "../minnal-store" }

# crates/minnal-hooks/Cargo.toml — adds:
[dependencies]
minnal-core = { path = "../minnal-core" }
minnal-ai   = { path = "../minnal-ai" }

# crates/minnal-export/Cargo.toml — adds:
[dependencies]
minnal-store = { path = "../minnal-store" }
minnal-core  = { path = "../minnal-core" }

# crates/minnal-ui/Cargo.toml — adds:
[dependencies]
minnal-core  = { path = "../minnal-core" }
minnal-ai    = { path = "../minnal-ai" }
minnal-hooks = { path = "../minnal-hooks" }
minnal-store = { path = "../minnal-store" }
```

`minnal-core` and `minnal-store` add **no internal deps** — pure layers, per architecture invariant.

### Step 6 — `.gitignore`

Overwrite the cargo/gh-generated `.gitignore` at repo root:

```gitignore
# E:\Arun\Workspace\Minnal\minnal\.gitignore
/target
**/*.rs.bk
Cargo.lock           # see decision below

# Local data
*.sqlite
*.sqlite-wal
*.sqlite-shm

# Model weights — NEVER committed (Apache 2.0 manifest)
*.gguf
*.ggml
/models

# Secrets
.minnal-secrets
.minnal-data/
.env
.env.local

# IDE
.vscode/*
!.vscode/extensions.json
.idea/
*.swp

# Windows
Thumbs.db
desktop.ini
```

**Decision**: per Minnal.md §4 ("Cargo.lock — Committed. Always."), do **NOT** ignore Cargo.lock. Remove that line — the snippet above includes it as a placeholder so you can confirm. **Final answer: keep Cargo.lock committed for the application crates** (this is a binary, not a library distributed on crates.io). Override:

```powershell
# After saving the gitignore above, remove the Cargo.lock line:
(Get-Content .gitignore) | Where-Object { $_ -notmatch '^Cargo\.lock' } | Set-Content .gitignore
```

### Step 7 — Apache 2.0 licence header in every `lib.rs`

Each `crates/minnal-X/src/lib.rs` starts with:

```rust
// Copyright 2026 Arun
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

//! <crate-name> — <one-line description from §5>.
```

### Step 8 — Verify clean build

```powershell
cargo build --workspace
# Must produce zero warnings. If any warning appears, STOP and resolve.
```

### Step 9 — CI workflow

Create `.github/workflows/ci.yml`:

```yaml
name: ci
on:
  push:
    branches: [main]
  pull_request:

jobs:
  build:
    runs-on: windows-2022
    steps:
      - uses: actions/checkout@v4

      - uses: dtolnay/rust-toolchain@stable
        with:
          components: clippy, rustfmt

      - uses: Swatinem/rust-cache@v2

      - name: cargo fmt
        run: cargo fmt --all -- --check

      - name: cargo clippy
        run: cargo clippy --workspace --all-targets -- -D warnings

      - name: cargo build
        run: cargo build --workspace --all-targets

      - name: cargo test
        run: cargo test --workspace
```

### Step 10 — First commit + push

```powershell
git add .
git commit -m "chore: initialise Minnal workspace"
git push -u origin main
```

Watch CI:

```powershell
gh run watch
```

### Phase 0 exit gate (mirrors plan.md §6)

- [ ] `cargo build --workspace` clean (zero warnings).
- [ ] Each crate `cargo build -p minnal-<X>` succeeds independently.
- [ ] CI green on first push.
- [ ] `.gitignore` contains all entries from Step 6 (without Cargo.lock).
- [ ] Apache 2.0 header present in every `lib.rs`.
- [ ] Spec docs committed at repo root: `Minnal.md`, `CLAUDE.md`, `CONTEXT.md`, `plan.md`, `design/`.

Update `CONTEXT.md` "Work Completed" section with: *Phase 0 complete (date)*.

---

## Phase 0.5 — Spike Week

Three independent spikes, all parallelisable. Each lives in a throwaway directory **outside the workspace** so no half-baked code pollutes the main tree. Outcomes are recorded in CONTEXT.md, not committed as code.

### Spike A — WinUI 3 + Rust hello-window

Goal: prove the rendering stack. A window with a tab control and a split pane, opened from a Rust `main()`. No content. No theming. Just: does it render?

```powershell
mkdir E:\Arun\Workspace\Minnal\spikes\spike-a-winui
cd E:\Arun\Workspace\Minnal\spikes\spike-a-winui
cargo init --bin
```

Add to `Cargo.toml`:

```toml
[dependencies]
windows        = { version = "0.58", features = [
    "Win32_Foundation",
    "Win32_System_Com",
    "UI_Xaml",
    "UI_Xaml_Controls",
    "UI_Xaml_Hosting",
] }
windows-app-sdk = "*"     # confirm exact version with `winget list`
```

Note: as of Jan 2026, the `windows-app-sdk` Rust crate ecosystem is sparse — Codex must verify availability. **If no usable crate exists**, the spike outcome is automatic Path B (WinUI 3 chrome + WebView2 — see plan.md §5.6).

Try the simplest possible XAML island host:

```rust
// src/main.rs (sketch — Codex completes)
fn main() -> windows::core::Result<()> {
    use windows::Win32::System::Com::*;
    unsafe { CoInitializeEx(None, COINIT_APARTMENTTHREADED)?; }

    // Bootstrap WindowsAppSDK runtime
    // Create a Window with UI::Xaml::Controls::Pivot (tabs)
    // Inside Pivot: a Grid with two columns (split pane) and a single TextBlock each
    // Run the message pump
    Ok(())
}
```

**Acceptance**:
- Window opens.
- Two visible tabs.
- Split pane visible inside one tab.
- Window closes cleanly (no Win32 leaks reported).

**If it works**: record in CONTEXT.md → "Spike A: Path A confirmed (date)". Phase 4 proceeds with pure XAML.

**If it fails after 2 days**: record outcome → "Spike A: Path A blocked, falling back to Path B". Update plan.md §5.6 modules to the WebView2 split. Escalate to Arun.

### Spike B — llama.cpp + DirectML smoke test

Goal: load a small GGUF, generate one token, log whether DirectML backend was used.

```powershell
mkdir E:\Arun\Workspace\Minnal\spikes\spike-b-llama
cd E:\Arun\Workspace\Minnal\spikes\spike-b-llama
cargo init --bin
```

```toml
[dependencies]
llama-cpp-2 = "*"     # check latest at scaffold time
anyhow      = "1"
tracing     = "0.1"
tracing-subscriber = "0.3"
```

**Important**: do NOT use the production target model (gemma-4-e4b 14B at ~8GB) for the smoke test. Use a tiny model — e.g. TinyLlama 1.1B Q4 (~600MB) — purely to prove the pipeline works.

Download tiny model to `models/` (gitignored):

```powershell
mkdir models
# Manual: download tinyllama-1.1b-chat-v1.0.Q4_K_M.gguf from a HF mirror to models/
```

Code sketch:

```rust
// src/main.rs (Codex completes)
use anyhow::Result;
use llama_cpp_2::*;

fn main() -> Result<()> {
    tracing_subscriber::fmt::init();

    let backend = LlamaBackend::init()?;          // logs DirectML / CPU selection
    let model_params = LlamaModelParams::default()
        .with_n_gpu_layers(99);                   // try GPU; falls back to CPU
    let model = LlamaModel::load_from_file(&backend, "models/tinyllama-...gguf", &model_params)?;

    let ctx_params = LlamaContextParams::default().with_n_ctx(Some(512.try_into()?));
    let mut ctx = model.new_context(&backend, ctx_params)?;

    let tokens = model.str_to_token("Hello", AddBos::Always)?;
    // Run one decode step
    // Log: backend used, time-to-first-token, embedding dimension if exposed
    Ok(())
}
```

**Acceptance**:
- One token generated without panic.
- Backend logged (DirectML / CPU).
- Time-to-first-token printed.
- **Embedding dimension** of the model logged via `LlamaModel::n_embd()` — record this number; it is the gating value for `V001.md` §3 `FLOAT[1024]`.

**If DirectML works**: CONTEXT.md → "Spike B: DirectML confirmed on Intel Xe (date), TTFT Xms".

**If DirectML fails but CPU works**: CONTEXT.md → "Spike B: CPU only on this binding (date)". `Minnal-ai` ships CPU-default for v1; DirectML behind feature flag.

**If `llama-cpp-2` does not expose DirectML at all**: ⚠️ Revisit decision §1 #5 — pivot to git-submodule + custom build.

### Spike C — sqlite-vec Windows load

Goal: prove sqlite-vec extension loads and answers a query on Windows MSVC build.

```powershell
mkdir E:\Arun\Workspace\Minnal\spikes\spike-c-sqlite-vec
cd E:\Arun\Workspace\Minnal\spikes\spike-c-sqlite-vec
cargo init --bin
```

```toml
[dependencies]
rusqlite     = { version = "0.31", features = ["bundled"] }
sqlite-vec   = "0.1"
anyhow       = "1"
```

Code:

```rust
// src/main.rs (Codex completes)
use anyhow::Result;
use rusqlite::Connection;

fn main() -> Result<()> {
    let conn = Connection::open_in_memory()?;
    sqlite_vec::load(&conn)?;

    conn.execute_batch("
        CREATE VIRTUAL TABLE v USING vec0(id INTEGER, embedding FLOAT[4]);
        INSERT INTO v(id, embedding) VALUES (1, '[0.1,0.2,0.3,0.4]');
        INSERT INTO v(id, embedding) VALUES (2, '[0.5,0.6,0.7,0.8]');
    ")?;

    let mut stmt = conn.prepare("
        SELECT id, distance FROM v
        WHERE embedding MATCH '[0.1,0.2,0.3,0.4]'
        ORDER BY distance LIMIT 2
    ")?;
    let rows: Vec<(i64, f64)> = stmt.query_map([], |r| Ok((r.get(0)?, r.get(1)?)))?
        .collect::<Result<Vec<_>,_>>()?;
    println!("{rows:?}");
    Ok(())
}
```

**Acceptance**:
- Compiles on Windows MSVC.
- Extension loads without error.
- Query returns 2 rows, id=1 first (zero distance to itself).

**If it works**: CONTEXT.md → "Spike C: sqlite-vec confirmed on Windows (date)". V001 ships with `vec0`.

**If it fails**: CONTEXT.md → "Spike C: sqlite-vec blocked on Windows". Switch `vec_index.rs` to `hnsw_rs`; update V001 design pack §9 (Codex flips the Cargo feature default; V001 schema removes `embeddings` virtual table and adds an `embeddings_blob` regular table).

---

## Phase 0.5 exit gate (mirrors plan.md §6)

- [ ] Spike A outcome recorded; Path A or Path B locked in writing in CONTEXT.md.
- [ ] Spike B outcome recorded; DirectML/CPU selection locked; **embedding dimension** verified against V001 schema.
- [ ] Spike C outcome recorded; sqlite-vec or hnsw_rs path locked.
- [ ] CONTEXT.md "Work Completed" updated with all three spike dates.
- [ ] Any plan.md §1 ⚠️ markers triggered by spike outcomes are cleared or escalated.

If all three spikes pass: Phase 1 unblocked.
If any spike forces a fallback: corresponding plan.md section updated *before* Phase 1 begins.

---

## What lands where after these phases

```
E:\Arun\Workspace\Minnal\minnal\        ← committed to GitHub
  ├── Cargo.toml                        ← Phase 0 Step 4
  ├── .gitignore                        ← Phase 0 Step 6
  ├── .github/workflows/ci.yml          ← Phase 0 Step 9
  ├── Minnal.md                         ← moved in Phase 0 Step 2
  ├── CLAUDE.md
  ├── CONTEXT.md                        ← updated end of every phase
  ├── plan.md
  ├── design/
  │   ├── Minnal-store/V001.md
  │   └── phase-0-and-0.5.md            ← this file
  └── crates/
      ├── minnal-core/
      ├── minnal-store/
      ├── minnal-ai/
      ├── minnal-hooks/
      ├── minnal-export/
      └── minnal-ui/

E:\Arun\Workspace\Minnal\spikes\        ← NOT committed
  ├── spike-a-winui/
  ├── spike-b-llama/
  └── spike-c-sqlite-vec/
```

The `spikes/` directory is throwaway — recorded outcomes matter, not the code.

---

*phase-0-and-0.5.md v0.1.*
*Companion to plan.md §2, §6.*
*Codex: execute strictly in order. Any deviation requires Arun approval before proceeding.*
*Never Settle.*
