namespace XamFormsFSharpRxUITipCalc

open System
open System.Reactive
open System.Runtime
open MonoTouch.UIKit
open MonoTouch.Foundation
open Xamarin.Forms

[<Register ("AppDelegate")>]
type AppDelegate() =
    inherit UIApplicationDelegate()

    let mutable window = Unchecked.defaultof<UIWindow>

    override x.FinishedLaunching (app, options) =
        Forms.Init ()
        let bounds = UIScreen.MainScreen.Bounds
        window <- new UIWindow (bounds)
        window.RootViewController <- App.GetMainPage().CreateViewController ()
        window.MakeKeyAndVisible ()

        true

module Main =
    [<EntryPoint>]
    let main args =
        UIApplication.Main (args, null, "AppDelegate")
        0
