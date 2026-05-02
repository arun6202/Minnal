namespace Minnal.AppModel

[<CLIMutable>]
type RequestDraft =
    {
        Method: string
        Name: string
        Url: string
        Auth: string
        Status: string
    }

[<CLIMutable>]
type ResponseSnapshot =
    {
        StatusCode: int
        DurationMs: int
        SizeKb: float
        Summary: string
    }

[<CLIMutable>]
type ModelState =
    {
        Mode: string
        Backend: string
        Memory: string
        UnloadPolicy: string
        ColdTargetMb: int
        WarmTargetMb: int
    }

[<CLIMutable>]
type HookState =
    {
        Name: string
        Phase: string
        Capability: string
        Verdict: string
    }

[<CLIMutable>]
type PlatformState =
    {
        PrimaryTarget: string
        FutureTargets: string
        UiHost: string
        RenderPolicy: string
    }

[<CLIMutable>]
type WorkbenchSnapshot =
    {
        Intent: string
        ActiveRequest: RequestDraft
        Requests: RequestDraft array
        Response: ResponseSnapshot
        Model: ModelState
        Hook: HookState
        Platform: PlatformState
    }

type WorkbenchSnapshotFactory =
    static member Create() =
        let active =
            {
                Method = "POST"
                Name = "Create token"
                Url = "https://api.minnal.local/v1/tokens"
                Auth = "OAuth 2.0 PKCE"
                Status = "Ready"
            }

        {
            Intent = "Explain the last 401 and draft a retry with fresh auth."
            ActiveRequest = active
            Requests =
                [|
                    active
                    {
                        Method = "GET"
                        Name = "List workspaces"
                        Url = "https://api.minnal.local/v1/workspaces"
                        Auth = "Bearer"
                        Status = "Cached"
                    }
                    {
                        Method = "PATCH"
                        Name = "Rotate secret"
                        Url = "https://api.minnal.local/v1/secrets/current"
                        Auth = "HMAC"
                        Status = "Needs review"
                    }
                |]
            Response =
                {
                    StatusCode = 401
                    DurationMs = 184
                    SizeKb = 3.8
                    Summary = "Token expired before replay. Refresh flow is required before retry."
                }
            Model =
                {
                    Mode = "On-demand"
                    Backend = "DirectML decision pending"
                    Memory = "Model unloaded"
                    UnloadPolicy = "Unload after idle, memory pressure, or workspace switch"
                    ColdTargetMb = 350
                    WarmTargetMb = 700
                }
            Hook =
                {
                    Name = "refresh-token-prehook"
                    Phase = "Pre"
                    Capability = "secrets:read, request:mutate"
                    Verdict = "Dry-run required"
                }
            Platform =
                {
                    PrimaryTarget = "Windows first"
                    FutureTargets = "macOS and mobile later"
                    UiHost = "MAUI Blazor Hybrid"
                    RenderPolicy = "Single BlazorWebView; no hidden secondary WebViews"
                }
        }
