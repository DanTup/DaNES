using System.Drawing;

namespace DanTup.DaNES.Emulation
{
	class Ppu
	{
		public Memory Ram { get; }
		public Bitmap Screen { get; }

		public Ppu(Memory ram, Bitmap screen)
		{
			Ram = ram;
			Screen = screen;

			for (var x = 0; x < 256; x++)
			{
				for (var y = 0; y < 240; y++)
				{
					Screen.SetPixel(x, y, Color.FromArgb(x, y, 128));
				}
			}
		}

		public void Step()
		{
		}
	}
}
