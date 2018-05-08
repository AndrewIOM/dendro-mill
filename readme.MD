# Oxford Dendro-Micromill Software

Cross platform software to control a micromill for dendroecological analyses.

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 2.0 or higher
* [node.js](https://nodejs.org) 6.11 or higher
* [yarn](https://yarnpkg.com)

On macOS and Linux you'll need [Mono](http://www.mono-project.com/) for other F# tooling like Paket or editor support.

## Building and running the app

* Install JS dependencies: `yarn install`
* Install F# dependencies: `dotnet restore`
* Rebuild dependencies: `yarn run electron-rebuild`
* Start Fable daemon and [Webpack](https://webpack.js.org/): `yarn start`
* In another terminal, run: `yarn run launch`

> The first two steps are only necessary the first time or whenever the dependencies change.

The app window will be refreshed when you modify any file in the Renderer project. For production, run `yarn run build` to get optimized JS code.
