using Microsoft.Extensions.Logging;
using Minnal.AppModel;

namespace Minnal.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
#if WINDOWS
        Environment.SetEnvironmentVariable(
            "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
            "--in-process-gpu --disable-features=Translate,AutofillAssistant,MediaRouter",
            EnvironmentVariableTarget.Process);
#endif

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddSingleton<IWorkbenchService, WorkbenchService>();
        builder.Services.AddSingleton<IAiService, LlamaAiService>();
        builder.Services.AddSingleton<IHttpExecutionService, HttpExecutionService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
