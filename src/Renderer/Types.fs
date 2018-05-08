module Types

open Fable.Import

///////////////////
/// Moving Stage
///////////////////

[<Measure>] type mm
[<Measure>] type step
[<Measure>] type degree
[<Measure>] type micrometre

/// Representation of an axis on a moving stage
module Axis =

    type CurrentStep =
    | Uncalibrated
    | Calibrated of int<step>

    type private Axis = {
        Step: CurrentStep
        StepLimit: int<step>
    }
    
    type LinearAxis = private LinearAxis of Axis
    type TiltAxis = private TiltAxis of Axis

    /// Create a linear axis
    let linear maxSteps startingPosition =
        match startingPosition with
        | None -> LinearAxis { Step = Uncalibrated; StepLimit = maxSteps }
        | Some s -> LinearAxis { Step = Calibrated s; StepLimit = maxSteps }

    /// Create a tilting axis
    let tilting maxSteps startingStep =
        match startingStep with
        | None -> TiltAxis { Step = Uncalibrated; StepLimit = maxSteps }
        | Some s -> TiltAxis { Step = Calibrated s; StepLimit = maxSteps }

    let angle (t:TiltAxis) (lengthToPivot:float<micrometre>) : float<degree> =
        2.214<degree>
        // TODO triangle calculation

module Stage =

    open Axis

    type Stage = {
        Tilt: TiltAxis list
        Linear: LinearAxis list
    }

    let x = 2


///////////////////
/// View Model
///////////////////

type Micromill =
| Disconnected
| Connected of MicromillState

and MicromillState = {
    Arduino: JohnnyFive.Board
    CartesianX: Motor
    CartesianY: Motor
    CartesianCalibration: Calibration
    Rotation: Motor
    Vertical: Motor
    TiltA: Motor
    TiltB: Motor
}

and Motor = 
| Disabled
| Enabled of MotorState

and MotorState = {
    CurrentStep: int<step>
    MaxStep: int<step>
    MinStep: int<step>
    Motor: JohnnyFive.Stepper
}

and CartesianCoordinate = float * float

and Calibration = 
| Uncalibrated
| ImageOnly of string //base64 or blob
| Calibrated of CalibrationState

and CalibrationState = {
    Image: string
    TopRight: CartesianCoordinate
    TopLeft: CartesianCoordinate
    BottomRight: CartesianCoordinate
    BottomLeft: CartesianCoordinate }

type Axis =
| X
| Y
| Vertical
| Rotation
| StageAngle

type ViewSection =
| Control
| Calibrate
| Paths
| Settings

[<Fable.Core.PojoAttribute>]
type ViewState = {
    Micromill: Micromill
    Calibration: Calibration
    Section: ViewSection }

type Message =
| ConnectArduino
| ActivateAxes
| UploadCalibrationImage of string
| Calibrate
| SwitchSection of ViewSection
| Move of Axis * int<step>
