module View

open Fable.Core.JsInterop
open Fable.Helpers.React.Props
open Fable.Import
open ViewState
open Types

module R = Fable.Helpers.React

/////////////////////////
/// View Styling
/////////////////////////

let sass = importAll<obj> "../Styles/main.scss"


/////////////////////////
/// View Components
/////////////////////////

let ReactFauxDOM = importAll<obj> "react-faux-dom/lib/ReactFauxDOM"  

let drawVerticalAxis model : React.ReactElement =

    let width,height = 25, 350

    let z = D3.Scale.Globals.linear()
              .range([| 0.; float (width * 2) |])
              .domain([| 0.; 50. |]) // 50 mm domain for movement

    let node = ReactFauxDOM?createElement("svg") :?>  Browser.EventTarget

    D3.Globals.select(node)
      .attr("height", unbox<D3.Primitive> height)
      .attr("width", unbox<D3.Primitive> width) |> ignore

    let svg = D3.Globals.select(node)

    svg.append("rect")
       ?attr("class", "z axis")
       ?attr("height", height)
       ?attr("width", width)
       |> ignore

    node?toReact() :?> React.ReactElement



let drawCartesianGrid (state:AppModel) : React.ReactElement =

    let width,height = 350, 350
    let margin = 25

    let x = D3.Scale.Globals.linear().range([|0.;float (width - margin * 2)|]).domain([|0.;215.|])
    let y = D3.Scale.Globals.linear().range([|float (height - margin * 2);0.|]).domain([|0.;215.|])

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

    let removeMM (x:float<_>) = float x
    let drawControlPoint xPos yPos =
        svg.append("circle")
          ?attr("r", 2)
          ?attr("fill", "darkred")
          ?attr("cx", xPos |> removeMM |> x.Invoke)
          ?attr("cy", yPos |> removeMM |> y.Invoke) |> ignore

    Config.controlPoints |> List.map (fun (x,y) -> drawControlPoint x y) |> ignore

    // match state.Stage with
    // | Disconnected -> ()
    // | Connected s ->
    //   match s.CartesianX with
    //   | Motor.Disabled -> ()
    //   | Enabled xPos ->
    //     match s.CartesianY with
    //     | Motor.Disabled -> ()
    //     | Enabled yPos ->
    //       svg.append("circle")
    //         ?attr("r", 4)
    //         ?attr("cx", xPos.CurrentStep |> float |> x.Invoke)
    //         ?attr("cy", yPos.CurrentStep |> float |> y.Invoke) |> ignore

    node?toReact() :?> React.ReactElement

let sidebarView (model:AppModel) onClick =
    R.section [ Id "sidebar" ] [
      R.button [ Id "sidebar-collapse" ] []
      R.nav [] [
        R.a [ onClick <| SoftwareMsg (Software.SwitchSection Software.ViewSection.Control) ] [ R.img [ Src "icons/control-section.svg" ] ; unbox "Control" ]
        R.a [ onClick <| SoftwareMsg (Software.SwitchSection Software.ViewSection.Calibrate) ] [ R.img [ Src "icons/calibrate-section.svg" ] ; unbox "Calibrate" ]
        R.a [ onClick <| SoftwareMsg (Software.SwitchSection Software.ViewSection.Paths) ] [ R.img [ Src "icons/paths-section.svg" ] ; unbox "Paths" ]
        R.a [ onClick <| SoftwareMsg (Software.SwitchSection Software.ViewSection.Settings) ] [ R.img [ Src "icons/settings-section.svg" ] ; unbox "Settings" ]
      ]
    ]

let settingsView onClick model =
    R.section [ Id "settings-view"; ClassName "main-section" ] [
      R.h1 [] [ unbox "Settings" ]
      R.button [ onClick <| HardwareMsg Hardware.EstablishConnection ] [ R.str "Connect Arduino" ]
      R.p [] [unbox "Icons designed by Alfredo Hernandez, Freepik, and SplashIcons, from Flaticon" ]
    ]

let pathsView model =
    R.section [ Id "paths-view"; ClassName "main-section" ] [
      R.h1 [] [ unbox "Paths View" ]
    ]

let calibrateView dispatch model =
    R.section [ Id "calibrate-view"; ClassName "main-section" ] [
      R.h1 [] [ unbox "Calibrate" ]
      R.p [] [ R.str "After your sample is fixed in position, take a top-down image of the drill stage surface. You can then calibrate it here." ]
      R.input [ Type "file"; OnChange (fun x -> 
        Software.UploadCalibrationImage (x.currentTarget?files[0] :?> string) |> SoftwareMsg |> dispatch)  ]
      R.canvas [ Id "calibration-canvas" ] []
    ]

let controlView (onClick:AppMsg->DOMAttr) (model:AppModel) =
    R.section [ Id "control-view"; ClassName "main-section" ] [
      R.h1 [] [ unbox "Control View" ]
      R.label [] [ unbox "Cartesian grid" ]
      R.ofFunction drawCartesianGrid model []
      R.ofFunction drawVerticalAxis model []
      R.div [ ClassName "direction-buttons" ] [
        R.str "Move manually..."
        R.button [ onClick <| HardwareMsg (Hardware.StartMoving (MovementDirection.Y,1.<mm>)) ] [ R.str "North" ]
        R.button [ onClick <| HardwareMsg (Hardware.StartMoving (MovementDirection.Y,-1.<mm>)) ] [ R.str "South" ]
        R.button [ onClick <| HardwareMsg (Hardware.StartMoving (MovementDirection.X,1.<mm>)) ] [ R.str "East" ]
        R.button [ onClick <| HardwareMsg (Hardware.StartMoving (MovementDirection.X,-1.<mm>)) ] [ R.str "West" ]
      ]
    ]


/////////////
/// Layouts
/////////////

let master (model:AppModel) dispatch =
    let onClick (msg:AppMsg) =
        OnClick <| fun _ -> msg |> dispatch

    let sectionView =
        match model.Software.Section with
        | Software.ViewSection.Calibrate -> calibrateView dispatch
        | Software.ViewSection.Control -> controlView onClick
        | Software.ViewSection.Paths -> pathsView
        | Software.ViewSection.Settings -> settingsView onClick

    R.div [] [
      sidebarView model onClick
      sectionView model ]
