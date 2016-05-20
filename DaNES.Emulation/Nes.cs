namespace DanTup.DaNES.Emulation
{
	public class Nes
	{
		internal Cpu6502 Cpu { get; set; }
		internal Memory Ram { get; set; }

		public Nes()
		{
			Ram = new Memory(0x10000);
			Cpu = new Cpu6502(Ram);
		}

		internal Nes(Cpu6502 cpu, Memory ram)
		{
			Cpu = cpu;
			Ram = ram;
		}

		internal void Run() => Cpu.Run();

		/// <summary>
		/// Loads a program for the CPU to execute, resetting all registers
		/// and flags.
		/// </summary>
		public void LoadProgram(byte[] program)
		{
			LoadProgram(program, true);
		}

		/// <summary>
		/// Loads a program for the CPU to execute, optionally resetting all
		/// registers and flags.
		/// </summary>
		internal void LoadProgram(byte[] program, bool resetState)
		{
			Ram.Write(0x8000, program);
			Ram.Write(0xC000, program);

			if (resetState)
				Cpu.Init();
		}
	}
}
