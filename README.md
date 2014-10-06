FSharpXamFormsRxUITipCalc
=========================

An example of using F#, Xamarin.Forms, and PCLs with Rx and ReactiveProperty on iOS and Android

From my blog post: [http://pillowsoft.com/blog/files/fsharp-xamforms-rx-ui.html](http://pillowsoft.com/blog/files/fsharp-xamforms-rx-ui.html)


Recently a PCL (Portable Class Library) compliant F# Core for Mono was released on [Nuget](https://www.nuget.org/packages/FSharp.Core.Mono.Signed/). This DLL, along with Mono assembly facades, allows one to use profile 78 with F# for iOS and Android development in Xamarin studio.  Larry O’Brien had a [blog post](http://www.knowing.net/index.php/2014/08/27/xamarin-forms-programming-in-f/) initially describing some of this. I have posted on GitHub a simple project for Xamarin studio,  that builds a simple tip calculator for iOS and Android using [ReactiveProperty from CodePlex](https://reactiveproperty.codeplex.com/).  I initially tried to do the demo using [ReactiveUI](http://www.reactiveui.net/), but ReactiveUI makes heavy use of expression lambdas, a.k.a. quotations in F sharp. Alas, these appear to be broken at the moment in the Mono PCL F# Core.  As soon as I get that figured out and quotations work, I will repost the same demo using ReactiveUI.

To get a better idea of why reactive programming is useful and important, check out this [video by Paul Betts on ReactiveUI](https://www.youtube.com/watch?v=1XNATGjqM6U).  Functional programming is largely about composition as a primary tool. Reactive programming also tends toward composition as a main development technique. In this demo, using reactive programming is a bit of an overkill compared to the data binding that is built-in to Xamarin.Forms. However, the demo is meant to be simple and hopefully will give you an idea as to how to start using reactive programming in F#.

The ReactiveProperty library starts off with the concept of a ‘reactive property’ data type. The idea is that you build your models or view models using these. When you set their values, they notify their observers. This is the mechanism by which we use reactive properties for data binding. In the demo code you will see some simple functions in F# to help with this.  Let’s look at one of the functions for two-way binding to a slider. Here’s the code:

___


[Example Code](https://gist.github.com/pillowsoft/ebf0e7b0a9db125ef09d)

F# is very helpful at exposing most events, like those from the slider control above as IObservables. This means we can use all Rx (Reactive Extensions)  methods directly on them. I am using Rx methods, but they are easier to use via this F# wrapper library [FSharp.Control.Reactive](https://github.com/fsprojects/FSharp.Control.Reactive), that makes them seem a bit more “functional”.

Since we are two-way binding (control to reactive property and reactive property to control), we will be creating two subscriptions, which return two IDisposable’s.  I am not handling disposing of these disposables in this demo application, but in a real application you would want to. To make it easier, this function takes the two disposables and combines them into a *composite disposable*.  When you dispose of the *composite disposable* it will dispose of the two disposables it is made from.  

The first line of this function takes the observable from the slider value changed event, and then applies a map on it to convert that into a stream of float values. In this demo app, I apply the observable.iter, which allows me to dump to the console the current slider value. In a real app you would not do this other than for debugging. We then take that result about and subscribe to it with a function that will set the value in reactive property using a data converter function also passed in.  The data converter function is used to do rounding so that we don’t see a lots of extra digits in the user interface for the tip calculator.

The second line is using the reactive property, again applying the Observable.iter function to it, to dump out its current value and then subscribing to it with a function which sets the sliders value (again through a data converter function).  The passed in data converter is a simple record containing two functions, one to convert from a data type to a another data type and the other to convert back the other way.

With these simple data binding functions we can build our tip calculator UI using Xamarin.Forms and then wire up the user interface to the view model.

You can find the code for the demo application here:

Note: I had to build FSharp.Control.Reactive and ReactiveProperty as part of the project, as neither currently have Nugets with PCL Profile78
