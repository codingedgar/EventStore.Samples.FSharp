module ReadOneEventTest

open FsUnit.Xunit
open FsUnit.CustomMatchers
open Xunit

open CommonTests
open Infrastructure

open ReadOneEvent

let defaultEventArgs() =
    let id = System.Guid.NewGuid()
    let data = System.Text.Encoding.UTF8.GetBytes """{ReadOneEvent-test: 0}"""
    let metadata = System.Text.Encoding.UTF8.GetBytes ""
    let typeName = "ReadOneEvent"
    let isJson = true // with this it shows nicely in the web, even if it isn't JSON 😅

    (id, typeName, isJson, data, metadata)

let beforeEach() =
    let streamName = "WtriteOneEvent-test-" + System.Guid.NewGuid().ToString()
    let event = [| EventStore.ClientAPI.EventData(defaultEventArgs()) |]
    let version = int64 (EventStore.ClientAPI.ExpectedVersion.Any)

    (streamName, version, event)

[<Fact>]
let ``try async read 1``() =
    let (streamName, version, event) = beforeEach()

    let writeFn() =
        AsyncWrite5 (streamName) (version) (event)
        |> Async.RunSynchronously
        |> ignore

    let readFn() =
        AsyncRead1 streamName (int64 0) false
        |> Async.RunSynchronously
        |> decode


    // debugResponse2 streamName version writeFn
    // debugResponse2 streamName version readFn
    
    writeFn()
    readFn() |> should equal "{ReadOneEvent-test: 0}"
