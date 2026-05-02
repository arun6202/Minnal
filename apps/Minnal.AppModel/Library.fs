namespace Minnal.AppModel

open System
open System.Diagnostics
open System.IO
open System.Net.Http
open System.Text
open System.Threading.Tasks
open LLama
open LLama.Common
open Microsoft.Data.Sqlite

type DomainError =
    | EmptyText of Field: string
    | NegativeInteger of Field: string * Value: int
    | NonPositiveDecimal of Field: string * Value: decimal
    | InvalidHttpStatusCode of Value: int
    | InvalidHttpHeaderName of Value: string
    | InvalidHttpHeaderValue of Name: string
    | StorageFailure of Operation: string * Reason: string
    | RuntimeFailure of Operation: string * Reason: string

    member this.Message =
        match this with
        | EmptyText field -> $"{field} must not be empty."
        | NegativeInteger (field, value) -> $"{field} must be zero or greater; got {value}."
        | NonPositiveDecimal (field, value) -> $"{field} must be greater than zero; got {value}."
        | InvalidHttpStatusCode value -> $"HTTP status code must be between 100 and 599; got {value}."
        | InvalidHttpHeaderName value -> $"HTTP header name is invalid: {value}"
        | InvalidHttpHeaderValue name -> $"HTTP header value for {name} contains invalid control characters."
        | StorageFailure (operation, reason) -> $"{operation} failed: {reason}"
        | RuntimeFailure (operation, reason) -> $"{operation} failed: {reason}"

type ResultBuilder() =
    member _.Bind(value, binder) = Result.bind binder value
    member _.Return(value) = Ok value
    member _.ReturnFrom(value) = value

module ResultSyntax =
    let result = ResultBuilder()

type NonEmptyText = private NonEmptyText of string

module NonEmptyText =
    let create field value =
        if String.IsNullOrWhiteSpace value then
            Error(EmptyText field)
        else
            Ok(NonEmptyText value)

    let value (NonEmptyText text) = text

type NonNegativeInt = private NonNegativeInt of int

module NonNegativeInt =
    let create field value =
        if value < 0 then
            Error(NegativeInteger(field, value))
        else
            Ok(NonNegativeInt value)

    let value (NonNegativeInt number) = number

type PositiveDecimal = private PositiveDecimal of decimal

module PositiveDecimal =
    let create field value =
        if value <= 0m then
            Error(NonPositiveDecimal(field, value))
        else
            Ok(PositiveDecimal value)

    let value (PositiveDecimal number) = number

type HttpStatusCode = private HttpStatusCode of int

module HttpStatusCode =
    let create value =
        if value < 100 || value > 599 then
            Error(InvalidHttpStatusCode value)
        else
            Ok(HttpStatusCode value)

    let value (HttpStatusCode statusCode) = statusCode

type HttpHeaderName = private HttpHeaderName of NonEmptyText

module HttpHeaderName =
    let private forbidden =
        [| '('; ')'; '<'; '>'; '@'; ','; ';'; ':'; '\\'; '"'; '/'; '['; ']'; '?'; '='; '{'; '}'; ' '; '\t'; '\r'; '\n' |]

    let create value =
        match NonEmptyText.create "HTTP header name" value with
        | Error error -> Error error
        | Ok text ->
            let raw = NonEmptyText.value text

            if raw.IndexOfAny forbidden >= 0 then
                Error(InvalidHttpHeaderName raw)
            else
                Ok(HttpHeaderName text)

    let value (HttpHeaderName name) = NonEmptyText.value name

type HttpHeaderValue = private HttpHeaderValue of NonEmptyText

module HttpHeaderValue =
    let create name value =
        match NonEmptyText.create $"HTTP header value for {name}" value with
        | Error error -> Error error
        | Ok text ->
            let raw = NonEmptyText.value text

            if raw.Contains '\r' || raw.Contains '\n' then
                Error(InvalidHttpHeaderValue name)
            else
                Ok(HttpHeaderValue text)

    let value (HttpHeaderValue value) = NonEmptyText.value value

type RequestHeader =
    private
        {
            Name: HttpHeaderName
            Value: HttpHeaderValue
        }

module RequestHeader =
    open ResultSyntax

    let create name value =
        result {
            let! parsedName = HttpHeaderName.create name
            let! parsedValue = HttpHeaderValue.create (HttpHeaderName.value parsedName) value

            return
                {
                    Name = parsedName
                    Value = parsedValue
                }
        }

    let name header = HttpHeaderName.value header.Name
    let value header = HttpHeaderValue.value header.Value

type RequestBody = private RequestBody of NonEmptyText

module RequestBody =
    let create value =
        if String.IsNullOrWhiteSpace value then
            Ok None
        else
            NonEmptyText.create "HTTP request body" value
            |> Result.map (RequestBody >> Some)

    let value (RequestBody body) = NonEmptyText.value body

type HttpMethod =
    | GET
    | POST
    | PATCH

module HttpMethod =
    let display method =
        match method with
        | GET -> "GET"
        | POST -> "POST"
        | PATCH -> "PATCH"

type AuthScheme =
    | OAuth2Pkce
    | Bearer
    | Hmac

module AuthScheme =
    let display scheme =
        match scheme with
        | OAuth2Pkce -> "OAuth 2.0 PKCE"
        | Bearer -> "Bearer"
        | Hmac -> "HMAC"

type RequestStatus =
    | Ready
    | Cached
    | NeedsReview

module RequestStatus =
    let display status =
        match status with
        | Ready -> "Ready"
        | Cached -> "Cached"
        | NeedsReview -> "Needs review"

type ModelMode =
    | Off
    | OnDemand
    | Loading
    | ReadyWarm
    | Busy
    | Unloading
    | Blocked

module ModelMode =
    let display mode =
        match mode with
        | Off -> "Off"
        | OnDemand -> "On-demand"
        | Loading -> "Loading"
        | ReadyWarm -> "Warm"
        | Busy -> "Busy"
        | Unloading -> "Unloading"
        | Blocked -> "Blocked"

type BackendState =
    | DirectMlDecisionPending
    | CpuOnly
    | Sidecar

module BackendState =
    let display backend =
        match backend with
        | DirectMlDecisionPending -> "DirectML decision pending"
        | CpuOnly -> "CPU only"
        | Sidecar -> "Sidecar"

type MemoryState =
    | ModelUnloaded
    | ModelWarm
    | ModelLoaded

module MemoryState =
    let display state =
        match state with
        | ModelUnloaded -> "Model unloaded"
        | ModelWarm -> "Model warm"
        | ModelLoaded -> "Model loaded"

type AiLifecycle =
    | AiOff
    | AiLoading
    | AiReadyWarm
    | AiBusy
    | AiUnloading
    | AiBlocked of NonEmptyText

module AiLifecycle =
    let display state =
        match state with
        | AiOff -> "Off"
        | AiLoading -> "Loading"
        | AiReadyWarm -> "Ready warm"
        | AiBusy -> "Busy"
        | AiUnloading -> "Unloading"
        | AiBlocked reason -> $"Blocked: {NonEmptyText.value reason}"

type HookPhase =
    | Pre
    | Post

module HookPhase =
    let display phase =
        match phase with
        | Pre -> "Pre"
        | Post -> "Post"

type PlatformTarget =
    | WindowsFirst
    | MacOsAndMobileLater

module PlatformTarget =
    let display target =
        match target with
        | WindowsFirst -> "Windows first"
        | MacOsAndMobileLater -> "macOS and mobile later"

type UiHost =
    | MauiBlazorHybrid

module UiHost =
    let display host =
        match host with
        | MauiBlazorHybrid -> "MAUI Blazor Hybrid"

type RequestDraft =
    {
        Method: HttpMethod
        Name: NonEmptyText
        Url: NonEmptyText
        Auth: AuthScheme
        Status: RequestStatus
    }

type ResponseSnapshot =
    {
        StatusCode: HttpStatusCode
        DurationMs: NonNegativeInt
        SizeKb: PositiveDecimal
        Summary: NonEmptyText
    }

type ModelState =
    {
        Mode: ModelMode
        Lifecycle: AiLifecycle
        Backend: BackendState
        Memory: MemoryState
        UnloadPolicy: NonEmptyText
        ColdTargetMb: NonNegativeInt
        WarmTargetMb: NonNegativeInt
    }

type HookState =
    {
        Name: NonEmptyText
        Phase: HookPhase
        Capability: NonEmptyText
        Verdict: NonEmptyText
    }

type PlatformState =
    {
        PrimaryTarget: PlatformTarget
        FutureTargets: PlatformTarget
        UiHost: UiHost
        RenderPolicy: NonEmptyText
    }

type WorkbenchSnapshot =
    {
        Intent: NonEmptyText
        ActiveRequest: RequestDraft
        Requests: RequestDraft list
        Response: ResponseSnapshot
        Model: ModelState
        Hook: HookState
        Platform: PlatformState
    }

type MemoryTelemetry =
    {
        HostProcessMb: NonNegativeInt
        ManagedHeapMb: NonNegativeInt
        WebView2TreeMb: NonNegativeInt  // all msedgewebview2 processes; may over-count if other WebView2 apps run concurrently
        ProcessTreeMb: NonNegativeInt   // HostProcessMb + WebView2TreeMb
        WebViewPolicy: NonEmptyText
    }

type StorageSnapshot =
    {
        DatabasePath: NonEmptyText
        RequestCount: NonNegativeInt
        ResponseBodiesStored: NonNegativeInt
    }

[<CLIMutable>]
type RequestDraftView =
    {
        Method: string
        Name: string
        Url: string
        Auth: string
        Status: string
    }

[<CLIMutable>]
type ResponseSnapshotView =
    {
        StatusCode: int
        DurationMs: int
        SizeKb: decimal
        Summary: string
    }

[<CLIMutable>]
type ModelStateView =
    {
        Mode: string
        Lifecycle: string
        Backend: string
        Memory: string
        UnloadPolicy: string
        ColdTargetMb: int
        WarmTargetMb: int
    }

[<CLIMutable>]
type HookStateView =
    {
        Name: string
        Phase: string
        Capability: string
        Verdict: string
    }

[<CLIMutable>]
type PlatformStateView =
    {
        PrimaryTarget: string
        FutureTargets: string
        UiHost: string
        RenderPolicy: string
    }

[<CLIMutable>]
type WorkbenchSnapshotView =
    {
        Intent: string
        ActiveRequest: RequestDraftView
        Requests: RequestDraftView array
        Response: ResponseSnapshotView
        Model: ModelStateView
        Hook: HookStateView
        Platform: PlatformStateView
    }

[<CLIMutable>]
type MemoryTelemetryView =
    {
        HostProcessMb: int
        ManagedHeapMb: int
        WebView2TreeMb: int
        ProcessTreeMb: int
        WebViewPolicy: string
    }

[<CLIMutable>]
type StorageSnapshotView =
    {
        DatabasePath: string
        RequestCount: int
        ResponseBodiesStored: int
    }

[<CLIMutable>]
type WorkbenchView =
    {
        Snapshot: WorkbenchSnapshotView
        Telemetry: MemoryTelemetryView
        Storage: StorageSnapshotView
    }

[<CLIMutable>]
type HttpHeaderView =
    {
        Enabled: bool
        Name: string
        Value: string
    }

[<CLIMutable>]
type HttpResultView =
    {
        StatusCode: int
        Body: string
        DurationMs: int64
        SizeBytes: int64
        IsError: bool
        ErrorMessage: string
    }

module private RequestDraft =
    open ResultSyntax

    let create (method: HttpMethod) name url (auth: AuthScheme) (status: RequestStatus) : Result<RequestDraft, DomainError> =
        result {
            let! name = NonEmptyText.create "request name" name
            let! url = NonEmptyText.create "request url" url

            return
                ({
                    Method = method
                    Name = name
                    Url = url
                    Auth = auth
                    Status = status
                } : RequestDraft)
        }

module private ResponseSnapshot =
    open ResultSyntax

    let create statusCode durationMs sizeKb summary : Result<ResponseSnapshot, DomainError> =
        result {
            let! statusCode = HttpStatusCode.create statusCode
            let! durationMs = NonNegativeInt.create "response duration" durationMs
            let! sizeKb = PositiveDecimal.create "response size" sizeKb
            let! summary = NonEmptyText.create "response summary" summary

            return
                ({
                    StatusCode = statusCode
                    DurationMs = durationMs
                    SizeKb = sizeKb
                    Summary = summary
                } : ResponseSnapshot)
        }

module private ModelState =
    open ResultSyntax

    let create (mode: ModelMode) (lifecycle: AiLifecycle) (backend: BackendState) (memory: MemoryState) unloadPolicy coldTargetMb warmTargetMb : Result<ModelState, DomainError> =
        result {
            let! unloadPolicy = NonEmptyText.create "model unload policy" unloadPolicy
            let! coldTargetMb = NonNegativeInt.create "cold memory target" coldTargetMb
            let! warmTargetMb = NonNegativeInt.create "warm memory target" warmTargetMb

            return
                ({
                    Mode = mode
                    Lifecycle = lifecycle
                    Backend = backend
                    Memory = memory
                    UnloadPolicy = unloadPolicy
                    ColdTargetMb = coldTargetMb
                    WarmTargetMb = warmTargetMb
                } : ModelState)
        }

module private HookState =
    open ResultSyntax

    let create name (phase: HookPhase) capability verdict : Result<HookState, DomainError> =
        result {
            let! name = NonEmptyText.create "hook name" name
            let! capability = NonEmptyText.create "hook capability" capability
            let! verdict = NonEmptyText.create "hook verdict" verdict

            return
                ({
                    Name = name
                    Phase = phase
                    Capability = capability
                    Verdict = verdict
                } : HookState)
        }

module private PlatformState =
    open ResultSyntax

    let create (primaryTarget: PlatformTarget) (futureTargets: PlatformTarget) (uiHost: UiHost) renderPolicy : Result<PlatformState, DomainError> =
        result {
            let! renderPolicy = NonEmptyText.create "render policy" renderPolicy

            return
                ({
                    PrimaryTarget = primaryTarget
                    FutureTargets = futureTargets
                    UiHost = uiHost
                    RenderPolicy = renderPolicy
                } : PlatformState)
        }

module private Projection =
    let requestToView (request: RequestDraft) : RequestDraftView =
        {
            Method = HttpMethod.display request.Method
            Name = NonEmptyText.value request.Name
            Url = NonEmptyText.value request.Url
            Auth = AuthScheme.display request.Auth
            Status = RequestStatus.display request.Status
        }

    let responseToView (response: ResponseSnapshot) : ResponseSnapshotView =
        {
            StatusCode = HttpStatusCode.value response.StatusCode
            DurationMs = NonNegativeInt.value response.DurationMs
            SizeKb = PositiveDecimal.value response.SizeKb
            Summary = NonEmptyText.value response.Summary
        }

    let modelToView (model: ModelState) : ModelStateView =
        {
            Mode = ModelMode.display model.Mode
            Lifecycle = AiLifecycle.display model.Lifecycle
            Backend = BackendState.display model.Backend
            Memory = MemoryState.display model.Memory
            UnloadPolicy = NonEmptyText.value model.UnloadPolicy
            ColdTargetMb = NonNegativeInt.value model.ColdTargetMb
            WarmTargetMb = NonNegativeInt.value model.WarmTargetMb
        }

    let hookToView (hook: HookState) : HookStateView =
        {
            Name = NonEmptyText.value hook.Name
            Phase = HookPhase.display hook.Phase
            Capability = NonEmptyText.value hook.Capability
            Verdict = NonEmptyText.value hook.Verdict
        }

    let platformToView (platform: PlatformState) : PlatformStateView =
        {
            PrimaryTarget = PlatformTarget.display platform.PrimaryTarget
            FutureTargets = PlatformTarget.display platform.FutureTargets
            UiHost = UiHost.display platform.UiHost
            RenderPolicy = NonEmptyText.value platform.RenderPolicy
        }

    let toView (snapshot: WorkbenchSnapshot) : WorkbenchSnapshotView =
        {
            Intent = NonEmptyText.value snapshot.Intent
            ActiveRequest = requestToView snapshot.ActiveRequest
            Requests = snapshot.Requests |> List.map requestToView |> List.toArray
            Response = responseToView snapshot.Response
            Model = modelToView snapshot.Model
            Hook = hookToView snapshot.Hook
            Platform = platformToView snapshot.Platform
        }

    let errorView (error: DomainError) : WorkbenchSnapshotView =
        let failedRequest: RequestDraftView =
            {
                Method = "GET"
                Name = "Invalid app model"
                Url = "about:blank"
                Auth = "None"
                Status = "Blocked"
            }

        {
            Intent = "App model construction failed."
            ActiveRequest = failedRequest
            Requests = [| failedRequest |]
            Response =
                {
                    StatusCode = 500
                    DurationMs = 0
                    SizeKb = 0m
                    Summary = error.Message
                }
            Model =
                {
                    Mode = "Blocked"
                    Lifecycle = "Blocked"
                    Backend = "Not initialised"
                    Memory = "Model unloaded"
                    UnloadPolicy = "No model may load while app state is invalid"
                    ColdTargetMb = 0
                    WarmTargetMb = 0
                }
            Hook =
                {
                    Name = "none"
                    Phase = "Pre"
                    Capability = "none"
                    Verdict = "Blocked"
                }
            Platform =
                {
                    PrimaryTarget = "Windows first"
                    FutureTargets = "macOS and mobile later"
                    UiHost = "MAUI Blazor Hybrid"
                    RenderPolicy = "Single BlazorWebView"
                }
        }

type WorkbenchSnapshotFactory =
    static member Create() : Result<WorkbenchSnapshot, DomainError> =
        ResultSyntax.result {
            let! active =
                RequestDraft.create
                    GET
                    "GitHub Zen"
                    "https://api.github.com/zen"
                    Bearer
                    Ready

            let! listWorkspaces =
                RequestDraft.create
                    GET
                    "List workspaces"
                    "https://api.minnal.local/v1/workspaces"
                    Bearer
                    Cached

            let! rotateSecret =
                RequestDraft.create
                    PATCH
                    "Rotate secret"
                    "https://api.minnal.local/v1/secrets/current"
                    Hmac
                    NeedsReview

            let! intent =
                NonEmptyText.create
                    "intent"
                    "Explain the last 401 and draft a retry with fresh auth."

            let! response =
                ResponseSnapshot.create
                    401
                    184
                    3.8m
                    "Token expired before replay. Refresh flow is required before retry."

            let! model =
                ModelState.create
                    OnDemand
                    AiOff
                    DirectMlDecisionPending
                    ModelUnloaded
                    "Unload after idle, memory pressure, or workspace switch"
                    350
                    700

            let! hook =
                HookState.create
                    "refresh-token-prehook"
                    Pre
                    "secrets:read, request:mutate"
                    "Dry-run required"

            let! platform =
                PlatformState.create
                    WindowsFirst
                    MacOsAndMobileLater
                    MauiBlazorHybrid
                    "Single BlazorWebView; no hidden secondary WebViews"

            return
                ({
                    Intent = intent
                    ActiveRequest = active
                    Requests = [ active; listWorkspaces; rotateSecret ]
                    Response = response
                    Model = model
                    Hook = hook
                    Platform = platform
                } : WorkbenchSnapshot)
        }

    static member CreateView() =
        match WorkbenchSnapshotFactory.Create() with
        | Ok snapshot -> Projection.toView snapshot
        | Error error -> Projection.errorView error

module private MemoryTelemetry =
    open ResultSyntax

    let fromCurrentProcess () : Result<MemoryTelemetry, DomainError> =
        result {
            let hostMb =
                int (Process.GetCurrentProcess().WorkingSet64 / 1024L / 1024L)
            let managedHeapMb =
                int (GC.GetTotalMemory(false) / 1024L / 1024L)
            // Sum all msedgewebview2 child processes. BlazorWebView spawns 3-5 of these
            // (browser, renderer, GPU, GPU-info, crash handler). May over-count by
            // ~50 MB if another WebView2-based app is running concurrently.
            let webView2Mb =
                Process.GetProcessesByName("msedgewebview2")
                |> Array.sumBy (fun p ->
                    try p.WorkingSet64 / 1024L / 1024L |> int
                    with _ -> 0)
            let treeMb = hostMb + webView2Mb

            let! hostProcessMb = NonNegativeInt.create "host process MB" hostMb
            let! managedHeapMb = NonNegativeInt.create "managed heap MB" managedHeapMb
            let! webView2TreeMb = NonNegativeInt.create "WebView2 tree MB" webView2Mb
            let! processTreeMb = NonNegativeInt.create "process tree MB" treeMb
            let! webViewPolicy =
                NonEmptyText.create
                    "WebView policy"
                    "Single BlazorWebView; no hidden secondary WebViews"

            return
                ({
                    HostProcessMb = hostProcessMb
                    ManagedHeapMb = managedHeapMb
                    WebView2TreeMb = webView2TreeMb
                    ProcessTreeMb = processTreeMb
                    WebViewPolicy = webViewPolicy
                } : MemoryTelemetry)
        }

module private StorageSnapshot =
    open ResultSyntax

    let create databasePath requestCount responseBodiesStored : Result<StorageSnapshot, DomainError> =
        result {
            let! databasePath = NonEmptyText.create "SQLite database path" databasePath
            let! requestCount = NonNegativeInt.create "request count" requestCount
            let! responseBodiesStored =
                NonNegativeInt.create "response bodies stored" responseBodiesStored

            return
                ({
                    DatabasePath = databasePath
                    RequestCount = requestCount
                    ResponseBodiesStored = responseBodiesStored
                } : StorageSnapshot)
        }

module private ViewProjection =
    let memoryTelemetryToView (telemetry: MemoryTelemetry) : MemoryTelemetryView =
        {
            HostProcessMb = NonNegativeInt.value telemetry.HostProcessMb
            ManagedHeapMb = NonNegativeInt.value telemetry.ManagedHeapMb
            WebView2TreeMb = NonNegativeInt.value telemetry.WebView2TreeMb
            ProcessTreeMb = NonNegativeInt.value telemetry.ProcessTreeMb
            WebViewPolicy = NonEmptyText.value telemetry.WebViewPolicy
        }

    let storageToView (storage: StorageSnapshot) : StorageSnapshotView =
        {
            DatabasePath = NonEmptyText.value storage.DatabasePath
            RequestCount = NonNegativeInt.value storage.RequestCount
            ResponseBodiesStored = NonNegativeInt.value storage.ResponseBodiesStored
        }

type AiRuntimeState =
    | AiOffRuntime
    | AiLoadingRuntime
    | AiReadyRuntime
    | AiBlockedRuntime of NonEmptyText

module private AiRuntimeState =
    let display state =
        match state with
        | AiOffRuntime -> "Off"
        | AiLoadingRuntime -> "Loading"
        | AiReadyRuntime -> "Ready"
        | AiBlockedRuntime reason -> $"Blocked: {NonEmptyText.value reason}"

    let isReady state =
        match state with
        | AiReadyRuntime -> true
        | AiOffRuntime
        | AiLoadingRuntime
        | AiBlockedRuntime _ -> false

type BundledGgufPath = private BundledGgufPath of string

module private BundledGgufPath =
    let value (BundledGgufPath path) = path

    let findFirstIn root : Result<BundledGgufPath, DomainError> =
        try
            Directory.GetFiles(root, "*.gguf", SearchOption.AllDirectories)
            |> Array.sortBy (fun path ->
                let name = Path.GetFileName(path)

                if name.StartsWith("mmproj-", StringComparison.OrdinalIgnoreCase) then
                    1, name
                else
                    0, name)
            |> Array.tryHead
            |> function
                | Some path -> Ok(BundledGgufPath path)
                | None -> Error(RuntimeFailure("find bundled GGUF", $"No .gguf file was found under app directory {root}"))
        with ex ->
            Error(RuntimeFailure("find bundled GGUF", ex.Message))

    let findFirst () : Result<BundledGgufPath, DomainError> =
        findFirstIn AppContext.BaseDirectory

module private HttpResultView =
    let create statusCode body durationMs sizeBytes isError errorMessage : HttpResultView =
        {
            StatusCode = statusCode
            Body = body
            DurationMs = durationMs
            SizeBytes = sizeBytes
            IsError = isError
            ErrorMessage = errorMessage
        }

// ⚠️ Interop port: MAUI/Razor consumes this from C#-generated component code.
// Correct F# idiom remains the DU/state modules above; this is a projection boundary.
type IAiService =
    abstract member State: string
    abstract member StatusText: string
    abstract member IsReady: bool
    abstract member LoadAsync: unit -> Task
    abstract member ExplainAsync: prompt: string -> Task<string>

// ⚠️ Interop port: Razor needs a service-shaped port. Domain callers should prefer
// pure request descriptions plus an executor boundary returning Result.
type IHttpExecutionService =
    abstract member SendAsync: methodText: string * url: string * headers: HttpHeaderView array * bodyText: string -> Task<HttpResultView>

type AiModelRoot = private AiModelRoot of string

module AiModelRoot =
    let bundledAppContent = AiModelRoot AppContext.BaseDirectory

    let create path =
        if String.IsNullOrWhiteSpace path then
            Error(EmptyText "AI model root")
        else
            Ok(AiModelRoot path)

    let value (AiModelRoot path) = path

// ⚠️ Mutable lifetime adapter: the llama.cpp model handle is stateful, disposable, and
// expensive. Mutation is confined to this singleton boundary; no domain type depends on it.
type LlamaAiService(modelRoot: AiModelRoot) =
    let gate = obj()
    let mutable runtimeState = AiOffRuntime
    let mutable statusText = "Bundled model not loaded"
    let mutable weights: LLamaWeights option = None
    let mutable modelParams: ModelParams option = None

    let setState state text =
        lock gate (fun () ->
            runtimeState <- state
            statusText <- text)

    let currentState () =
        lock gate (fun () -> runtimeState, statusText, weights, modelParams)

    let loadSync () =
        setState AiLoadingRuntime "Scanning bundled app content for GGUF..."

        match BundledGgufPath.findFirstIn (AiModelRoot.value modelRoot) with
        | Error error ->
            match NonEmptyText.create "AI blocked reason" error.Message with
            | Ok reason -> setState (AiBlockedRuntime reason) error.Message
            | Error _ -> setState AiOffRuntime "AI model discovery failed"
        | Ok gguf ->
            try
                let path = BundledGgufPath.value gguf
                let loadedParams = ModelParams(path)
                loadedParams.ContextSize <- 512u
                loadedParams.GpuLayerCount <- 0

                let loadedWeights = LLamaWeights.LoadFromFile(loadedParams)

                lock gate (fun () ->
                    weights <- Some loadedWeights
                    modelParams <- Some loadedParams
                    runtimeState <- AiReadyRuntime
                    statusText <- Path.GetFileName(path))
            with ex ->
                match NonEmptyText.create "AI load failure" ex.Message with
                | Ok reason -> setState (AiBlockedRuntime reason) $"Load failed: {ex.Message}"
                | Error _ -> setState AiOffRuntime "AI load failed"

    new() = new LlamaAiService(AiModelRoot.bundledAppContent)

    interface IAiService with
        member _.State =
            let state, _, _, _ = currentState ()
            AiRuntimeState.display state

        member _.StatusText =
            let _, text, _, _ = currentState ()
            text

        member _.IsReady =
            let state, _, _, _ = currentState ()
            AiRuntimeState.isReady state

        member _.LoadAsync() =
            Task.Run(fun () -> loadSync ())

        member _.ExplainAsync(prompt: string) =
            task {
                let state, text, loadedWeights, loadedParams = currentState ()

                match state, loadedWeights, loadedParams with
                | AiReadyRuntime, Some weights, Some modelParams ->
                    let executor = StatelessExecutor(weights, modelParams)

                    let inferenceParams =
                        InferenceParams(
                            MaxTokens = 64,
                            AntiPrompts = [| "\n\nUser:"; "###"; "<|end|>" |])

                    let fullPrompt =
                        $"Explain this HTTP response briefly:\n{prompt}\n\nExplanation:"

                    let tokens = executor.InferAsync(fullPrompt, inferenceParams)
                    let enumerator = tokens.GetAsyncEnumerator()
                    let builder = StringBuilder()

                    try
                        let mutable hasToken = true

                        while hasToken do
                            let! moved = enumerator.MoveNextAsync().AsTask()
                            hasToken <- moved

                            if moved then
                                builder.Append(enumerator.Current) |> ignore
                    finally
                        enumerator.DisposeAsync().AsTask().Wait()

                    return builder.ToString().Trim()

                | _ ->
                    return $"[AI {AiRuntimeState.display state}: {text}]"
            }

    interface IDisposable with
        member _.Dispose() =
            let loadedWeights =
                lock gate (fun () ->
                    let loaded = weights
                    weights <- None
                    modelParams <- None
                    runtimeState <- AiOffRuntime
                    statusText <- "Disposed"
                    loaded)

            loadedWeights
            |> Option.iter (fun model -> model.Dispose())
// ⚠️ Adapter: HttpClient is class/disposable based. Request execution is an effect boundary;
// domain logic must still describe requests as values before reaching this adapter.
type HttpExecutionService() =
    let client =
        new HttpClient(
            new HttpClientHandler(AllowAutoRedirect = true),
            Timeout = TimeSpan.FromSeconds(30.0))

    let parseHeaders (headers: HttpHeaderView array) =
        let source =
            if isNull headers then
                Array.empty
            else
                headers

        let folder state (header: HttpHeaderView) =
            match state with
            | Error error -> Error error
            | Ok parsed ->
                if isNull (box header) || not header.Enabled then
                    Ok parsed
                elif String.IsNullOrWhiteSpace header.Name && String.IsNullOrWhiteSpace header.Value then
                    Ok parsed
                else
                    match RequestHeader.create header.Name header.Value with
                    | Ok requestHeader -> Ok(requestHeader :: parsed)
                    | Error error -> Error error

        source
        |> Array.fold folder (Ok [])
        |> Result.map List.rev

    let addHeader (request: HttpRequestMessage) header =
        let name = RequestHeader.name header
        let value = RequestHeader.value header

        if request.Headers.TryAddWithoutValidation(name, value) then
            Ok()
        elif isNull request.Content then
            Error(RuntimeFailure("add HTTP header", $"{name} requires request content, but the body is empty."))
        elif name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) then
            request.Content.Headers.Remove name |> ignore

            if request.Content.Headers.TryAddWithoutValidation(name, value) then
                Ok()
            else
                Error(RuntimeFailure("add HTTP header", $"{name} is not valid for content headers."))
        elif request.Content.Headers.TryAddWithoutValidation(name, value) then
            Ok()
        else
            Error(RuntimeFailure("add HTTP header", $"{name} is not valid for request headers in the current request shape."))

    interface IHttpExecutionService with
        member _.SendAsync(methodText: string, url: string, headers: HttpHeaderView array, bodyText: string) =
            task {
                let sw = Stopwatch.StartNew()

                try
                    use request = new HttpRequestMessage(HttpMethod(methodText), url)
                    request.Headers.UserAgent.ParseAdd("Minnal/0.1 (spike)")

                    match RequestBody.create bodyText with
                    | Error error ->
                        sw.Stop()

                        return
                            HttpResultView.create
                                0
                                ""
                                sw.ElapsedMilliseconds
                                0L
                                true
                                error.Message
                    | Ok body ->
                        body
                        |> Option.iter (fun requestBody ->
                            request.Content <- new StringContent(RequestBody.value requestBody, Encoding.UTF8, "application/json"))

                        let headerResult =
                            match parseHeaders headers with
                            | Error error -> Error error
                            | Ok parsedHeaders ->
                                parsedHeaders
                                |> List.fold
                                    (fun state header ->
                                        match state with
                                        | Error error -> Error error
                                        | Ok () -> addHeader request header)
                                    (Ok())

                        match headerResult with
                        | Error error ->
                            sw.Stop()

                            return
                                HttpResultView.create
                                    0
                                    ""
                                    sw.ElapsedMilliseconds
                                    0L
                                    true
                                    error.Message
                        | Ok _ ->
                            use! response = client.SendAsync(request)
                            let! responseBody = response.Content.ReadAsStringAsync()
                            sw.Stop()

                            return
                                HttpResultView.create
                                    (int response.StatusCode)
                                    responseBody
                                    sw.ElapsedMilliseconds
                                    (int64 (Encoding.UTF8.GetByteCount responseBody))
                                    (not response.IsSuccessStatusCode)
                                    ""
                with ex ->
                    sw.Stop()

                    return
                        HttpResultView.create
                            0
                            ""
                            sw.ElapsedMilliseconds
                            0L
                            true
                            ex.Message
            }

    interface IDisposable with
        member _.Dispose() = client.Dispose()

// ⚠️ Interop adapter: ADO.NET is class/disposable based. The F# domain above stays in DUs,
// smart constructors, and pure projection modules; this class is the database boundary.
type SqliteWorkbenchStore(databasePath: string) =
    let connectionString = $"Data Source={databasePath}"

    let execute operation action =
        try
            Ok(action ())
        with ex ->
            Error(StorageFailure(operation, ex.Message))

    member _.EnsureCreated() : Result<unit, DomainError> =
        execute "create SQLite schema" (fun () ->
            Directory.CreateDirectory(Path.GetDirectoryName(databasePath)) |> ignore

            use connection = new SqliteConnection(connectionString)
            connection.Open()

            use command = connection.CreateCommand()
            command.CommandText <-
                """
                CREATE TABLE IF NOT EXISTS requests (
                    id TEXT PRIMARY KEY,
                    method TEXT NOT NULL,
                    name TEXT NOT NULL,
                    url TEXT NOT NULL,
                    auth_scheme TEXT NOT NULL,
                    status TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS response_bodies (
                    sha256 TEXT PRIMARY KEY,
                    body TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS responses (
                    id TEXT PRIMARY KEY,
                    request_id TEXT NOT NULL,
                    status_code INTEGER NOT NULL,
                    duration_ms INTEGER NOT NULL,
                    size_kb REAL NOT NULL,
                    summary TEXT NOT NULL,
                    body_sha256 TEXT NOT NULL,
                    FOREIGN KEY(request_id) REFERENCES requests(id),
                    FOREIGN KEY(body_sha256) REFERENCES response_bodies(sha256)
                );
                """
            command.ExecuteNonQuery() |> ignore)

    member _.SeedDemo() : Result<unit, DomainError> =
        execute "seed SQLite demo data" (fun () ->
            use connection = new SqliteConnection(connectionString)
            connection.Open()

            use command = connection.CreateCommand()
            command.CommandText <-
                """
                INSERT OR IGNORE INTO requests(id, method, name, url, auth_scheme, status)
                VALUES
                    ('github-zen', 'GET', 'GitHub Zen', 'https://api.github.com/zen', 'Bearer', 'Ready'),
                    ('list-workspaces', 'GET', 'List workspaces', 'https://api.minnal.local/v1/workspaces', 'Bearer', 'Cached'),
                    ('rotate-secret', 'PATCH', 'Rotate secret', 'https://api.minnal.local/v1/secrets/current', 'HMAC', 'Needs review');

                INSERT OR IGNORE INTO response_bodies(sha256, body)
                VALUES
                    ('demo-body-sha256', '{"message":"hit Send to fetch a live response"}');

                INSERT OR IGNORE INTO responses(id, request_id, status_code, duration_ms, size_kb, summary, body_sha256)
                VALUES
                    ('github-zen-response', 'github-zen', 0, 0, 0.0, 'Hit Send to execute the request.', 'demo-body-sha256');
                """
            command.ExecuteNonQuery() |> ignore)

    member _.Snapshot() : Result<StorageSnapshot, DomainError> =
        try
            use connection = new SqliteConnection(connectionString)
            connection.Open()

            let scalarInt sql =
                use command = connection.CreateCommand()
                command.CommandText <- sql
                command.ExecuteScalar() :?> int64 |> int

            let requestCount = scalarInt "SELECT COUNT(*) FROM requests;"
            let responseBodiesStored = scalarInt "SELECT COUNT(*) FROM response_bodies;"

            StorageSnapshot.create databasePath requestCount responseBodiesStored
        with ex ->
            Error(StorageFailure("read SQLite snapshot", ex.Message))

// ⚠️ MAUI DI interop port. Correct F# core shape is the pure WorkbenchSnapshot +
// projection pipeline above; this interface exists so C# host glue can resolve it.
type IWorkbenchService =
    abstract member GetView: unit -> WorkbenchView

// ⚠️ MAUI DI interop adapter. Keep side effects here: telemetry, filesystem, SQLite.
// Domain construction still returns Result and never throws for control flow.
type WorkbenchService() =
    let databasePath =
        Path.Combine(
            AppContext.BaseDirectory,
            "state",
            "maui-spike.sqlite")

    let store = SqliteWorkbenchStore(databasePath)

    let blockedView error : WorkbenchView =
        let snapshot = Projection.errorView error
        let telemetry: MemoryTelemetryView =
            match MemoryTelemetry.fromCurrentProcess () with
            | Ok telemetry -> ViewProjection.memoryTelemetryToView telemetry
            | Error telemetryError ->
                {
                    HostProcessMb = 0
                    ManagedHeapMb = 0
                    WebView2TreeMb = 0
                    ProcessTreeMb = 0
                    WebViewPolicy = telemetryError.Message
                }
        let storage: StorageSnapshotView =
            {
                DatabasePath = databasePath
                RequestCount = 0
                ResponseBodiesStored = 0
            }

        {
            Snapshot = snapshot
            Telemetry = telemetry
            Storage = storage
        }

    interface IWorkbenchService with
        member _.GetView() =
            ResultSyntax.result {
                do! store.EnsureCreated()
                do! store.SeedDemo()
                let! snapshot = WorkbenchSnapshotFactory.Create()
                let! telemetry = MemoryTelemetry.fromCurrentProcess()
                let! storage = store.Snapshot()

                return
                    ({
                        Snapshot = Projection.toView snapshot
                        Telemetry = ViewProjection.memoryTelemetryToView telemetry
                        Storage = ViewProjection.storageToView storage
                    } : WorkbenchView)
            }
            |> function
                | Ok view -> view
                | Error error -> blockedView error
