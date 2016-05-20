using System;
using System.Collections.Generic;
using System.Threading;

namespace DanTup.DaNES.Emulation
{
	partial class Cpu6502
	{
		// Cycles since started.
		public long CycleCount { get; private set; }

		public TimeSpan CycleDuration { get; private set; }

		// Registers.
		public byte Accumulator { get; internal set; }
		public byte XRegister { get; internal set; }
		public byte YRegister { get; internal set; }

		public ushort ProgramCounter { get; internal set; } = 0xC000;
		public ushort StackPointer { get; internal set; } = 0xFD;

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

		/// <summary>
		/// All known OpCodes as an Enum to assign meaningful names.
		/// </summary>
		enum OpCode
		{
			NOP = 0xEA,
			LDA_IMD = 0xA9,
			LDA_ZERO = 0xA5,
			LDA_ZERO_X = 0xB5,
			STA_ZERO = 0x85,
			STA_ZERO_X = 0x95,
			STA_ABS = 0x8D,
			STA_ABS_X = 0x9D,
			STA_ABS_Y = 0x99,
			STA_IND_X = 0x81,
			STA_IND_Y = 0x91,
			LDX_IMD = 0xA2,
			LDX_ZERO = 0xA6,
			LDX_ZERO_Y = 0xB6,
			STX_IMD = 0x86,
			STX_ZERO_Y = 0x96,
			JMP_ABS = 0x4C,
			JSR = 0x20,
			CLC = 0x18,
			SEC = 0x38,
			CLI = 0x58,
			SEI = 0x78,
			CLV = 0xB8,
			CLD = 0xD8,
			SED = 0xF8,
			BPL = 0x10,
			BMI = 0x30,
			BVC = 0x50,
			BVS = 0x70,
			BCC = 0x90,
			BCS = 0xB0,
			BNE = 0xD0,
			BEQ = 0xF0,
		}

		/// <summary>
		/// A lookup of OpCodes and their functions.
		/// </summary>
		readonly Dictionary<OpCode, Action> opCodes;

		public Cpu6502(Memory ram)
		{
			Ram = ram;

			// TODO: Allow passing in a speed (or "Fastest").
			CycleDuration = TimeSpan.Zero;

			// Build a dictionary of known OpCodes.
			// A good reference can be found here:
			//   http://www.6502.org/tutorials/6502opcodes.html
			opCodes = new Dictionary<OpCode, Action>
			{
				{ OpCode.NOP,        () => NOP()               },
				{ OpCode.LDA_IMD,    () => LDA(Immediate())    },
				{ OpCode.LDA_ZERO,   () => LDA(ZeroPage())     },
				{ OpCode.LDA_ZERO_X, () => LDA(ZeroPageX())    },
				{ OpCode.LDX_IMD,    () => LDX(Immediate())    },
				{ OpCode.LDX_ZERO,   () => LDX(ZeroPage())     },
				{ OpCode.LDX_ZERO_Y, () => LDX(ZeroPageY())    },
				{ OpCode.STX_IMD,    () => STX(Immediate())    },
				{ OpCode.STX_ZERO_Y, () => STX(ZeroPageY())    },
				{ OpCode.JMP_ABS,    () => JMP(Absolute())     },
				{ OpCode.JSR,        () => JSR(Absolute())     },
				{ OpCode.CLC,        () => CLC()               },
				{ OpCode.SEC,        () => SEC()               },
				{ OpCode.CLI,        () => CLI()               },
				{ OpCode.SEI,        () => SEI()               },
				{ OpCode.CLV,        () => CLV()               },
				{ OpCode.CLD,        () => CLD()               },
				{ OpCode.SED,        () => SED()               },
				{ OpCode.BPL,        () => Branch(!Negative)   },
				{ OpCode.BMI,        () => Branch(Negative)    },
				{ OpCode.BVC,        () => Branch(!Overflow)   },
				{ OpCode.BVS,        () => Branch(Overflow)    },
				{ OpCode.BCC,        () => Branch(!Carry)      },
				{ OpCode.BCS,        () => Branch(Carry)       },
				{ OpCode.BNE,        () => Branch(!ZeroResult) },
				{ OpCode.BEQ,        () => Branch(ZeroResult)  },
				{ OpCode.STA_ZERO,   () => STA(ZeroPage())  },
				{ OpCode.STA_ZERO_X, () => STA(ZeroPageX())  },
				{ OpCode.STA_ABS,    () => STA(Absolute())  },
				{ OpCode.STA_ABS_X,  () => STA(AbsoluteX())  },
				{ OpCode.STA_ABS_Y,  () => STA(AbsoluteY())  },
				{ OpCode.STA_IND_X,  () => STA(IndirectX())  },
				{ OpCode.STA_IND_Y,  () => STA(IndirectY())  },
			};
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

			opCodes[opCode]();
			cyclesToSpend = opCodeCosts[(int)instr];

			return true;
		}

		void NOP() { }

		void LDA(byte value) => Accumulator = SetZN(value);
		void LDX(byte value) => XRegister = SetZN(value);

		void STA(ushort address) => Ram.Write(address, Accumulator);
		void STX(ushort address) => Ram.Write(address, XRegister);
		void STY(ushort address) => Ram.Write(address, YRegister);

		void JMP(ushort address) => ProgramCounter = address;

		void JSR(ushort address)
		{
			Push((ushort)(StackPointer - 1));
			JMP(address);
		}

		void CLC() => Carry = false;
		void SEC() => Carry = true;
		void CLI() => InterruptsEnabled = false;
		void SEI() => InterruptsEnabled = true;
		void CLV() => Overflow = false;
		void CLD() => DecimalMode = false;
		void SED() => DecimalMode = true;

		void Push(ushort value) => Push(ToBytes(value));
		void Push(byte[] value)
		{
			Ram.Write(StackPointer - (value.Length - 1), value);
			StackPointer -= (ushort)value.Length;
		}

		void Branch(bool condition)
		{
			var loc = ReadNext(); // Always need to consume the next byte.
			if (condition)
				ProgramCounter += loc;
		}

		byte Immediate() => ReadNext();
		ushort Absolute() => FromBytes(ReadNext(), ReadNext());
		ushort AbsoluteX() => (ushort)(Absolute() + XRegister);
		ushort AbsoluteY() => (ushort)(Absolute() + YRegister);
		byte ZeroPage() => Ram.Read(ReadNext());
		byte ZeroPageX() => Ram.Read((ReadNext() + XRegister) % 256);
		byte ZeroPageY() => Ram.Read((ReadNext() + YRegister) % 256);
		ushort IndirectX() => Ram.Read(AbsoluteX());
		ushort IndirectY() => Ram.Read(AbsoluteY());

		protected virtual byte ReadNext() => Ram.Read(ProgramCounter++);

		byte SetZN(byte value)
		{
			ZeroResult = value == 0;
			Negative = (value & 128) != 0;
			return value;
		}

		byte[] ToBytes(ushort value) => new[] { (byte)(value >> 8), (byte)value };
		ushort FromBytes(byte b1, byte b2) => (ushort)(b1 | b2 << 8);
	}
}
