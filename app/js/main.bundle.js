module.exports =
/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId])
/******/ 			return installedModules[moduleId].exports;
/******/
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// identity function for calling harmony imports with the correct context
/******/ 	__webpack_require__.i = function(value) { return value; };
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, {
/******/ 				configurable: false,
/******/ 				enumerable: true,
/******/ 				get: getter
/******/ 			});
/******/ 		}
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 285);
/******/ })
/************************************************************************/
/******/ ({

/***/ 133:
/***/ (function(module, exports) {

module.exports = require("electron");

/***/ }),

/***/ 134:
/***/ (function(module, exports) {

module.exports = require("path");

/***/ }),

/***/ 135:
/***/ (function(module, exports) {

module.exports = require("url");

/***/ }),

/***/ 285:
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
Object.defineProperty(__webpack_exports__, "__esModule", { value: true });
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_electron__ = __webpack_require__(133);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_0_electron___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_0_electron__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_path__ = __webpack_require__(134);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_1_path___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_1_path__);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_url__ = __webpack_require__(135);
/* harmony import */ var __WEBPACK_IMPORTED_MODULE_2_url___default = __webpack_require__.n(__WEBPACK_IMPORTED_MODULE_2_url__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "mainWindow", function() { return mainWindow; });
/* harmony export (immutable) */ __webpack_exports__["createMainWindow"] = createMainWindow;



var mainWindow = null;
function createMainWindow() {
    var options = {};
    options.width = 800;
    options.height = 600;
    var window = new __WEBPACK_IMPORTED_MODULE_0_electron___default.a.BrowserWindow(options);
    var opts = {};
    opts.pathname = __WEBPACK_IMPORTED_MODULE_1_path__["join"]("/Users/andrewmartin/Desktop/MicromillLight/src", "../app/index.html");
    opts.protocol = "file:";
    window.loadURL(__WEBPACK_IMPORTED_MODULE_2_url__["format"](opts));
    window.on("closed", function () {
        mainWindow = null;
    });
    mainWindow = window;
}
__WEBPACK_IMPORTED_MODULE_0_electron___default.a.app.on("ready", function () {
    createMainWindow();
});
__WEBPACK_IMPORTED_MODULE_0_electron___default.a.app.on("window-all-closed", function () {
    if (process.platform !== "darwin") {
        __WEBPACK_IMPORTED_MODULE_0_electron___default.a.app.quit();
    }
});
__WEBPACK_IMPORTED_MODULE_0_electron___default.a.app.on("activate", function () {
    if (function () {
        return mainWindow == null;
    }(null)) {
        createMainWindow();
    }
});
//# sourceMappingURL=main.js.map

/***/ })

/******/ });
//# sourceMappingURL=main.bundle.js.map