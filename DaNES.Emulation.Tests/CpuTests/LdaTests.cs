using Xunit;

namespace DanTup.DaNES.Emulation.Tests.CpuTests
{
	public class LdaTests : CpuTests
	{
		[Theory]
		[InlineData(0, true, false)]
		[InlineData(1, false, false)]
		[InlineData(127, false, false)]
		[InlineData(128, false, true)]
		[InlineData(129, false, true)]
		[InlineData(255, false, true)]
		public void Lda_Immediate(byte value_to_load, bool expectZero, bool expectNegative)
		{
			Run(0xA9, value_to_load);

			Assert.Equal(value_to_load, cpu.Accumulator);
			Assert.Equal(expectZero, cpu.ZeroResult);
			Assert.Equal(expectNegative, cpu.Negative);
		}
	}
}
