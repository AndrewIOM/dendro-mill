module App

open Elmish
open Elmish.React
open ViewState

let init () =
    { Hardware = Hardware.init()
      Software = Software.init() }, Cmd.none

let update (msg:AppMsg) (model:AppModel) : AppModel * Cmd<AppMsg> =
    match msg with
    | HardwareMsg m -> 
        let r = Hardware.update m model.Hardware
        { model with Hardware = r |> fst }, (r |> snd |> Cmd.map HardwareMsg)
    | SoftwareMsg m -> 
        let r = Software.update m model.Software
        { model with Software = r |> fst }, (r |> snd |> Cmd.map SoftwareMsg)

// App
Program.mkProgram init update View.master
|> Program.withConsoleTrace
|> Program.withReact "elmish-app"
|> Program.run
