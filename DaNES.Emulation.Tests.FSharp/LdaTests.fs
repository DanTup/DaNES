module LdaTests

open System
open FsCheck.Xunit

[<Property>]
let lda_immediate_a x =
    let state = run [ byte Opcode.LDA_IMD; x ]
    state.Accumulator = x

[<Property>]
let lda_immediate_z x =
    let state = run [ byte Opcode.LDA_IMD; x ]
    state.ZeroResult = (x = 0uy)

[<Property>]
let lda_immediate_n x =
    let state = run [ byte Opcode.LDA_IMD; x ]
    state.Negative = (x > 127uy)
