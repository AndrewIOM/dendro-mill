module Movement

open Types
open Types.Axis

module Axis =

    let angle (t:TiltAxis) (lengthToPivot:float<micrometre>) : float<degree> =
        2.214<degree>
        // TODO triangle calculation


//////////////////////////
/// Manhattan Movement
//////////////////////////

// In Manhattan movement, all movements are sequential
