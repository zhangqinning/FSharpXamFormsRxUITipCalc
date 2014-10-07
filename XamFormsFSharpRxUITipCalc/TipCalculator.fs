namespace XamFormsFSharpRxUITipCalc

open System
open Codeplex.Reactive
open Xamarin.Forms
open ReactivePropertyUIHelpers
open FSharp.Control.Reactive
open System.Diagnostics


type TipCalculatorViewModel() = 

    member val TipAmount = new ReactiveProperty<float>(0.0) with get, set
    member val TipPercent = new ReactiveProperty<float>(0.0) with get, set

    member val SubTotal = new ReactiveProperty<float>(0.0) with get, set
    member val ReceiptTotal = new ReactiveProperty<float>(0.0) with get, set
    member val TotalAmount = new ReactiveProperty<float>(0.0) with get, set

    member this.Recalculate() : unit =
        this.TipAmount.Value <- (this.SubTotal.Value * this.TipPercent.Value) / 100.0
        this.TotalAmount.Value <- this.TipAmount.Value + this.ReceiptTotal.Value


type TitCalculatorPage(vm:TipCalculatorViewModel) as this = 
    inherit ContentPage()


    [<DefaultValue>] val mutable BillLabel: Label
    [<DefaultValue>] val mutable BillEntry: Entry
    [<DefaultValue>] val mutable BillLayout: StackLayout

    [<DefaultValue>] val mutable TipLabel: Label
    [<DefaultValue>] val mutable TipEntry: Entry
    [<DefaultValue>] val mutable TipSlider: Slider
    [<DefaultValue>] val mutable TipLayout: StackLayout


    [<DefaultValue>] val mutable TipAmountLabel: Label
    [<DefaultValue>] val mutable TotalBillLabel: Label

    [<DefaultValue>] val mutable Layout: StackLayout
    [<DefaultValue>] val mutable Grid: Grid

    [<DefaultValue>] val mutable Merged: IDisposable


    let ViewModel = vm


    let addLabelAndEntry (grid:Grid) row labelText placeholderText =
        let label = Label(Text = labelText, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End)
        Grid.SetRow(label, row)
        Grid.SetColumn(label, 0)
        grid.Children.Add(label)

        let entry = Entry(Keyboard=Keyboard.Numeric, Text = placeholderText)
        Grid.SetRow(entry, row)
        Grid.SetColumn(entry, 1)
        grid.Children.Add(entry)
        entry

    let addLabelAndLabel (grid:Grid) row labelText placeholderText =
        let label = Label(Text = labelText, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End)
        Grid.SetRow(label, row)
        Grid.SetColumn(label, 0)
        grid.Children.Add(label)

        let label2 = Label(Text = placeholderText, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.End)
        Grid.SetRow(label2, row)
        Grid.SetColumn(label2, 1)
        grid.Children.Add(label2)
        label2

    let addSlider (grid:Grid) row min max =
        let slider = Slider(Minimum = min, Maximum = max)
        Grid.SetRow(slider, row)
        Grid.SetColumn(slider, 0)
        Grid.SetColumnSpan(slider, 2)
        grid.Children.Add(slider)
        slider

    do
        this.Layout <- StackLayout(VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand)
        this.Layout.Padding <- Thickness(20.)

        this.Grid <- Grid();
        let autoRow = RowDefinition(Height = GridLength (0., GridUnitType.Auto))
        [autoRow; autoRow; autoRow; autoRow; autoRow; autoRow] |> List.map (fun element -> this.Grid.RowDefinitions.Add(element)) |> ignore
        let starColumn = ColumnDefinition(Width = GridLength(1., GridUnitType.Star))
        [starColumn; starColumn] |> List.map (fun element -> this.Grid.ColumnDefinitions.Add(element)) |> ignore

        let subTotal = addLabelAndEntry this.Grid 0 "Food & Drink:" "Subtotal"
        let receiptTotal = addLabelAndEntry this.Grid 1 "Total After Tax:" "Receipt total"
        let tipPercent = addLabelAndEntry this.Grid 2 "Tip Percent:" "0."
        let tipPercentSlider = addSlider this.Grid 3 0. 100.
        let tipAmount = addLabelAndLabel this.Grid 4 "Tip Amount:" "0."
        let totalAmount = addLabelAndLabel this.Grid 5 "Total:" "0."

        bindEntryTextChangesTwoWayConverter (tipPercent) (vm.TipPercent) (DoubleRoundingFloatToTextConverter 1.) |> ignore
        bindSliderValueChangedTwoWayConverter tipPercentSlider (vm.TipPercent) (DoubleRoundingFloatToFloatConverter 1.) |> ignore

        let doit(fValue:float) =
            let name = toPropName <@ vm.TipAmount @>
            Debug.WriteLine(name)

        // uncomment these two lines, this slide the slider to invoke the quotation and see this issue with quotations and PCL-Profile78
        //let doit = fun x -> <@ vm.TipAmount @> |> ignore
        bindSliderValueChanged tipPercentSlider doit |> ignore


        bindEntryTextChangesTwoWayConverter (subTotal) (vm.SubTotal) (DoubleRoundingFloatToTextConverter 0.1) |> ignore
        bindEntryTextChangesTwoWayConverter (receiptTotal) (vm.ReceiptTotal) (DoubleRoundingFloatToTextConverter 0.1) |> ignore

        bindLabelFromText (totalAmount) (vm.TotalAmount) (DoubleRoundingFloatToTextConverter 0.1) |> ignore
        bindLabelFromText (tipAmount) (vm.TipAmount) (DoubleRoundingFloatToTextConverter 0.1) |> ignore

        this.Merged <- Observable.combineLatest [vm.SubTotal; vm.TipPercent; vm.ReceiptTotal] |> Observable.subscribe (fun value -> vm.Recalculate())

        this.Layout.Children.Add(this.Grid)

        this.Content <- this.Layout


