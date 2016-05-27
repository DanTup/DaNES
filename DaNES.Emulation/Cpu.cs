namespace DanTup.DaNES.Emulation
{
	class Cpu
	{
		public ushort ProgramCounter { get; internal protected set; }
		public ushort StackPointer { get; protected set; }
		public Memory Ram { get; }

		public Cpu(Memory ram)
		{
			Ram = ram;
		}
	}
}
