using System;
using System.Threading;

namespace DanTup.DaNES.Emulation
{
	public class Nes
	{
		internal TimeSpan PpuCycleDuration { get; set; }
		DateTime lastTick = DateTime.Now;
		internal Cpu6502 Cpu { get; set; }
		internal Memory Ram { get; set; }

		protected const ushort InitialProgramCounter = 0xC004; /* TOOD: This is correct for nestest (non-automated), but we need to read from reset vector */
		protected const ushort InitialStackPointer = 0xFD;

		public Nes()
		{
			Ram = new Memory(0x10000);
			Cpu = new Cpu6502(Ram, programCounter: InitialProgramCounter, stackPointer: InitialStackPointer);

			var ppuSpeed = 21.477272 / 4;
			PpuCycleDuration = TimeSpan.FromMilliseconds(1.0f / ppuSpeed);

		}

		public void Run()
		{
			int clock = 0;
			int? cpuCyclesToBurn = 0;
			while (true)
			{
				// CPU only steps on every third tick.
				// And only if we don't have to burn some cycles.
				if (clock++ % 3 == 0 && cpuCyclesToBurn-- <= 0)
					cpuCyclesToBurn = Cpu.Step();

				// If we get null back, the program has ended/hit unknown opcode.
				if (cpuCyclesToBurn == null)
					return;

				// Ppu.Step();

				// Sleep until it's time for the next cycle.
				var elapsed = DateTime.Now - lastTick;
				var timeToSleep = (int)(PpuCycleDuration - elapsed).TotalMilliseconds;
				if (timeToSleep > 0)
					Thread.Sleep(timeToSleep);
				lastTick = DateTime.Now;
			}
		}

		/// <summary>
		/// Loads a program for the CPU to execute.
		/// </summary>
		public void LoadProgram(params byte[] program)
		{
			// TODO: Should we duplicate this, or just re-point requests?
			Ram.Write(0x8000, program);
			Ram.Write(0xC000, program);
		}
	}
}
