#r "../node_modules/fable-core/Fable.Core.dll"
#r "../node_modules/fable-react/Fable.React.dll"
#r "../node_modules/fable-elmish/Fable.Elmish.dll"
#r "../node_modules/fable-elmish-react/Fable.Elmish.React.dll"

open Elmish
open Elmish.React
open Fable.Import
open Fable.React

// Messages
type Message =
  | Calibrate

// Internal State
type Model = {
    CartesianX: Motor
    CartesianY: Motor
    CartesianCalibration: Calibration
    Rotation: Motor
    TiltA: Motor
    TiltB: Motor }

and Motor = 
| Disabled
| Enabled of MotorState

and MotorState = {
    MaxStep: int
}

and CartesianCoordinate = float * float

and Calibration = 
| Uncalibrated
| Calibrated of CalibrationState

and CalibrationState = {
    Image: string
    TopRight: CartesianCoordinate
    TopLeft: CartesianCoordinate
    BottomRight: CartesianCoordinate
    BottomLeft: CartesianCoordinate }

let init () =  
    {CartesianX = Disabled
     CartesianY = Disabled
     CartesianCalibration = Uncalibrated
     Rotation = Disabled
     TiltA = Disabled
     TiltB = Disabled}


let inline rand() = JS.Math.random()
let update (msg:Message) (model:Model)  =
  match msg with
  | Calibrate ->
        model


// View
open Fable.Core.JsInterop
open Fable.Helpers.React.Props
module R = Fable.Helpers.React

let view count dispatch =
  let onClick msg =
    OnClick <| fun _ -> msg |> dispatch

  R.div []
    [ R.button [ onClick Calibrate ] [ R.str "Calibrate" ]
      R.label [] [ R.str "Hello Woodmill!" ] ]

// App
Program.mkSimple init update view
|> Program.withConsoleTrace
|> Program.withReact "elmish-app"
|> Program.run