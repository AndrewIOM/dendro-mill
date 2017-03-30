var webpack = require('webpack');

var config = {
  context: __dirname + '/temp/src',
  entry: {
    renderer: './renderer.js',
    main: './main.js'
  },
  output: {
    path: __dirname + '/app/js',
    filename: '[name].bundle.js',
    libraryTarget: "commonjs2"
  },
  externals: {
    electron: true
  },
  target: "node",
  devtool: "source-map"
};

module.exports = config;

// module.exports = {
//   entry: {
//     main: "temp/src/main",
//     renderer: "temp/src/renderer"
//   },
//   output: {
//     filename: "[name].js",
//     path: path.resolve(process.cwd(), 'app/js'),
//     libraryTarget: "commonjs2"
//   },
//   externals: {
//     electron: true
//   },
//   target: "node",
//   node: {
//     __dirname: false,
//     __filename: false
//   },
//   devtool: "source-map",
//   module: {
//     rules: [{
//       enforce: "pre",
//       loader: "source-map-loader",
//       exclude: /node_modules/,
//       test: /\.js$/
//     }]
//   }
// };