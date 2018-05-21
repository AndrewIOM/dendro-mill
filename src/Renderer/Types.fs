module Types

///////////////////
/// Moving Stage
///////////////////

[<Measure>] type mm
[<Measure>] type step
[<Measure>] type degree
[<Measure>] type micrometre
[<Measure>] type rpm

let removeUnit (x:int<_>) = int x
let removeUnitFloat (x:float<_>) = float x

type Coordinate = float * float

type MovementDirection =
| X
| Y
| Vertical
| Rotation
| StageAngle

/// Representation of an axis on a moving stage
module Axis =

    open Fable.Import.JS

    type CurrentStep =
    | Uncalibrated
    | Calibrated of int<step>

    type MoveResult =
    | HitLowerBound
    | Finished

    type MoveMotor = int<step> -> Promise<MoveResult>
    
    type private AxisState = {
        Step: CurrentStep
        StepLimit: int<step>
        Move: MoveMotor
    }
    
    type LinearAxis = private LinearAxis of AxisState
    type TiltAxis = private TiltAxis of AxisState

    type Axis =
    | Linear of LinearAxis
    | Tilt of TiltAxis

    /// Create a linear axis
    let linear maxSteps startingPosition move =
        match startingPosition with
        | None -> LinearAxis { Step = Uncalibrated; StepLimit = maxSteps; Move = move }
        | Some s -> LinearAxis { Step = Calibrated s; StepLimit = maxSteps; Move = move }

    /// Create a tilting axis
    let tilting maxSteps startingStep move =
        match startingStep with
        | None -> TiltAxis { Step = Uncalibrated; StepLimit = maxSteps; Move = move }
        | Some s -> TiltAxis { Step = Calibrated s; StepLimit = maxSteps; Move = move }

    let private unwrapLinear (LinearAxis l) = l
    let private unwrapTilt (TiltAxis t) = t

    let currentStep axis =
        match axis with
        | Linear l -> (l |> unwrapLinear).Step
        | Tilt l -> (l |> unwrapTilt).Step

    let move axis =
        match axis with
        | Linear l -> (l |> unwrapLinear).Move
        | Tilt l -> (l |> unwrapTilt).Move
