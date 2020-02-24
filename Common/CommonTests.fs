module CommonTests

open System.Text.RegularExpressions
open NHamcrest.Core

let throwWithRegexMessage (m: string) (t: System.Type) =
    CustomMatcher<obj>
        (sprintf "%A \"%A\"" (string t) m,
         (fun f ->
             match f with
             | :? (unit -> unit) as testFunc ->
                 try
                     testFunc()
                     false
                 with ex ->
                     printfn "%A %A" (ex.GetType()) (ex.GetType() = t)
                     printfn "%A %A " (ex.Message) (Regex.IsMatch(ex.Message, m))
                     // this is due lack of good diff
                     ex.GetType() = t && Regex.IsMatch(ex.Message, m)
             | _ -> false))

let debugResponse f =
    try
        f() |> fun r -> Ok(f.GetType(), r)
    with e -> Error e
    |> printfn "%A"

let debugResponse2 streamName version f =
    printfn "http://localhost:2113/web/index.html#/streams/%s" streamName
    printfn "%A" version
    try
        f() |> fun r -> Ok(f.GetType(), r)
    with e -> Error e
    |> printfn "%A"

let defaultEventArgs() =
    let id = System.Guid.NewGuid()
    let data = System.Text.Encoding.UTF8.GetBytes """WtriteOneEvent-test"""
    let metadata = System.Text.Encoding.UTF8.GetBytes ""
    let typeName = "WtriteOneEvent"
    let isJson = true // with this it shows nicely in the web, even if it isn't JSON 😅

    (id, typeName, isJson, data, metadata)
