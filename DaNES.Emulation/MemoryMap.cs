using System;
using System.Linq;

namespace DanTup.DaNES.Emulation
{
	class MemoryMap
	{
		Memory working = new Memory(0x800);
		Memory registers = new Memory(0x20);
		Memory expansion = new Memory(0x1FDF);
		Memory sram = new Memory(0x2000);
		Memory cart1;
		Memory cart2;

		Ppu ppu;

		public MemoryMap(Ppu ppu)
		{
			this.ppu = ppu;
		}

		public void LoadCart(params byte[] program)
		{
			if (program.Length > 0x4000)
			{
				cart1 = new Memory(0x4000);
				cart2 = new Memory(0x4000);
				cart1.Write(0x0, new ArraySegment<byte>(program, 0, 0x4000).ToArray());
				cart2.Write(0x0, new ArraySegment<byte>(program, 0x4000, 0x4000).ToArray());
			}
			else
			{
				cart1 = new Memory(0x4000);
				cart2 = cart1;
				cart1.Write(0x0, program);
			}
		}

		public byte Read(ushort address)
		{
			if (address < 0x2000)
				return working.Read((ushort)(address % 0x800));
			else if (address < 0x4000)
				return ppu.ReadRegister((ushort)(0x2000 + ((address - 0x2000) % 8)));
			else if (address < 0x4020)
				return registers.Read((ushort)(address - 0x4020));
			else if (address < 0x8000)
				return sram.Read((ushort)(address - 0x4020));
			else if (address < 0xC000)
				return cart1.Read((ushort)(address - 0x8000));
			else
				return cart2.Read((ushort)(address - 0xC000));
		}

		public byte Write(ushort address, byte value)
		{
			if (address < 0x2000)
				return working.Write((ushort)(address % 0x800), value);
			else if (address < 0x4000)
				return ppu.WriteRegister((ushort)(0x2000 + ((address - 0x2000) % 8)), value);
			else if (address < 0x4020)
				return registers.Write((ushort)(address - 0x4020), value);
			else if (address < 0x8000)
				return sram.Write((ushort)(address - 0x4020), value);
			else if (address < 0xC000)
				return cart1.Write((ushort)(address - 0x8000), value);
			else
				return cart2.Write((ushort)(address - 0xC000), value);
		}
	}
}
