module Minnal.AppModel.Tests

open System
open Expecto
open FsCheck
open Minnal.AppModel

let private assertOk (result: Result<'a, DomainError>) =
    match result with
    | Ok value -> value
    | Error error -> failtest error.Message

let domainPrimitiveTests =
    testList
        "Domain primitives"
        [
            testCase "NonEmptyText.create accepts exactly non-whitespace text" <| fun _ ->
                let property (text: string) =
                    match String.IsNullOrWhiteSpace text, NonEmptyText.create "text" text with
                    | true, Error (EmptyText "text") -> true
                    | false, Ok value -> NonEmptyText.value value = text
                    | _ -> false

                Check.QuickThrowOnFailure property

            testCase "NonNegativeInt rejects negative integers" <| fun _ ->
                match NonNegativeInt.create "count" -1 with
                | Error (NegativeInteger ("count", -1)) -> ()
                | other -> failtestf "Expected NegativeInteger, got %A" other

            testCase "HttpStatusCode rejects values outside HTTP status range" <| fun _ ->
                match HttpStatusCode.create 99 with
                | Error (InvalidHttpStatusCode 99) -> ()
                | other -> failtestf "Expected InvalidHttpStatusCode, got %A" other
        ]

let workbenchTests =
    testList
        "Workbench"
        [
            testCase "WorkbenchSnapshotFactory creates GitHub Zen active request" <| fun _ ->
                let snapshot = WorkbenchSnapshotFactory.Create() |> assertOk
                Expect.equal (HttpMethod.display snapshot.ActiveRequest.Method) "GET" "active method"
                Expect.equal (NonEmptyText.value snapshot.ActiveRequest.Name) "GitHub Zen" "active request name"
                Expect.equal (NonEmptyText.value snapshot.ActiveRequest.Url) "https://api.github.com/zen" "active URL"

            testCase "WorkbenchService returns seeded storage counts" <| fun _ ->
                let service = WorkbenchService() :> IWorkbenchService
                let view = service.GetView()
                Expect.isGreaterThanOrEqual view.Storage.RequestCount 3 "request seed count"
                Expect.isGreaterThanOrEqual view.Storage.ResponseBodiesStored 1 "body seed count"
        ]

let aiTests =
    testList
        "Bundled AI"
        [
            testTask "LlamaAiService blocks cleanly when no GGUF is bundled" {
                let service = new LlamaAiService()
                let ai = service :> IAiService

                try
                    do! ai.LoadAsync()

                    Expect.isFalse ai.IsReady "AI must not be ready without bundled GGUF"
                    Expect.stringContains ai.State "Blocked" "state names the block"

                    let! explanation = ai.ExplainAsync("hello")
                    Expect.stringContains explanation "[AI" "blocked explanation reports AI state"
                finally
                    (service :> IDisposable).Dispose()
            }
        ]

let httpTests =
    testList
        "HTTP execution"
        [
            testTask "HttpExecutionService reports invalid URL as effect error" {
                let service = new HttpExecutionService()
                let http = service :> IHttpExecutionService

                try
                    let! result = http.SendAsync("GET", "http://[::1")

                    Expect.isTrue result.IsError "invalid URL is an error result"
                    Expect.equal result.StatusCode 0 "no HTTP status exists for construction failure"
                    Expect.isNonEmpty result.ErrorMessage "error message is projected"
                finally
                    (service :> IDisposable).Dispose()
            }
        ]

[<EntryPoint>]
let main args =
    testList
        "Minnal.AppModel"
        [
            domainPrimitiveTests
            workbenchTests
            aiTests
            httpTests
        ]
    |> testSequenced
    |> runTestsWithCLIArgs [] args
