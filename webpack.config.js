var path = require("path");
var webpack = require("webpack");
var fableUtils = require("fable-utils");
 
function resolve(relativePath) {
    return path.join(__dirname, relativePath);
}
 
var babelOptions = fableUtils.resolveBabelOptions({
  presets: [["es2015", { "modules": false }]],
  plugins: ["transform-runtime"]
});

var isProduction = process.argv.indexOf("-p") >= 0;
console.log("Bundling for " + (isProduction ? "production" : "development") + "...");

var basicConfig = {
  mode: isProduction ? "production" : "development",
  devtool: "source-map",
  resolve: {
    modules: [resolve("./node_modules/")]
  },
  node: {
    __dirname: false,
    __filename: false
  },
  target: "electron-main",  
  externals: {
    bindings: true,
    serialport: true
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: {
          loader: "fable-loader",
          options: {
            babel: babelOptions,
            define: isProduction ? [] : ["DEBUG"]
          }
        }
      },
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: babelOptions
        },
      },
      {
        test: /\.scss$/,
        use: [
          'style-loader',
          'css-loader',
          'sass-loader'
        ]
      },
      {
        test: /\.css$/,
        use: [ 'style-loader', 'css-loader' ]
      }
    ]
  }
};

var mainConfig = Object.assign({
  target: "electron-main",
  entry: resolve("src/Main/Main.fsproj"),
  output: {
    path: resolve("app"),
    filename: "main.js",
    libraryTarget: "commonjs2"
  }
}, basicConfig);

var rendererConfig = Object.assign({
  target: "electron-renderer",
  entry: resolve("src/Renderer/Renderer.fsproj"),
  output: {
    path: resolve("app"),
    filename: "renderer.js",
    libraryTarget: "commonjs2"
  }
}, basicConfig);

module.exports = [mainConfig, rendererConfig]