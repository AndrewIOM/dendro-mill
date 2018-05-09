module App

open Elmish
open Elmish.React
open ViewState

let init () =
    let hardware = Hardware.init()
    let software = Software.init()
    { Hardware = fst hardware
      Software = fst software }, List.concat [snd hardware |> Cmd.map HardwareMsg; snd software |> Cmd.map SoftwareMsg]

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
