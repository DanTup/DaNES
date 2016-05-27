using System;
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

		byte OamAddress;
		byte OamData;
		byte PpuScroll;
		byte PpuAddr;
		byte PpuData;
		byte OamDma;

		public Ppu(Memory ram, Bitmap screen)
		{
			Ram = ram;
			Screen = screen;

			// Initialise with a nice rainbow screen.
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

		public void WriteRegister(ushort address, byte value)
		{
			// CPU addresses registers as 8 bytes from 0x2000.
			// https://en.wikibooks.org/wiki/NES_Programming/Memory_Map
			switch (address)
			{
				case 0x2000:
					NmiEnable = (value & 0x80) != 0;
					PpuMasterSlave = (value & 0x40) != 0;
					SpriteHeight = (value & 0x20) != 0;
					BackgroundTileSelect = (value & 0x10) != 0;
					SpriteTileSelect = (value & 0x8) != 0;
					IncrementMode = (value & 0x4) != 0;
					NameTableSelect1 = (value & 0x2) != 0;
					NameTableSelect0 = (value & 0x1) != 0;
					break;
				case 0x2001:
					TintBlue = (value & 0x80) != 0;
					TintGreen = (value & 0x40) != 0;
					TintRed = (value & 0x20) != 0;
					ShowSprites = (value & 0x10) != 0;
					ShowBackground = (value & 0x8) != 0;
					ShowLeftSprites = (value & 0x4) != 0;
					ShowLeftBackground = (value & 0x2) != 0;
					Greyscale = (value & 0x1) != 0;
					break;
				case 0x2002:
					VBlank = (value & 0x80) != 0;
					Sprite0Hit = (value & 0x40) != 0;
					SpriteOverflow = (value & 0x20) != 0;
					break;
				case 0x2003:
					OamAddress = value;
					break;
				case 0x2004:
					OamData = value;
					break;
				case 0x2005:
					PpuScroll = value;
					break;
				case 0x2006:
					PpuAddr = value;
					break;
				case 0x2007:
					PpuData = value;
					break;
				case 0x4014:
					OamDma = value;
					break;
				default:
					throw new InvalidOperationException(string.Format("Invalid attempt to write to PPU address {0:X2}", address));
			}
		}
	}
}
