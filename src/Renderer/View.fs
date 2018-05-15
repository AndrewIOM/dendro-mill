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

    let getMatrix dispatch (e:React.MouseEvent) = 
        let matrix =
            Browser.document.getElementById("transform-layer")
                .style.transform
                .Replace("matrix3d(", "")
                .Replace(")", "")
                .Split(',')
            |> Array.map float
            |> Software.MatrixTransform.create
        matrix |> Software.SaveCalibrationPosition |> SoftwareMsg |> dispatch

    let calibrate dispatch model =

        let calibrateGrid model : React.ReactElement =
            let layout : Graphics.GraphElement.Layout = { Height = 300; Width = 300; Margin = { Top = 0; Bottom = 25; Left = 0; Right = 25 } }
            let imageBase64,matrix =
                match model.Software.Calibration with
                | Software.ImageCalibration s ->
                    match s with 
                    | Software.CalibrationState.Fixed (i,m) -> i, Some m.Value
                    | Software.CalibrationState.Floating i -> i, None
                | _ -> "", None
            Graphics.Grid.create layout 215.<mm>
            |> Graphics.Grid.withControlPoints Config.controlPoints
            |> Graphics.Grid.withOverlay layout imageBase64 matrix
            |> Graphics.Grid.withOverlayAdjustment layout
            |> Graphics.Grid.toReact

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
                            | Software.Calibration.ImageCalibration s -> 
                                match s with
                                | Software.CalibrationState.Floating i
                                | Software.CalibrationState.Fixed (i,_) -> R.img [ Src i ] ) ]
                    ]
                    R.div [ Class "six columns" ] ( 
                        match model.Software.Calibration with
                        | Software.Calibration.Uncalibrated -> [ R.p [] [ unbox "Add an image to continue" ] ]
                        | Software.Calibration.ImageCalibration s ->
                            match s with
                            | Software.CalibrationState.Floating i
                            | Software.CalibrationState.Fixed (i,_) -> 
                                [ R.button [ OnClick <| getMatrix dispatch ] [ R.str "Save Base Layer Position" ]
                                  //Transform.create i ]
                                  calibrateGrid model ]
                    )
                ]
            ]
        ]

    let grid model : React.ReactElement =
        let layout : Graphics.GraphElement.Layout = { Height = 300; Width = 300; Margin = { Top = 0; Bottom = 25; Left = 0; Right = 25 } }
        let imageBase64,matrix =
            match model.Software.Calibration with
            | Software.ImageCalibration s ->
                match s with 
                | Software.CalibrationState.Fixed (i,m) -> i, Some m.Value
                | Software.CalibrationState.Floating i -> i, None
            | _ -> "", None
        Graphics.Grid.create layout 215.<mm>
        |> Graphics.Grid.withControlPoints Config.controlPoints
        |> Graphics.Grid.withCurrentPosition model
        |> Graphics.Grid.withOverlay layout imageBase64 matrix
        |> Graphics.Grid.toReact


    let control (onClick:AppMsg->DOMAttr) (model:AppModel) =
        R.section [ Id "control-view"; ClassName "main-section" ] [
          R.h1 [] [ unbox "Control" ]
          R.hr []

          Components.container [
              Components.row [
                  R.div [ Class "eight columns" ] [
                      R.label [] [ unbox "Top View" ]
                      R.ofFunction grid model []
                    //   R.ofFunction Graphing.drawVerticalAxis model []
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
