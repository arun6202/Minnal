# Minnal MAUI Spike

This branch keeps the v1 product cross-platform in shape while targeting Windows first.

## Shape

- `Minnal.Maui`: .NET MAUI Blazor Hybrid host with a single `BlazorWebView`.
- `Minnal.AppModel`: F# app state and product policies consumed by the UI.
- `Minnal.MauiSpike.slnx`: solution entry point for the experiment.

## Memory Rules

- AI is off/on-demand by default.
- Never load a model during shell startup.
- Keep at most one active WebView.
- Keep large response bodies and indexes out of UI state.
- Cold shell target: under 350 MB.
- Warm UI target, excluding model mmap: under 700 MB.
- Unload model/context on idle, memory pressure, or workspace switch.

## First Build

```powershell
$env:DOTNET_CLI_HOME="$PWD\.dotnet-cli-home"
dotnet build apps\Minnal.Maui\Minnal.Maui.csproj -f net10.0-windows10.0.19041.0
```
