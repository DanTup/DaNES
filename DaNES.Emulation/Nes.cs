namespace DanTup.DaNES.Emulation
{
	public class Nes
	{
		internal Cpu6502 Cpu { get; set; }
		internal Memory Ram { get; set; }

		protected const ushort InitialProgramCounter = 0xC000;
		protected const ushort InitialStackPointer = 0xFD;

		public Nes()
		{
			Ram = new Memory(0x10000);
			Cpu = new Cpu6502(Ram, programCounter: InitialProgramCounter, stackPointer: InitialStackPointer);
		}

		internal void Run() => Cpu.Run();

		/// <summary>
		/// Loads a program for the CPU to execute.
		/// </summary>
		internal void LoadProgram(params byte[] program)
		{
			// TODO: Should we duplicate this, or just re-point requests?
			Ram.Write(0x8000, program);
			Ram.Write(0xC000, program);
		}
	}
}
