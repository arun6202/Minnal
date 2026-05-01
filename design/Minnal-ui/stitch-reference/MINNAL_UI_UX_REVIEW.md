# Minnal UI/UX Review - Stitch Direction

Source artifact: `stitch_minnal_ai_native_api_workspace`
Status: inspiration review, not an implementation spec
Date: 2026-05-01

---

## Read of the Artifact

The Stitch screen is strong as a mood board. It gives Minnal a serious, local-first developer cockpit: dark canvas, high-density panels, graph-centered workspace, and an AI explanation drawer that feels operational rather than decorative.

It should influence Minnal, but it should not be copied directly. The current composition leans toward an enterprise monitoring dashboard. Minnal is an API workspace, so the request execution loop must remain immediately available: intent, request, response, history, explain, hook approval, and promote.

---

## Keep

1. **Graph-led mental model**
   The central canvas supports Minnal's typed-DAG premise better than a folder tree. Clusters and request relationships can become the primary navigation surface.

2. **Right-side intelligence drawer**
   The AI panel works because it explains a concrete event with evidence, diff, severity, and action. This aligns with the "Why everywhere" principle.

3. **Dense dark workstation tone**
   The design feels like a real tool, not marketing. This is the right direction for a Windows developer app used for long sessions.

4. **Clear status chips**
   Labels like model, severity, offline state, and drift type should become first-class status signals.

5. **Diff-first explanation**
   Showing the artifact that caused the AI conclusion is more trustworthy than prose alone.

---

## Avoid

1. **Do not make Intelligence a separate destination only**
   AI should be embedded across request, response, auth, search, and hooks. A separate Intelligence tab is useful, but it cannot become the only AI surface.

2. **Do not hide the request builder**
   The current screen is mostly graph + insight drawer. Minnal still needs a fast path for method, URL, auth, headers, body, send, response, and history.

3. **Do not over-index on schema drift**
   Schema drift is powerful, but it is a Tier 2/v2-ish feeling. v1 must foreground the signed invariant: import, execute, explain, generate hook, semantic search.

4. **Do not use too much blue glow**
   The concept is clean, but strong glow and pulse effects can make the app feel like observability/SOC tooling. Minnal should be quieter and more precise.

5. **Do not use pills everywhere**
   The project direction prefers sharp, compact controls. Status chips are acceptable; primary controls should stay rectangular and stable.

---

## Suggested Minnal Direction

Minnal's first screen should be a **workbench**, not a dashboard.

Recommended primary layout:

- **Left rail:** Workspace switch, imports, recent collections, semantic clusters, history.
- **Top intent bar:** Natural language command/search: "Create a login request", "Find token refresh failures", "Explain last 401".
- **Center split:** Request surface and response thread, with the semantic graph available as a mode or canvas pane.
- **Right drawer:** Contextual Why panel: response explanation, auth diagnosis, hook review, schema clue, search rationale.
- **Bottom/status strip:** Offline/model state, memory pressure, environment, selected collection, hook safety state.

The graph should not replace execution. It should help users understand and navigate API systems once requests exist.

---

## Questions Arun Should Decide

1. **What is the default first screen?**
   Suggestion: request workbench with semantic canvas available, not canvas-only.
   Why: v1 must prove request execution before higher-level intelligence feels useful.

2. **Should the graph be always visible or mode-based?**
   Suggestion: mode-based or collapsible.
   Why: always-visible graph consumes space needed for request/response work on laptops.

3. **What is the primary action: New Request, Ask Minnal, or Import?**
   Suggestion: Import when empty; Ask Minnal once data exists; Send remains dominant inside a request.
   Why: the best next action changes with workspace state.

4. **What should AI be allowed to do from the UI?**
   Suggestion: explain, draft, search, and dry-run by default; require explicit approval for writes, hooks, auth changes, and network-touching actions.
   Why: this preserves trust and matches the signed architecture.

5. **Where does Playground live?**
   Suggestion: a visible top-level mode, not buried in settings.
   Why: ephemeral experimentation with zero SQLite writes is a core differentiator.

6. **How should Minnal show offline/local trust?**
   Suggestion: persistent status strip: `gemma-4-e4b | Q4/Q8 | offline | memory ok | no cloud`.
   Why: local-first is not a footnote; it is a product promise.

7. **Should hook review look like a code review or a security approval?**
   Suggestion: both: code diff + capability grants + dry-run output + approve/reject.
   Why: users need to trust generated WASM before it touches auth.

8. **What is the visual accent?**
   Suggestion: keep blue for selected/navigation states, reserve orange for irreversible/promote actions, reserve teal for AI-generated suggestions.
   Why: color should encode action class, not decoration.

9. **How much enterprise language do we want?**
   Suggestion: less enterprise wording in product UI; more direct developer language.
   Why: phrases like "AI-Native Environment" feel like positioning. The app itself should speak in actions and evidence.

10. **What must be visible above the fold in v1?**
    Suggestion: method, URL, auth state, Send, latest response status, Why, semantic search/intent bar, offline/model state.
    Why: these are the signed invariant's daily-use controls.

---

## Concrete UI Screens To Design Next

1. **Empty workspace**
   Import Postman/OpenAPI, start from intent, or create request.

2. **Request execution workbench**
   Method/URL/auth/body, response thread, Why drawer.

3. **Semantic search results**
   Query by intent, cluster explanation, matched requests with evidence.

4. **Playground session**
   Ephemeral draft request, explicit Promote action, zero-write indicator.

5. **Hook review flow**
   Generated hook source, capabilities, dry-run result, approval gate.

6. **Memory pressure state**
   Model holding behavior, restart-to-reassess messaging, no silent downgrade.

---

## Verdict

Use this Stitch direction as visual inspiration for the **Semantic Canvas** and **Why drawer**. Do not use it as the whole app shell. Minnal needs a more execution-first workbench with intelligence woven into every surface.
