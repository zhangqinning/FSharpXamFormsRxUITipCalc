﻿module FSharp.Reactive.Tests.ObservableSpecs

open System
open System.Reactive.Linq
open FSharp.Control.Reactive
open Builders
open NUnit.Framework

let ``should be`` expectedNext expectedError expectedCompleted (observable:'a IObservable) =
    let next = ref 0
    let error = ref false
    let completed = ref false

    let subscription = observable |> Observable.subscribeWithCallbacks (fun _ -> incr next) (fun _ -> error := true) (fun () -> completed := true)

    Assert.That(!next, Is.EqualTo expectedNext)
    Assert.That(!error, Is.EqualTo expectedError)
    Assert.That(!completed, Is.EqualTo expectedCompleted)

let tuple x y = x,y

[<Test>]
let ``When subscribing to a single value observable, OnNext and OnCompleted should be fired``() =
    Observable.Return(1) |> ``should be`` 1 false true

[<Test>]
let ``When subscribing to an empty observable, only OnCompleted should be fired``() =
    Observable.Empty() |> ``should be`` 0 false true

[<Test>]
let ``When subscribing to an observable that fires an exception, only OnError should be fired``() =
    Observable.Throw(Exception()) |> ``should be`` 0 true false

[<Test>]
let ``When subscribing to an F# event, only OnNext should be called``() =
    let next = ref 0
    let error = ref false
    let completed = ref false
    let testEvent = Event<EventHandler, EventArgs>()

    let subscription = testEvent.Publish.Subscribe((fun _ -> incr next), (fun _ -> error := true), (fun () -> completed := true))
    testEvent.Trigger(null, EventArgs())

    Assert.That(!next, Is.EqualTo 1)
    Assert.That(!error, Is.False)
    Assert.That(!completed, Is.False)

type TestType() =
    let testEvent = Event<EventHandler, EventArgs>()
    [<CLIEvent>] member this.TestEvent = testEvent.Publish
    member this.Trigger() = testEvent.Trigger(this, EventArgs())

[<Test>]
let ``When subscribing to an event, only OnNext should be fired once.``() =
    let next = ref 0
    let error = ref false
    let completed = ref false

    let tester = TestType()
//    let subscription = tester.TestEvent.Subscribe((fun _ -> incr next), (fun _ -> error := true), (fun () -> completed := true))
    let observable = Observable.fromEventPattern "TestEvent" tester
    let subscription = observable.Subscribe((fun _ -> incr next), (fun _ -> error := true), (fun () -> completed := true))
    tester.Trigger()

    Assert.That(!next, Is.EqualTo 1)
    Assert.That(!error, Is.False)
    Assert.That(!completed, Is.False)

[<Test>]
let ``When subscribing to an observable that fires an exception using the ObservableBuilder, only OnError should be fired``() =
    let builder = observe {
        failwith "Test"
        return 1 }
    builder |> ``should be`` 0 true false

[<Test>]
let ``When zip is defined with the applicative, it should match the result of Observable.zip``() =
    let inline (<*>) f m = Observable.apply f m
    let inline (<!>) f m = Observable.map f m
    let a = Observable.Return 1
    let b = Observable.Return 2
    let zip a b = tuple <!> a <*> b

    let actual = ref (0,0)
    let expected = ref (0,0)

    (zip a b).Subscribe(fun x -> actual := x) |> ignore
    (Observable.zip a b tuple).Subscribe(fun x -> expected := x) |> ignore

    Assert.That(!actual, Is.EqualTo (!expected))

[<Test>]
let ``Test should show the stack overflow is fixed with Rx 2 beta``() =
    let test() =
        let rec g x = observe {
            yield x
            if x < 100000 then
                yield! g (x + 1) }
        g 5 |> Observable.subscribeWithCallbacks ignore ignore ignore |> ignore
    Assert.DoesNotThrow(TestDelegate(fun () -> test()))

[<Test>]
let ``Zipping two observable sequences of different types creates a single zipped observable`` =
    let obs1 = Observable.Return 1
    let obs2 = Observable.Return "A"
    let zipped = Observable.zip obs1 obs2 tuple
    let result = zipped |> Observable.First
    let expected = ( 1, "A" )

    Assert.That(result, Is.EqualTo expected)
