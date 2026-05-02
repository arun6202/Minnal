# Bundled AI Assets

Place the spike GGUF model in this directory before packaging, for example:

```text
apps/Minnal.Maui/Resources/Raw/ai/gemma-3-1b-it-q4_k_m.gguf
```

The app scans its own installed content directory for `*.gguf`. It does not read model weights from `%LOCALAPPDATA%`.

GGUF weights are intentionally not committed by default because they are large binary artifacts.
