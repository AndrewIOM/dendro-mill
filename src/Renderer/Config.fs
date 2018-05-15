module Config

open Types

let stageDimension = 215.<mm>

let controlPoints = [
  stageDimension * 0.75, stageDimension * 0.75
  stageDimension * 0.25, stageDimension * 0.75
  stageDimension * 0.25, stageDimension * 0.25
  stageDimension * 0.75, stageDimension * 0.25
]