module WriteOneEvent

open Infrastructure

open EventStore.ClientAPI

let AsyncWrite1 (streamName: string) (version: int64) (events: EventData []) =
    // following https://eventstore.com/docs/getting-started/
    async {

        let conn = EventStoreConnection.Create("tcp://admin:changeit@localhost:1113", "InputFromFileConsoleApp")
        // This should be enough to connect according to the "Getting Started"
        // https://eventstore.com/docs/getting-started/?tabs=tabid-1%2Ctabid-dotnet-client%2Ctabid-dotnet-client-connect%2Ctabid-4#connecting-to-event-store
        // this fails 🤔

        return! conn.AppendToStreamAsync(streamName, version, events) |> Async.AwaitTask
    }

let AsyncWrite2 (streamName: string) (version: int64) (events: EventData []) =
    // using connectTo
    async {

        let conn =
            EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113", "InputFromFileConsoleApp")
        // ConnecTo is in the "connecting to a server section
        // https://eventstore.com/docs/dotnet-api/connecting-to-a-server/index.html#connection-string
        // new error message 👏

        return! conn.AppendToStreamAsync(streamName, version, events) |> Async.AwaitTask
    }

let AsyncWrite3 (streamName: string) (version: int64) (events: EventData []) =
    async {
        let conn = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113")
        // second parameter removed
        // new error message 👏

        return! conn.AppendToStreamAsync(streamName, version, events) |> Async.AwaitTask
    }

let AsyncWrite4 (streamName: string) (version: int64) (events: EventData []) =
    async {
        let conn = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113")

        do! conn.ConnectAsync() |> Async.AwaitTask
        // ConnectAsync
        // https://eventstore.com/docs/dotnet-api/index.html?q=ConnectAsync#quick-start
        // this works!
        return! conn.AppendToStreamAsync(streamName, version, events) |> Async.AwaitTask
    }

let AsyncWrite5 (streamName: string) (version: int64) (events: EventData []) =
    async {
        let conn = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113")

        do! conn.ConnectAsync() |> Async.AwaitTask
        return! conn.AppendToStreamAsync(streamName, version, events) |> Async.AwaitTaskCorrect
        // await task correctly and bubbles the right error
    }

