namespace Fable.Import

#r "../node_modules/fable-core/Fable.Core.dll"

open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import.JS

[<Import("*","johnny-five")>] 
module JohnnyFive = 

    type [<AllowNullLiteral>] AccelerometerOption =
        abstract controller: string with get, set

    and [<AllowNullLiteral>] AccelerometerGeneralOption =
        abstract controller: string option with get, set

    and [<AllowNullLiteral>] AccelerometerAnalogOption =
        inherit AccelerometerGeneralOption
        abstract pins: ResizeArray<string> with get, set
        abstract sensitivity: float option with get, set
        abstract aref: float option with get, set
        abstract zeroV: U2<float, ResizeArray<float>> option with get, set
        abstract autoCalibrate: bool option with get, set

    and [<AllowNullLiteral>] AccelerometerMPU6050Option =
        inherit AccelerometerGeneralOption
        abstract sensitivity: float option with get, set

    and [<AllowNullLiteral>] AccelerometerMMA7361Option =
        inherit AccelerometerGeneralOption
        abstract sleepPin: U2<float, string> option with get, set

    and [<AllowNullLiteral>] [<Import("*","Accelerometer")>] Accelerometer(option: U4<AccelerometerGeneralOption, AccelerometerAnalogOption, AccelerometerMPU6050Option, AccelerometerMMA7361Option>) =
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
        member __.hasAxis(name: string): unit = jsNative
        member __.enable(): unit = jsNative
        member __.disable(): unit = jsNative

    and [<AllowNullLiteral>] [<Import("*","Animation")>] Animation(option: U2<Servo, ResizeArray<Servo>>) =
        member __.target with get(): float = jsNative and set(v: float): unit = jsNative
        member __.duration with get(): float = jsNative and set(v: float): unit = jsNative
        member __.cuePoints with get(): ResizeArray<float> = jsNative and set(v: ResizeArray<float>): unit = jsNative
        member __.keyFrames with get(): float = jsNative and set(v: float): unit = jsNative
        member __.easing with get(): string = jsNative and set(v: string): unit = jsNative
        member __.loop with get(): bool = jsNative and set(v: bool): unit = jsNative
        member __.loopback with get(): float = jsNative and set(v: float): unit = jsNative
        member __.metronomic with get(): bool = jsNative and set(v: bool): unit = jsNative
        member __.progress with get(): float = jsNative and set(v: float): unit = jsNative
        member __.currentSpeed with get(): float = jsNative and set(v: float): unit = jsNative
        member __.fps with get(): float = jsNative and set(v: float): unit = jsNative
        member __.enqueue(segment: obj): unit = jsNative
        member __.play(): unit = jsNative
        member __.pause(): unit = jsNative
        member __.stop(): unit = jsNative
        member __.next(): unit = jsNative
        member __.speed(speed: ResizeArray<float>): unit = jsNative

    and [<AllowNullLiteral>] ButtonOptions =
        abstract pin: U2<float, string> with get, set
        abstract invert: bool option with get, set
        abstract isPullup: bool option with get, set
        abstract holdtime: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","Button")>] Button(pin: U3<float, string, ButtonOptions>) =
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('hold',$1...)")>] member __.on_hold(cb: Func<float, unit>): obj = jsNative
        [<Emit("$0.on('down',$1...)")>] member __.on_down(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('press',$1...)")>] member __.on_press(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('up',$1...)")>] member __.on_up(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('release',$1...)")>] member __.on_release(cb: Func<unit, unit>): obj = jsNative

    and [<AllowNullLiteral>] BoardOptions =
        abstract id: U2<float, string> option with get, set
        abstract port: U2<string, obj> option with get, set
        abstract repl: bool option with get, set

    and [<AllowNullLiteral>] Repl =
        abstract inject: ``object``: obj -> unit

    and [<AllowNullLiteral>] [<Import("Board","johnny-five")>] Board(?option: BoardOptions) =
        member __.isReady with get(): bool = jsNative and set(v: bool): unit = jsNative
        member __.io with get(): obj = jsNative and set(v: obj): unit = jsNative
        member __.id with get(): string = jsNative and set(v: string): unit = jsNative
        member __.pins with get(): ResizeArray<Pin> = jsNative and set(v: ResizeArray<Pin>): unit = jsNative
        member __.port with get(): string = jsNative and set(v: string): unit = jsNative
        member __.inject with get(): Repl = jsNative and set(v: Repl): unit = jsNative
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('ready',$1...)")>] member __.on_ready(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('connect',$1...)")>] member __.on_connect(cb: Func<unit, unit>): obj = jsNative
        member __.pinMode(pin: float, mode: float): unit = jsNative
        member __.analogWrite(pin: float, value: float): unit = jsNative
        member __.analogRead(pin: float, cb: Func<float, unit>): unit = jsNative
        member __.digitalWrite(pin: float, value: float): unit = jsNative
        member __.digitalRead(pin: float, cb: Func<float, unit>): unit = jsNative
        member __.shiftOut(dataPin: Pin, clockPin: Pin, isBigEndian: bool, value: float): unit = jsNative
        member __.wait(ms: float, cb: Func<unit, unit>): unit = jsNative
        member __.loop(ms: float, cb: Func<unit, unit>): unit = jsNative

    and [<AllowNullLiteral>] CompassOptions =
        abstract controller: string with get, set
        abstract gauss: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","Compass")>] Compass(option: CompassOptions) =
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative

    and [<AllowNullLiteral>] ESCOption =
        abstract pin: U2<float, string> with get, set
        abstract range: ResizeArray<float> option with get, set
        abstract startAt: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","ESC")>] ESC(option: U3<float, string, ESCOption>) =
        member __.speed(value: float): unit = jsNative
        member __.min(): unit = jsNative
        member __.max(): unit = jsNative
        member __.stop(): unit = jsNative

    and [<AllowNullLiteral>] GyroGeneralOption =
        abstract controller: string option with get, set

    and [<AllowNullLiteral>] GyroAnalogOption =
        inherit GyroGeneralOption
        abstract pins: ResizeArray<string> with get, set
        abstract sensitivity: float with get, set
        abstract resolution: float option with get, set

    and [<AllowNullLiteral>] GyroMPU6050Option =
        inherit GyroGeneralOption
        abstract sensitivity: float with get, set

    and [<AllowNullLiteral>] [<Import("*","Gyro")>] Gyro(option: U3<GyroGeneralOption, GyroAnalogOption, GyroMPU6050Option>) =
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
        member __.recalibrate(): unit = jsNative

    and [<AllowNullLiteral>] IMUGeneralOption =
        abstract controller: string option with get, set
        abstract freq: float option with get, set

    and [<AllowNullLiteral>] IMUMPU6050Option =
        inherit IMUGeneralOption
        abstract address: float with get, set

    and [<AllowNullLiteral>] [<Import("*","IMU")>] IMU(option: U2<IMUGeneralOption, IMUMPU6050Option>) =
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative

    and [<AllowNullLiteral>] JoystickOption =
        abstract pins: ResizeArray<string> with get, set

    and [<AllowNullLiteral>] [<Import("*","Joystick")>] Joystick(option: JoystickOption) =
        member __.axis with get(): ResizeArray<float> = jsNative and set(v: ResizeArray<float>): unit = jsNative
        member __.raw with get(): ResizeArray<float> = jsNative and set(v: ResizeArray<float>): unit = jsNative
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('axismove',$1...)")>] member __.on_axismove(cb: Func<Error, DateTime, unit>): obj = jsNative

    and [<AllowNullLiteral>] LCDGeneralOption =
        abstract rows: float option with get, set
        abstract cols: float option with get, set

    and [<AllowNullLiteral>] LCDI2COption =
        inherit LCDGeneralOption
        abstract controller: string with get, set
        abstract backlight: float option with get, set

    and [<AllowNullLiteral>] LCDParallelOption =
        inherit LCDGeneralOption
        abstract pins: ResizeArray<obj> with get, set
        abstract backlight: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","LCD")>] LCD(option: U3<LCDGeneralOption, LCDI2COption, LCDParallelOption>) =
        member __.print(message: string): unit = jsNative
        member __.useChar(char: string): unit = jsNative
        member __.clear(): unit = jsNative
        member __.cursor(row: float, col: float): unit = jsNative
        member __.home(): unit = jsNative
        member __.display(): unit = jsNative
        member __.noDisplay(): unit = jsNative
        member __.blink(): unit = jsNative
        member __.noBlink(): unit = jsNative
        member __.autoscroll(): unit = jsNative
        member __.noAutoscroll(): unit = jsNative

    and [<AllowNullLiteral>] LedOption =
        abstract pin: float with get, set
        abstract ``type``: string option with get, set
        abstract controller: string option with get, set
        abstract address: float option with get, set
        abstract isAnode: bool option with get, set

    and [<AllowNullLiteral>] [<Import("*","Led")>] Led(option: U2<float, LedOption>) =
        member __.on(): unit = jsNative
        member __.off(): unit = jsNative
        member __.toggle(): unit = jsNative
        member __.strobe(ms: float): unit = jsNative
        member __.blink(): unit = jsNative
        member __.blink(ms: float): unit = jsNative
        member __.brightness(``val``: float): unit = jsNative
        member __.fade(brightness: float, ms: float): unit = jsNative
        member __.fadeIn(ms: float): unit = jsNative
        member __.fadeOut(ms: float): unit = jsNative
        member __.pulse(ms: float): unit = jsNative
        member __.stop(ms: float): unit = jsNative

    and [<AllowNullLiteral>] MotorOption =
        abstract pins: obj with get, set
        abstract current: obj option with get, set
        abstract invertPWM: bool option with get, set
        abstract address: float option with get, set
        abstract controller: string option with get, set
        abstract register: obj option with get, set
        abstract bits: obj option with get, set

    and [<AllowNullLiteral>] [<Import("*","Motor")>] Motor(option: U2<ResizeArray<float>, MotorOption>) =
        member __.forward(speed: float): unit = jsNative
        member __.fwd(speed: float): unit = jsNative
        member __.reverse(speed: float): unit = jsNative
        member __.rev(speed: float): unit = jsNative
        member __.start(): unit = jsNative
        member __.start(speed: float): unit = jsNative
        member __.stop(): unit = jsNative
        member __.brake(): unit = jsNative
        member __.release(): unit = jsNative

    and [<AllowNullLiteral>] PiezoOption =
        abstract pin: float with get, set

    and [<AllowNullLiteral>] [<Import("*","Piezo")>] Piezo(option: U2<float, PiezoOption>) =
        member __.frequency(frequency: float, duration: float): unit = jsNative
        member __.play(tune: obj, ?cb: Func<unit, unit>): unit = jsNative
        member __.tone(frequency: float, duration: float): unit = jsNative
        member __.noTone(): unit = jsNative
        member __.off(): unit = jsNative

    and [<AllowNullLiteral>] PinOption =
        abstract id: U2<float, string> option with get, set
        abstract pin: U2<float, string> with get, set
        abstract ``type``: string option with get, set

    and [<AllowNullLiteral>] PinState =
        abstract supportedModes: ResizeArray<float> with get, set
        abstract mode: float with get, set
        abstract value: float with get, set
        abstract report: float with get, set
        abstract analogChannel: float with get, set

    and [<AllowNullLiteral>] [<Import("*","Pin")>] Pin(option: U3<float, string, PinOption>) =
        member __.query(cb: Func<PinState, unit>): unit = jsNative
        member __.high(): unit = jsNative
        member __.low(): unit = jsNative
        member __.write(value: float): unit = jsNative
        member __.read(cb: Func<float, unit>): unit = jsNative
        static member write(pin: float, value: float): unit = jsNative
        static member read(pin: float, cb: Func<float, unit>): unit = jsNative

    and [<AllowNullLiteral>] PingOption =
        abstract pin: U2<float, string> with get, set
        abstract freq: float option with get, set
        abstract pulse: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","Ping")>] Ping(option: U2<float, PingOption>) =
        class end

    and [<AllowNullLiteral>] RelayOption =
        abstract pin: U2<float, string> with get, set
        abstract ``type``: string option with get, set

    and [<AllowNullLiteral>] [<Import("*","Relay")>] Relay(option: U2<float, RelayOption>) =
        member __.``open``(): unit = jsNative
        member __.close(): unit = jsNative
        member __.toggle(): unit = jsNative

    and [<AllowNullLiteral>] SensorOption =
        abstract pin: U2<float, string> with get, set
        abstract freq: bool option with get, set
        abstract threshold: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","Sensor")>] Sensor(option: U3<float, string, SensorOption>) =
        member __.scale(low: float, high: float): Sensor = jsNative
        member __.scale(range: ResizeArray<float>): Sensor = jsNative
        member __.scale(): Sensor = jsNative
        member __.booleanAt(barrier: float): bool = jsNative
        member __.within(range: ResizeArray<float>, cb: Func<unit, unit>): unit = jsNative
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative

    and [<AllowNullLiteral>] ServoGeneralOption =
        abstract pin: U2<float, string> with get, set
        abstract range: ResizeArray<float> option with get, set
        abstract ``type``: string option with get, set
        abstract startAt: float option with get, set
        abstract isInverted: bool option with get, set
        abstract center: bool option with get, set
        abstract controller: string option with get, set

    and [<AllowNullLiteral>] ServoPCA9685Option =
        inherit ServoGeneralOption
        abstract address: float option with get, set

    and [<AllowNullLiteral>] ServoSweepOpts =
        abstract range: ResizeArray<float> with get, set
        abstract interval: float option with get, set
        abstract step: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","Servo")>] Servo(option: U3<float, string, ServoGeneralOption>) =
        member __.``to``(degrees: float, ?ms: float, ?rage: float): unit = jsNative
        member __.min(): unit = jsNative
        member __.max(): unit = jsNative
        member __.center(): unit = jsNative
        member __.sweep(): unit = jsNative
        member __.sweep(range: ResizeArray<float>): unit = jsNative
        member __.sweep(opt: ServoSweepOpts): unit = jsNative
        member __.stop(): unit = jsNative
        member __.cw(speed: float): unit = jsNative
        member __.ccw(speed: float): unit = jsNative
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('move:complete',$1...)")>] member __.``on_move:complete``(cb: Func<unit, unit>): obj = jsNative

    and [<AllowNullLiteral>] ShiftRegisterOption =
        abstract pins: obj with get, set

    and [<AllowNullLiteral>] [<Import("*","ShiftRegister")>] ShiftRegister(option: ShiftRegisterOption) =
        member __.send([<ParamArray>] value: float[]): unit = jsNative

    and [<AllowNullLiteral>] SonarOption =
        abstract pin: U2<float, string> with get, set
        abstract device: string with get, set
        abstract freq: float option with get, set
        abstract threshold: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","Sonar")>] Sonar(option: U3<float, string, SonarOption>) =
        member __.within(range: ResizeArray<float>, cb: Func<unit, unit>): unit = jsNative
        member __.within(range: ResizeArray<float>, unit: string, cb: Func<unit, unit>): unit = jsNative
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative

    and [<AllowNullLiteral>] StepperOption =
        abstract pins: obj with get, set
        abstract stepsPerRev: float with get, set
        abstract ``type``: float with get, set
        abstract rpm: float option with get, set
        abstract direction: float option with get, set

    and [<AllowNullLiteral>] [<Import("Stepper","johnny-five")>] Stepper(option: U3<float, string, StepperOption>) =
        member __.step(stepsOrOpts: obj, cb: Func<unit, unit>): unit = jsNative
        member __.rpm(): Stepper = jsNative
        member __.rpm(value: float): Stepper = jsNative
        member __.speed(): Stepper = jsNative
        member __.speed(value: float): Stepper = jsNative
        member __.direction(): Stepper = jsNative
        member __.direction(value: float): Stepper = jsNative
        member __.accel(): Stepper = jsNative
        member __.accel(value: float): Stepper = jsNative
        member __.decel(): Stepper = jsNative
        member __.decel(value: float): Stepper = jsNative
        member __.cw(): Stepper = jsNative
        member __.ccw(): Stepper = jsNative
        member __.within(range: ResizeArray<float>, cb: Func<unit, unit>): unit = jsNative
        member __.within(range: ResizeArray<float>, unit: string, cb: Func<unit, unit>): unit = jsNative
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative

    and [<AllowNullLiteral>] TemperatureOption =
        abstract controller: string option with get, set
        abstract pin: U2<string, float> with get, set
        abstract toCelsius: Func<float, float> option with get, set
        abstract freq: float option with get, set

    and [<AllowNullLiteral>] [<Import("*","Temperature")>] Temperature(option: TemperatureOption) =
        member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
        [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
        [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative

    module IR =
        type [<AllowNullLiteral>] MotionOption =
            abstract pin: U2<float, string> with get, set

        and [<AllowNullLiteral>] [<Import("Motion","IR")>] Motion(option: U2<float, MotionOption>) =
            member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
            [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
            [<Emit("$0.on('motionstart',$1...)")>] member __.on_motionstart(cb: Func<unit, unit>): obj = jsNative
            [<Emit("$0.on('motionend',$1...)")>] member __.on_motionend(cb: Func<unit, unit>): obj = jsNative
            [<Emit("$0.on('calibrated',$1...)")>] member __.on_calibrated(cb: Func<unit, unit>): obj = jsNative

        and [<AllowNullLiteral>] PloximityOption =
            abstract pin: U2<float, string> with get, set
            abstract controller: string with get, set

        and [<AllowNullLiteral>] [<Import("Proximity","IR")>] Proximity(option: U2<float, PloximityOption>) =
            member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
            [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
            [<Emit("$0.on('change',$1...)")>] member __.on_change(cb: Func<unit, unit>): obj = jsNative

        and [<AllowNullLiteral>] ArrayOption =
            abstract pins: U2<ResizeArray<float>, ResizeArray<string>> with get, set
            abstract emitter: U2<float, string> with get, set
            abstract freq: float option with get, set

        and [<AllowNullLiteral>] LoadCalibrationOption =
            abstract min: ResizeArray<float> with get, set
            abstract max: ResizeArray<float> with get, set

        module Reflect =
            type [<AllowNullLiteral>] [<Import("Reflect.Array","IR")>] Array(option: ArrayOption) =
                member __.enable(): unit = jsNative
                member __.disable(): unit = jsNative
                member __.calibrate(): unit = jsNative
                member __.calibrateUntil(predicate: Func<unit, unit>): unit = jsNative
                member __.loadCalibration(option: LoadCalibrationOption): unit = jsNative
                member __.on(``event``: string, cb: Func<unit, unit>): obj = jsNative
                [<Emit("$0.on('data',$1...)")>] member __.on_data(cb: Func<obj, unit>): obj = jsNative
                [<Emit("$0.on('calibratedData',$1...)")>] member __.on_calibratedData(cb: Func<obj, unit>): obj = jsNative
                [<Emit("$0.on('line',$1...)")>] member __.on_line(cb: Func<obj, unit>): obj = jsNative



    module Led =
        type [<AllowNullLiteral>] DigitsOption =
            abstract pins: obj with get, set
            abstract devices: float option with get, set

        and [<AllowNullLiteral>] [<Import("Digits","Led")>] Digits(option: DigitsOption) =
            member __.on(): unit = jsNative
            member __.on(index: float): unit = jsNative
            member __.off(): unit = jsNative
            member __.off(index: float): unit = jsNative
            member __.clear(): unit = jsNative
            member __.clear(index: float): unit = jsNative
            member __.brightness(value: float): unit = jsNative
            member __.brightness(index: float, value: float): unit = jsNative
            member __.draw(position: float, character: float): unit = jsNative
            member __.draw(index: float, position: float, character: float): unit = jsNative

        and [<AllowNullLiteral>] MatrixOption =
            abstract pins: obj with get, set
            abstract devices: float option with get, set

        and [<AllowNullLiteral>] MatrixIC2Option =
            abstract controller: string with get, set
            abstract addresses: ResizeArray<obj> option with get, set
            abstract isBicolor: bool option with get, set
            abstract dims: obj option with get, set
            abstract rotation: float option with get, set

        and [<AllowNullLiteral>] [<Import("Matrix","Led")>] Matrix(option: U2<MatrixOption, MatrixIC2Option>) =
            member __.on(): unit = jsNative
            member __.on(index: float): unit = jsNative
            member __.off(): unit = jsNative
            member __.off(index: float): unit = jsNative
            member __.clear(): unit = jsNative
            member __.clear(index: float): unit = jsNative
            member __.brightness(value: float): unit = jsNative
            member __.brightness(index: float, value: float): unit = jsNative
            member __.led(row: float, col: float, state: obj): unit = jsNative
            member __.led(index: float, row: float, col: float, state: obj): unit = jsNative
            member __.row(row: float, ``val``: float): unit = jsNative
            member __.row(index: float, row: float, ``val``: float): unit = jsNative
            member __.column(row: float, ``val``: float): unit = jsNative
            member __.column(index: float, row: float, ``val``: float): unit = jsNative
            member __.draw(position: float, character: float): unit = jsNative
            member __.draw(index: float, position: float, character: float): unit = jsNative

        and [<AllowNullLiteral>] RGBOption =
            abstract pins: ResizeArray<float> with get, set
            abstract isAnode: bool option with get, set
            abstract controller: string option with get, set

        and [<AllowNullLiteral>] [<Import("RGB","Led")>] RGB(option: RGBOption) =
            member __.on(): unit = jsNative
            member __.off(): unit = jsNative
            member __.color(value: float): unit = jsNative
            member __.toggle(): unit = jsNative
            member __.strobe(ms: float): unit = jsNative
            member __.brightness(value: float): unit = jsNative
            member __.fadeIn(ms: float): unit = jsNative
            member __.fadeOut(ms: float): unit = jsNative
            member __.pulse(ms: float): unit = jsNative
            member __.stop(ms: float): unit = jsNative



    module Stepper =
        type [<AllowNullLiteral>] [<Import("TYPE","Stepper")>] TYPE() =
            member __.DRIVER with get(): float = jsNative and set(v: float): unit = jsNative
            member __.TWO_WIRE with get(): float = jsNative and set(v: float): unit = jsNative
            member __.FOUR_WIRE with get(): float = jsNative and set(v: float): unit = jsNative


