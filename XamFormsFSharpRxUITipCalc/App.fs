namespace XamFormsFSharpRxUITipCalc

open Xamarin.Forms
    type App =

        static member GetMainPage () =
                let tipVm = TipCalculatorViewModel()
                let newNav = new NavigationPage(TitCalculatorPage(tipVm))
                newNav
