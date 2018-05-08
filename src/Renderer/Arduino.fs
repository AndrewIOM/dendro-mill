module Arduino

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.JohnnyFive

type PinMode =
| Input = 0
| Output = 1
| Analog = 2
| Pwm = 3
| Servo = 4

type DRV8825 = {
    Dir:        int
    Step:       int
    Sleep:      int
    Reset:      int
    Mode0:      int
    Mode1:      int
    Mode2:      int
    Enabled:    int
}

type LimiterSwitch = {
    Active: int
}

/////////////////////
/// Pin Configuration
/////////////////////

let xPins = {
    Dir         = 30
    Step        = 32
    Sleep       = 34
    Reset       = 36
    Mode0       = 33
    Mode1       = 35
    Mode2       = 37
    Enabled     = 31
}

let yPins = {
    Dir         = 22
    Step        = 24
    Sleep       = 26
    Reset       = 28
    Mode0       = 25
    Mode1       = 27
    Mode2       = 29
    Enabled     = 23
}
let zPins = {
    Dir         = 3
    Step        = 4
    Sleep       = 5
    Reset       = 6
    Mode0       = 9
    Mode1       = 8
    Mode2       = 7
    Enabled     = 10
}
let rPins = {
    Dir         = 38
    Step        = 40
    Sleep       = 42
    Reset       = 44
    Mode0       = 41
    Mode1       = 43
    Mode2       = 45
    Enabled     = 39
}
let abPins = {
    Dir         = 46
    Step        = 48
    Sleep       = 50
    Reset       = 52
    Mode0       = 49
    Mode1       = 51
    Mode2       = 53
    Enabled     = 47
}

/////////////////////
/// Micromill Interface
/////////////////////

module MovingStage =

    open Types

    type State =
    | Disconnected
    | Connecting
    | Connected of MicromillState

    and MicromillState = {
        X: Axis.LinearAxis
        Y: Axis.LinearAxis
        Vertical: Axis.LinearAxis
        Tilt: Axis.TiltAxis
    }

    let private activatePin (pin:int) (board:Board) =
        board.digitalWrite(float pin,1.)

    let private activateSleepAndReset sleep reset board =
        board |> activatePin sleep
        board |> activatePin reset

    let private setupMotor (pinConfig:DRV8825) board =
        let motorOptions (step:int) (dir:int) =         
            let opt = createEmpty<StepperOption>
            opt.``type`` <- 1. //Stepper.TYPE.Driver
            opt.rpm <- Some 400.
            opt.direction <- Some 1.
            opt.stepsPerRev <- 200.
            opt.pins <- createObj [ "step" ==> step; "dir" ==> dir ]
            opt
        board |> activateSleepAndReset pinConfig.Sleep pinConfig.Reset
        let opts = motorOptions pinConfig.Step pinConfig.Dir
        U3.Case3 opts |> Stepper

    let activateAxis axis (board:Board) =
        match board.isReady with
        | false -> invalidOp "Board not ready yet"
        | true ->
            match axis with
            | X -> setupMotor xPins board
            | Y -> setupMotor yPins board
            | Vertical -> setupMotor zPins board
            | Rotation -> setupMotor rPins board
            | StageAngle -> setupMotor abPins board

    let connect() =
        let board = JohnnyFive.Board()
        // board.on("ready", fun () -> board |> setupMotors) |> ignore
        board
