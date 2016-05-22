module LdaTests

open System
open Xunit

[<Theory>]
[<InlineData(0, true, false)>]
[<InlineData(1, false, false)>]
[<InlineData(127, false, false)>]
[<InlineData(128, false, true)>]
[<InlineData(129, false, true)>]
[<InlineData(255, false, true)>]
let lda_immediate value_to_load expect_zero expect_negative =
    let state = run [ byte Opcode.LDA_IMD; value_to_load ]
    state.Accumulator |> shouldBe value_to_load
    state.ZeroResult |> shouldBe expect_zero
    state.Negative |> shouldBe expect_negative
