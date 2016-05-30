using System;
using System.Drawing;
using System.Threading;

namespace DanTup.DaNES.Emulation
{
	public class Nes
	{
		internal TimeSpan CpuCycleDuration { get; set; }
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

			var cpuSpeed = 21.477272 / 12;
			CpuCycleDuration = TimeSpan.FromMilliseconds(1.0f / cpuSpeed);
		}

		public void Run() => Run(null);

		public void Run(Action<Bitmap> drawFrame)
		{
			while (true)
			{
				var cpuCyclesSpent = Cpu.Step();

				// If we get null back, the program has ended/hit unknown opcode.
				if (cpuCyclesSpent == null)
					return;

				// Step PPU 3x as many as CPU used.
				for (var i = 0; i < 3 * cpuCyclesSpent.Value; i++)
					Ppu.Step();

				// TODO: Probably don't need to do this so often?
				drawFrame?.Invoke(Screen);

				// Sleep until it's time for the next cycle.
				var elapsed = DateTime.Now - lastTick;
				var timeToSleep = cpuCyclesSpent.Value *  (int)((CpuCycleDuration) - elapsed).TotalMilliseconds;
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
