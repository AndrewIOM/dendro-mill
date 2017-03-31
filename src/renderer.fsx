#r "../node_modules/fable-core/Fable.Core.dll"
#r "../node_modules/fable-react/Fable.React.dll"
#r "../node_modules/fable-elmish/Fable.Elmish.dll"
#r "../node_modules/fable-elmish-react/Fable.Elmish.React.dll"
#load "../node_modules/fable-import-d3/Fable.Import.D3.fs"
#load "serialport.fsx"

open Elmish
open Elmish.React
open Fable.Import
open Fable.Import.Browser
open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Fable.Import.SerialPort

let sass = importAll<obj> "./sass/main.scss"
let ReactFauxDOM = importAll<obj> "react-faux-dom/lib/ReactFauxDOM"  

// Messages
type Message =
  | Calibrate

// Internal State
[<Pojo>]
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
open Fable.Helpers.React
module R = Fable.Helpers.React

let view (model:Model) dispatch =
    let onClick msg =
      OnClick <| fun _ -> msg |> dispatch

    let drawCartesianGrid (model:Model) : React.ReactElement =

      let width,height = 500, 500
      
      let x = D3.Scale.Globals.linear().range([|0.;float width|]).domain([|1.;10.|])
      let y = D3.Scale.Globals.linear().range([|float height;0.|]).domain([|1.;10.|])
      
      let xAxis = D3.Svg.Globals.axis().scale(x).tickSize(-(float height))
      let yAxis = D3.Svg.Globals.axis().scale(y).ticks(4).orient("right")

      let node  = ReactFauxDOM?createElement("svg") :?>  Browser.EventTarget

      D3.Globals.select(node)
        .attr("height", unbox<D3.Primitive> height)
        .attr("width", unbox<D3.Primitive> width) |> ignore

      let svg = D3.Globals.select(node)
      
      svg?append("g")
          ?attr("class",  "x axis")
          ?attr("transform", "translate(0," + height.ToString() + ")") 
          ?call(xAxis) 
          |> ignore

      svg.append("g")
        .attr("class", unbox<D3.Primitive> "y axis")
        ?call(yAxis)
        ?append("text")
        ?attr("transform", "rotate(-90)")
        ?attr("y", 6)
        ?attr("dy", ".71em")
        ?style("text-anchor", "end")
        |> ignore

      node?toReact() :?> React.ReactElement

    R.div [] [
      R.label [] [ unbox "Cartesian grid" ]
      R.fn drawCartesianGrid model []
      ]

// App
Program.mkSimple init update view
|> Program.withConsoleTrace
|> Program.withReact "elmish-app"
|> Program.run