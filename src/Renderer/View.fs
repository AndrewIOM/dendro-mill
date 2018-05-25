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

    let statusDot (state:Hardware.MoveState) =
        match state with
            | Hardware.MoveState.Calibrating -> R.span [ Class "dot dot-calibrating" ] []
            | Hardware.MoveState.Moving -> R.span [ Class "dot dot-moving" ] []
            | Hardware.MoveState.Ready -> R.span [ Class "dot dot-ready" ] []

    let calibrationStatusDots (model:AppModel) =
        match model.Hardware with
        | Hardware.Online s ->
            [ statusDot (s.X |> fst)
              statusDot (s.Y |> fst) 
              statusDot (s.Vertical |> fst) 
              statusDot (s.Tilt |> fst) ]
        | _ -> []

    let statusBar (model:AppModel) =
        let statusText = 
            match model.Hardware with
            | Hardware.Model.NotConnected -> "Status: Not Connected"
            | Hardware.Model.Connecting -> "Status: Looking for Micromill..."
            | Hardware.Model.Online _ -> "Status: Micromill Online"
            | Hardware.Model.Offline error -> sprintf "Status: Micromill Offline (Error: %s)" error
        R.div [ Class "status-bar" ] [ 
            R.div [ Class "status-left" ] [ R.span [] [ unbox statusText ] ]
            R.div [ Class "status-right" ] (calibrationStatusDots model) ]


module Components =

    let card (title:string) content =
        R.div [ Class "card card-secondary" ] [
            R.h3 [] [ unbox title ]
            content
        ]

    let container content =
        R.div [ Class "container" ] content

    let row content =
        R.div [ Class "row" ] content

    let dropZone slim dispatch =
        R.div [ 
            Class (if slim then "drop-zone slim" else "drop-zone")
            OnDrop(fun e -> 
                e.preventDefault() 
                e.stopPropagation()
                if e.dataTransfer.files.length = 1. then 
                    ((e.dataTransfer.files.item 0.)?path.ToString())
                    |> dispatch )
            OnDragOver(fun e ->
                e.preventDefault()
                e.stopPropagation() )
        ]

    let pathQueue (queue:Software.PathQueue) =
        let active =
            match queue.InProgress with
            | Some p -> R.li [ Class "active" ] [ R.str (snd p) ]
            | None -> R.li [] []

        R.div [ Class "card" ] [
            R.ul [] (active :: (queue.Queued |> List.map (fun p -> R.li [] [ R.str (snd p) ]) ))
        ]

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

    let castToInt (o:obj) =
        match o with
        | :? int as e -> e
        | _ -> 0

    let getMatrix dispatch (e:React.MouseEvent) = 
        let matrix =
            Browser.document.getElementById("transform-layer")
                .style.transform
                .Replace("matrix3d(", "")
                .Replace(")", "")
                .Split(',')
            |> Array.map float
            |> Software.MatrixTransform.create
        printfn "%A" matrix
        matrix |> Software.SaveCalibrationPosition |> SoftwareMsg |> dispatch

    let calibrateGrid model : React.ReactElement =
        let layout : Graphics.GraphElement.Layout = { Height = 300; Width = 300; Margin = { Top = 0; Bottom = 25; Left = 0; Right = 25 } }
        let imageBase64,matrix =
            match model.Software.Calibration with
            | Software.ImageCalibration s ->
                match s with 
                | Software.ImageState.Fixed (i,m) -> i, Some m.Value
                | Software.ImageState.Floating i -> i, None
            | _ -> "", None
        Graphics.Grid.create layout 215.<mm>
        |> Graphics.Grid.withControlPoints Config.controlPoints
        |> Graphics.Grid.withTransform layout imageBase64 matrix
        |> Graphics.Grid.toReact

    let calibrate dispatch model =
        R.section [ Id "calibrate-view"; ClassName "main-section" ] [
            R.h1 [] [ unbox "Calibrate" ]
            R.hr []
            R.p [] [ R.str "After your sample is fixed in position, take a top-down image of the drill stage surface. You can then calibrate it here." ]
            Components.container [
                Components.row [
                    R.div [ Class "six columns" ] [
                        Components.dropZone false (Software.UploadCalibrationImage >> SoftwareMsg >> dispatch) [ 
                            ( match model.Software.Calibration with
                            | Software.Calibration.Uncalibrated -> unbox "Drop file here" 
                            | Software.Calibration.ImageCalibration s -> 
                                match s with
                                | Software.ImageState.Floating i
                                | Software.ImageState.Fixed (i,_) -> R.img [ Src i ] ) ] 
                    ]
                    R.div [ Class "six columns" ] ( 
                        match model.Software.Calibration with
                        | Software.Calibration.Uncalibrated -> [ R.p [] [ unbox "Add an image to continue" ] ]
                        | Software.Calibration.ImageCalibration s ->
                            match s with
                            | Software.ImageState.Floating i
                            | Software.ImageState.Fixed (i,_) -> 
                                [ R.button [ OnClick <| getMatrix dispatch ] [ R.str "Save Base Layer Position" ]
                                  calibrateGrid model ]
                    )
                ]
            ]
        ]

    let pathGrid model : React.ReactElement =
        let layout : Graphics.GraphElement.Layout = { Height = 550; Width = 550; Margin = { Top = 10; Bottom = 25; Left = 0; Right = 25 } }
        let calibrationBase64,calibrationMatrix =
            match model.Software.Calibration with
            | Software.ImageCalibration s ->
                match s with 
                | Software.ImageState.Fixed (i,m) -> i, Some m.Value
                | Software.ImageState.Floating i -> i, None
            | _ -> "", None
        let templateBase64,templateMatrix =
            match model.Software.PathTemplate with
            | Some s ->
                match s with 
                | Software.ImageState.Fixed (i,m) -> i, Some m.Value
                | Software.ImageState.Floating i -> i, None
            | None -> "", None
        Graphics.Grid.create layout 215.<mm>
        |> Graphics.Grid.withStaticOverlay layout calibrationBase64 calibrationMatrix
        |> Graphics.Grid.withTransform layout templateBase64 templateMatrix
        |> Graphics.Grid.withDrawTool
        |> Graphics.Grid.withExistingPaths model.Software.Paths
        |> Graphics.Grid.toReact

    let savePath dispatch =
        let currentPathElement = Browser.document.getElementById "path-creating"
        if not (isNull currentPathElement) then
            let path = (currentPathElement.getAttribute "data-points").Split(',')
            let path2 = seq { for i in 0 .. 2 .. path.Length - 2 -> (float path.[i], float path.[i+1]) } |> Seq.toArray
            path2 |> Array.toList |> Software.QueuePath |> SoftwareMsg |> dispatch

    let paths dispatch model =
        R.section [ Id "paths-view"; ClassName "main-section" ] [
          R.h1 [] [ unbox "Paths" ]
          R.p [] [ R.str "Create paths for the drill to follow. You can use custom template overlays, for example wood anatomy slide scans." ]
          R.hr []
          Components.container [
              Components.row [
                    R.div [ Class "eight columns" ] [
                        pathGrid model
                    ]
                    R.div [ Class "four columns" ] [
                        R.label [] [ R.str "Use a Template" ]
                        Components.dropZone true (Software.UploadPathImage >> SoftwareMsg >> dispatch) [ 
                            ( match model.Software.PathTemplate with
                              | None -> R.str "Drop an image here" 
                              | Some s -> 
                                match s with
                                | Software.ImageState.Floating i
                                | Software.ImageState.Fixed (i,_) -> R.img [ Src i ] ) ]
                        R.hr []
                        R.label [] [ R.str "Current Path Name" ]
                        R.input [ Value model.Software.EditingPathName
                                  OnChange (fun e -> e.target?value |> string |> Software.Msg.ChangePathName |> SoftwareMsg |> dispatch  ) ] 
                        R.button [ OnClick (fun _ -> savePath dispatch ) ] [ R.str "Save Path" ]
                        R.label [] [ R.str "Saved Paths:" ]
                        Components.pathQueue model.Software.Paths
                    ]
                ]
            ]
        ]

    let grid model : React.ReactElement =
        let layout : Graphics.GraphElement.Layout = { Height = 550; Width = 550; Margin = { Top = 10; Bottom = 25; Left = 0; Right = 25 } }
        let imageBase64,matrix =
            match model.Software.Calibration with
            | Software.ImageCalibration s ->
                match s with 
                | Software.ImageState.Fixed (i,m) -> i, Some m.Value
                | Software.ImageState.Floating i -> i, None
            | _ -> "", None
        Graphics.Grid.create layout 215.<mm>
        |> Graphics.Grid.withControlPoints Config.controlPoints
        |> Graphics.Grid.withCurrentPosition model
        |> Graphics.Grid.withStaticOverlay layout imageBase64 matrix
        |> Graphics.Grid.withExistingPaths model.Software.Paths
        |> Graphics.Grid.toReact

    let verticalGraph model : React.ReactElement =
        let layout : Graphics.GraphElement.Layout = { Height = 200; Width = 200; Margin = {Top = 0; Left = 0; Right = 25; Bottom = 10 } }
        match model.Hardware with
        | Hardware.Online s -> Graphics.Vertical.complete layout 120. (snd s.Tilt) 12.<mm>
        | _ -> R.text [] [ R.str "Micromill not connected" ]

    let control (onClick:AppMsg->DOMAttr) (model:AppModel) =
        R.section [ Id "control-view"; ClassName "main-section" ] [
            Components.container [
                Components.row [
                    R.div [ Class "eight columns" ] [
                        R.label [] [ R.str "Top View" ]
                        R.ofFunction grid model [] 
                    ]
                    R.div [ Class "four columns" ] [
                        R.label [] [ R.str "Vertical" ]
                        verticalGraph model
                        R.label [] [ R.str "Path Queue" ]
                        Components.pathQueue model.Software.Paths
                    ]
                ]
                Components.row [
                    R.div [ Class "twelve columns" ] [
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
        | Software.ViewSection.Paths -> Pages.paths dispatch
        | Software.ViewSection.Settings -> Pages.settings onClick

    R.div [] [
      Layout.sideBar model onClick
      Layout.statusBar model
      sectionView model ]
