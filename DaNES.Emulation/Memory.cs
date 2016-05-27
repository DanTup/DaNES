using System;

namespace DanTup.DaNES.Emulation
{
	class Memory
	{
		byte[] memory;

		public Memory(int size)
		{
			memory = new byte[size];
		}

		public byte Read(ushort address) => memory[address];

		public byte Write(ushort address, byte value) => memory[address] = value;

		public void Write(ushort address, byte[] value) => Array.Copy(value, 0, memory, address, value.Length);

		public byte[] Raw => memory;
	}
}
