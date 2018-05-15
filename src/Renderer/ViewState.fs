module ViewState

open Elmish
open Fable.Core.JsInterop
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

    [<AutoOpen>]
    module MatrixTransform =
        type MatrixTransform = private MatrixTransform of float array
        let create arr =
            match arr |> Array.length with
            | 16 -> MatrixTransform arr
            | _ -> invalidOp "Transform is invalid"

        let private unwrap (MatrixTransform m) = m
        type MatrixTransform with
            member this.Value = 
                unwrap this

    [<Fable.Core.PojoAttribute>]
    type Model = {
        Calibration: Calibration
        Section: ViewSection }

    and Calibration = 
    | Uncalibrated
    | ImageCalibration of CalibrationState

    and CalibrationState =
    | Floating of string
    | Fixed of string * MatrixTransform // Transform matrix is relative to whole stage width and height

    and ViewSection =
    | Control
    | Calibrate
    | Paths
    | Settings

    type Msg =
    | SwitchSection of ViewSection
    | UploadCalibrationImage of string
    | SaveCalibrationPosition of MatrixTransform

    let init () =
         { Calibration = Calibration.Uncalibrated
           Section = Control }, Cmd.none

    let uploadImage imageUrl state =
        let data = File.encodeBase64 imageUrl
        { state with Calibration = data |> Floating |> ImageCalibration }

    let saveMatrix matrix state =
        match state.Calibration with
        | Calibration.ImageCalibration s ->
            match s with
            | Floating i -> { state with Calibration = ImageCalibration <| Fixed (i,matrix) }
            | Fixed (i,_) -> { state with Calibration = ImageCalibration <| Fixed (i,matrix) }
        | _ -> state

    let update (msg:Msg) (state:Model)  =
      match msg with
      | SwitchSection s -> { state with Section = s }, Cmd.none
      | UploadCalibrationImage imageDataUrl -> uploadImage imageDataUrl state, Cmd.none
      | SaveCalibrationPosition matrix -> saveMatrix matrix state, Cmd.none


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