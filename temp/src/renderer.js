var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

import * as main_scss from "../../src/sass/main.scss";
import { setType } from "fable-core/Symbol";
import _Symbol from "fable-core/Symbol";
import { Tuple, compareRecords, equalsRecords, compareUnions, equalsUnions } from "fable-core/Util";
import { createElement } from "react";
import { fold } from "fable-core/Seq";
import { ProgramModule } from "fable-elmish/elmish";
import { withReact } from "fable-elmish-react/react";
export var sass = main_scss;
export var Message = function () {
  function Message(caseName, fields) {
    _classCallCheck(this, Message);

    this.Case = caseName;
    this.Fields = fields;
  }

  _createClass(Message, [{
    key: _Symbol.reflection,
    value: function () {
      return {
        type: "Renderer.Message",
        interfaces: ["FSharpUnion", "System.IEquatable", "System.IComparable"],
        cases: {
          Calibrate: []
        }
      };
    }
  }, {
    key: "Equals",
    value: function (other) {
      return equalsUnions(this, other);
    }
  }, {
    key: "CompareTo",
    value: function (other) {
      return compareUnions(this, other);
    }
  }]);

  return Message;
}();
setType("Renderer.Message", Message);
export var Model = function () {
  function Model(cartesianX, cartesianY, cartesianCalibration, rotation, tiltA, tiltB) {
    _classCallCheck(this, Model);

    this.CartesianX = cartesianX;
    this.CartesianY = cartesianY;
    this.CartesianCalibration = cartesianCalibration;
    this.Rotation = rotation;
    this.TiltA = tiltA;
    this.TiltB = tiltB;
  }

  _createClass(Model, [{
    key: _Symbol.reflection,
    value: function () {
      return {
        type: "Renderer.Model",
        interfaces: ["FSharpRecord", "System.IEquatable", "System.IComparable"],
        properties: {
          CartesianX: Motor,
          CartesianY: Motor,
          CartesianCalibration: Calibration,
          Rotation: Motor,
          TiltA: Motor,
          TiltB: Motor
        }
      };
    }
  }, {
    key: "Equals",
    value: function (other) {
      return equalsRecords(this, other);
    }
  }, {
    key: "CompareTo",
    value: function (other) {
      return compareRecords(this, other);
    }
  }]);

  return Model;
}();
setType("Renderer.Model", Model);
export var Motor = function () {
  function Motor(caseName, fields) {
    _classCallCheck(this, Motor);

    this.Case = caseName;
    this.Fields = fields;
  }

  _createClass(Motor, [{
    key: _Symbol.reflection,
    value: function () {
      return {
        type: "Renderer.Motor",
        interfaces: ["FSharpUnion", "System.IEquatable", "System.IComparable"],
        cases: {
          Disabled: [],
          Enabled: [MotorState]
        }
      };
    }
  }, {
    key: "Equals",
    value: function (other) {
      return equalsUnions(this, other);
    }
  }, {
    key: "CompareTo",
    value: function (other) {
      return compareUnions(this, other);
    }
  }]);

  return Motor;
}();
setType("Renderer.Motor", Motor);
export var MotorState = function () {
  function MotorState(maxStep) {
    _classCallCheck(this, MotorState);

    this.MaxStep = maxStep;
  }

  _createClass(MotorState, [{
    key: _Symbol.reflection,
    value: function () {
      return {
        type: "Renderer.MotorState",
        interfaces: ["FSharpRecord", "System.IEquatable", "System.IComparable"],
        properties: {
          MaxStep: "number"
        }
      };
    }
  }, {
    key: "Equals",
    value: function (other) {
      return equalsRecords(this, other);
    }
  }, {
    key: "CompareTo",
    value: function (other) {
      return compareRecords(this, other);
    }
  }]);

  return MotorState;
}();
setType("Renderer.MotorState", MotorState);
export var Calibration = function () {
  function Calibration(caseName, fields) {
    _classCallCheck(this, Calibration);

    this.Case = caseName;
    this.Fields = fields;
  }

  _createClass(Calibration, [{
    key: _Symbol.reflection,
    value: function () {
      return {
        type: "Renderer.Calibration",
        interfaces: ["FSharpUnion", "System.IEquatable", "System.IComparable"],
        cases: {
          Calibrated: [CalibrationState],
          Uncalibrated: []
        }
      };
    }
  }, {
    key: "Equals",
    value: function (other) {
      return equalsUnions(this, other);
    }
  }, {
    key: "CompareTo",
    value: function (other) {
      return compareUnions(this, other);
    }
  }]);

  return Calibration;
}();
setType("Renderer.Calibration", Calibration);
export var CalibrationState = function () {
  function CalibrationState(image, topRight, topLeft, bottomRight, bottomLeft) {
    _classCallCheck(this, CalibrationState);

    this.Image = image;
    this.TopRight = topRight;
    this.TopLeft = topLeft;
    this.BottomRight = bottomRight;
    this.BottomLeft = bottomLeft;
  }

  _createClass(CalibrationState, [{
    key: _Symbol.reflection,
    value: function () {
      return {
        type: "Renderer.CalibrationState",
        interfaces: ["FSharpRecord", "System.IEquatable", "System.IComparable"],
        properties: {
          Image: "string",
          TopRight: Tuple(["number", "number"]),
          TopLeft: Tuple(["number", "number"]),
          BottomRight: Tuple(["number", "number"]),
          BottomLeft: Tuple(["number", "number"])
        }
      };
    }
  }, {
    key: "Equals",
    value: function (other) {
      return equalsRecords(this, other);
    }
  }, {
    key: "CompareTo",
    value: function (other) {
      return compareRecords(this, other);
    }
  }]);

  return CalibrationState;
}();
setType("Renderer.CalibrationState", CalibrationState);
export function init() {
  return new Model(new Motor("Disabled", []), new Motor("Disabled", []), new Calibration("Uncalibrated", []), new Motor("Disabled", []), new Motor("Disabled", []), new Motor("Disabled", []));
}
export function update(msg, model) {
  return model;
}
export function view(count, dispatch) {
  var onClick = function onClick(msg) {
    return ["onClick", function (_arg1) {
      dispatch(msg);
    }];
  };

  return createElement("div", {}, createElement("button", fold(function (o, kv) {
    o[kv[0]] = kv[1];
    return o;
  }, {}, [onClick(new Message("Calibrate", []))]), "Calibrate"), createElement("label", {}, "Hello Woodmill!"));
}
ProgramModule.run(withReact("elmish-app", ProgramModule.withConsoleTrace(ProgramModule.mkSimple(function () {
  return init();
}, function (msg) {
  return function (model) {
    return update(msg, model);
  };
}, function (count) {
  return function (dispatch) {
    return view(count, dispatch);
  };
}))));
//# sourceMappingURL=renderer.js.map