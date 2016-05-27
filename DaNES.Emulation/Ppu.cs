using System.Drawing;

namespace DanTup.DaNES.Emulation
{
	class Ppu
	{
		public Memory Ram { get; }
		public Bitmap Screen { get; }

		// PPU Control
		bool NmiEnable;
		bool PpuMasterSlave;
		bool SpriteHeight;
		bool BackgroundTileSelect;
		bool SpriteTileSelect;
		bool IncrementMode;
		bool NameTableSelect1;
		bool NameTableSelect0;

		// PPU Mask
		bool TintBlue;
		bool TintGreen;
		bool TintRed;
		bool ShowSprites;
		bool ShowBackground;
		bool ShowLeftSprites;
		bool ShowLeftBackground;
		bool Greyscale;

		// PPU Status
		bool VBlank;
		bool Sprite0Hit;
		bool SpriteOverflow;

		byte OamAddress { get; }
		byte OamData { get; }
		byte PpuScroll { get; }
		byte PpuAddr { get; }
		byte PpuData { get; }
		byte OamDma { get; }

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
