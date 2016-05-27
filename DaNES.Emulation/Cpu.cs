using System;

namespace DanTup.DaNES.Emulation
{
	class Cpu
	{
		protected long TotalCycles { get; set; }
		protected TimeSpan CycleDuration { get; set; }

		public ushort ProgramCounter { get; protected set; }
		public ushort StackPointer { get; protected set; }
		public Memory Ram { get; }

		public Cpu(Memory ram)
		{
			Ram = ram;

			// TODO: What is this?
			var cpuSpeed = 21.477272 / 12;
			CycleDuration = TimeSpan.FromMilliseconds(1.0f / cpuSpeed);
		}
	}
}
