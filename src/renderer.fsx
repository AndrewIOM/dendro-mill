#r "../node_modules/fable-core/Fable.Core.dll"
#r "../node_modules/fable-react/Fable.React.dll"
#r "../node_modules/fable-elmish/Fable.Elmish.dll"
#r "../node_modules/fable-elmish-react/Fable.Elmish.React.dll"
#load "../node_modules/fable-import-d3/Fable.Import.D3.fs"
#load "johnnyfive.fsx"

open Elmish
open Elmish.React
open Fable.Import
open Fable.Import.Browser
open Fable.Core
open Fable.Core.JsInterop
open Fable.React
open Fable.Import.JohnnyFive

// Arduino - Global variables to avoid them being disposed
let mutable arduino: Board option = None
let mutable xAxis: Stepper option = None
let mutable yAxis: Stepper option = None
let mutable zAxis: Stepper option = None
let mutable rAxis: Stepper option = None

// Arduino Setup
type PinMode =
| Input = 0
| Output = 1
| Analog = 2
| Pwm = 3
| Servo = 4

let connectArduino () =
    let board = JohnnyFive.Board()

    board.on("ready", unbox(fun () -> 
        board.pinMode(13.,float PinMode.Output)

        // Setup motors
        let defaultOptions =         
            let opt = createEmpty<StepperOption>
            opt.rpm <- Some 200.
            opt.direction <- Some 1.
            opt.stepsPerRev <- 16.
            opt

        let xOption = defaultOptions
        xOption.pins <- [1.;2.]

        let yOption = defaultOptions
        yOption.pins <- [3.;4.]

        xAxis <- Some (Stepper(U3.Case3 xOption))
        yAxis <- Some (Stepper(U3.Case3 yOption))
        ()

        ) ) |> ignore
    arduino <- Some board

connectArduino()

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
    CurrentStep: int // Where motor starts and stops
    MaxStep: int
    MinStep: int
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

type Axis =
  | X
  | Y
  | Z
  | R
  | A
  | B

type Message =
  | ConnectArduino of string
  | Calibrate
  | SwitchSection of Section
  | Move of Axis * int

let update (msg:Message) (model:Model)  =
  match msg with
  | ConnectArduino port -> 
      connectArduino() |> ignore
      { model with CartesianX = Enabled { CurrentStep = 0; MaxStep = 100; MinStep = 100 } 
                   CartesianY = Enabled { CurrentStep = 0; MaxStep = 100; MinStep = 100 } }
  | Message.Calibrate -> model
  | SwitchSection s -> { model with Section = s }
  | Move (axis,steps) ->
      match axis with
      | X ->
        match model.CartesianX with
        | Disabled -> model
        | Enabled ax -> {model with CartesianX = Enabled { ax with CurrentStep = ax.CurrentStep + steps }}
      | Y ->
        match model.CartesianY with
        | Disabled -> model
        | Enabled ax -> {model with CartesianY = Enabled { ax with CurrentStep = ax.CurrentStep + steps }}
      | _ -> model

// View
open Fable.Core.JsInterop
open Fable.Helpers.React.Props
open Fable.Helpers.React
module R = Fable.Helpers.React

let drawCartesianGrid (model:Model) : React.ReactElement =

  let width,height = 350, 350
  let margin = 10
  
  let x = D3.Scale.Globals.linear().range([|0.;float (width - margin * 2)|]).domain([|-100.;100.|])
  let y = D3.Scale.Globals.linear().range([|float (height - margin * 2);0.|]).domain([|-100.;100.|])
  
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

  match model.CartesianX with
  | Motor.Disabled -> ()
  | Enabled xPos ->
    match model.CartesianY with
    | Motor.Disabled -> ()
    | Enabled yPos ->
      svg.append("circle")
        ?attr("r", 4)
        ?attr("cx", xPos.CurrentStep |> float |> x.Invoke)
        ?attr("cy", yPos.CurrentStep |> float |> y.Invoke) |> ignore

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

let settingsView dispatch model =
  R.section [ Id "settings-view"; ClassName "main-section" ] [
    R.h1 [] [ unbox "Settings" ]

    R.label [] [ R.str "Arduino Port" ]
    R.select [ OnChange (fun ev -> !!ev.target?value |> ConnectArduino |> dispatch )  ] []

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

let controlView (onClick:Message->DOMAttr) model =
  R.section [ Id "control-view"; ClassName "main-section" ] [
    R.h1 [] [ unbox "Control View" ]
    R.label [] [ unbox "Cartesian grid" ]
    R.fn drawCartesianGrid model []
    R.div [ ClassName "direction-buttons" ] [
      R.str "Move manually..."
      R.button [ onClick <| ConnectArduino "random-port" ] [ R.str "Connect Arduino" ]
      R.button [ onClick <| Move (Axis.Y,1) ] [ R.str "North" ]
      R.button [ onClick <| Move (Axis.Y,-1) ] [ R.str "South" ]
      R.button [ onClick <| Move (Axis.X,1) ] [ R.str "East" ]
      R.button [ onClick <| Move (Axis.X,-1) ] [ R.str "West" ]
    ]
  ]


let view (model:Model) dispatch =
    let onClick msg =
      OnClick <| fun _ -> msg |> dispatch

    let sectionView =
      match model.Section with
      | Section.Calibrate -> calibrateView
      | Section.Control -> controlView onClick
      | Section.Paths -> pathsView
      | Section.Settings -> settingsView dispatch

    R.div [] [
      sidebarView model onClick
      sectionView model
    ]

// App
Program.mkSimple init update view
|> Program.withConsoleTrace
|> Program.withReact "elmish-app"
|> Program.run