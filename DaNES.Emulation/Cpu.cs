using System;

namespace DanTup.DaNES.Emulation
{
	class Cpu
	{
		public long TotalCycles { get; protected set; }
		public TimeSpan CycleDuration { get; private set; }

		public ushort ProgramCounter { get; internal set; } = 0xC000;
		public ushort StackPointer { get; internal set; } = 0xFD;
		public Memory Ram { get; private set; }

		public Cpu(Memory ram)
		{
			Ram = ram;

			// TODO: Allow passing in a speed (or "Fastest").
			CycleDuration = TimeSpan.Zero;
		}
	}
}
