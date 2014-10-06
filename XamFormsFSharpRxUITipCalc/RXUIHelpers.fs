namespace XamFormsFSharpRxUITipCalc

open System
open Codeplex.Reactive
open FSharp.Control.Reactive
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Linq.RuntimeHelpers.LeafExpressionConverter
open System.Linq.Expressions
open System.Threading.Tasks
open System.Diagnostics
open Xamarin.Forms
open System.Reactive.Disposables

module ReactivePropertyUIHelpers = 

    let toPropName(query : Expr) = 
        match query with
        | PropertyGet(a, b, list) ->
            b.Name
        | _ -> ""


    type DataConverter<'fromType, 'toType> = {
        ConvertBack: ('toType) -> 'fromType;
        Convert: ('fromType) -> 'toType;
    }

    let Round(number:float, precision) =
        precision * Math.Round(number / precision)

    let stringToFloat str =
        let success, result = System.Double.TryParse(str)
        if success then result else 0.

    let DoubleRoundingFloatToTextConverter round = {
                                        Convert = fun (value:float) -> Round(value, round).ToString();
                                        ConvertBack = fun (value:string) -> Round(stringToFloat value, round);
                                    }

    let DoubleRoundingFloatToFloatConverter round = {
                                        Convert = fun (value:float) -> Round(value, round);
                                        ConvertBack = fun (value:float) -> Round(value, round);
                                    }


    let awaitTask (t: Task) = t |> Async.AwaitIAsyncResult |> Async.Ignore

    let bindObservable observable bindingFunc =
        observable |> Observable.subscribe (bindingFunc)


    // Entry
    let bindEntryTextChanges (textField:Entry) bindingFunc =
        textField.TextChanged |> Observable.map (fun (ea:TextChangedEventArgs) -> (textField.Text)) |> Observable.subscribe (bindingFunc)

    let bindEntryTextChangesTwoWay (textField:Entry) (reactiveProperty:ReactiveProperty<string>) =
        let disp1 = textField.TextChanged |> Observable.map (fun (ea:TextChangedEventArgs) -> (textField.Text)) |> Observable.iter (fun txt -> Debug.WriteLine(txt)) |> Observable.subscribe (fun str -> reactiveProperty.Value <- str) 
        let disp2 = reactiveProperty |> Observable.subscribe (fun str -> textField.Text <- str)
        new CompositeDisposable([ disp1; disp2 ])

    let bindEntryTextChangesTwoWayFloat (textField:Entry) (reactiveProperty:ReactiveProperty<float>) =
        let disp1 = textField.TextChanged |> Observable.map (fun (ea:TextChangedEventArgs) -> (textField.Text)) |> Observable.iter (fun txt -> Debug.WriteLine(txt)) |> Observable.subscribe (fun str -> reactiveProperty.Value <- stringToFloat str)
        let disp2 = reactiveProperty |> Observable.subscribe (fun num -> textField.Text <- num.ToString())
        new CompositeDisposable([ disp1; disp2 ])

    let bindEntryTextChangesTwoWayConverter<'destType> (textField:Entry) (reactiveProperty:ReactiveProperty<'destType>) (converter: DataConverter<'destType, string>) =
        let disp1 = textField.TextChanged |> Observable.map (fun (ea:TextChangedEventArgs) -> (textField.Text))
                                |> Observable.iter (fun txt -> Debug.WriteLine(txt))
                                |> Observable.subscribe (fun str -> reactiveProperty.Value <- converter.ConvertBack str)

        let disp2 = reactiveProperty |> Observable.subscribe (fun num -> textField.Text <- converter.Convert num)
        new CompositeDisposable([ disp1; disp2 ])

    let bindEntryTextFocused (textField:Entry) bindingFunc =
        textField.Focused |> Observable.map (fun (ea) -> (textField)) |> Observable.subscribe (bindingFunc)

    let bindEntryTextUnfocused(textField:Entry) bindingFunc =
        textField.Unfocused |> Observable.map (fun (ea) -> (textField)) |> Observable.subscribe (bindingFunc)


    // Label
    let bindLabelFromText<'destType> (textField:Label) (reactiveProperty:ReactiveProperty<'destType>) (converter: DataConverter<'destType, string>) =
        reactiveProperty |> Observable.subscribe (fun value -> textField.Text <- converter.Convert value)

    let bindLabelTextFocused (textField:Label) bindingFunc =
        textField.Focused |> Observable.map (fun (ea) -> (textField)) |> Observable.subscribe (bindingFunc)

    let bindLabelTextUnfocused(textField:Label) bindingFunc =
        textField.Unfocused |> Observable.map (fun (ea) -> (textField)) |> Observable.subscribe (bindingFunc)


    // Slider
    let bindSliderValueChanged (slider:Slider) bindingFunc =
        slider.ValueChanged |> Observable.map (fun (ea) -> (slider.Value)) |> Observable.iter (fun value -> Debug.WriteLine(value.ToString())) |> Observable.subscribe (bindingFunc)

    let bindSliderValueChangedTwoWayFloat (slider:Slider) (reactiveProperty:ReactiveProperty<float>) =
        let disp1 = slider.ValueChanged |> Observable.map (fun (ea) -> (slider.Value)) |> Observable.iter (fun value -> Debug.WriteLine("Slider:" + value.ToString())) |> Observable.subscribe (fun value -> reactiveProperty.Value <- value)
        let disp2 = reactiveProperty |> Observable.iter (fun value -> Debug.WriteLine("Prop:" + value.ToString()))  |> Observable.subscribe (fun value -> slider.Value <- value)
        new CompositeDisposable([ disp1; disp2 ])

    let bindSliderValueChangedTwoWayConverter<'destType> (slider:Slider) (reactiveProperty:ReactiveProperty<'destType>)  (converter: DataConverter<'destType, float>) =
        let disp1 = slider.ValueChanged |> Observable.map (fun (ea) -> (slider.Value)) |> Observable.iter (fun value -> Debug.WriteLine("Slider:" + value.ToString())) |> Observable.subscribe (fun value -> reactiveProperty.Value <- converter.ConvertBack value)
        let disp2 = reactiveProperty |> Observable.iter (fun value -> Debug.WriteLine("Prop:" + value.ToString()))  |> Observable.subscribe (fun value -> slider.Value <- converter.Convert value)
        new CompositeDisposable([ disp1; disp2 ])


    let bindSliderFocused (slider:Slider) bindingFunc =
        slider.Focused |> Observable.map (fun (ea) -> (slider)) |> Observable.subscribe (bindingFunc)

    let bindSliderUnfocused(slider:Slider) bindingFunc =
        slider.Unfocused |> Observable.map (fun (ea) -> (slider)) |> Observable.subscribe (bindingFunc)

    // Progress
    let bindProgressBarFocused (progressBar:ProgressBar) bindingFunc =
        progressBar.Focused |> Observable.map (fun (ea) -> (progressBar)) |> Observable.subscribe (bindingFunc)

    let bindProgressBarUnfocused(progressBar:ProgressBar) bindingFunc =
        progressBar.Unfocused |> Observable.map (fun (ea) -> (progressBar)) |> Observable.subscribe (bindingFunc)

    // TextCell
    let bindTextCellAppearing(textCell:TextCell) bindingFunc =
        textCell.Appearing |> Observable.map (fun (ea) -> (textCell)) |> Observable.subscribe (bindingFunc)

    let bindTextCellDisappearing(textCell:TextCell) bindingFunc =
        textCell.Disappearing |> Observable.map (fun (ea) -> (textCell)) |> Observable.subscribe (bindingFunc)

    let bindTextCellTapped(textCell:TextCell) bindingFunc =
        textCell.Tapped |> Observable.map (fun (ea) -> (textCell)) |> Observable.subscribe (bindingFunc)


    // Two way binding

    // Navigation


