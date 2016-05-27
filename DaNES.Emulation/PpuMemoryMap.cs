namespace DanTup.DaNES.Emulation
{
	class PpuMemoryMap
	{
		Memory tables = new Memory(0x3000);
		Memory palettes = new Memory(0x20);
		
		public byte Read(ushort address)
		{
			if (address < 0x3000)
				return tables.Read(address);
			else if (address < 0x3F00)
				return tables.Read((ushort)(address - 0x1000));
			else if (address < 0x3F20)
				return palettes.Read((ushort)(0x3F00 + ((address - 0x3F00) % 0x20)));
			else
				return palettes.Read((ushort)(address - 0x3F00));
		}

		public byte Write(ushort address, byte value)
		{
			if (address < 0x3000)
				return tables.Write(address, value);
			else if (address < 0x3F00)
				return tables.Write((ushort)(address - 0x1000), value);
			else if (address < 0x3F20)
				return palettes.Write((ushort)(0x3F00 + ((address - 0x3F00) % 0x20)), value);
			else
				return palettes.Write((ushort)(address - 0x3F00), value);
		}
	}
}
