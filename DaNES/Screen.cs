using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DanTup.DaNES.Emulation;

namespace DanTup.DaNES
{
	public partial class Screen : Form
	{
		const string RomFile = @"..\..\..\DaNES.Emulation.Tests\NesTest\nestest.nes";
		Nes nes = new Nes();

		public Screen()
		{
			InitializeComponent();

			var program = new ArraySegment<byte>(File.ReadAllBytes(RomFile), 0x0010, 0x4000).ToArray();
			nes.LoadProgram(program);

			nes.Run();
		}
	}
}
