module LdaTests

open System
open Xunit
open DanTup.DaNES.Emulation

// Helper to run a program and return the CPU state.
let run bytes =
    let nes = Nes()
    nes.LoadProgram(bytes |> List.toArray)
    nes.Run()
    nes.Cpu

// Test helpers
let shouldBe (expected : 'a) (actual : 'a) =
    Assert.Equal<'a>(expected, actual)

// OpCodes
[<Literal>]
let op_lda_immediate = 0xA9uy

[<Theory>]
[<InlineData(0, true, false)>]
[<InlineData(1, false, false)>]
[<InlineData(127, false, false)>]
[<InlineData(128, false, true)>]
[<InlineData(129, false, true)>]
[<InlineData(255, false, true)>]
let lda_immediate value_to_load expect_zero expect_negative =
    let state = run [ op_lda_immediate; value_to_load ]
    state.Accumulator |> shouldBe value_to_load
    state.ZeroResult |> shouldBe expect_zero
    state.Negative |> shouldBe expect_negative
