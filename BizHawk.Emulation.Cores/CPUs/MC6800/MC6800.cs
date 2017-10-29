using System;
using System.Globalization;
using System.IO;

using BizHawk.Common;
using BizHawk.Emulation.Common;
using BizHawk.Common.NumberExtensions;

// GameBoy CPU (Sharp LR35902)
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
		public const ushort ADD16 = 5;
		public const ushort ADD8 = 6;
		public const ushort SUB8 = 7;
		public const ushort ADC8 = 8;
		public const ushort SBC8 = 9;
		public const ushort INC16 = 10;
		public const ushort INC8 = 11;
		public const ushort DEC16 = 12;
		public const ushort DEC8 = 13;
		public const ushort ASL = 14;
		public const ushort ROL = 15;
		public const ushort ASR = 16;
		public const ushort ROR = 17;
		public const ushort TST = 18;
		public const ushort LSR = 19;
		public const ushort CPL = 20;
		public const ushort DA = 21;
		public const ushort AND8 = 22;
		public const ushort XOR8 = 23;
		public const ushort OR8 = 24;
		public const ushort CP8 = 25;
		public const ushort SLA = 26;
		public const ushort SWAP = 29;
		public const ushort BIT = 30;
		public const ushort RES = 31;
		public const ushort SET = 32;		
		public const ushort EI = 33;
		public const ushort DI = 34;
		public const ushort HALT = 35;
		public const ushort STOP = 36;
		public const ushort ASGN = 38;
		public const ushort ADDS = 39; // signed 16 bit operation used in 2 instructions
		public const ushort OP_G = 40; // glitchy opcode read performed by halt when interrupts disabled
		public const ushort JAM = 41;  // all undocumented opcodes jam the machine
		public const ushort RD_F = 42; // special read case to pop value into F
		public const ushort EI_RETI = 43; // reti has no delay in interrupt enable
		public const ushort TR_16 = 44; // 16 bit transfer
		public const ushort NEG8 = 45; 
		public const ushort CLR = 46;
		public const ushort BIT8 = 47;
		public const ushort CP16 = 48;

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
		public void ExecuteOne(ref byte interrupt_src, byte interrupt_enable)
		{
			switch (cur_instr[instr_pntr++])
			{
				case IDLE:
					// do nothing
					break;
				case OP:
					// Read the opcode of the next instruction				
					if (EI_pending > 0)
					{
						EI_pending--;
						if (EI_pending == 0)
						{
							interrupts_enabled = true;
						}
					}

					if (FlagI && interrupts_enabled && !jammed)
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

						// call interrupt processor with the appropriate source
						// lowest bit set is highest priority
						ushort priority = 0;

						if (interrupt_src.Bit(0) && interrupt_enable.Bit(0)) { priority = 0; interrupt_src -= 1; }
						else if (interrupt_src.Bit(1) && interrupt_enable.Bit(1)) { priority = 1; interrupt_src -= 2; }
						else if (interrupt_src.Bit(2) && interrupt_enable.Bit(2)) { priority = 2; interrupt_src -= 4; }
						else if (interrupt_src.Bit(3) && interrupt_enable.Bit(3)) { priority = 3; interrupt_src -= 8; }
						else if (interrupt_src.Bit(4) && interrupt_enable.Bit(4)) { priority = 4; interrupt_src -= 16; }
						else { /*Console.WriteLine("No source"); }*/throw new Exception("Interrupt without Source"); }

						if ((interrupt_src & interrupt_enable) == 0) { FlagI = false; }

						INTERRUPT_(priority);
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
				case ADD16:
					ADD16_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
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
				case SWAP:
					SWAP_Func(cur_instr[instr_pntr++]);
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
				case EI:
					EI_pending = 2;
					break;
				case DI:
					interrupts_enabled = false;
					EI_pending = 0;
					break;
				case HALT:
					halted = true;

					if (EI_pending > 0)
					{
						EI_pending--;
						if (EI_pending == 0)
						{
							interrupts_enabled = true;
						}
					}

					// if the I flag is asserted at the time of halt, don't halt

					if (FlagI && interrupts_enabled && !jammed)
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
						halted = false;
						// call interrupt processor with the appropriate source
						// lowest bit set is highest priority
						// call interrupt processor with the appropriate source
						// lowest bit set is highest priority
						ushort priority = 0;

						if (interrupt_src.Bit(0) && interrupt_enable.Bit(0)) { priority = 0; interrupt_src -= 1; }
						else if (interrupt_src.Bit(1) && interrupt_enable.Bit(1)) { priority = 1; interrupt_src -= 2; }
						else if (interrupt_src.Bit(2) && interrupt_enable.Bit(2)) { priority = 2; interrupt_src -= 4; }
						else if (interrupt_src.Bit(3) && interrupt_enable.Bit(3)) { priority = 3; interrupt_src -= 8; }
						else if (interrupt_src.Bit(4) && interrupt_enable.Bit(4)) { priority = 4; interrupt_src -= 16; }
						else { /*Console.WriteLine("No source"); }*/throw new Exception("Interrupt without Source"); }

						if ((interrupt_src & interrupt_enable) == 0) { FlagI = false; }
						instr_pntr = 0;
						INTERRUPT_(priority);
					}
					else if (FlagI)
					{
						// even if interrupt servicing is disabled, any interrupt flag raised still resumes execution
						if (TraceCallback != null)
						{
							TraceCallback(new TraceInfo
							{
								Disassembly = "====un-halted====",
								RegisterInfo = ""
							});
						}
						halted = false;
						if (OnExecFetch != null) OnExecFetch(RegPC);
						if (TraceCallback != null) TraceCallback(State());
						FetchInstruction(ReadMemory(RegPC++));
						instr_pntr = 0;
					}
					else
					{
						instr_pntr = 0;
						cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						HALT };
					}
					break;
				case STOP:
					stopped = true;

					if (interrupt_src.Bit(4)) // button pressed, not actually an interrupt though
					{
						if (TraceCallback != null)
						{
							TraceCallback(new TraceInfo
							{
								Disassembly = "====un-stop====",
								RegisterInfo = ""
							});
						}

						stopped = false;
						if (OnExecFetch != null) OnExecFetch(RegPC);
						if (TraceCallback != null) TraceCallback(State());
						FetchInstruction(ReadMemory(RegPC++));
						instr_pntr = 0;
					}
					else
					{
						instr_pntr = 0;
						cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						STOP };
					}
					break;
				case ASGN:
					ASGN_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case ADDS:
					ADDS_Func(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case OP_G:
					if (OnExecFetch != null) OnExecFetch(RegPC);
					if (TraceCallback != null) TraceCallback(State());

					FetchInstruction(ReadMemory(RegPC)); // note no increment

					instr_pntr = 0;
					break;
				case JAM:
					jammed = true;
					instr_pntr--;
					break;
				case RD_F:
					Read_Func_F(cur_instr[instr_pntr++], cur_instr[instr_pntr++], cur_instr[instr_pntr++]);
					break;
				case EI_RETI:
					EI_pending = 1;
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
			ser.Sync("IRQ", ref interrupts_enabled);
			ser.Sync("NMI", ref nonMaskableInterrupt);
			ser.Sync("NMIPending", ref nonMaskableInterruptPending);
			ser.Sync("IM", ref interruptMode);
			ser.Sync("IFF1", ref iff1);
			ser.Sync("IFF2", ref iff2);
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