using System;
using System.IO;
using System.Linq;
using Xunit;

namespace DanTup.DaNES.Emulation.Tests.CpuTests
{
	public class NesTest : CpuTests
	{
		const string RomFile = "../../Roms/nestest.nes";

		[Fact]
		public void RunTests()
		{
			var program = new ArraySegment<byte>(File.ReadAllBytes(RomFile), 0x0010, 0x4000).ToArray();
			Run(program);
		}
	}
}
