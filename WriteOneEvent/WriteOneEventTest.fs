open FsUnit.Xunit
open FsUnit.CustomMatchers
open Xunit
open CommonTests

open WriteOneEvent

let beforeEach () =
    let streamName = "WtriteOneEvent-test-" + System.Guid.NewGuid().ToString()
    let event = [|EventStore.ClientAPI.EventData (defaultEventArgs())|]
    let version = int64(EventStore.ClientAPI.ExpectedVersion.Any)

    (streamName, version, event)

[<Fact>]
let ``try async write 1``() =
    let (streamName, version, event) = beforeEach()

    let f () = AsyncWrite1 (streamName) (version) (event)
               |> Async.RunSynchronously
               |> ignore
    
    // debugResponse f
    
    f       
    |> should (throwWithMessage "Format of the initialization string does not conform to specification starting at index 0.") typeof<System.ArgumentException>
   
[<Fact>]
let ``try async write 2``() =
    let (streamName, version, event) = beforeEach()

    let f () = AsyncWrite2 (streamName) (version) (event)
               |> Async.RunSynchronously
               |> ignore

    // debugResponse f
    
    f
    |> should (throwWithMessage "One or more errors occurred. (EventStoreConnection 'InputFromFileConsoleApp' is not active.)") typeof<System.AggregateException>

[<Fact>]
let ``try async write 3``() =
    let (streamName, version, event) = beforeEach()

    let f () = AsyncWrite3 (streamName) (version) (event)
               |> Async.RunSynchronously
               |> ignore
    
    // debugResponse f
    
    f
    |> should (throwWithRegexMessage @"EventStoreConnection 'ES-.{36}' is not active.") typeof<System.AggregateException>

[<Fact>]
let ``try async write 4``() =

    let (streamName, version, event) = beforeEach()

    let f () = AsyncWrite4 (streamName) (version) (event)
                |> Async.RunSynchronously
                |> ignore

    // debugResponse f
    
    f()
    |> should equal null

[<Fact>]
let ``version EmptyStream same event``() =

    let (streamName, _, event0) = beforeEach()
    let version = int64(EventStore.ClientAPI.ExpectedVersion.EmptyStream)
   
    let f (event) ()=
                AsyncWrite4 streamName version event
                |> Async.RunSynchronously

    // debugResponse2 streamName version (f event0)
    // debugResponse2 streamName version (f event0)
    // debugResponse2 streamName version (f event0)
    
    f event0 ()
    |> should not' throw
    
    f event0 ()
    |> should not' throw

    f event0 ()
    |> should not' throw
    // If I send the same event, it doest thow, and only appends one event (or at least the stream length 1).
    // weid, but lets consider that a feature (?) I do not see any case when this is not useful.

[<Fact>]
let ``version EmptyStream diferent event data same id``() =

    let (streamName, _, _) = beforeEach()
    let version = int64(EventStore.ClientAPI.ExpectedVersion.EmptyStream)
    
    let (a1,a2,a3,_,a5) = defaultEventArgs()
    
    let args0 = (a1,a2,a3,(System.Text.Encoding.UTF8.GetBytes "one"),a5)
    let args1 = (a1,a2,a3,(System.Text.Encoding.UTF8.GetBytes "two"),a5)
    let args2 = (a1,a2,a3,(System.Text.Encoding.UTF8.GetBytes "thre"),a5)

    let event0 = [| EventStore.ClientAPI.EventData args0 |]
    let event1 = [| EventStore.ClientAPI.EventData args1 |]
    let event2 = [| EventStore.ClientAPI.EventData args2 |]

    let f (event) ()=
                AsyncWrite4 streamName version event
                |> Async.RunSynchronously
                |> ignore

    // debugResponse2 streamName version (f event0)
    // debugResponse2 streamName version (f event1)
    // debugResponse2 streamName version (f event2)
    
    f event0 ()
    |> should equal null
    
    f event1 ()
    |> should equal null
    
    f event2 ()
    |> should equal null
    // Writes only the first one, maybe it does something with the id.

[<Fact>]
let ``version EmptyStream diferent id same data``() =

    let (streamName, _, _) = beforeEach()
    let version = int64(EventStore.ClientAPI.ExpectedVersion.EmptyStream)
    
    let (_,a2,a3,a4,a5) = defaultEventArgs()
    
    let args0 = (System.Guid.NewGuid(),a2,a3, a4,a5)
    let args1 = (System.Guid.NewGuid(),a2,a3, a4,a5)
    let args2 = (System.Guid.NewGuid(),a2,a3, a4,a5)

    let event0 = [| EventStore.ClientAPI.EventData args0 |]
    let event1 = [| EventStore.ClientAPI.EventData args1 |]
    let event2 = [| EventStore.ClientAPI.EventData args2 |]

    let f event ()=
                AsyncWrite4 streamName version event
                |> Async.RunSynchronously
                |> ignore
    
    // debugResponse2 streamName version (f event0)
    // debugResponse2 streamName version (f event1)
    // debugResponse2 streamName version (f event2)

    f event0 ()
    |> should equal null
    
    f event1
    |> should (throwWithRegexMessage @"One or more errors occurred. \(Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -1, Current version: 0\)") typeof<System.AggregateException>
    
    f event2
    |> should (throwWithRegexMessage @"One or more errors occurred. \(Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -1, Current version: 0\)") typeof<System.AggregateException>
    // I'll conclude that ExpectedVersion.EmptyStream
    // behaves as expected, and that the id of the event is relevant
    // same id makes writing indempotent by not writig again.

[<Fact>]
let ``async write 5 correr erros``() =

    let (streamName, _, _) = beforeEach()
    let version = int64(EventStore.ClientAPI.ExpectedVersion.EmptyStream)
    
    let (_,a2,a3,a4,a5) = defaultEventArgs()
    
    let args0 = (System.Guid.NewGuid(),a2,a3, a4,a5)
    let args1 = (System.Guid.NewGuid(),a2,a3, a4,a5)
    let args2 = (System.Guid.NewGuid(),a2,a3, a4,a5)

    let event0 = [| EventStore.ClientAPI.EventData args0 |]
    let event1 = [| EventStore.ClientAPI.EventData args1 |]
    let event2 = [| EventStore.ClientAPI.EventData args2 |]

    let f event ()=
                AsyncWrite5 streamName version event
                |> Async.RunSynchronously
                |> ignore
    
    // debugResponse2 streamName version (f event0)
    // debugResponse2 streamName version (f event1)
    // debugResponse2 streamName version (f event2)

    f event0 ()
    |> should equal null
    
    f event1
    |> should (throwWithRegexMessage @"Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -1, Current version: 0") typeof<EventStore.ClientAPI.Exceptions.WrongExpectedVersionException>
    
    f event2
    |> should (throwWithRegexMessage @"Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -1, Current version: 0") typeof<EventStore.ClientAPI.Exceptions.WrongExpectedVersionException>
    // I'll conclude that ExpectedVersion.EmptyStream
    // behaves as expected, and that the id of the event is relevant
    // same id makes writing indempotent by not writig again.

[<Fact>]
let ``version NoStream``() =

    let (streamName, _, event0) = beforeEach()
    let (_, _, event1) = beforeEach()
    let (_, _, event2) = beforeEach()
    let version = int64(EventStore.ClientAPI.ExpectedVersion.NoStream)

    let f event ()=
                AsyncWrite5 streamName version event
                |> Async.RunSynchronously
                |> ignore
    
    // debugResponse2 streamName version (f event0)
    // debugResponse2 streamName version (f event1)
    // debugResponse2 streamName version (f event2)

    f event0 ()
    |> should equal null
    
    f event1
    |> should (throwWithRegexMessage @"Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -1, Current version: 0") typeof<EventStore.ClientAPI.Exceptions.WrongExpectedVersionException>
    
    f event2
    |> should (throwWithRegexMessage @"Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -1, Current version: 0") typeof<EventStore.ClientAPI.Exceptions.WrongExpectedVersionException>
    
[<Fact>]
let ``version StreamExists``() =

    let (streamName, _, event0) = beforeEach()
    let (_, _, event1) = beforeEach()
    let (_, _, event2) = beforeEach()
    let version = int64(EventStore.ClientAPI.ExpectedVersion.StreamExists)

    let f event ()=
                AsyncWrite5 streamName version event
                |> Async.RunSynchronously
                |> ignore
    
    // debugResponse2 streamName version (f event0)
    // debugResponse2 streamName version (f event1)
    // debugResponse2 streamName version (f event2)

    f event0
    |> should (throwWithRegexMessage @"Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -4, Current version: -1") typeof<EventStore.ClientAPI.Exceptions.WrongExpectedVersionException>
    f event1
    |> should (throwWithRegexMessage @"Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -4, Current version: -1") typeof<EventStore.ClientAPI.Exceptions.WrongExpectedVersionException>
    
    f event2
    |> should (throwWithRegexMessage @"Append failed due to WrongExpectedVersion. Stream: WtriteOneEvent-test-.{36}, Expected version: -4, Current version: -1") typeof<EventStore.ClientAPI.Exceptions.WrongExpectedVersionException>
    // I cannot find any documentation aboy SreamExists version, but it looks like its -4

[<Fact>]
let ``version Any``() =

    let (streamName, _, event0) = beforeEach()
    let (_, _, event1) = beforeEach()
    let (_, _, event2) = beforeEach()
    let version = int64(EventStore.ClientAPI.ExpectedVersion.Any)

    let f event ()=
                AsyncWrite5 streamName version event
                |> Async.RunSynchronously
                |> ignore
    
    // debugResponse2 streamName version (f event0)
    // debugResponse2 streamName version (f event1)
    // debugResponse2 streamName version (f event2)

    f event0 ()
    |> should equal null
    
    f event1 ()
    |> should equal null
    
    f event2 ()
    |> should equal null
    // works wanderful as expected
