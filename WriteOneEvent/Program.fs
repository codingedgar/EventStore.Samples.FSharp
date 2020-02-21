module WriteOneEvent

open System.Diagnostics
open System.Threading.Tasks

open EventStore.ClientAPI

type Async with
    /// <summary>
    ///     Gets the result of given task so that in the event of exception
    ///     the actual user exception is raised as opposed to being wrapped
    ///     in a System.AggregateException.
    /// </summary>
    /// <param name="task">Task to be awaited.</param>
    [<DebuggerStepThrough>]
    static member AwaitTaskCorrect(task: Task<'T>): Async<'T> =
        Async.FromContinuations(fun (sc, ec, _) ->
            task.ContinueWith(fun (t: Task<'T>) ->
                if t.IsFaulted then
                    let e = t.Exception
                    if e.InnerExceptions.Count = 1 then ec e.InnerExceptions.[0] else ec e
                elif t.IsCanceled then
                    ec (new TaskCanceledException())
                else
                    sc t.Result)
            |> ignore)

    [<DebuggerStepThrough>]
    static member AwaitTaskCorrect(task: Task): Async<unit> =
        Async.FromContinuations(fun (sc, ec, _) ->
            task.ContinueWith(fun (task: Task) ->
                if task.IsFaulted then
                    let e = task.Exception
                    if e.InnerExceptions.Count = 1 then ec e.InnerExceptions.[0] else ec e
                elif task.IsCanceled then
                    ec (TaskCanceledException())
                else
                    sc())
            |> ignore)

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
        return! conn.AppendToStreamAsync(streamName, version, events) |> Async.AwaitTaskCorrect
    }

let AsyncWrite5 (streamName: string) (version: int64) (events: EventData []) =
    async {
        let conn = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113")

        do! conn.ConnectAsync() |> Async.AwaitTask
        return! conn.AppendToStreamAsync(streamName, version, events) |> Async.AwaitTaskCorrect
        // await task correctly and bubbles the right error
    }

