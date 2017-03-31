namespace Fable.Import

#r "../node_modules/fable-core/Fable.Core.dll"

open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import.JS

module SerialPort =
    type [<AllowNullLiteral>] portConfig =
        abstract comName: string with get, set
        abstract manufacturer: string with get, set
        abstract serialNumber: string with get, set
        abstract pnpId: string with get, set
        abstract locationId: string with get, set
        abstract vendorId: string with get, set
        abstract productId: string with get, set

    and [<AllowNullLiteral>] setOptions =
        abstract brk: bool option with get, set
        abstract cts: bool option with get, set
        abstract dsr: bool option with get, set
        abstract dtr: bool option with get, set
        abstract rts: bool option with get, set

    and [<AllowNullLiteral>] updateOptions =
        abstract baudRate: float option with get, set

module serialport =
    type [<AllowNullLiteral>] [<Import("SerialPort","serialport")>] SerialPort(path: string, ?options: obj, ?callback: Func<obj, unit>) =
        member __.parsers with get(): obj = jsNative and set(v: obj): unit = jsNative
        member __.isOpen(): bool = jsNative
        member __.on(``event``: string, ?callback: Func<obj, unit>): unit = jsNative
        member __.``open``(?callback: Func<obj, unit>): unit = jsNative
        member __.write(buffer: obj, ?callback: Func<obj, float, unit>): unit = jsNative
        member __.pause(): unit = jsNative
        member __.resume(): unit = jsNative
        member __.disconnected(err: Error): unit = jsNative
        member __.close(?callback: Func<obj, unit>): unit = jsNative
        member __.flush(?callback: Func<obj, unit>): unit = jsNative
        member __.set(options: SerialPort.setOptions, callback: Func<obj, unit>): unit = jsNative
        member __.drain(?callback: Func<obj, unit>): unit = jsNative
        member __.update(options: SerialPort.updateOptions, ?callback: Func<obj, unit>): unit = jsNative
        static member list(callback: Func<obj, ResizeArray<SerialPort.portConfig>, unit>): unit = jsNative
