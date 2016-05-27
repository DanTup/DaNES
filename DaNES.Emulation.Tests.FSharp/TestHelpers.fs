[<AutoOpen>]
module TestHelpers

open DanTup.DaNES.Emulation
open Xunit
open FsCheck

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

// Ensure tests that take bytes get sensible initial values.
type MyGenerators =
    static member Byte() =
        {
            new Arbitrary<byte>() with
            override x.Generator = Gen.choose(0, 1) |> Gen.map byte
            override x.Shrinker t = Seq.empty
        }


TODO: Make me work
Arb.register<MyGenerators>() |> ignore
