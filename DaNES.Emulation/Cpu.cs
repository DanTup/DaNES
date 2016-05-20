namespace DanTup.DaNES.Emulation
{
	public class Cpu
	{
		byte Accumulator, XRegister, YRegister;
		ushort ProgramCounter, StackPointer;
		bool Negative, Overflow, Interrupted, DecimalMode, InterruptsEnabled, ZeroResult, Carry;
		Memory Ram;

		public Cpu()
		{
			Ram = new Memory(0x10000);
		}

		public void Run()
		{
		}
	}
}
