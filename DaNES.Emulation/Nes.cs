using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace DanTup.DaNES.Emulation
{
	public class Nes // TODO: Make disposable or remove Bitmap.
	{
		internal double CpuCycleDurationMilliseconds { get; set; }
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
			CpuCycleDurationMilliseconds = 1.0f / cpuSpeed;
		}

		public void Run() => Run(null);

		public void Run(Action<Bitmap> drawFrame)
		{
			var sw = new Stopwatch();
			while (true)
			{
				sw.Start();
				var cpuCyclesSpent = Cpu.Step();

				// If we get null back, the program has ended/hit unknown opcode.
				if (cpuCyclesSpent == null)
					return;

				// Step PPU 3x as many as CPU used.
				for (var i = 0; i < 3 * cpuCyclesSpent.Value; i++)
				{
					if (Ppu.Step())
						drawFrame?.Invoke(Screen);
				}

				// Sleep until it's time for the next cycle.
				var timeToSleep = cpuCyclesSpent.Value * (CpuCycleDurationMilliseconds - sw.ElapsedMilliseconds);
				if (timeToSleep > 0)
					Thread.Sleep((int)timeToSleep);
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
