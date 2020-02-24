module ReadOneEvent

open Infrastructure

open EventStore.ClientAPI
open System.Text

let AsyncRead1 (streamName: string) (eventNumber: int64) (resolveLinkTos: bool) =
    async {
        let conn = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113")

        do! conn.ConnectAsync() |> Async.AwaitTaskCorrect

        return! conn.ReadEventAsync(streamName, eventNumber, resolveLinkTos) |> Async.AwaitTaskCorrect
    }

let decode (readResult: EventReadResult) = readResult.Event.Value.Event.Data |> Encoding.UTF8.GetString
