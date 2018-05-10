module ViewState

open Elmish
open Fable.Core.JsInterop
open Fable.Import
open Fable.PowerPack
open Types

let patchProcessStdin : unit -> unit = import "patchProcessStdin" "./electron-patch.js"
patchProcessStdin()

module File =

    let encodeBase64 imageUrl =
        let data = Fable.Import.Node.Exports.fs.readFileSync imageUrl
        let buffer = Fable.Import.Node.Globals.Buffer.Create data
        "data:image/png;base64," + buffer.toString("base64")


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
    type Model = 
    | NotConnected
    | Connecting
    | Online of ModelState
    | Offline of string

    and ModelState = {
        X: float<mm>
        Y: float<mm>
        Vertical: float<mm>
        Tilt: float<degree>
    }

    let connectArduino (dispatch:Dispatch<Msg>) =
        movingStage <- MovingStage.Status.Connecting
        let success s = 
            movingStage <- s
            dispatch ConnectionEstablished
        promise {
            let! stage = MovingStage.connect()
            success stage |> ignore
        } |> Promise.start

    let connected =
        Online { X = 0.<mm>; Y = 0.<mm>; Vertical = 0.<mm>; Tilt = 0.<degree> }

    let beginMove steps (dispatch:Dispatch<Msg>) =
        let callback = fun () -> dispatch (FinishedMoving (MovementDirection.X, 100.<mm>))
        match movingStage with
        | MovingStage.Connected s -> (s.X |> Axis.move) steps callback
        | _ -> ()

    let endMove direction distance model =
        match model with
        | Online s ->
            match direction with
            | X -> Online { s with X = s.X + distance }
            | _ -> model
        | _ -> model

    let init () =
        NotConnected, Cmd.ofMsg EstablishConnection

    let update msg model =
        match msg with
        | EstablishConnection -> 
            Connecting, Cmd.ofSub connectArduino
        | ConnectionEstablished ->
            connected, Cmd.none
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
         { Calibration = Calibration.Uncalibrated
           Section = Control }, Cmd.none

    let activateMotors state = 
        state

    let connect state =
        state

    let calibrate state =
        state

    let uploadImage imageUrl state =
        let data = File.encodeBase64 imageUrl
        { state with Calibration = ImageOnly data }


    let move axis steps state =
      state

    let update (msg:Msg) (state:Model)  =
      match msg with
      | ConnectArduino -> connect state, Cmd.none
      | ActivateAxes -> activateMotors state, Cmd.none
      | Msg.Calibrate -> calibrate state, Cmd.none
      | SwitchSection s -> { state with Section = s }, Cmd.none
      | Move (axis,steps) -> move axis steps state, Cmd.none
      | UploadCalibrationImage imageDataUrl -> uploadImage imageDataUrl state, Cmd.none


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