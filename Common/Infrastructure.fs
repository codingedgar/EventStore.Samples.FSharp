namespace Infrastructure


[<AutoOpen>]
module Infrastructure =
    open System.Diagnostics
    open System.Threading.Tasks
    open EventStore.ClientAPI

    let tee f x =
        f x
        x

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
    
    let AsyncWrite5 (streamName: string) (version: int64) (events: EventData []) =
        async {
            let conn = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@localhost:1113")

            do! conn.ConnectAsync() |> Async.AwaitTask
            return! conn.AppendToStreamAsync(streamName, version, events) |> Async.AwaitTaskCorrect
            // await task correctly and bubbles the right error
        }
