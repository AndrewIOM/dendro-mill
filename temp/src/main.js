import electron from "electron";
import * as path from "path";
import * as url from "url";
export var mainWindow = null;
export function createMainWindow() {
    var options = {};
    options.width = 800;
    options.height = 600;
    var window = new electron.BrowserWindow(options);
    var opts = {};
    opts.pathname = path.join("/Users/andrewmartin/Desktop/MicromillLight/src", "../app/index.html");
    opts.protocol = "file:";
    window.loadURL(url.format(opts));
    window.on("closed", function () {
        mainWindow = null;
    });
    mainWindow = window;
}
electron.app.on("ready", function () {
    createMainWindow();
});
electron.app.on("window-all-closed", function () {
    if (process.platform !== "darwin") {
        electron.app.quit();
    }
});
electron.app.on("activate", function () {
    if (function () {
        return mainWindow == null;
    }(null)) {
        createMainWindow();
    }
});
//# sourceMappingURL=main.js.map