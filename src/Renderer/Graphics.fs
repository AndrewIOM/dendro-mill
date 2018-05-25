module Graphics

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Types

[<Import("event", from="d3")>]
let currentEvent : React.MouseEvent = jsNative

module GraphElement = 

    type GraphElement = {
        Element: D3.Selection<obj>
        Node: Browser.EventTarget
    }

    type Margins = { Top: int; Right: int; Bottom: int; Left: int }

    type Layout = {
        Margin: Margins
        Height: int
        Width: int
    }

    let ReactFauxDOM = importAll<obj> "react-faux-dom/lib/ReactFauxDOM"  

    let appendSvg layout name (elem:GraphElement) =
        let svg = 
            elem.Element.append("svg")
                .attr("id",unbox<D3.Primitive> name)
                .attr("width", unbox<D3.Primitive> (layout.Width) )
                .attr("height", unbox<D3.Primitive> (layout.Height) )
        svg

    let toReactElement elem = elem.Node?toReact() :?> React.ReactElement

    let createElement name =
        let node = ReactFauxDOM?createElement(name) :?>  Browser.EventTarget
        { Element = D3.Globals.select node; Node = node }


/////////////////////////
/// Numeric Solver
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
/// Custom Visualisations
/////////////////////////

let private removeMM (x:float<_>) = float x

module Grid =

    open GraphElement

    type Grid = {
        Container: GraphElement
        X: D3.Scale.Linear<float,float>
        Y: D3.Scale.Linear<float,float>
        BaseLayer: D3.Selection<obj>
        OverlayLayer: D3.Selection<obj>
        TransformLayer: D3.Selection<obj>
    }

    type LayerDisplayMode =
    | Static
    | Transformable

    let create (layout:Layout) (dimension:float<mm>) =
    
        let container = createElement "div"
        container.Element.attr("class", unbox<D3.Primitive> "visualisation-grid")
            .style("width", unbox<D3.Primitive> layout.Width) 
            .style("height", unbox<D3.Primitive> layout.Height) |> ignore

        let grid = {
            X = D3.Scale.Globals
                    .linear()
                    .range([|0.; float (layout.Width - layout.Margin.Left - layout.Margin.Right)|]).domain([|0. ;dimension |> removeMM|])
            Y = D3.Scale.Globals
                    .linear()
                    .range([|float (layout.Height - layout.Margin.Bottom); float layout.Margin.Top |]).domain([|0.; dimension |> removeMM|])
            Container = container
            TransformLayer = (container |> appendSvg layout "transform-layer")
            OverlayLayer = container |> appendSvg layout "overlay-layer"
            BaseLayer = container |> appendSvg layout "base-layer" }

        let xAxis = D3.Svg.Globals
                        .axis()
                        .scale(grid.X)
                        .tickSize(-float (layout.Height - layout.Margin.Top - layout.Margin.Bottom))

        let yAxis = D3.Svg.Globals
                        .axis()
                        .scale(grid.Y)
                        .orient("right")
                        .tickSize(float (layout.Width - layout.Margin.Left - layout.Margin.Right))

        grid.BaseLayer
          .attr("height", unbox<D3.Primitive> layout.Height)
          .attr("width", unbox<D3.Primitive> layout.Width) |> ignore

        grid.BaseLayer?append("g")
            ?attr("class",  "x axis")
            ?attr("transform", "translate(" + (layout.Margin.Left.ToString()) + "," + (layout.Height - layout.Margin.Bottom).ToString() + ")") 
            ?call(xAxis) 
            |> ignore

        grid.BaseLayer.append("g")
          .attr("class", unbox<D3.Primitive> "y axis")
          .attr("transform", unbox<D3.Primitive> ("translate(" + (layout.Margin.Left.ToString()) + ",0)"))
          ?call(yAxis)
          ?append("text")
          ?attr("transform", "rotate(-90)")
          ?attr("y", 6)
          ?attr("dy", ".71em")
          ?style("text-anchor", "end")
          |> ignore

        // Container for further items (transformed..)
        let appendable = 
            grid.BaseLayer.append("g")
                .attr("id", unbox<D3.Primitive> "elements")
                .attr("transform", unbox<D3.Primitive> ("translate(" + (layout.Margin.Left.ToString()) + ",0)"))
        { grid with BaseLayer = appendable }

    let withControlPoints (points:(float<mm> * float<mm>) list) (grid:Grid) =
        points |> List.iter (fun (x,y) -> 
            grid.BaseLayer.append("circle")
              ?attr("r", 2)
              ?attr("fill", "darkred")
              ?attr("cx", x |> removeMM |> grid.X.Invoke)
              ?attr("cy", y |> removeMM |> grid.Y.Invoke)
              |> ignore )
        grid

    let withCurrentPosition (state:ViewState.AppModel) grid =
        match state.Hardware with
        | ViewState.Hardware.Model.Online s ->
              grid.BaseLayer.append("circle")
                ?attr("r", 4)
                ?attr("cx", s.X |> snd |> float |> grid.X.Invoke)
                ?attr("cy", s.Y |> snd |> float |> grid.Y.Invoke) |> ignore
        | _ -> ()
        grid

    let withImageLayer layout (imageBase64:string) (transform:float[] option) mode grid =
        match imageBase64.Length with
        | 0 -> grid
        | _ ->
            let layer =
                match mode with
                | Transformable -> grid.TransformLayer
                | Static -> grid.OverlayLayer

            layer
                .style("transform-origin", unbox<D3.Primitive> (sprintf "%ipx %ipx 0" layout.Margin.Left layout.Margin.Top))
                |> ignore

            // Attach currently selected image
            layer
                .append("image")
                .attr("xlink:href", unbox<D3.Primitive> imageBase64 )
                .attr("preserveAspectRatio", unbox<D3.Primitive> "none")
                .attr("opacity", unbox<D3.Primitive> 0.5)
                .attr("width", unbox<D3.Primitive> (layout.Width - layout.Margin.Left - layout.Margin.Right))
                .attr("height", unbox<D3.Primitive> (layout.Height - layout.Margin.Top - layout.Margin.Bottom)) |> ignore

            // Grid lines (x axis)
            layer
                .selectAll(".line--x")
                .data(D3.Globals.range(0., (float <| layout.Width - layout.Margin.Right - layout.Margin.Left) + 1., (float <| layout.Width - layout.Margin.Right - layout.Margin.Left) / 4.))
                ?enter()
                ?append("line")
                ?attr("class", "line line--x")
                ?attr("x1", id)
                ?attr("x2", id)
                ?attr("y1", 0)
                ?attr("y2", layout.Height - layout.Margin.Bottom - layout.Margin.Top) |> ignore

            // Grid lines (y axis)
            layer
                .selectAll(".line--y")
                .data(D3.Globals.range(0., (float <| layout.Height - layout.Margin.Bottom - layout.Margin.Top) + 1., (float <| layout.Height - layout.Margin.Bottom - layout.Margin.Top) / 4.))
                ?enter()
                ?append("line")
                ?attr("class", "line line--y")
                ?attr("x1", 0)
                ?attr("x2", layout.Width - layout.Margin.Left - layout.Margin.Right)
                ?attr("y1", id)
                ?attr("y2", id) |> ignore

            // If image is already transformed, set it
            match transform with
            | None -> ()
            | Some x ->
                let mString = ("matrix3d(" + (x |> Array.rev |> Array.map (fun x -> x.ToString()) |> Array.fold(fun x s -> s + "," + x) "") + ")").Replace(",)", ")")
                layer.style("transform", unbox<D3.Primitive> mString) |> ignore
            grid

    let withTransformHandles layout grid =

        let mutable movingHandle : int option = None

        let getBounds () =
            let minX = 0 |> float
            let minY = 0 |> float
            let maxX = layout.Width - layout.Margin.Right - layout.Margin.Left |> float
            let maxY = layout.Height - layout.Margin.Bottom - layout.Margin.Top |> float
            [| [| minX ; minY |]
               [| maxX ; minY |]
               [| maxX ; maxY |]
               [| minX ; maxY |] |]

        let sourcePoints = getBounds()
        let mutable targetPoints = getBounds()

        let moveHandle (x:float) (y:float) =
            match movingHandle with
            | None -> ()
            | Some h ->
                let svg = Browser.document.getElementById("transform-layer")
                let rect = svg.getBoundingClientRect()
                let xFromOrigin = (float x - float rect.left) 
                let yFromOrigin = (float y - rect.top)
                D3.Globals.select(sprintf ".handle-%i" (int h))
                    .attr("transform", unbox<D3.Primitive> (sprintf "translate(%f,%f)" xFromOrigin yFromOrigin))
                    |> ignore
                targetPoints.[int h] <- [| xFromOrigin ; yFromOrigin |]
                let x = Numeric.transform sourcePoints targetPoints
                let matrix = 
                    [| x.[0]; x.[3]; 0.; x.[6]
                       x.[1]; x.[4]; 0.; x.[7]
                       0.;    0.;    1.; 0.
                       x.[2]; x.[5]; 0.; 1. |] |> Array.map(fun i -> D3.Globals.round(i,6.))
                let mString = (sprintf "matrix3d(%A)" matrix).Replace("[", "").Replace("]", "")
                D3.Globals.select("#transform-layer").style("transform", unbox<D3.Primitive> mString) |> ignore

        // Grab behaviour for grab handles
        grid.Container.Element
            ?on("mousemove", fun _ -> moveHandle currentEvent.clientX currentEvent.clientY)
            ?on("mouseup", fun _ -> movingHandle <- None)
            |> ignore

        // Grab Handles
        grid.BaseLayer
            .append("g")
            .attr("transform", unbox<D3.Primitive> (sprintf "translate(%i,%i)" 0 layout.Margin.Top))
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
        grid

    let withTransform layout (imageBase64:string) (transform:float[] option) grid =
        grid
        |> withImageLayer layout imageBase64 transform Transformable
        |> withTransformHandles layout

    let withStaticOverlay layout (imageBase64:string) (transform:float[] option) grid =
        grid |> withImageLayer layout imageBase64 transform Static

    let withDrawTool grid =

        let mutable drawingLine : D3.Selection<obj> option  = None
        let mutable linePoints : (float*float) array = [||] // Saved in mm * mm space

        let lineGenerator = D3.Svg.Globals.line()
                                .interpolate_basis()

        let relativeMousePosition x y =
            let svg = Browser.document.getElementById("base-layer")
            let rect = svg.getBoundingClientRect()
            let dx = (float x - float rect.left) 
            let dy = (float y - rect.top)
            dx,dy

        let restartLine () =
            D3.Globals.select(".path-creating").remove() |> ignore
            linePoints <- [||]

        let startLine x y =
            let x0,y0 = relativeMousePosition x y
            restartLine()
            drawingLine <- D3.Globals.select("#elements")
                                .append("path")
                                .attr("id", unbox<D3.Primitive> "path-creating")
                                .attr("class", unbox<D3.Primitive> "movement-path path-creating") |> Some
            linePoints <- linePoints |> Array.append [|(x0,y0)|]

        let moveMouse (x:float) (y:float) =
            match drawingLine with
            | None -> ()
            | Some active ->
                let dx,dy = relativeMousePosition x y
                if (dx * dx + dy * dy > 100.)
                then linePoints <- Array.append [|(dx,dy)|] linePoints
                else linePoints <- Array.append [|(dx,dy)|] linePoints
                let path = lineGenerator.Invoke linePoints
                active.attr("data-points", unbox<D3.Primitive> linePoints) |> ignore
                active.attr("d", unbox<D3.Primitive> path) |> ignore

        grid.Container.Element
            ?on("mousemove", fun _ -> moveMouse currentEvent.clientX currentEvent.clientY)
            ?on("mousedown", fun _ -> startLine currentEvent.clientX currentEvent.clientY)
            ?on("mouseup", fun _ -> drawingLine <- None)
            |> ignore
        grid

    let withExistingPaths (queue:ViewState.Software.PathQueue) (grid:Grid) =
        let lineGenerator = D3.Svg.Globals.line().interpolate_basis()
        grid.BaseLayer
            .selectAll(".path-queued")
            ?data(queue.Queued |> List.toArray)
            ?enter()
            ?append("path")
            ?attr("class", unbox<D3.Primitive> "movement-path path-queued")
            ?attr("d", fst >> List.toArray >> lineGenerator.Invoke)
             |> ignore
        grid

    let toReact grid =
        grid.Container
        |> toReactElement    


module Vertical =

    open GraphElement

    type VerticalGraph = {
        Container: GraphElement
        Svg: D3.Selection<obj>
        Axis: D3.Scale.Linear<float,float>
    }

    let create layout zLimit =
        let parent = createElement "div"
        let svg = parent |> appendSvg layout "vertical"
        let z = D3.Scale.Globals.linear()
                    .range([|layout.Margin.Top |> float; float (layout.Height - layout.Margin.Bottom)|]).domain([|0. ; zLimit |])

        { Container = parent; Svg = svg; Axis = z }

    let withPlatformAngle layout (angle:float<degree>) (g:VerticalGraph) =
        let stage = g.Svg.append("g")
                        .attr("class",unbox<D3.Primitive> "rotating-stage")
                        .style("transform-origin", unbox<D3.Primitive> "bottom right")
        let woodWidth, woodHeight = 50,10
        stage.append("rect")
            .attr("class", unbox<D3.Primitive> "woodblock")
            .style("transform", unbox<D3.Primitive> (sprintf "translate(%ipx,%ipx)" (layout.Margin.Left + (woodWidth / 2)) (layout.Height - layout.Margin.Bottom - woodHeight)))
            .attr("height", unbox<D3.Primitive> woodHeight)
            .attr("width", unbox<D3.Primitive> <| (layout.Width - layout.Margin.Left - layout.Margin.Right) / 2) |> ignore
        stage.append("line")
            .attr("class", unbox<D3.Primitive> "stage")
            .attr("x1", unbox<D3.Primitive> layout.Margin.Left)
            .attr("x2", unbox<D3.Primitive> <| layout.Width - layout.Margin.Right)
            .attr("y1", unbox<D3.Primitive> (layout.Height - layout.Margin.Bottom))
            .attr("y2", unbox<D3.Primitive> (layout.Height - layout.Margin.Bottom)) |> ignore
        stage.style("transform", unbox<D3.Primitive> (sprintf "rotate(%fdeg)" (angle |> removeUnitFloat))) |> ignore
        g

    let withCurrentPosition (position:float) (g:VerticalGraph) =
          g.Svg.append("circle")
            .attr("r", unbox<D3.Primitive> 4)
            ?attr("cx", unbox<D3.Primitive> 10)
            ?attr("cy", unbox<D3.Primitive> (position |> float |> g.Axis.Invoke)) |> ignore

    let withAxis layout (g:VerticalGraph) =
        let zAxis = D3.Svg.Globals
                        .axis()
                        .scale(g.Axis)
                        .orient("right")
                        .tickSize(float (layout.Width - layout.Margin.Left - layout.Margin.Right))
        g.Svg.append("g")
          .attr("class", unbox<D3.Primitive> "z axis")
          .attr("transform", unbox<D3.Primitive> ("translate(" + (layout.Margin.Left.ToString()) + ",0)"))
          ?call(zAxis)
          ?append("text")
          ?attr("transform", "rotate(-90)")
          ?attr("y", 6)
          ?attr("dy", ".71em")
          ?style("text-anchor", "end")
          |> ignore
        g

    let withDrillHeight layout height (g:VerticalGraph) =
        let currentHeight = g.Svg.append("g")
        currentHeight.append("line")
            .attr("class", unbox<D3.Primitive> "drill-height")
            .attr("x1", unbox<D3.Primitive> layout.Margin.Left)
            .attr("x2", unbox<D3.Primitive> <| layout.Width - layout.Margin.Right)
            .attr("y1", unbox<D3.Primitive> (height |> removeMM |> g.Axis.Invoke))
            .attr("y2", unbox<D3.Primitive> (height |> removeMM |> g.Axis.Invoke)) |> ignore
        currentHeight.append("line")
            .attr("class", unbox<D3.Primitive> "drill-vertical-alignment")
            .attr("y1", unbox<D3.Primitive> layout.Margin.Top)
            .attr("y2", unbox<D3.Primitive> <| layout.Height - layout.Margin.Bottom)
            .attr("x1", unbox<D3.Primitive> <| layout.Margin.Left + (layout.Width - layout.Margin.Left - layout.Margin.Right) / 2 )
            .attr("x2", unbox<D3.Primitive> <| layout.Margin.Left + (layout.Width - layout.Margin.Left - layout.Margin.Right) / 2 ) |> ignore
        g

    let toReact vert =
        vert.Container |> toReactElement

    let complete layout zLimit angle height =
        create layout zLimit
        |> withPlatformAngle layout angle
        |> withAxis layout
        |> withDrillHeight layout height
        |> toReact