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
		public bool InterruptsDisabled { get; internal set; } = true;
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
			RTS = 0x60,
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
			BIT_ZERO = 0x24,
			BIT_ABS = 0x2C,
			TXS = 0x9A,
			TSX = 0xBA,
			PHA = 0x48,
			PLA = 0x68,
			PHP = 0x08,
			PLP = 0x28,
			AND_IMD = 0x29,
			AND_ZERO = 0x25,
			AND_ZERO_X = 0x35,
			AND_ABS = 0x2D,
			AND_ABS_X = 0x3D,
			AND_ABS_Y = 0x39,
			AND_IND_X = 0x21,
			AND_IND_Y = 0x31,
			CMP_IMD = 0xC9,
			CMP_ZERO = 0xC5,
			CMP_ZERO_X = 0xD5,
			CMP_ABS = 0xCD,
			CMP_ABS_X = 0xDD,
			CMP_ABS_Y = 0xD9,
			CMP_IND_X = 0xC1,
			CMP_IND_Y = 0xD1,
			ORA_IMD = 0x09,
			ORA_ZERO = 0x05,
			ORA_ZERO_X = 0x15,
			ORA_ABS = 0x0D,
			ORA_ABS_X = 0x1D,
			ORA_ABS_Y = 0x19,
			ORA_IND_X = 0x01,
			ORA_IND_Y = 0x11,
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
				{ OpCode.RTS,        () => RTS()               },
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
				{ OpCode.STA_ZERO,   () => STA(ZeroPage())     },
				{ OpCode.STA_ZERO_X, () => STA(ZeroPageX())    },
				{ OpCode.STA_ABS,    () => STA(Absolute())     },
				{ OpCode.STA_ABS_X,  () => STA(AbsoluteX())    },
				{ OpCode.STA_ABS_Y,  () => STA(AbsoluteY())    },
				{ OpCode.STA_IND_X,  () => STA(IndirectX())    },
				{ OpCode.STA_IND_Y,  () => STA(IndirectY())    },
				{ OpCode.BIT_ZERO,   () => BIT(ZeroPage())     },
				{ OpCode.BIT_ABS,    () => BIT(Absolute())     },
				{ OpCode.TXS,        () => TXS()               },
				{ OpCode.TSX,        () => TSX()               },
				{ OpCode.PHA,        () => PHA()               },
				{ OpCode.PLA,        () => PLA()               },
				{ OpCode.PHP,        () => PHP()               },
				{ OpCode.PLP,        () => PLP()               },
				{ OpCode.AND_IMD,    () => AND(Immediate())    },
				{ OpCode.AND_ZERO,   () => AND(ZeroPage())     },
				{ OpCode.AND_ZERO_X, () => AND(ZeroPageX())    },
				{ OpCode.AND_ABS,    () => AND(ZeroPageY())    },
				{ OpCode.AND_ABS_X,  () => AND(AbsoluteX())    },
				{ OpCode.AND_ABS_Y,  () => AND(AbsoluteY())    },
				{ OpCode.AND_IND_X,  () => AND(IndirectX())    },
				{ OpCode.AND_IND_Y,  () => AND(IndirectY())    },
				{ OpCode.CMP_IMD,    () => CMP(Immediate())    },
				{ OpCode.CMP_ZERO,   () => CMP(ZeroPage())     },
				{ OpCode.CMP_ZERO_X, () => CMP(ZeroPageX())    },
				{ OpCode.CMP_ABS,    () => CMP(ZeroPageY())    },
				{ OpCode.CMP_ABS_X,  () => CMP(AbsoluteX())    },
				{ OpCode.CMP_ABS_Y,  () => CMP(AbsoluteY())    },
				{ OpCode.CMP_IND_X,  () => CMP(IndirectX())    },
				{ OpCode.CMP_IND_Y,  () => CMP(IndirectY())    },
				{ OpCode.ORA_IMD,    () => ORA(Immediate())    },
				{ OpCode.ORA_ZERO,   () => ORA(ZeroPage())     },
				{ OpCode.ORA_ZERO_X, () => ORA(ZeroPageX())    },
				{ OpCode.ORA_ABS,    () => ORA(ZeroPageY())    },
				{ OpCode.ORA_ABS_X,  () => ORA(AbsoluteX())    },
				{ OpCode.ORA_ABS_Y,  () => ORA(AbsoluteY())    },
				{ OpCode.ORA_IND_X,  () => ORA(IndirectX())    },
				{ OpCode.ORA_IND_Y,  () => ORA(IndirectY())    },
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
		void LDA(ushort address) => Accumulator = SetZN(Ram.Read(address));
		void LDX(byte value) => XRegister = SetZN(value);
		void LDX(ushort address) => XRegister = SetZN(Ram.Read(address));

		void STA(ushort address) => Ram.Write(address, Accumulator);
		void STX(ushort address) => Ram.Write(address, XRegister);
		void STY(ushort address) => Ram.Write(address, YRegister);

		void JMP(ushort address) => ProgramCounter = address;

		void JSR(ushort address)
		{
			Push((ushort)(ProgramCounter - 1));
			JMP(address);
		}

		void RTS() => ProgramCounter = (ushort)(FromBytes(Pop(), Pop()) + 1);

		void CLC() => Carry = false;
		void SEC() => Carry = true;
		void CLI() => InterruptsDisabled = false;
		void SEI() => InterruptsDisabled = true;
		void CLV() => Overflow = false;
		void CLD() => DecimalMode = false;
		void SED() => DecimalMode = true;

		void BIT(ushort address)
		{
			var value = Ram.Read(address);
			ZeroResult = (value & Accumulator) == 0;
			Negative = (value & 128) != 0;
			Overflow = (value & 64) != 0;
		}

		void TXS() => StackPointer = XRegister;
		void TSX() => XRegister = SetZN((byte)StackPointer);
		void PHA() => Push(Accumulator);
		void PLA() => Accumulator = SetZN(Pop());
		void PHP() => Push((byte)(GetStatus() | 16));
		void PLP() => SetStatus(Pop());

		void Push(ushort value) => Push(ToBytes(value));
		void Push(byte[] value)
		{
			for (var i = 0; i < value.Length; i++)
				Push(value[i]);
		}
		void Push(byte value) => Ram.Write(StackPointer--, value);

		byte Pop() => Ram.Read(++StackPointer);

		void Branch(bool condition)
		{
			var loc = ReadNext(); // Always need to consume the next byte.
			if (condition)
				ProgramCounter += loc;

			byte b = byte.MinValue;
			var a = b & b;
		}

		void AND(byte value) => Accumulator = SetZN((byte)(Accumulator & value));
		void AND(ushort address) => AND(Ram.Read(address));

		void CMP(byte value) => Carry = (SetZN((byte)(Accumulator - value)) & 256) == 0;
		void CMP(ushort address) => CMP(Ram.Read(address));

		void ORA(byte value) => Accumulator = SetZN((byte)(Accumulator | value));
		void ORA(ushort address) => ORA(Ram.Read(address));

		byte Immediate() => ReadNext();
		ushort Absolute() => FromBytes(ReadNext(), ReadNext());
		ushort AbsoluteX() => (ushort)(FromBytes(ReadNext(), ReadNext()) + XRegister);
		ushort AbsoluteY() => (ushort)(FromBytes(ReadNext(), ReadNext()) + YRegister);
		ushort ZeroPage() => ReadNext();
		ushort ZeroPageX() => (ushort)((ReadNext() + XRegister) % 256);
		ushort ZeroPageY() => (ushort)((ReadNext() + YRegister) % 256);
		ushort IndirectX() => Ram.Read(AbsoluteX());
		ushort IndirectY() => Ram.Read(AbsoluteY());

		protected virtual byte ReadNext() => Ram.Read(ProgramCounter++);

		byte SetZN(byte value)
		{
			ZeroResult = value == 0;
			Negative = (value & 128) != 0;
			return value;
		}

		internal byte GetStatus()
		{
			return (byte)(
				(Negative ? 128 : 0)
				+ (Overflow ? 64 : 0)
				+ 32
				+ (Interrupted ? 16 : 0)
				+ (DecimalMode ? 8 : 0)
				+ (InterruptsDisabled ? 4 : 0)
				+ (ZeroResult ? 2 : 0)
				+ (Carry ? 1 : 0)
			);
		}

		void SetStatus(byte value)
		{
			// TODO: Decide if it's faster doing this and using bools elsewhere, or just having a byte for Status
			// and doing the required bit operations elsewhere.
			Negative = (value & 128) != 0;
			Overflow = (value & 64) != 0;
			Interrupted = false;
			DecimalMode = (value & 8) != 0;
			InterruptsDisabled = (value & 4) != 0;
			ZeroResult = (value & 2) != 0;
			Carry = (value & 1) != 0;
		}

		byte[] ToBytes(ushort value) => new[] { (byte)(value >> 8), (byte)value };
		ushort FromBytes(byte b1, byte b2) => (ushort)(b1 | b2 << 8);
	}
}
