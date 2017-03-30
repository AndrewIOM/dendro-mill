const webpack = require('webpack')
const path = require('path')

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
  devtool: "source-map",
  module: {
    rules: [{
      test: /\.js$/,
      include: path.resolve(__dirname, 'src'),
      use: [{
        loader: 'babel-loader',
        options: {
          presets: [
            ['es2015', { modules: false }]
          ]
        }
      }]
    },
    {
      test: /\.scss$/,
      use: [
        'style-loader',
        'css-loader',
        'sass-loader'
      ]
    }]
  }
};

module.exports = config;
