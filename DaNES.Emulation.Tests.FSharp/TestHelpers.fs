[<AutoOpen>]
module TestHelpers

open DanTup.DaNES.Emulation
open Xunit

type Opcode = Cpu6502.OpCode

// Test asserts
let shouldBe (expected : 'a) (actual : 'a) =
    Assert.Equal<'a>(expected, actual)

// Helper to run a program and return the CPU state.
let run bytes =
    let nes = Nes()
    nes.LoadProgram(bytes |> List.toArray)
    nes.Run()
    nes.Cpu
