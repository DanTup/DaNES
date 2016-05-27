using System;
using System.Drawing;
using System.Threading;

namespace DanTup.DaNES.Emulation
{
	public class Nes
	{
		internal TimeSpan PpuCycleDuration { get; set; }
		DateTime lastTick = DateTime.Now;
		internal MemoryMap Ram { get; set; }
		internal Cpu6502 Cpu { get; set; }
		internal PpuMemoryMap PpuRam { get; set; }
		internal Ppu Ppu { get; set; }
		internal Bitmap Screen { get; } = new Bitmap(256, 240);

		protected const ushort InitialProgramCounter = 0xC004; /* TOOD: This is correct for nestest (non-automated), but we need to read from reset vector */
		protected const ushort InitialStackPointer = 0xFD;

		public Nes()
		{
			PpuRam = new PpuMemoryMap();
			Ppu = new Ppu(PpuRam, Screen);
			Ram = new MemoryMap(Ppu);
			Cpu = new Cpu6502(Ram, programCounter: InitialProgramCounter, stackPointer: InitialStackPointer);

			var ppuSpeed = 21.477272 / 4;
			PpuCycleDuration = TimeSpan.FromMilliseconds(1.0f / ppuSpeed);
		}

		public void Run() => Run(null);

		public void Run(Action<Bitmap> drawFrame)
		{
			int clock = -1;
			int? cpuCyclesToBurn = 0;
			while (true)
			{
				// CPU only steps on every third tick.
				// And only if we don't have to burn some cycles.
				clock = (clock + 1) % 3;
				if (clock % 3 == 0 && cpuCyclesToBurn-- <= 0)
					cpuCyclesToBurn = Cpu.Step();

				// If we get null back, the program has ended/hit unknown opcode.
				if (cpuCyclesToBurn == null)
					return;

				Ppu.Step();
				drawFrame?.Invoke(Screen);

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
			Ram.LoadCart(program);
		}
	}
}
