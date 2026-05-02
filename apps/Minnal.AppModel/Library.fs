namespace Minnal.AppModel

open System

type DomainError =
    | EmptyText of Field: string
    | NegativeInteger of Field: string * Value: int
    | NonPositiveDecimal of Field: string * Value: decimal
    | InvalidHttpStatusCode of Value: int

    member this.Message =
        match this with
        | EmptyText field -> $"{field} must not be empty."
        | NegativeInteger (field, value) -> $"{field} must be zero or greater; got {value}."
        | NonPositiveDecimal (field, value) -> $"{field} must be greater than zero; got {value}."
        | InvalidHttpStatusCode value -> $"HTTP status code must be between 100 and 599; got {value}."

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

    let create (mode: ModelMode) (backend: BackendState) (memory: MemoryState) unloadPolicy coldTargetMb warmTargetMb : Result<ModelState, DomainError> =
        result {
            let! unloadPolicy = NonEmptyText.create "model unload policy" unloadPolicy
            let! coldTargetMb = NonNegativeInt.create "cold memory target" coldTargetMb
            let! warmTargetMb = NonNegativeInt.create "warm memory target" warmTargetMb

            return
                ({
                    Mode = mode
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
                    POST
                    "Create token"
                    "https://api.minnal.local/v1/tokens"
                    OAuth2Pkce
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
