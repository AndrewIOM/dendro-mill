#r "../node_modules/fable-core/Fable.Core.dll"
#load "johnnyfive.fsx"
#load "../node_modules/fable-import-electron/Fable.Import.Electron.fs"

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Import.Electron
open Fable.Import.JohnnyFive

// Serial Port
let mutable arduino: Board option = Option.None
let mutable xAxis: Stepper option = Option.None
let mutable yAxis: Stepper option = Option.None
let mutable zAxis: Stepper option = Option.None
let mutable rAxis: Stepper option = Option.None

// Arduino Setup
type PinMode =
| Input = 0
| Output = 1
| Analog = 2
| Pwm = 3
| Servo = 4

let connectArduino portAddress =
    let board = JohnnyFive.Board(port = portAddress)

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

// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mutable mainWindow: BrowserWindow option = Option.None

let createMainWindow () =

    connectArduino "someport"

    let options = createEmpty<BrowserWindowOptions>
    options.width <- Some 800.
    options.height <- Some 600.
    let window = electron.BrowserWindow.Create(options)

    // Load the index.html of the app.
    let opts = createEmpty<Node.url_types.UrlOptions>
    opts.pathname <- Some <| Node.path.join(__SOURCE_DIRECTORY__,  "../app/index.html")
    opts.protocol <- Some "file:"
    window.loadURL(Node.url.format(opts))

    #if DEBUG
    fs.watch(Node.path.join(Node.__dirname, "renderer.js"), fun _ ->
        window.webContents.reloadIgnoringCache() |> ignore
    ) |> ignore
    #endif

    // Emitted when the window is closed.
    window.on("closed", unbox(fun () ->
        // Dereference the window object, usually you would store windows
        // in an array if your app supports multi windows, this is the time
        // when you should delete the corresponding element.
        mainWindow <- Option.None
    )) |> ignore

    mainWindow <- Some window

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
electron.app.on("ready", unbox createMainWindow)

// Quit when all windows are closed.
electron.app.on("window-all-closed", unbox(fun () ->
    // On OS X it is common for applications and their menu bar
    // to stay active until the user quits explicitly with Cmd + Q
    if Node.``process``.platform <> "darwin" then
        electron.app.quit()
))

electron.app.on("activate", unbox(fun () ->
    // On OS X it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if mainWindow.IsNone then
        createMainWindow()
))