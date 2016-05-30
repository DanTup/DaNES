using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

			Task.Run(() => nes.Run(UpdateScreen));
		}

		void UpdateScreen(Bitmap img)
		{
			try
			{
				Invoke((Action)(() => pbScreen.Image = (Bitmap)img.Clone()));
			}
			catch {
				// TODO: This happens when form gets closed/disposed (expected).
				// However may also masked real errors here :(
			}
		}
	}
}
