using System;

namespace DanTup.DaNES.Emulation
{
	class Cpu
	{
		protected long TotalCycles { get; set; }
		protected TimeSpan CycleDuration { get; }

		public ushort ProgramCounter { get; protected set; }
		public ushort StackPointer { get; protected set; }
		public Memory Ram { get; }

		public Cpu(Memory ram)
		{
			Ram = ram;

			// TODO: Allow passing in a speed (or "Fastest").
			CycleDuration = TimeSpan.Zero;
		}
	}
}
