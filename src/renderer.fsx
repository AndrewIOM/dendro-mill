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

// Internal State
[<Pojo>]
type Model = {
    CartesianX: Motor
    CartesianY: Motor
    CartesianCalibration: Calibration
    Rotation: Motor
    TiltA: Motor
    TiltB: Motor
    Section: Section }

and Section =
| Control
| Calibrate
| Paths
| Settings

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
     TiltB = Disabled
     Section = Control }


type Message =
  | Calibrate
  | SwitchSection of Section

let update (msg:Message) (model:Model)  =
  match msg with
  | Message.Calibrate -> model
  | SwitchSection s -> { model with Section = s }


// View
open Fable.Core.JsInterop
open Fable.Helpers.React.Props
open Fable.Helpers.React
module R = Fable.Helpers.React

let drawCartesianGrid (model:Model) : React.ReactElement =

  let width,height = 350, 350
  let margin = 10
  
  let x = D3.Scale.Globals.linear().range([|0.;float (width - margin * 2)|]).domain([|1.;10.|])
  let y = D3.Scale.Globals.linear().range([|float (height - margin * 2);0.|]).domain([|1.;10.|])
  
  let xAxis = D3.Svg.Globals.axis().scale(x).tickSize(-(float (height - margin * 2)))
  let yAxis = D3.Svg.Globals.axis().scale(y).orient("right").tickSize(float (width - margin * 2))

  let node  = ReactFauxDOM?createElement("svg") :?>  Browser.EventTarget

  D3.Globals.select(node)
    .attr("height", unbox<D3.Primitive> height)
    .attr("width", unbox<D3.Primitive> width) |> ignore

  let svg = D3.Globals.select(node)
  
  svg?append("g")
      ?attr("class",  "x axis")
      ?attr("transform", "translate(0," + (height - margin * 2).ToString() + ")") 
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

  svg.append("circle")
    ?attr("r", 4)
    ?attr("cx", 10)
    ?attr("cy", 50) |> ignore


  node?toReact() :?> React.ReactElement

let sidebarView model (onClick: Message -> DOMAttr) =
  R.section [ Id "sidebar" ] [
    R.button [ Id "sidebar-collapse" ] []
    R.nav [] [
      R.a [ onClick <| SwitchSection Control ] [ R.img [ Src "icons/control-section.svg" ] [] ; unbox "Control" ]
      R.a [ onClick <| SwitchSection Section.Calibrate ] [ R.img [ Src "icons/calibrate-section.svg" ] [] ; unbox "Calibrate" ]
      R.a [ onClick <| SwitchSection Paths ] [ R.img [ Src "icons/paths-section.svg" ] [] ; unbox "Paths" ]
      R.a [ onClick <| SwitchSection Settings ] [ R.img [ Src "icons/settings-section.svg" ] [] ; unbox "Settings" ]
    ]
  ]

let settingsView model =
  R.section [ Id "settings-view"; ClassName "main-section" ] [
    R.h1 [] [ unbox "Settings View" ]
    R.p [] [unbox "Icons designed by Alfredo Hernandez, Freepik, and SplashIcons, from Flaticon" ]
  ]

let pathsView model =
  R.section [ Id "paths-view"; ClassName "main-section" ] [
    R.h1 [] [ unbox "Paths View" ]
  ]

let calibrateView model =
  R.section [ Id "calibrate-view"; ClassName "main-section" ] [
    R.h1 [] [ unbox "Settings View" ]
  ]

let controlView model =
  R.section [ Id "control-view"; ClassName "main-section" ] [
    R.h1 [] [ unbox "Control View" ]
    R.label [] [ unbox "Cartesian grid" ]
    R.fn drawCartesianGrid model []
  ]


let view (model:Model) dispatch =
    let onClick msg =
      OnClick <| fun _ -> msg |> dispatch

    let sectionView =
      match model.Section with
      | Section.Calibrate -> calibrateView
      | Section.Control -> controlView
      | Section.Paths -> pathsView
      | Section.Settings -> settingsView

    R.div [] [
      sidebarView model onClick
      sectionView model
    ]

// App
Program.mkSimple init update view
|> Program.withConsoleTrace
|> Program.withReact "elmish-app"
|> Program.run