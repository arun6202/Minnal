using LLama;
using LLama.Common;

namespace Minnal.Maui.Services;

public enum AiState { Off, Loading, Ready, Blocked }

public interface IAiService
{
    AiState State { get; }
    string StatusText { get; }
    Task LoadAsync(CancellationToken ct = default);
    Task<string> ExplainAsync(string prompt, CancellationToken ct = default);
}

public sealed class LlamaAiService : IAiService, IDisposable
{
    private LLamaWeights? _weights;
    private ModelParams? _params;

    public AiState State { get; private set; } = AiState.Off;
    public string StatusText { get; private set; } = "Not loaded";

    public Task LoadAsync(CancellationToken ct = default) => Task.Run(() => LoadSync(ct), ct);

    private void LoadSync(CancellationToken ct)
    {
        State = AiState.Loading;
        StatusText = "Scanning for GGUF...";

        var modelsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Minnal", "models");

        var gguf = Directory.Exists(modelsDir)
            ? Directory.GetFiles(modelsDir, "*.gguf").FirstOrDefault()
            : null;

        if (gguf is null)
        {
            State = AiState.Blocked;
            StatusText = $"No GGUF in %LOCALAPPDATA%\\Minnal\\models\\";
            return;
        }

        ct.ThrowIfCancellationRequested();

        try
        {
            _params = new ModelParams(gguf) { ContextSize = 512, GpuLayerCount = 0 };
            _weights = LLamaWeights.LoadFromFile(_params);
            State = AiState.Ready;
            StatusText = Path.GetFileName(gguf);
        }
        catch (Exception ex)
        {
            State = AiState.Blocked;
            StatusText = $"Load failed: {ex.Message}";
        }
    }

    public async Task<string> ExplainAsync(string prompt, CancellationToken ct = default)
    {
        if (State != AiState.Ready || _weights is null || _params is null)
            return $"[AI {State}: {StatusText}]";

        return await Task.Run(async () =>
        {
            var executor = new StatelessExecutor(_weights, _params);
            var inferenceParams = new InferenceParams
            {
                MaxTokens = 256,
                AntiPrompts = ["\n\nUser:", "###", "<|end|>"]
            };

            var sb = new System.Text.StringBuilder();
            await foreach (var token in executor.InferAsync(
                $"Explain this HTTP response briefly:\n{prompt}\n\nExplanation:", inferenceParams, ct))
            {
                sb.Append(token);
            }
            return sb.ToString().Trim();
        }, ct);
    }

    public void Dispose() => _weights?.Dispose();
}
