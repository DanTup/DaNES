using System;

namespace DanTup.DaNES.Emulation
{
	public class Memory
	{
		byte[] memory;

		public Memory(int size)
		{
			memory = new byte[size];
		}

		public byte Read(int address) => memory[address];

		public void Write(int address, byte value) => memory[address] = value;

		public void Write(int address, byte[] value) => Array.Copy(value, 0, memory, address, value.Length);
	}
}
