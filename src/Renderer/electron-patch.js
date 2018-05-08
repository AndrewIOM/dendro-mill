// Patch required for process.stdin, which is not available in Electron.
// Must be called before JohnnyFive library is used.

module.exports = {
  // parseJson : string -> obj option
  patchProcessStdin: function () {
    console.log('patching stdin');
    var Readable = require("stream").Readable;
    var util = require("util");
    util.inherits(MyStream, Readable);

    function MyStream(opt) {
      Readable.call(this, opt);
    }
    MyStream.prototype._read = function () {};
    // hook in our stream
    process.__defineGetter__("stdin", function () {
      if (process.__stdin) return process.__stdin;
      process.__stdin = new MyStream();
      return process.__stdin;
    });
    console.log('patched');
  },
};