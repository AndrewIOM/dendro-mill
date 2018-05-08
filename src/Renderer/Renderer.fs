module Renderer

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Fable.Import

open Arduino
open Types

//////////////////////
/// Connect Device
//////////////////////

let patchProcessStdin : unit -> unit = import "patchProcessStdin" "./electron-patch.js"
patchProcessStdin()

// let micromill = Micromill.connect()

//////////////////////
/// Messages + Router
//////////////////////

module State =

    let init () =
        {Micromill = Disconnected
         Calibration = Calibration.Uncalibrated
         Section = Control }

    let activateMotors state = 
      match state.Micromill with
      | Disconnected -> invalidOp "Arduino not connected"
      | Connected c ->
          match c.Arduino.isReady with
          | false -> state
          | true ->
            let x = MovingStage.activateAxis Axis.X c.Arduino
            let y = MovingStage.activateAxis Axis.Y c.Arduino
            let z = MovingStage.activateAxis Axis.Vertical c.Arduino
            let mm = Connected { 
                      c with 
                        CartesianX = Enabled { Motor = x; CurrentStep = 0<step>; MaxStep = 20000<step>; MinStep = 20000<step> }
                        CartesianY = Enabled { Motor = y; CurrentStep = 0<step>; MaxStep = 20000<step>; MinStep = 20000<step> } 
                        Vertical = Enabled { Motor = z; CurrentStep = 0<step>; MaxStep = 20000<step>; MinStep = 20000<step> } 
                      }
            { state with Micromill = mm }

    let connect state =
      let mm = {
        Arduino = MovingStage.connect()
        CartesianX = Disabled
        CartesianY = Disabled
        CartesianCalibration = Calibration.Uncalibrated
        Rotation = Disabled
        TiltA = Disabled
        TiltB = Disabled
        Vertical = Disabled
      }
      { state with Micromill = mm |> Connected }

    let calibrate state =
      state

    let uploadImage image state =
        Browser.console.log "file selected!" |> ignore
        { state with Calibration = ImageOnly image }


    let move axis steps state =
      match state.Micromill with
      | Connected mm ->
          match axis with
          | X ->
            match mm.CartesianX with
            | Disabled -> state
            | Enabled ax -> 
              let updated = { mm with CartesianX = Enabled { ax with CurrentStep = ax.CurrentStep + steps }}
              {state with Micromill = Connected mm }
          // | Y ->
          //   match mm.CartesianY with
          //   | Disabled -> model
          //   | Enabled ax -> {model with CartesianY = Enabled { ax with CurrentStep = ax.CurrentStep + steps }}
          | _ -> state
      | _ -> state

    let route (msg:Message) (state:ViewState)  =
      match msg with
      | ConnectArduino -> connect state
      | ActivateAxes -> activateMotors state
      | Message.Calibrate -> calibrate state
      | SwitchSection s -> { state with Section = s }
      | Move (axis,steps) -> move axis steps state
      | UploadCalibrationImage imageDataUrl -> uploadImage imageDataUrl state


// App
Program.mkSimple State.init State.route View.view
|> Program.withConsoleTrace
|> Program.withReact "elmish-app"
|> Program.run
