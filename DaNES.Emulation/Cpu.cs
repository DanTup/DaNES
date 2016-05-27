namespace DanTup.DaNES.Emulation
{
	class Cpu
	{
		public ushort ProgramCounter { get; internal protected set; }
		public ushort StackPointer { get; protected set; }
		public MemoryMap Ram { get; }

		public Cpu(MemoryMap ram)
		{
			Ram = ram;
		}
	}
}
