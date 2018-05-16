module Arduino

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.JohnnyFive
open Fable.PowerPack
open Types

[<Measure>] type pin

type PinMode =
| Input = 0
| Output = 1
| Analog = 2
| Pwm = 3
| Servo = 4

type DRV8825 = {
    Dir:        int<pin>
    Step:       int<pin>
    Sleep:      int<pin>
    Reset:      int<pin>
    Mode0:      int<pin>
    Mode1:      int<pin>
    Mode2:      int<pin>
    Enabled:    int<pin>
}

/////////////////////
/// Pin Configuration
/////////////////////

let xPins = {
    Dir         = 30<pin>
    Step        = 32<pin>
    Sleep       = 34<pin>
    Reset       = 36<pin>
    Mode0       = 33<pin>
    Mode1       = 35<pin>
    Mode2       = 37<pin>
    Enabled     = 31<pin>
}

let yPins = {
    Dir         = 22<pin>
    Step        = 24<pin>
    Sleep       = 26<pin>
    Reset       = 28<pin>
    Mode0       = 25<pin>
    Mode1       = 27<pin>
    Mode2       = 29<pin>
    Enabled     = 23<pin>
}
let zPins = {
    Dir         = 3<pin>    
    Step        = 4<pin>    
    Sleep       = 5<pin>    
    Reset       = 6<pin>    
    Mode0       = 9<pin>
    Mode1       = 8<pin>    
    Mode2       = 7<pin>    
    Enabled     = 10<pin>
}
let rPins = {
    Dir         = 38<pin>
    Step        = 40<pin>
    Sleep       = 42<pin>
    Reset       = 44<pin>
    Mode0       = 41<pin>
    Mode1       = 43<pin>
    Mode2       = 45<pin>
    Enabled     = 39<pin>
}
let abPins = {
    Dir         = 46<pin>
    Step        = 48<pin>
    Sleep       = 50<pin>
    Reset       = 52<pin>
    Mode0       = 49<pin>
    Mode1       = 51<pin>
    Mode2       = 53<pin>
    Enabled     = 47<pin>
}

let xTotalSteps = 1000<step>
let yTotalSteps = 1000<step>
let zTotalSteps = 1000<step>
let rTotalSteps = 400<step>
let abTotalSteps = 500<step>


/////////////////////
/// Micromill Interface
/////////////////////

module Stepper =

    let private removeUnit (x:int<_>) =
        int x

    let toMoveAction (stepper:Stepper) : Axis.MoveMotor =
        fun (steps:int<step>) (callback:unit->unit) ->
            stepper.step(
                createObj [ 
                    "steps" ==> (steps |> removeUnit)
                    "direction" ==> 1 //Clockwise (0 is counter-clockwise)
                ], System.Func<unit,unit> callback )


module MovingStage =

    type Status =
    | Disconnected
    | Connecting
    | Connected of StageState

    and StageState = {
        X: Axis.Axis
        Y: Axis.Axis
        Vertical: Axis.Axis
        Tilt: Axis.Axis
    }

    let private unPin (pin:int<pin>) =
        int pin

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
        board |> activateSleepAndReset (pinConfig.Sleep |> unPin) (pinConfig.Reset |> unPin)
        let opts = motorOptions (pinConfig.Step |> unPin) (pinConfig.Dir |> unPin)
        U3.Case3 opts |> Stepper

    let activateAxis axis (board:Board) =
        match board.isReady with
        | false -> invalidOp "Board not ready yet"
        | true ->
            match axis with
            | X -> Axis.linear xTotalSteps None (setupMotor xPins board |> Stepper.toMoveAction) |> Axis.Linear
            | Y -> Axis.linear yTotalSteps None (setupMotor yPins board |> Stepper.toMoveAction) |> Axis.Linear
            | Vertical -> Axis.linear zTotalSteps None (setupMotor zPins board |> Stepper.toMoveAction) |> Axis.Linear
            | Rotation -> Axis.tilting rTotalSteps None (setupMotor rPins board |> Stepper.toMoveAction) |> Axis.Tilt
            | StageAngle -> Axis.tilting abTotalSteps None (setupMotor abPins board |> Stepper.toMoveAction) |> Axis.Tilt

    let private boardReady (board:JohnnyFive.Board) =
        Fable.PowerPack.Promise.create(fun res rej -> 
            board.on_ready(fun () ->
                printfn "The board is ready"
                let status = 
                    {
                        X = board |> activateAxis X
                        Y = board |> activateAxis Y
                        Vertical = board |> activateAxis Vertical
                        Tilt = board |> activateAxis StageAngle
                    } |> Connected
                res status ) |> ignore )

    let connect () =
        promise {
            printfn "Connecting..."
            let board = JohnnyFive.Board()
            let! status = boardReady board 
            return status
        }