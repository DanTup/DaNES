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
			LDA_ABS = 0xAD,
			LDA_ABS_X = 0xBD,
			LDA_ABS_Y = 0xB9,
			LDA_IND_X = 0xA1,
			LDA_IND_Y = 0xB1,
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
			LDX_ABS = 0xAE,
			LDX_ABS_Y = 0xBE,
			LDY_IMD = 0xA0,
			LDY_ZERO = 0xA4,
			LDY_ZERO_X = 0xB4,
			LDY_ABS = 0xAC,
			LDY_ABS_X = 0xBC,
			STX_IMD = 0x86,
			STX_ZERO_Y = 0x96,
			STX_ABS = 0x8E,
			STY_IMD = 0x84,
			STY_ZERO_Y = 0x94,
			STY_ABS = 0x8C,
			JMP_ABS = 0x4C,
			JSR = 0x20,
			RTS = 0x60,
			RTI = 0x40,
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
			TAX = 0xAA,
			TXA = 0x8A,
			DEX = 0xCA,
			INX = 0xE8,
			TAY = 0xA8,
			TYA = 0x98,
			DEY = 0x88,
			INY = 0xC8,
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
			CPX_IMD = 0xE0,
			CPX_ZERO = 0xE4,
			CPX_ABS = 0xEC,
			CPY_IMD = 0xC0,
			CPY_ZERO = 0xC4,
			CPY_ABS = 0xCC,
			ORA_IMD = 0x09,
			ORA_ZERO = 0x05,
			ORA_ZERO_X = 0x15,
			ORA_ABS = 0x0D,
			ORA_ABS_X = 0x1D,
			ORA_ABS_Y = 0x19,
			ORA_IND_X = 0x01,
			ORA_IND_Y = 0x11,
			EOR_IMD = 0x49,
			EOR_ZERO = 0x45,
			EOR_ZERO_X = 0x55,
			EOR_ABS = 0x4D,
			EOR_ABS_X = 0x5D,
			EOR_ABS_Y = 0x59,
			EOR_IND_X = 0x41,
			EOR_IND_Y = 0x51,
			ADC_IMD = 0x69,
			ADC_ZERO = 0x65,
			ADC_ZERO_X = 0x75,
			ADC_ABS = 0x6D,
			ADC_ABS_X = 0x7D,
			ADC_ABS_Y = 0x79,
			ADC_IND_X = 0x61,
			ADC_IND_Y = 0x71,
			SBC_IMD = 0xE9,
			SBC_ZERO = 0xE5,
			SBC_ZERO_X = 0xF5,
			SBC_ABS = 0xED,
			SBC_ABS_X = 0xFD,
			SBC_ABS_Y = 0xF9,
			SBC_IND_X = 0xE1,
			SBC_IND_Y = 0xF1,
			LSR_A = 0x4A,
			LSR_ZERO = 0x46,
			LSR_ZERO_X = 0x56,
			LSR_ABS = 0x4E,
			LSR_ABS_X = 0x5E,
			ASL_A = 0x0A,
			ASL_ZERO = 0x06,
			ASL_ZERO_X = 0x16,
			ASL_ABS = 0x0E,
			ASL_ABS_X = 0x1E,
			ROR_A = 0x6A,
			ROR_ZERO = 0x66,
			ROR_ZERO_X = 0x76,
			ROR_ABS = 0x6E,
			ROR_ABS_X = 0x7E,
			ROL_A = 0x2A,
			ROL_ZERO = 0x26,
			ROL_ZERO_X = 0x36,
			ROL_ABS = 0x2E,
			ROL_ABS_X = 0x3E,
			INC_ZERO = 0xE6,
			INC_ZERO_X = 0xF6,
			INC_ABS = 0xEE,
			INC_ABS_X = 0xFE,
			DEC_ZERO = 0xC6,
			DEC_ZERO_X = 0xD6,
			DEC_ABS = 0xCE,
			DEC_ABS_X = 0xDE,
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
				{ OpCode.LDA_ABS,    () => LDA(Absolute())     },
				{ OpCode.LDA_ABS_X,  () => LDA(AbsoluteX())    },
				{ OpCode.LDA_ABS_Y,  () => LDA(AbsoluteY())    },
				{ OpCode.LDA_IND_X,  () => LDA(IndirectX())    },
				{ OpCode.LDA_IND_Y,  () => LDA(IndirectY())    },
				{ OpCode.LDX_IMD,    () => LDX(Immediate())    },
				{ OpCode.LDX_ZERO,   () => LDX(ZeroPage())     },
				{ OpCode.LDX_ZERO_Y, () => LDX(ZeroPageY())    },
				{ OpCode.LDX_ABS,    () => LDX(Absolute())     },
				{ OpCode.LDX_ABS_Y,  () => LDX(AbsoluteY())    },
				{ OpCode.LDY_IMD,    () => LDY(Immediate())    },
				{ OpCode.LDY_ZERO,   () => LDY(ZeroPage())     },
				{ OpCode.LDY_ZERO_X, () => LDY(ZeroPageX())    },
				{ OpCode.LDY_ABS,    () => LDY(Absolute())     },
				{ OpCode.LDY_ABS_X,  () => LDY(AbsoluteX())    },
				{ OpCode.STX_IMD,    () => STX(Immediate())    },
				{ OpCode.STX_ZERO_Y, () => STX(ZeroPageY())    },
				{ OpCode.STX_ABS,    () => STX(Absolute())     },
				{ OpCode.STY_IMD,    () => STY(Immediate())    },
				{ OpCode.STY_ZERO_Y, () => STY(ZeroPageY())    },
				{ OpCode.STY_ABS,    () => STY(Absolute())     },
				{ OpCode.JMP_ABS,    () => JMP(Absolute())     },
				{ OpCode.JSR,        () => JSR(Absolute())     },
				{ OpCode.RTS,        () => RTS()               },
				{ OpCode.RTI,        () => RTI()               },
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
				{ OpCode.TAX,        () => TAX()               },
				{ OpCode.TXA,        () => TXA()               },
				{ OpCode.DEX,        () => DEX()               },
				{ OpCode.INX,        () => INX()               },
				{ OpCode.TAY,        () => TAY()               },
				{ OpCode.TYA,        () => TYA()               },
				{ OpCode.DEY,        () => DEY()               },
				{ OpCode.INY,        () => INY()               },
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
				{ OpCode.CPX_IMD,    () => CPX(Immediate())    },
				{ OpCode.CPX_ZERO,   () => CPX(ZeroPage())     },
				{ OpCode.CPX_ABS,    () => CPX(Absolute())     },
				{ OpCode.CPY_IMD,    () => CPY(Immediate())    },
				{ OpCode.CPY_ZERO,   () => CPY(ZeroPage())     },
				{ OpCode.CPY_ABS,    () => CPY(Absolute())     },
				{ OpCode.ORA_IMD,    () => ORA(Immediate())    },
				{ OpCode.ORA_ZERO,   () => ORA(ZeroPage())     },
				{ OpCode.ORA_ZERO_X, () => ORA(ZeroPageX())    },
				{ OpCode.ORA_ABS,    () => ORA(Absolute())    },
				{ OpCode.ORA_ABS_X,  () => ORA(AbsoluteX())    },
				{ OpCode.ORA_ABS_Y,  () => ORA(AbsoluteY())    },
				{ OpCode.ORA_IND_X,  () => ORA(IndirectX())    },
				{ OpCode.ORA_IND_Y,  () => ORA(IndirectY())    },
				{ OpCode.EOR_IMD,    () => EOR(Immediate())    },
				{ OpCode.EOR_ZERO,   () => EOR(ZeroPage())     },
				{ OpCode.EOR_ZERO_X, () => EOR(ZeroPageX())    },
				{ OpCode.EOR_ABS,    () => EOR(ZeroPageY())    },
				{ OpCode.EOR_ABS_X,  () => EOR(AbsoluteX())    },
				{ OpCode.EOR_ABS_Y,  () => EOR(AbsoluteY())    },
				{ OpCode.EOR_IND_X,  () => EOR(IndirectX())    },
				{ OpCode.EOR_IND_Y,  () => EOR(IndirectY())    },
				{ OpCode.ADC_IMD,    () => ADC(Immediate())    },
				{ OpCode.ADC_ZERO,   () => ADC(ZeroPage())     },
				{ OpCode.ADC_ZERO_X, () => ADC(ZeroPageX())    },
				{ OpCode.ADC_ABS,    () => ADC(ZeroPageY())    },
				{ OpCode.ADC_ABS_X,  () => ADC(AbsoluteX())    },
				{ OpCode.ADC_ABS_Y,  () => ADC(AbsoluteY())    },
				{ OpCode.ADC_IND_X,  () => ADC(IndirectX())    },
				{ OpCode.ADC_IND_Y,  () => ADC(IndirectY())    },
				{ OpCode.SBC_IMD,    () => SBC(Immediate())    },
				{ OpCode.SBC_ZERO,   () => SBC(ZeroPage())     },
				{ OpCode.SBC_ZERO_X, () => SBC(ZeroPageX())    },
				{ OpCode.SBC_ABS,    () => SBC(ZeroPageY())    },
				{ OpCode.SBC_ABS_X,  () => SBC(AbsoluteX())    },
				{ OpCode.SBC_ABS_Y,  () => SBC(AbsoluteY())    },
				{ OpCode.SBC_IND_X,  () => SBC(IndirectX())    },
				{ OpCode.SBC_IND_Y,  () => SBC(IndirectY())    },
				{ OpCode.LSR_A,      () => LSR_A()             },
				{ OpCode.LSR_ZERO,   () => LSR(ZeroPage())     },
				{ OpCode.LSR_ZERO_X, () => LSR(ZeroPageX())    },
				{ OpCode.LSR_ABS,    () => LSR(Absolute())     },
				{ OpCode.LSR_ABS_X,  () => LSR(AbsoluteX())    },
				{ OpCode.ASL_A,      () => ASL_A()             },
				{ OpCode.ASL_ZERO,   () => ASL(ZeroPage())     },
				{ OpCode.ASL_ZERO_X, () => ASL(ZeroPageX())    },
				{ OpCode.ASL_ABS,    () => ASL(Absolute())     },
				{ OpCode.ASL_ABS_X,  () => ASL(AbsoluteX())    },
				{ OpCode.ROR_A,      () => ROR_A()             },
				{ OpCode.ROR_ZERO,   () => ROR(ZeroPage())     },
				{ OpCode.ROR_ZERO_X, () => ROR(ZeroPageX())    },
				{ OpCode.ROR_ABS,    () => ROR(Absolute())     },
				{ OpCode.ROR_ABS_X,  () => ROR(AbsoluteX())    },
				{ OpCode.ROL_A,      () => ROL_A()             },
				{ OpCode.ROL_ZERO,   () => ROL(ZeroPage())     },
				{ OpCode.ROL_ZERO_X, () => ROL(ZeroPageX())    },
				{ OpCode.ROL_ABS,    () => ROL(Absolute())     },
				{ OpCode.ROL_ABS_X,  () => ROL(AbsoluteX())    },
				{ OpCode.INC_ZERO,   () => INC(ZeroPage())     },
				{ OpCode.INC_ZERO_X, () => INC(ZeroPageX())    },
				{ OpCode.INC_ABS,    () => INC(Absolute())     },
				{ OpCode.INC_ABS_X,  () => INC(AbsoluteX())    },
				{ OpCode.DEC_ZERO,   () => DEC(ZeroPage())     },
				{ OpCode.DEC_ZERO_X, () => DEC(ZeroPageX())    },
				{ OpCode.DEC_ABS,    () => DEC(Absolute())     },
				{ OpCode.DEC_ABS_X,  () => DEC(AbsoluteX())    },
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
		void LDY(byte value) => YRegister = SetZN(value);
		void LDY(ushort address) => YRegister = SetZN(Ram.Read(address));

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
		void RTI()
		{
			SetStatus(Pop());
			ProgramCounter = FromBytes(Pop(), Pop());
		}

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
		void TAX() => XRegister = SetZN(Accumulator);
		void TXA() => Accumulator = SetZN(XRegister);
		void DEX() => SetZN(--XRegister);
		void INX() => SetZN(++XRegister);
		void TAY() => YRegister = SetZN(Accumulator);
		void TYA() => Accumulator = SetZN(YRegister);
		void DEY() => SetZN(--YRegister);
		void INY() => SetZN(++YRegister);

		byte LSR(byte value)
		{
			Carry = (value & 1) != 0;
			return SetZN((byte)(value >> 1));
		}
		void LSR(ushort address) => Ram.Write(address, LSR(Ram.Read(address)));
		void LSR_A() => Accumulator = LSR(Accumulator);

		byte ASL(byte value)
		{
			Carry = (value & 128) != 0;
			return SetZN((byte)(value << 1));
		}
		void ASL(ushort address) => Ram.Write(address, ASL(Ram.Read(address)));
		void ASL_A() => Accumulator = ASL(Accumulator);

		byte ROR(byte value)
		{
			var old_carry = Carry;
			Carry = (value & 1) != 0;
			return SetZN((byte)((value >> 1) | (old_carry ? 128 : 0)));
		}
		void ROR(ushort address) => Ram.Write(address, ROR(Ram.Read(address)));
		void ROR_A() => Accumulator = ROR(Accumulator);

		byte ROL(byte value)
		{
			var old_carry = Carry;
			Carry = (value & 128) != 0;
			return SetZN((byte)((value << 1) | (old_carry ? 1 : 0)));
		}
		void ROL(ushort address) => Ram.Write(address, ROL(Ram.Read(address)));
		void ROL_A() => Accumulator = ROL(Accumulator);

		void INC(ushort address) => Ram.Write(address, SetZN((byte)(Ram.Read(address) + 1)));
		void DEC(ushort address) => Ram.Write(address, SetZN((byte)(Ram.Read(address) - 1)));

		void Push(ushort value) => Push(ToBytes(value));
		void Push(byte[] value)
		{
			for (var i = 0; i < value.Length; i++)
				Push(value[i]);
		}
		void Push(byte value) => Ram.Write((ushort)(0x100 + StackPointer--), value);

		byte Pop() => Ram.Read((ushort)(0x100 + ++StackPointer));

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

		void CMP(byte value)
		{
			var result = Accumulator - value;
			Carry = (result & 256) == 0;
			SetZN((byte)result);
		}
		void CMP(ushort address) => CMP(Ram.Read(address));

		void CPX(byte value)
		{
			var result = XRegister - value;
			Carry = (result & 256) == 0;
			SetZN((byte)result);
		}
		void CPX(ushort address) => CPX(Ram.Read(address));

		void CPY(byte value)
		{
			var result = YRegister - value;
			Carry = (result & 256) == 0;
			SetZN((byte)result);
		}
		void CPY(ushort address) => CPY(Ram.Read(address));

		void ORA(byte value) => Accumulator = SetZN((byte)(Accumulator | value));
		void ORA(ushort address) => ORA(Ram.Read(address));

		void EOR(byte value) => Accumulator = SetZN((byte)(Accumulator ^ value));
		void EOR(ushort address) => EOR(Ram.Read(address));

		void ADC(byte value)
		{
			var result = Accumulator + value + (Carry ? 1 : 0);
			Carry = (result & 256) != 0;
			// If both inputs have the opposite sign to the result, it's an overflow.
			Overflow = ((Accumulator ^ result) & (value ^ result) & 128) != 0;
			Accumulator = SetZN((byte)result);
		}
		void ADC(ushort address) => ADC(Ram.Read(address));

		void SBC(byte value) => ADC((byte)(value ^ 0xFF)); // SBC is the same as ADC with bits inverted?
		void SBC(ushort address) => SBC(Ram.Read(address));

		byte Immediate() => ReadNext();
		ushort Absolute() => FromBytes(ReadNext(), ReadNext());
		ushort AbsoluteX() => (ushort)(FromBytes(ReadNext(), ReadNext()) + XRegister);
		ushort AbsoluteY() => (ushort)(FromBytes(ReadNext(), ReadNext()) + YRegister);
		ushort ZeroPage() => ReadNext();
		ushort ZeroPageX() => (ushort)((ReadNext() + XRegister) % 256);
		ushort ZeroPageY() => (ushort)((ReadNext() + YRegister) % 256);
		ushort IndirectX()
		{
			var addr1 = ZeroPageX();
			var addr2 = (ushort)(addr1 + 1);
			return FromBytes(Ram.Read(addr1), Ram.Read((ushort)(addr2 % 256)));
		}
		ushort IndirectY()
		{
			throw new Exception();
		}

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
