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

    open MovingStage
    open Movement

    let mutable movingStage = MovingStage.Status.Disconnected

    type Msg =
    | EstablishConnection
    | ConnectionEstablished
    | ConnectionError
    | CalibratedAxis of MovementDirection
    | StartMoving of MovementDirection * float<mm>
    | FinishedMoving of MovementDirection * float<mm>

    type MoveState =
    | Ready
    | Calibrating
    | Moving

    [<Fable.Core.PojoAttribute>]
    type Model = 
    | NotConnected
    | Connecting
    | Online of ModelState
    | Offline of string

    and ModelState = {
        X: MoveState * float<mm>
        Y: MoveState * float<mm>
        Vertical: MoveState * float<mm>
        Tilt: MoveState * float<degree>
    }

    let connectArduino (dispatch:Dispatch<Msg>) =
        movingStage <- MovingStage.Status.Connecting
        let success s = 
            movingStage <- s
            dispatch ConnectionEstablished
        promise {
            let! stage = FakeStage.connect()
            success stage |> ignore
        } |> Promise.start

    let rec moveToLowerLimit (move:Axis.MoveMotor) =
        move 50<step>
        |> Promise.bind (fun s ->
            match s with
            | Axis.MoveResult.Finished -> moveToLowerLimit move
            | Axis.MoveResult.HitLowerBound -> promise { return () } )

    let calibrateAxis direction (dispatch:Dispatch<Msg>) =
        promise {
            match movingStage with
            | Status.Connected s ->
                match direction with
                | MovementDirection.X -> do! s.X |> Axis.move |> moveToLowerLimit
                | MovementDirection.Y -> do! s.Y |> Axis.move |> moveToLowerLimit
                | MovementDirection.Vertical -> do! s.Vertical |> Axis.move |> moveToLowerLimit
                | _ -> ()
            | _ -> ()
            dispatch <| CalibratedAxis direction
        } |> Promise.start

    let calibrated direction model =
        match model with
        | Online s ->
            match direction with
            | MovementDirection.X -> Online { s with X = Ready, 0.<mm> }
            | MovementDirection.Y -> Online { s with Y = Ready, 0.<mm> }
            | MovementDirection.Vertical -> Online { s with Vertical = Ready, 0.<mm> }
            | _ -> model
        | _ -> model

    let connected =
        Online { X = Calibrating, 0.<mm>; Y = Calibrating, 0.<mm>; Vertical = Calibrating, 0.<mm>; Tilt = Ready, 0.<degree> },
        Cmd.batch [
            Cmd.ofSub (calibrateAxis MovementDirection.X)
            Cmd.ofSub (calibrateAxis MovementDirection.Y)
            Cmd.ofSub (calibrateAxis MovementDirection.Vertical) ]

    let beginMove direction distance (dispatch:Dispatch<Msg>) =
        let steps = 50<step>
        let callback = fun () -> dispatch (FinishedMoving (MovementDirection.X, 100.<mm>))
        match movingStage with
        | MovingStage.Connected s ->
            promise {
                let! result = Axis.move s.X steps
                match result with
                | Axis.MoveResult.Finished -> FinishedMoving (direction,distance) |> dispatch
                | Axis.MoveResult.HitLowerBound -> invalidOp "Not implemented"
            } |> Promise.start
        | _ -> ()

    let startMove direction model =
        printfn "Started moving"
        match model with
        | Online s ->
            match direction with
            | X -> Online { s with X = Moving, snd s.X }
            | Y -> Online { s with Y = Moving, snd s.Y }
            | Vertical -> Online { s with Vertical = Moving, snd s.Vertical }
            | _ -> model
        | _ -> model


    let endMove direction distance model =
        printfn "Finished moving"
        match model with
        | Online s ->
            match direction with
            | X -> Online { s with X = Ready, snd s.X + distance }
            | Y -> Online { s with Y = Ready, snd s.Y + distance }
            | Vertical -> Online { s with Vertical = Ready, snd s.Vertical + distance }
            | _ -> model
        | _ -> model

    let init () =
        NotConnected, Cmd.ofMsg EstablishConnection

    let update msg model =
        match msg with
        | EstablishConnection -> Connecting, Cmd.ofSub connectArduino
        | ConnectionEstablished -> connected
        | StartMoving (direction,distance) -> startMove direction model, Cmd.ofSub (beginMove direction distance)
        | FinishedMoving (direction,distance) -> endMove direction distance model, Cmd.none
        | CalibratedAxis direction -> model |> calibrated direction, Cmd.none
        | ConnectionError -> model, Cmd.none


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
        PathTemplate: ImageState option
        Paths: PathQueue
        EditingPathName: Label
        Section: ViewSection }

    and Label = string
    and Path = Coordinate list * Label

    and PathQueue = {
        Queued: Path list
        InProgress: Path option
        Completed: Path list
    }

    and Calibration = 
    | Uncalibrated
    | ImageCalibration of ImageState

    and ImageState =
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
    | UploadPathImage of string
    | QueuePath of Coordinate list
    | UnQueuePath of Label
    | ChangePathName of Label

    let init () =
         { Calibration = Calibration.Uncalibrated
           PathTemplate = None
           Paths = { Queued = []; InProgress = None; Completed = [] }
           EditingPathName = ""
           Section = Control }, Cmd.none

    let uploadImage imageUrl state =
        let data = File.encodeBase64 imageUrl
        { state with Calibration = data |> Floating |> ImageCalibration }

    let uploadPathImage imageUrl state =
        let data = File.encodeBase64 imageUrl
        { state with PathTemplate = data |> Floating |> Some }

    let saveMatrix matrix state =
        match state.Calibration with
        | Calibration.ImageCalibration s ->
            match s with
            | Floating i -> { state with Calibration = ImageCalibration <| Fixed (i,matrix) }
            | Fixed (i,_) -> { state with Calibration = ImageCalibration <| Fixed (i,matrix) }
        | _ -> state

    let queuePath path state =
        if not <| System.String.IsNullOrEmpty state.EditingPathName 
        then { state with Paths = { state.Paths with Queued = (path,state.EditingPathName) :: state.Paths.Queued } }
        else state

    let update (msg:Msg) (state:Model)  =
      match msg with
      | SwitchSection s -> { state with Section = s }, Cmd.none
      | UploadCalibrationImage imageDataUrl -> uploadImage imageDataUrl state, Cmd.none
      | SaveCalibrationPosition matrix -> saveMatrix matrix state, Cmd.none
      | UploadPathImage imageDataUrl -> uploadPathImage imageDataUrl state, Cmd.none
      | QueuePath path -> queuePath path state, Cmd.none
      | UnQueuePath label -> state, Cmd.none
      | ChangePathName label -> { state with EditingPathName = label }, Cmd.none


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