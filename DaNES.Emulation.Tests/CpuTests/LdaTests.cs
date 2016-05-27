using Xunit;

namespace DanTup.DaNES.Emulation.Tests.CpuTests
{
	public class LdaTests
	{
		class TestNes : Nes
		{
			public TestNes() : base()
			{
				Cpu.ProgramCounter = 0x8000;
			}
		}

		Nes nes = new TestNes();

		[Theory]
		[InlineData(0, true, false)]
		[InlineData(1, false, false)]
		[InlineData(127, false, false)]
		[InlineData(128, false, true)]
		[InlineData(129, false, true)]
		[InlineData(255, false, true)]
		public void Lda_Immediate(byte value_to_load, bool expectZero, bool expectNegative)
		{
			nes.LoadProgram(0xA9, value_to_load);

			nes.Run();

			Assert.Equal(value_to_load, nes.Cpu.Accumulator);
			Assert.Equal(expectZero, nes.Cpu.ZeroResult);
			Assert.Equal(expectNegative, nes.Cpu.Negative);
		}

		[Theory]
		[InlineData(0, 0, true, false)]
		[InlineData(1, 1, false, false)]
		[InlineData(127, 2, false, false)]
		[InlineData(128, 3, false, true)]
		[InlineData(129, 4, false, true)]
		[InlineData(255, 255, false, true)]
		public void Lda_Zero_Page(byte value_to_load, byte ram_location, bool expectZero, bool expectNegative)
		{
			nes.Cpu.Ram.Write(ram_location, value_to_load);
			nes.LoadProgram(0xA5, ram_location);

			nes.Run();

			Assert.Equal(value_to_load, nes.Cpu.Accumulator);
			Assert.Equal(expectZero, nes.Cpu.ZeroResult);
			Assert.Equal(expectNegative, nes.Cpu.Negative);
		}

		[Theory]
		[InlineData(0, 0, 10, true, false)]
		[InlineData(1, 1, 11, false, false)]
		[InlineData(127, 2, 12, false, false)]
		[InlineData(128, 3, 13, false, true)]
		[InlineData(129, 4, 14, false, true)]
		[InlineData(255, 255, 255, false, true)]
		public void Lda_Zero_Page_Offset_X(byte value_to_load, byte ram_location, byte offset, bool expectZero, bool expectNegative)
		{
			var actual_ram_location = (ushort)((ram_location + offset) % 256); // Zero page operations wrap around and remain on zero page.
			nes.Cpu.Ram.Write(actual_ram_location, value_to_load);
			nes.Cpu.XRegister = offset;
			nes.LoadProgram(0xB5, ram_location);

			nes.Run();

			Assert.Equal(value_to_load, nes.Cpu.Accumulator);
			Assert.Equal(expectZero, nes.Cpu.ZeroResult);
			Assert.Equal(expectNegative, nes.Cpu.Negative);
		}
	}
}
