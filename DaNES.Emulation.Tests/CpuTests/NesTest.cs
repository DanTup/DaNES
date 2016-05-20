using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace DanTup.DaNES.Emulation.Tests.CpuTests
{
	public class NesTest : CpuTests
	{
		const string ActualLogFile = "../../Logs/nestest.actual.log";
		const string ExpectedLogFile = "../../Logs/nestest.expected.log";
		const string RomFile = "../../Roms/nestest.nes";

		{
		[Fact]
		public void RunTests()
		{
			var program = new ArraySegment<byte>(File.ReadAllBytes(RomFile), 0x0010, 0x4000).ToArray();
			Run(program);

			{
			}
		}
	}
}
