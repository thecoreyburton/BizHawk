using System;
using System.Globalization;
using System.IO;

using BizHawk.Common;
using BizHawk.Emulation.Common;
using BizHawk.Common.NumberExtensions;

// Motorola Corp 6800
namespace BizHawk.Emulation.Common.Cores.MC6800
{
	public sealed partial class MC6800
	{
		// operations that can take place in an instruction
		public const ushort IDLE = 0; 
		public const ushort OP = 1;
		public const ushort RD = 2;
		public const ushort WR = 3;
		public const ushort TR = 4;
		public const ushort ADD8 = 5;
		public const ushort SUB8 = 6;
		public const ushort ADC8 = 7;
		public const ushort SBC8 = 8;
		public const ushort INC16 = 9;
		public const ushort INC8 = 10;
		public const ushort DEC16 = 11;
		public const ushort DEC8 = 12;
		public const ushort ASL = 13;
		public const ushort ROL = 14;
		public const ushort ASR = 15;
		public const ushort ROR = 16;
		public const ushort TST = 17;
		public const ushort LSR = 18;
		public const ushort CPL = 19;
		public const ushort DA = 20;
		public const ushort AND8 = 21;
		public const ushort XOR8 = 22;
		public const ushort OR8 = 23;
		public const ushort CP8 = 24;
		public const ushort BIT = 25;
		public const ushort RES = 26;
		public const ushort SET = 27;		
		public const ushort WAI = 28;
		public const ushort ASGN = 29;
		public const ushort ADDS = 30; // signed 16 bit operation used in 2 instructions
		public const ushort JAM = 31;  // all undocumented opcodes jam the machine
		public const ushort RD_P = 32; // special read case to pop value into P
		public const ushort TR_16 = 34; // 16 bit transfer
		public const ushort NEG8 = 35; 
		public const ushort CLR = 36;
		public const ushort BIT8 = 37;
		public const ushort CP16 = 38;

		public MC6800()
		{
			Reset();
		}

		public void Reset()
		{
			ResetRegisters();
			ResetInterrupts();
			TotalExecutedCycles = 0;
			cur_instr = new ushort[] { OP };
		}

		// Memory Access 

		public Func<ushort, byte> ReadMemory;
		public Action<ushort, byte> WriteMemory;
		public Func<ushort, byte> PeekMemory;
		public Func<ushort, byte> DummyReadMemory;

		//this only calls when the first byte of an instruction is fetched.
		public Action<ushort> OnExecFetch;

		public void UnregisterMemoryMapper()
		{
			ReadMemory = null;
			ReadMemory = null;
			PeekMemory = null;
			DummyReadMemory = null;
		}

		public void SetCallbacks
		(
			Func<ushort, byte> ReadMemory,
			Func<ushort, byte> DummyReadMemory,
			Func<ushort, byte> PeekMemory,
			Action<ushort, byte> WriteMemory
		)
		{
			this.ReadMemory = ReadMemory;
			this.DummyReadMemory = DummyReadMemory;
			this.PeekMemory = PeekMemory;
			this.WriteMemory = WriteMemory;
		}

		// Execute instructions
		public void ExecuteOne()
		{
			switch (cur_instr[instr_pntr++])
			{
				case IDLE:
					// do nothing
					break;
				case OP:
					// Read the opcode of the next instruction				
					if (NMI)
					{
						if (TraceCallback != null)
						{
							TraceCallback(new TraceInfo
							{
								Disassembly = "====NMI====",
								RegisterInfo = ""
							});
						}

						NMI_();
					}
					else if (FlagI && interrupts_enabled && !jammed)
					{
						interrupts_enabled = false;

						if (TraceCallback != null)
						{
							TraceCallback(new TraceInfo
							{
								Disassembly = "====IRQ====",
								RegisterInfo = ""
							});
						}

						INTERRUPT_();
					}
					else
					{
						if (OnExecFetch != null) OnExecFetch(RegPC);
						if (TraceCallback != null) TraceCallback(State());
						FetchInstruction(ReadMemory(RegPC++));
					}
					instr_pntr = 0;
					break;
				case RD:
					Read_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case WR:
					Write_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case TR:
					TR_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case ADD8:
					ADD8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case SUB8:
					SUB8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case ADC8:
					ADC8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case SBC8:
					SBC8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case INC16:
					INC16_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case INC8:
					INC8_Func(cur_instr[instr_pntr++]);
					break;
				case DEC16:
					DEC16_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case DEC8:
					DEC8_Func(cur_instr[instr_pntr++]);
					break;
				case ASL:
					ASL_Func(cur_instr[instr_pntr++]);
					break;
				case ROL:
					ROL_Func(cur_instr[instr_pntr++]);
					break;
				case ASR:
					ASR_Func(cur_instr[instr_pntr++]);
					break;
				case ROR:
					ROR_Func(cur_instr[instr_pntr++]);
					break;
				case TST:
					TST_Func(cur_instr[instr_pntr++]);
					break;
				case LSR:
					LSR_Func(cur_instr[instr_pntr++]);
					break;
				case CPL:
					CPL_Func(cur_instr[instr_pntr++]);
					break;
				case DA:
					DA_Func(cur_instr[instr_pntr++]);
					break;
				case AND8:
					AND8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case XOR8:
					XOR8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case OR8:
					OR8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case CP8:
					CP8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case BIT:
					BIT_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case RES:
					RES_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case SET:
					SET_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case WAI:
					halted = true;
					// Read the opcode of the next instruction				
					if (NMI)
					{
						if (TraceCallback != null)
						{
							TraceCallback(new TraceInfo
							{
								Disassembly = "====NMI====",
								RegisterInfo = ""
							});
						}

						NMI_FAST();
					}
					else if (FlagI && interrupts_enabled && !jammed)
					{
						interrupts_enabled = false;

						if (TraceCallback != null)
						{
							TraceCallback(new TraceInfo
							{
								Disassembly = "====IRQ====",
								RegisterInfo = ""
							});
						}

						INTERRUPT_FAST();
					}
					else
					{
						cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						WAI };
					}
					instr_pntr = 0;
					break;
				case ASGN:
					ASGN_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case ADDS:
					ADDS_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case JAM:
					jammed = true;
					instr_pntr--;
					break;
				case RD_P:
					Read_Func_P(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case TR_16:
					TR_16_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case NEG8:
					NEG_8_Func(cur_instr[instr_pntr++]);
					break;
				case CLR:
					CLR_Func(cur_instr[instr_pntr++]);
					break;
				case BIT8:
					BIT8_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case CP16:
					CP16_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
			}
			totalExecutedCycles++;
		}

		// tracer stuff

		public Action<TraceInfo> TraceCallback;

		public string TraceHeader
		{
			get { return "LR35902: PC, machine code, mnemonic, operands, registers (A, B, P, Ix, SP), Cy, flags (HINZVCE)"; }
		}

		public TraceInfo State(bool disassemble = true)
		{
			ushort notused;

			return new TraceInfo
			{
				Disassembly = string.Format(
					"{0} ",
					disassemble ? Disassemble(RegPC, ReadMemory, out notused) : "---").PadRight(40),
				RegisterInfo = string.Format(
					"A:{0:X2} B:{1:X2} P:{2:X2} Ix:{3:X2} SP:{4:X2} Cy:{5} LY:{6} {7}{8}{9}{10}{11}{12}{13}",
					Regs[A],
					Regs[B],
					Regs[P],
					Regs[Ixl] | (Regs[Ixh] << 8),
					Regs[SPl] | (Regs[SPh] << 8),
					TotalExecutedCycles,
					LY,
					FlagH ? "H" : "h",
					FlagI ? "I" : "i",
					FlagN ? "N" : "n",
					FlagZ ? "Z" : "z",
					FlagN ? "V" : "v",
					FlagC ? "C" : "c",			
					
					interrupts_enabled ? "E" : "e")
			};
		}
		// State Save/Load

		public void SyncState(Serializer ser)
		{
			ser.BeginSection("LR35902");
			ser.Sync("Regs", ref Regs, false);
			ser.Sync("IRQE", ref interrupts_enabled);
			ser.Sync("NMI", ref NMI);
			ser.Sync("NMIPending", ref NMIPending);
			ser.Sync("IRQ", ref IRQ);
			ser.Sync("IRQPending", ref IRQPending);
			ser.Sync("Halted", ref halted);
			ser.Sync("ExecutedCycles", ref totalExecutedCycles);
			ser.Sync("EI_pending", ref EI_pending);

			ser.Sync("instruction_pointer", ref instr_pntr);
			ser.Sync("current instruction", ref cur_instr, false);
			ser.Sync("Stopped", ref stopped);
			ser.Sync("opcode", ref opcode);
			ser.Sync("jammped", ref jammed);
			ser.Sync("LY", ref LY);

			ser.EndSection();
		}
	}
}