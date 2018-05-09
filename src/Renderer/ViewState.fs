module ViewState

open Elmish
open Fable.Core.JsInterop
open Fable.Import
open Types

let patchProcessStdin : unit -> unit = import "patchProcessStdin" "./electron-patch.js"
patchProcessStdin()

module Hardware =

    open Arduino
    open Movement

    let mutable movingStage = MovingStage.Status.Disconnected

    type Msg =
    | EstablishConnection
    | ConnectionEstablished
    | ConnectionError
    | StartMoving of MovementDirection * float<mm>
    | FinishedMoving of MovementDirection * float<mm>

    [<Fable.Core.PojoAttribute>]
    type Model = {
      X: float<mm>
    }

    let beginMove steps (dispatch:Dispatch<Msg>) =
        let callback = fun () -> dispatch (FinishedMoving (MovementDirection.X, 100.<mm>))
        match movingStage with
        | MovingStage.Connected s -> (s.X |> Axis.move) steps callback
        | _ -> ()

    let endMove direction distance model =
        { model with X = model.X + distance }

    let init () =
        { X = 0.<mm> }

    let update msg model =
        match msg with
        | StartMoving (direction,distance) ->
            model, Cmd.ofSub (beginMove 100<step>)
        | FinishedMoving (direction,distance) ->
            model |> endMove direction distance, Cmd.none
        | _ -> model, Cmd.none


module Software =

    [<Fable.Core.PojoAttribute>]
    type Model = {
        Calibration: Calibration
        Section: ViewSection }

    and Calibration = 
    | Uncalibrated
    | ImageOnly of string //base64 or blob
    | Calibrated of CalibrationState

    and CalibrationState = {
        Image: string
        TopRight: Coordinate
        TopLeft: Coordinate
        BottomRight: Coordinate
        BottomLeft: Coordinate }

    and ViewSection =
    | Control
    | Calibrate
    | Paths
    | Settings

    type Msg =
    | ConnectArduino
    | ActivateAxes
    | UploadCalibrationImage of string
    | Calibrate
    | SwitchSection of ViewSection
    | Move of MovementDirection * int<step>

    let init () =
      // Try to connect micromill on init?
         { Calibration = Calibration.Uncalibrated
           Section = Control }

    let activateMotors state = 
      // match movingStage with
      // | Disconnected -> invalidOp "Arduino not connected"
      // | Connected c ->
      //     match c.Arduino.isReady with
      //     | false -> state
      //     | true ->
      //       let x = MovingStage.activateAxis MovementDirection.X c.Arduino
      //       let y = MovingStage.activateAxis MovementDirection.Y c.Arduino
      //       let z = MovingStage.activateAxis MovementDirection.Vertical c.Arduino
      //       let mm = Connected { 
      //                 c with 
      //                   CartesianX = Enabled { Motor = x; CurrentStep = 0<step>; MaxStep = 20000<step>; MinStep = 20000<step> }
      //                   CartesianY = Enabled { Motor = y; CurrentStep = 0<step>; MaxStep = 20000<step>; MinStep = 20000<step> } 
      //                   Vertical = Enabled { Motor = z; CurrentStep = 0<step>; MaxStep = 20000<step>; MinStep = 20000<step> } 
      //                 }
            state

    let connect state =
      state

    let calibrate state =
      state

    let uploadImage image state =
        Browser.console.log "file selected!" |> ignore
        { state with Calibration = ImageOnly image }


    let move axis steps state =
      // match state.Micromill with
      // | Connected mm ->
      //     match axis with
      //     | X ->
      //       match mm.CartesianX with
      //       | Disabled -> state
      //       | Enabled ax -> 
      //         let updated = { mm with CartesianX = Enabled { ax with CurrentStep = ax.CurrentStep + steps }}
      //         {state with Micromill = Connected mm }
      //     // | Y ->
      //     //   match mm.CartesianY with
      //     //   | Disabled -> model
      //     //   | Enabled ax -> {model with CartesianY = Enabled { ax with CurrentStep = ax.CurrentStep + steps }}
      //     | _ -> state
      // | _ -> state
      state

    let update (msg:Msg) (state:Model)  =
      match msg with
      | ConnectArduino -> connect state
      | ActivateAxes -> activateMotors state
      | Msg.Calibrate -> calibrate state
      | SwitchSection s -> { state with Section = s }
      | Move (axis,steps) -> move axis steps state
      | UploadCalibrationImage imageDataUrl -> uploadImage imageDataUrl state
      , Cmd.none

//////////////
/// App State
//////////////

type AppMsg = 
    | HardwareMsg of Hardware.Msg
    | SoftwareMsg of Software.Msg

[<Fable.Core.PojoAttribute>]
type AppModel =
    { Hardware : Hardware.Model
      Software : Software.Model }