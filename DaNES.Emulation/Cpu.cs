using System;
using System.Collections.Generic;
using System.Threading;

namespace DanTup.DaNES.Emulation
{
	public class Cpu
	{
		// Cycles since started.
		public long CycleCount { get; private set; }

		public TimeSpan CycleDuration { get; private set; }

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

		/// <summary>
		/// Keeps track of how many cycles an operation is expected to take so we can
		/// keep the correct timing.
		/// </summary>
		int cyclesToSpend;

		public Cpu()
		{
			// TODO: Allow passing in a speed (or "Fastest").
			CycleDuration = TimeSpan.Zero;

			Ram = new Memory(0x10000);

			Init();
		}

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
			// TODO: This is NES specific, needs moving out...
			Ram.Write(0x8000, program);
			Ram.Write(0xC000, program);

			if (resetState)
				Init();
		}

		void Init()
		{
			// Reset all the registers.
			Accumulator = 0;
			XRegister = 0;
			YRegister = 0;
			ProgramCounter = 0xC000;
			StackPointer = 0xFD;
			Negative = false;
			Overflow = false;
			Interrupted = false;
			DecimalMode = false;
			InterruptsEnabled = false;
			ZeroResult = false;
			Carry = false;

			CycleCount = 0;
		}

		public void Run()
		{
			while (true)
			{

				DateTime startTime = DateTime.Now;

				if (!ProcessNextOpCode())
					break;

				// Subtract processing time from initial cycle.
				var currentCycleRemainingDuration = CycleDuration - (DateTime.Now - startTime);
				while (cyclesToSpend-- > 0)
				{
					// Sleep for however long is left for this cycle.
					if (currentCycleRemainingDuration > TimeSpan.Zero)
						Thread.Sleep(currentCycleRemainingDuration);
					CycleCount++;
					// Sleep for full duration on subsequent cycles.
					currentCycleRemainingDuration = CycleDuration;
				}
			}
		}

		public virtual bool ProcessNextOpCode()
		{
			var instr = ReadNext();
			if (instr == 0)
				return false;

			var opCode = (OpCode)instr;
			if (!opCodes.ContainsKey(opCode))
				throw new InvalidOperationException(string.Format("Unknown opcode: 0x{0}", instr.ToString("X2")));

			opCodes[opCode](this);
			cyclesToSpend = opCodeCosts[(int)instr];

			return true;
		}

		enum OpCode
		{
			LDA_IMD = 0xA9,
			LDA_ZERO = 0xA5,
			LDA_ZERO_X = 0xB5,
			LDX_IMD = 0xA2,
			LDX_ZERO = 0xA6,
			LDX_ZERO_Y = 0xB6,
			STX_IMD = 0x86,
			STX_ZERO_Y = 0x96,
			JMP_ABS = 0x4C,
		}

		// TODO: Fill these in!
		int[] opCodeCosts = new int[]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x00 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x10 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x20 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x30 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x40 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x50 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x60 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x70 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x80 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0x90 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0xA0 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0xB0 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0xC0 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0xD0 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0xE0 */
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, /* 0xF0 */
		};

		Dictionary<OpCode, Action<Cpu>> opCodes = new Dictionary<OpCode, Action<Cpu>>
		{
			{ OpCode.LDA_IMD,    cpu => cpu.LDA(cpu.Immediate()) },
			{ OpCode.LDA_ZERO,   cpu => cpu.LDA(cpu.ZeroPage())  },
			{ OpCode.LDA_ZERO_X, cpu => cpu.LDA(cpu.ZeroPageX()) },
			{ OpCode.LDX_IMD,    cpu => cpu.LDX(cpu.Immediate()) },
			{ OpCode.LDX_ZERO,   cpu => cpu.LDX(cpu.ZeroPage())  },
			{ OpCode.LDX_ZERO_Y, cpu => cpu.LDX(cpu.ZeroPageY()) },
			{ OpCode.STX_IMD,    cpu => cpu.STX(cpu.Immediate()) },
			{ OpCode.STX_ZERO_Y, cpu => cpu.STX(cpu.ZeroPageY()) },
			{ OpCode.JMP_ABS,    cpu => cpu.JMP(cpu.Absolute())  },
		};

		void LDA(byte value) => Accumulator = SetZN(value);
		void LDX(byte value) => XRegister = SetZN(value);

		void STA(ushort address) => Ram.Write(address, Accumulator);
		void STX(ushort address) => Ram.Write(address, XRegister);
		void STY(ushort address) => Ram.Write(address, YRegister);

		void JMP(ushort address) => ProgramCounter = address;

		byte Immediate() => this.ReadNext();
		ushort Absolute() => (ushort)(ReadNext() | ReadNext() << 8);
		byte ZeroPage() => Ram.Read(ReadNext());
		byte ZeroPageX() => Ram.Read(ReadNext() + XRegister);
		byte ZeroPageY() => Ram.Read(ReadNext() + YRegister);

		protected virtual byte ReadNext() => Ram.Read(ProgramCounter++);

		byte SetZN(byte value)
		{
			ZeroResult = value == 0;
			Negative = (value & 128) != 0;
			return value;
		}
	}
}
