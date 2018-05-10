module View

open Fable.Core.JsInterop
open Fable.Helpers.React.Props
open Fable.Import
open ViewState
open Types
open Fable.Import

module R = Fable.Helpers.React

/////////////////////////
/// View Styling
/////////////////////////

let sass = importAll<obj> "../Styles/main.scss"


/////////////////////////
/// View Components
/////////////////////////

let ReactFauxDOM = importAll<obj> "react-faux-dom/lib/ReactFauxDOM"  

module Layout =

    let sideBar (model:AppModel) onClick =
        let isActive section =
            if section = model.Software.Section
            then "active"
            else "inactive"
        R.section [ Id "sidebar" ] [
            R.nav [] [
                R.a [ onClick <| SoftwareMsg (Software.SwitchSection Software.ViewSection.Control); Class (isActive Software.ViewSection.Control) ] [ R.img [ Src "icons/control-section.svg" ] ; unbox "Control" ]
                R.a [ onClick <| SoftwareMsg (Software.SwitchSection Software.ViewSection.Calibrate); Class (isActive Software.ViewSection.Calibrate) ] [ R.img [ Src "icons/calibrate-section.svg" ] ; unbox "Calibrate" ]
                R.a [ onClick <| SoftwareMsg (Software.SwitchSection Software.ViewSection.Paths); Class (isActive Software.ViewSection.Paths) ] [ R.img [ Src "icons/paths-section.svg" ] ; unbox "Paths" ]
                R.a [ onClick <| SoftwareMsg (Software.SwitchSection Software.ViewSection.Settings); Class (isActive Software.ViewSection.Settings) ] [ R.img [ Src "icons/settings-section.svg" ] ; unbox "Settings & Help" ]
            ]
        ]

    let statusBar (model:AppModel) =
        let statusText = 
            match model.Hardware with
            | Hardware.Model.NotConnected -> "Status: Not Connected"
            | Hardware.Model.Connecting -> "Status: Looking for Micromill..."
            | Hardware.Model.Online _ -> "Status: Micromill Online"
            | Hardware.Model.Offline error -> sprintf "Status: Micromill Offline (Error: %s)" error
        R.div [ Class "status-bar" ] [ R.span [] [ unbox statusText ] ]


module Components =

    let card (title:string) content =
        R.div [ Class "card" ] [
            R.h3 [] [ unbox title ]
            content
        ]

    let container content =
        R.div [ Class "container" ] content

    let row content =
        R.div [ Class "row" ] content
        

module Grid =

    let responsiveGrid (size:float<mm>) =

        // let x = D3.Scale.Globals.linear().range([|0.;float (width - margin * 2)|]).domain([|0.;215.|])
        // let y = D3.Scale.Globals.linear().range([|float (height - margin * 2);0.|]).domain([|0.;215.|])

        // y
        2.


module Graphing =

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

    let cartesianGrid (state:AppModel) : React.ReactElement =

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

        match state.Hardware with
        | Hardware.Model.Online s ->
              svg.append("circle")
                ?attr("r", 4)
                ?attr("cx", s.X |> float |> x.Invoke)
                ?attr("cy", s.Y |> float |> y.Invoke) |> ignore
        | _ -> ()

        node?toReact() :?> React.ReactElement


let settingsView onClick model =
    R.section [ Id "settings-view"; ClassName "main-section" ] [
      R.h1 [] [ unbox "Settings" ]
      R.hr []
      
      Components.container [
          Components.row [
              R.div [ Class "six columns" ] [
                  R.button [ onClick <| HardwareMsg Hardware.EstablishConnection ] [ R.str "Connect Arduino" ]
              ]
              R.div [ Class "six columns" ] [
                  Components.card "Help" (R.small [] [unbox "Icons designed by Alfredo Hernandez, Freepik, and SplashIcons, from Flaticon" ])
              ]
          ]
      ]
    ]

let pathsView model =
    R.section [ Id "paths-view"; ClassName "main-section" ] [
      R.h1 [] [ unbox "Paths View" ]
    ]

let calibrateView dispatch model =
    R.section [ Id "calibrate-view"; ClassName "main-section" ] [
        R.h1 [] [ unbox "Calibrate" ]
        R.p [] [ R.str "After your sample is fixed in position, take a top-down image of the drill stage surface. You can then calibrate it here." ]
        R.div [ 
            Class "drop-zone" 
            OnDrop(fun e -> 
                e.preventDefault() 
                e.stopPropagation()
                if e.dataTransfer.files.length = 1. then 
                    ((e.dataTransfer.files.item 0.)?path.ToString())
                    |> Software.UploadCalibrationImage
                    |> SoftwareMsg
                    |> dispatch )
            OnDragOver(fun e ->
                e.preventDefault()
                e.stopPropagation() )
        ] [ (
            match model.Software.Calibration with
            | Software.Calibration.Uncalibrated -> unbox "Drop file here" 
            | Software.Calibration.ImageOnly i -> printfn "%s" i; R.img [ Src i ]
            | Software.Calibration.Calibrated s -> R.img [ Src s.Image ] ) ]
        // R.input [ Type "file"; OnChange (fun x -> 
        //     (Software.UploadCalibrationImage (printfn "%A" x.target?value; x.target?value.ToString()) |> SoftwareMsg) |> dispatch )  ]
        R.canvas [ Id "calibration-canvas" ] []
    ]

let controlView (onClick:AppMsg->DOMAttr) (model:AppModel) =
    R.section [ Id "control-view"; ClassName "main-section" ] [
      R.h1 [] [ unbox "Control" ]
      R.hr []

      Components.container [
          Components.row [
              R.div [ Class "eight columns" ] [
                  R.label [] [ unbox "Top View" ]
                  R.ofFunction Graphing.cartesianGrid model []
                  R.ofFunction Graphing.drawVerticalAxis model []
              ]
              R.div [ Class "four columns" ] [
                    Components.card "Manual Controls" (R.div [] 
                        [
                            R.button [ onClick <| HardwareMsg (Hardware.StartMoving (MovementDirection.Y,1.<mm>)) ] [ R.str "North" ]
                            R.button [ onClick <| HardwareMsg (Hardware.StartMoving (MovementDirection.Y,-1.<mm>)) ] [ R.str "South" ]
                            R.button [ onClick <| HardwareMsg (Hardware.StartMoving (MovementDirection.X,1.<mm>)) ] [ R.str "East" ]
                            R.button [ onClick <| HardwareMsg (Hardware.StartMoving (MovementDirection.X,-1.<mm>)) ] [ R.str "West" ]
                        ])
                ]
            ]
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
      Layout.sideBar model onClick
      Layout.statusBar model
      sectionView model ]
