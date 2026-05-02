# Progress Report - MAUI/F# Spike

Date: 2026-05-02  
Branch: `maui-fsharp-spike`  
Current commit before this report: `157ce8e spike: add maui blazor fsharp shell`

## Current Status

The MAUI/F# cross-platform spike is alive and pushed.

The branch contains:

- `apps/Minnal.Maui`: .NET MAUI Blazor Hybrid shell.
- `apps/Minnal.AppModel`: F# app-state/model layer.
- `apps/Minnal.MauiSpike.slnx`: solution entry point.
- `apps/README.md`: short architecture and memory-policy note.

The Windows MAUI target builds:

```powershell
dotnet build apps\Minnal.Maui\Minnal.Maui.csproj -f net10.0-windows10.0.19041.0
```

Build result: success.

One warning remains: NuGet vulnerability metadata could not be fetched during restore. Package restore and compilation still succeeded.

## What Went Well

The cross-platform path is practical.

The installed SDK already has .NET 10 and MAUI workloads, including `maui-windows`, Android, iOS, and Mac Catalyst manifests. That means the machine can scaffold and build a MAUI app without a long setup detour.

MAUI Blazor Hybrid fits Minnal better than plain MAUI XAML.

Minnal needs a dense API workbench UI: request lists, response panels, drawers, command surfaces, evidence blocks, hook review, and future graph/search views. HTML/CSS inside `BlazorWebView` is a better fit for that density than pure XAML. It also keeps the door open for sharing UI components with a future web companion.

F# works cleanly as the app model layer.

The first F# project compiles and is referenced by the MAUI app. Current F# types model:

- request drafts
- response snapshots
- model lifecycle state
- hook review state
- platform policy
- workbench snapshot factory

This is the right separation: MAUI hosts and renders; F# owns product state and later domain logic.

The first screen is not a landing page.

The MAUI branch opens directly into a Minnal-like workbench:

- intent bar
- request rail
- request/response surface
- Why drawer
- hook review panel
- model status panel
- cross-platform/platform policy panel
- memory policy panel

This follows the product direction better than template screens or marketing pages.

Memory discipline is already represented in app state.

The branch records these rules in code and docs:

- AI is on-demand by default.
- No model load during app startup.
- One active `BlazorWebView`.
- Large response bodies and indexes stay out of UI state.
- Cold shell target: under 350 MB.
- Warm UI target, excluding model mmap: under 700 MB.
- Unload model/context on idle, memory pressure, or workspace switch.

That matters because local AI will dominate memory. We should not confuse MAUI overhead with model overhead, but we also should not let MAUI grow unchecked.

## What Did Not Go Well

F# MAUI templates are not available out of the box.

The installed MAUI templates are C# only. A pure F# MAUI app would require custom project setup and likely more friction around XAML/source generation. The pragmatic compromise is:

- C# for thin MAUI host.
- Razor/CSS for UI.
- F# for state, domain logic, storage rules, import/export logic, and AI orchestration.

This is a good compromise, not a failure.

`dotnet` needed CLI-home handling in the sandbox.

The default sandbox user profile could not write .NET first-run sentinel files. The branch uses a local ignored `.dotnet-cli-home/` during commands. This is not product code, but it is useful for Codex sessions.

NuGet access required escalation.

The first restore failed under sandbox network restrictions. Running the build with approval allowed restore/build to complete.

MAUI is heavier than Windows-only stacks.

Compared with WPF/WinForms/Rust-native shells, MAUI Blazor Hybrid has more baseline memory cost. That cost buys cross-platform reach. Since cross-platform is a requirement, the right response is measurement and lifecycle discipline, not dropping MAUI prematurely.

The current UI is static.

The first screen is a high-fidelity static shell backed by F# snapshot data. It does not yet execute requests, persist anything, load models, or run hooks.

That is intentional. The branch is proving the shell direction and architecture shape first.

## Architecture Direction

Recommended shape:

```text
Minnal.Maui
  Thin MAUI host.
  One BlazorWebView.
  Platform lifecycle hooks.
  Native file pickers, notifications, secure storage adapters.

Minnal.Web
  Razor components / CSS workbench UI.
  Eventually split from Minnal.Maui if reuse becomes valuable.

Minnal.AppModel
  F# domain state.
  Request/session models.
  AI lifecycle policy.
  Memory policy.
  Platform capability model.

Minnal.Store
  SQLite persistence.
  Response-body deduplication.
  Vector index integration.

Minnal.AI
  Model lifecycle.
  Inference backend abstraction.
  On-demand/warm/unload behavior.

Minnal.Core
  HTTP execution, auth, environments, import models.
```

For now, only `Minnal.Maui` and `Minnal.AppModel` exist in this branch.

## MAUI Memory Reality

Expected rough memory shape:

```text
MAUI shell without model:             moderate
MAUI BlazorWebView active:            higher
Local AI model loaded:                dominant memory cost
Model context / KV cache:             can be very large
Embedding/index caches:               must be bounded
Large response bodies in UI memory:   must be avoided
```

The important rule: do not load the model at startup.

Default lifecycle should be:

```text
App opens
  -> no model loaded
User asks Explain/Search/Generate
  -> load model or connect sidecar
  -> run task
  -> keep warm briefly
Idle or pressure
  -> release context/model
```

This keeps MAUI viable even if it is not the lightest shell.

## What Arun Should Know

MAUI Blazor Hybrid is the right cross-platform bet if cross-platform is real.

If Minnal were Windows-only, WPF + WebView2 would be leaner. Since cross-platform is now a must, MAUI's extra baseline is acceptable if we hold strict memory gates.

Blazor Hybrid means the UI is web-rendered, but not Electron.

It uses platform WebView controls inside a native MAUI app. On Windows that is WebView2. On other platforms it uses the platform web view. This is heavier than native controls but lighter and more integrated than shipping Chromium through Electron.

F# should own the product brain.

Do not force F# into the MAUI host layer. Let C# do thin host glue where the ecosystem expects it. Put durable logic in F# where it pays off.

Avoid multiple WebViews.

One hidden extra WebView can erase a lot of memory discipline. The app should keep one primary `BlazorWebView` and route panels inside it.

Measure before arguing.

The next meaningful decision is not theoretical. The branch should run, capture cold working set, then capture warm working set after the first AI-adjacent flow without actually loading a huge model.

## F# Engineering Law

F# is the primary product language for the model, domain, planning, storage orchestration, AI lifecycle, and cross-platform policy layers. C# is allowed only as thin MAUI/platform glue where the ecosystem forces it.

Treat F# as a proof-carrying language:

- Every type is a proof.
- Every function is a theorem.
- Illegal states must be unrepresentable at compile time.
- If a C# pattern appears in F# code, mark it with `⚠️` in review and replace it with the F# idiom.

Core rules:

- Parse, do not validate.
- Prefer discriminated unions over classes.
- Use `Result<'T, 'Error>` over exception-based domain control flow.
- Use `Option<'T>` over `null`.
- Immutability is the default.
- Mutation requires a written justification and a narrow scope.
- Prefer composition, pipelines, and functions over inheritance.

Type discipline:

- Use single-case discriminated unions for domain primitives such as non-empty strings, positive integers, model identifiers, route names, and request identifiers.
- Use smart constructors that return `Result`, never raw unchecked domain values.
- Use phantom types when compile-time constraints matter.
- Do not pass meaningful values as primitive `string`, `int`, or `Guid` once they enter domain code.

Effects and architecture:

- Keep the planner/DAG layer pure.
- Separate descriptions of work from execution of work.
- Use railway-oriented programming for domain error flows.
- Use computation expressions for domain-specific effects where they simplify composition.
- Use writer-style telemetry accumulation in pure logic; do not log from pure functions.
- Model streaming and pagination with cursor algebra, not ad hoc nullable tokens.

Testing:

- Prefer FsCheck properties before unit examples.
- Use Expecto as the F# test runner.
- Name tests as properties, not procedures.
- A test should state the theorem it proves.

F# stack direction:

- .NET 8 or newer, with F# 8+ semantics.
- FsCheck + Expecto for tests.
- SQLite first for Minnal local state; add provider-specific libraries only behind typed ports.
- Any future ETL-style integrations should follow the same typed-boundary discipline.

Patterns to reject in F#:

- Mutable state without a written reason.
- Exception-based control flow.
- Stringly typed APIs.
- OOP inheritance hierarchies.
- Service-locator patterns.
- Null-tolerant APIs in domain code.
- C#-style DTO mutation as the core model.

Known tradeoff:

This style is slower to scaffold than C#-shaped F#, but it will make Minnal safer. The app is going to manage auth, hooks, model state, persistence, and local execution. Primitive obsession and nullable state will become product bugs quickly.

## Way Forward

1. Run the MAUI app locally on Windows.
   - Confirm the workbench renders correctly.
   - Check layout at 1366x768 and a wide desktop viewport.

2. Add a memory telemetry strip.
   - Show process working set.
   - Show managed heap.
   - Show WebView/model state.

3. Add a real app-state service in F#.
   - Replace `WorkbenchSnapshotFactory.Create()` with an injectable state service.
   - Keep UI state small and immutable where practical.

4. Add platform capability detection.
   - OS/runtime.
   - WebView availability.
   - memory floor.
   - local model availability.

5. Add SQLite next, before AI.
   - Store request metadata.
   - Store response bodies out of UI memory.
   - Prove cross-platform SQLite packaging.

6. Add AI lifecycle as a stub first.
   - States: `Off`, `Loading`, `ReadyWarm`, `Busy`, `Unloading`, `Blocked`.
   - Do not integrate a real model until the lifecycle is visible and measurable.

7. Decide whether to split `Minnal.Web`.
   - Keep Razor components inside `Minnal.Maui` for now.
   - Split only when reuse or build boundaries justify it.

8. Add CI for the MAUI branch.
   - Windows build first.
   - Android/Mac/iOS later, because those require runner/tooling decisions.

## Immediate Next Command

```powershell
$env:DOTNET_CLI_HOME="$PWD\.dotnet-cli-home"
dotnet build apps\Minnal.Maui\Minnal.Maui.csproj -f net10.0-windows10.0.19041.0
```

Then run the app from Visual Studio or with the appropriate MAUI Windows launch command and capture baseline memory.
