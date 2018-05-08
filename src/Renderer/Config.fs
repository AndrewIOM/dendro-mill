module Config

open Types

let stageDimension = 215.<mm>

let controlPoints = [
  stageDimension * 0.5, stageDimension * 0.5
  stageDimension * 0.8, stageDimension * 0.8
  stageDimension * 0.6, stageDimension * 0.6
  stageDimension * 0.6, stageDimension * 0.4
  stageDimension * 0.8, stageDimension * 0.2
  stageDimension * 0.2, stageDimension * 0.8
  stageDimension * 0.4, stageDimension * 0.6
  stageDimension * 0.4, stageDimension * 0.4
  stageDimension * 0.2, stageDimension * 0.2
]