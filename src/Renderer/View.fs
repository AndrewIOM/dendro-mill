module View

open Fable.Core
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

[<Import("event", from="d3")>]
let currentEvent : React.MouseEvent = jsNative

/////////////////////////
/// Required Maths
/////////////////////////

module Numeric =

    let numeric = importAll<obj> "../../node_modules/numeric/numeric-1.2.6.js"

    [<Emit("numeric.solve($0,$1,true)")>]
    let solve (a: float[][]) (b: float[]) : float[] = jsNative

    let transform (sourcePoints:float[][]) (targetPoints: float[][]) : float[] =
        let a =
            sourcePoints
            |> Array.mapi (fun i s ->
                let t = targetPoints.[i]
                let a1 = [|s.[0]; s.[1]; 1.; 0.; 0.; 0.; -s.[0] * t.[0]; -s.[1] * t.[0]|]
                let a2 = [|0.; 0.; 0.; s.[0] ;s.[1] ;1. ;-s.[0] * t.[1]; -s.[1] * t.[1]|]
                [| a1; a2 |] )
            |> Array.concat
        let b =
            targetPoints
            |> Array.mapi (fun _ t -> [|t.[0]; t.[1]|] )
            |> Array.concat
        solve a b


/////////////////////////
/// View Components
/////////////////////////

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


module Graphing =

    type GraphElement = {
        Element: D3.Selection<obj>
        Node: Browser.EventTarget
    }

    let ReactFauxDOM = importAll<obj> "react-faux-dom/lib/ReactFauxDOM"  

    let removeMM (x:float<_>) = float x

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

    let toReactElement elem = elem.Node?toReact() :?> React.ReactElement

    let createElement name =
        let node = ReactFauxDOM?createElement(name) :?>  Browser.EventTarget
        { Element = D3.Globals.select node; Node = node }

    let basicGrid' (elem:D3.Selection<obj>) =
        let width,height = 200,200
        let margin = 25
        let x = D3.Scale.Globals.linear().range([|0.;float (width - margin * 2)|]).domain([|0.;215.|])
        let y = D3.Scale.Globals.linear().range([|float (height - margin * 2);0.|]).domain([|0.;215.|])

        let xAxis = D3.Svg.Globals.axis().scale(x).tickSize(-(float (height - margin * 2)))
        let yAxis = D3.Svg.Globals.axis().scale(y).orient("right").tickSize(float (width - margin * 2))

        elem
          .attr("height", unbox<D3.Primitive> height)
          .attr("width", unbox<D3.Primitive> width) |> ignore

        elem?append("g")
            ?attr("class",  "x axis")
            ?attr("transform", "translate(0," + (height - margin * 2).ToString() + ")") 
            ?call(xAxis) 
            |> ignore 

        elem.append("g")
          .attr("class", unbox<D3.Primitive> "y axis")
          ?call(yAxis)
          ?append("text")
          ?attr("transform", "rotate(-90)")
          ?attr("y", 6)
          ?attr("dy", ".71em")
          ?style("text-anchor", "end")
          |> ignore

        elem

    let basicGrid (state:AppModel) elem =
        { elem with Element = basicGrid' elem.Element }

    let grid model =
        createElement "svg"
        |> basicGrid model
        |> toReactElement

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


module Transform =

    type Margins = { Top: int; Right: int; Bottom: int; Left: int }

    let margins = { Top = 20; Right = 30; Bottom = 20; Left = 30 }

    let width = 300 - margins.Left - margins.Right
    let height = 300 - margins.Top - margins.Bottom
    
    let pointPadding = 50

    let appendSvg name (elem:Graphing.GraphElement) =
        let svg = 
            elem.Element.append("svg")
                .attr("id",unbox<D3.Primitive> name)
                .attr("width", unbox<D3.Primitive> (width + margins.Left + margins.Right) )
                .attr("height", unbox<D3.Primitive> (height + margins.Top + margins.Bottom) )
        svg.append("g").attr("transform", unbox<D3.Primitive> (sprintf "translate(%i,%i)" margins.Left margins.Top) ) |> ignore
        svg

    let create imageBase64 =
        let container = Graphing.createElement "div"
        container.Element.attr("class", unbox<D3.Primitive> "transform-canvas") |> ignore
        let svgTransform = container |> appendSvg "transform"
        let svgFlat = container |> appendSvg "flat"

        svgTransform.style("transform-origin", unbox<D3.Primitive> (sprintf "%ipx %ipx 0" margins.Left margins.Top)) |> ignore

        // Attach currently selected image
        svgTransform.select("g")
            .append("image")
            .attr("xlink:href", unbox<D3.Primitive> imageBase64 )
            .attr("preserveAspectRatio", unbox<D3.Primitive> "none")
            .attr("width", unbox<D3.Primitive> width)
            .attr("height", unbox<D3.Primitive> height) |> ignore

        // Grid lines (x axis)
        svgTransform.select("g")
            .selectAll(".line--x")
            .data(D3.Globals.range(0., (float width) + 1., (float width) / 10.))
            ?enter()
            ?append("line")
            ?attr("class", "line line--x")
            ?attr("x1", id)
            ?attr("x2", id)
            ?attr("y1", 0)
            ?attr("y2", height) |> ignore

        // Grid lines (y axis)
        svgTransform.select("g")
            .selectAll(".line--y")
            .data(D3.Globals.range(0., (float height) + 1., (float height) / 10.))
            ?enter()
            ?append("line")
            ?attr("class", "line line--y")
            ?attr("x1", 0)
            ?attr("x2", width)
            ?attr("y1", id)
            ?attr("y2", id) |> ignore

        let mutable movingHandle : int option = None
        let sourcePoints = [|[|0.;0.|]; [|float width;0.|]; [|float width;float height|]; [|0.;float height|]|]
        let mutable targetPoints = [|[|0.;0.|]; [|float width;0.|]; [|float width;float height|]; [|0.;float height|]|]

        let moveHandle (x:int) (y:int) =
            match movingHandle with
            | None -> ()
            | Some h ->
                let svg = Browser.document.getElementById("transform")
                let rect = svg.getBoundingClientRect()
                D3.Globals.select(sprintf ".handle-%i" (int h))
                    .attr("transform", unbox<D3.Primitive> (sprintf "translate(%f,%f)" (float x - (float margins.Left) - rect.left) (float y - rect.top - (float margins.Top))))
                    |> ignore
                targetPoints.[int h] <- [| float (x - margins.Left) - rect.left ; float (y - margins.Top) - rect.top |]
                let x = Numeric.transform sourcePoints targetPoints
                let matrix = 
                    [| x.[0]; x.[3]; 0.; x.[6]
                       x.[1]; x.[4]; 0.; x.[7]
                       0.;    0.;    1.; 0.
                       x.[2]; x.[5]; 0.; 1. |] |> Array.map(fun i -> D3.Globals.round(i,6.))
                let mString = (sprintf "matrix3d(%A)" matrix).Replace("[", "").Replace("]", "")
                printfn "%s" mString
                D3.Globals.select("#transform").style("transform", unbox<D3.Primitive> mString) |> ignore

        // Grab behaviour for grab handles
        svgFlat
            ?on("mousemove", fun _ -> moveHandle (int currentEvent.clientX) (int currentEvent.clientY))
            ?on("mouseup", fun _ -> movingHandle <- None)
            |> ignore

        // Grab Handles
        svgFlat.select("g")
            .selectAll(".handle")
            ?data(targetPoints)
            ?enter()
            ?append("circle")
            ?attr("class", fun _ (i:float) -> "handle handle-" + i.ToString())
            ?attr("transform", fun (d:int list) -> (sprintf "translate(%i,%i)" d.[0] d.[1]))
            ?attr("r",7)
            ?on("mousedown", fun _ (i:float) -> movingHandle <- Some (int i))
            |> ignore
            // NB: Using d3.drag() doesn't work, as the event always has x and y as NaN.

        container |> Graphing.toReactElement


module Pages =

    let settings onClick model =
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

    let paths model =
        R.section [ Id "paths-view"; ClassName "main-section" ] [
          R.h1 [] [ unbox "Paths View" ]
        ]

    let castToInt (o:obj) =
        match o with
        | :? int as e -> e
        | _ -> 0

    let getCoordinate dispatch (e:React.MouseEvent) = 
        let x = e.nativeEvent?offsetX |> castToInt
        let y = e.nativeEvent?offsetY |> castToInt
        (x,y) |> Software.AddCalibrationPoint |> SoftwareMsg |> dispatch

    let calibrate dispatch model =
        R.section [ Id "calibrate-view"; ClassName "main-section" ] [
            R.h1 [] [ unbox "Calibrate" ]
            R.hr []
            R.p [] [ R.str "After your sample is fixed in position, take a top-down image of the drill stage surface. You can then calibrate it here." ]
            Components.container [
                Components.row [
                    R.div [ Class "six columns" ] [
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
                            | Software.Calibration.ImageCalibration s -> R.img [ Src s.Image; OnMouseDown (getCoordinate dispatch) ] )
                            R.canvas [ Id "calibration-canvas" ] [] ]
                    ]
                    R.div [ Class "six columns" ] [
                        ( match model.Software.Calibration with
                        | Software.Calibration.Uncalibrated -> R.p [] [ unbox "Hello" ]
                        | Software.Calibration.ImageCalibration i ->
                            Transform.create i.Image )
                            //Graphing.grid model )
                    ]
                ]
            ]
        ]

    let control (onClick:AppMsg->DOMAttr) (model:AppModel) =
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
        | Software.ViewSection.Calibrate -> Pages.calibrate dispatch
        | Software.ViewSection.Control -> Pages.control onClick
        | Software.ViewSection.Paths -> Pages.paths
        | Software.ViewSection.Settings -> Pages.settings onClick

    R.div [] [
      Layout.sideBar model onClick
      Layout.statusBar model
      sectionView model ]
