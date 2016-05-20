namespace DanTup.DaNES.Emulation
{
	public class Cpu
	{
		// Registers.
		public byte Accumulator { get; internal set; }
		public byte XRegister { get; internal set; }
		public byte YRegister { get; internal set; }

		public ushort ProgramCounter { get; internal set; }
		public ushort StackPointer { get; internal set; }

		// Status register.
		public bool Negative { get; internal set; }
		public bool Overflow { get; internal set; }
		public bool Interrupted { get; internal set; }
		public bool DecimalMode { get; internal set; }
		public bool InterruptsEnabled { get; internal set; }
		public bool ZeroResult { get; internal set; }
		public bool Carry { get; internal set; }

		// Memory.
		public Memory Ram { get; private set; }


		public Cpu()
		{
			Ram = new Memory(0x10000);
		}

		/// <summary>
		/// Loads a program for the CPU to execute.
		/// </summary>
		public void LoadProgram(byte[] program)
		{
			// TODO: This is NES specific, needs moving out...
			Ram.Write(0x8000, program);
			Ram.Write(0xC000, program);
			
			ProgramCounter = 0xC000;
		}
		public void Run()
		{
			while (true)
			{
				if (!ProcessNextOpCode())
					break;
			}
		}
		public virtual bool ProcessNextOpCode()
		{
			switch (ReadNext())
			{
				case 0xA9: // LDA Immediate
					Accumulator = ReadNext();
					ZeroResult = Accumulator == 0;
					Negative = (Accumulator & 128) != 0;
					break;

				default:
					return false;
			}

			return true;
		}

		byte ReadNext() => Ram.Read(ProgramCounter++);
	}
}
