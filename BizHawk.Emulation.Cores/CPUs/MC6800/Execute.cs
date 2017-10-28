using System;

namespace BizHawk.Emulation.Common.Cores.MC6800
{
	public partial class MC6800
	{
		private int totalExecutedCycles;
		public int TotalExecutedCycles { get { return totalExecutedCycles; } set { totalExecutedCycles = value; } }

		private int EI_pending;
		private bool interrupts_enabled;

		// variables for executing instructions
		public int instr_pntr = 0;
		public ushort[] cur_instr;
		public int opcode;
		public bool CB_prefix;
		public bool halted;
		public bool stopped;
		public bool jammed;
		public int LY;

		public void FetchInstruction(byte opcode)
		{
			if (!CB_prefix)
			{
				switch (opcode)
				{
					case 0x00: JAM_();									break; // JAM
					case 0x01: NOP_();									break; // NOP
					case 0x02: JAM_();									break; // JAM
					case 0x03: JAM_();									break; // JAM
					case 0x04: JAM_();									break; // JAM
					case 0x05: JAM_();									break; // JAM
					case 0x06: REG_OP(TR, P, A);						break; // TAP
					case 0x07: REG_OP(TR, A, P);						break; // TPA
					case 0x08: INC_16(Ixl, Ixl);						break; // INX
					case 0x09: DEC_16(Ixl, Ixl);						break; // DEX
					case 0x0A: BIT_OP(RES, 1, P);						break; // CLV
					case 0x0B: BIT_OP(SET, 1, P);						break; // SEV
					case 0x0C: BIT_OP(RES, 0, P);						break; // CLC
					case 0x0D: BIT_OP(SET, 0, P);						break; // SEC
					case 0x0E: BIT_OP(RES, 4, P);						break; // CLI
					case 0x0F: BIT_OP(SET, 4, P);						break; // SEI
					case 0x10: REG_OP(SUB8, A, B);						break; // SBA
					case 0x11: REG_OP(CP8, A, B);						break; // CBA
					case 0x12: JAM_();									break; // JAM
					case 0x13: JAM_();									break; // JAM
					case 0x14: REG_OP(AND8, A, B);						break; // NBA
					case 0x15: JAM_();									break; // JAM
					case 0x16: REG_OP(TR, B, A);						break; // TAB
					case 0x17: REG_OP(TR, A, B);						break; // TBA
					case 0x18: JAM_();									break; // JAM
					case 0x19: INT_OP(DA, A);							break; // DAA
					case 0x1A: JAM_();									break; // JAM
					case 0x1B: REG_OP(ADD8, A, B);						break; // ABA
					case 0x1C: JAM_();									break; // JAM
					case 0x1D: JAM_();									break; // JAM
					case 0x1E: JAM_();									break; // JAM
					case 0x1F: JAM_();									break; // JAM
					case 0x20: JR_COND(true);							break; // BRA
					case 0x21: JAM_();									break; // JAM
					case 0x22: JR_COND((FlagC | FlagZ) == false);		break; // BHI
					case 0x23: JR_COND((FlagC & FlagZ) == true);		break; // BLS
					case 0x24: JR_COND(!FlagC);							break; // BCC
					case 0x25: JR_COND(FlagC);							break; // BCS
					case 0x26: JR_COND(!FlagZ);							break; // BNE
					case 0x27: JR_COND(FlagZ);							break; // BEQ
					case 0x28: JR_COND(!FlagV);							break; // BVC
					case 0x29: JR_COND(FlagV);							break; // BVS
					case 0x2A: JR_COND(!FlagN);							break; // BPL
					case 0x2B: JR_COND(FlagN);							break; // BMI
					case 0x2C: JR_COND(FlagN == FlagV);					break; // BGE
					case 0x2D: JR_COND(FlagN != FlagV);					break; // BLT
					case 0x2E: JR_COND(!FlagZ && (FlagN == FlagV));		break; // BGT
					case 0x2F: JR_COND(FlagZ && (FlagN != FlagV));		break; // BLE
					case 0x30: TR_16_(Ixl, Ixh, SPl, SPh);				break; // TSX
					case 0x31: INC_16(SPl, SPh);						break; // INS
					case 0x32: LD_8_IND_DEC(A, SPl, SPh);				break; // PUL A
					case 0x33: LD_8_IND_DEC(B, SPl, SPh);				break; // PUL B
					case 0x34: DEC_16(SPl, SPh);						break; // DES
					case 0x35: TR_16_(SPl, SPh, Ixl, Ixh);				break; // TXS
					case 0x36: LD_IND_8_INC(A, SPl, SPh);				break; // PSH A
					case 0x37: LD_IND_8_INC(B, SPl, SPh);				break; // PSH B
					case 0x38: JAM_();									break; // JAM
					case 0x39: RET_();									break; // RTS
					case 0x3A: JAM_();									break; // JAM
					case 0x3B: RETI_();									break; // RTI
					case 0x3C: JAM_();									break; // JAM
					case 0x3D: JAM_();									break; // JAM
					case 0x3E: HALT_();									break; // WAI
					case 0x3F: STOP_();									break; // SWI
					case 0x40: INT_OP(NEG8, A);							break; // NEG A
					case 0x41: JAM_();									break; // JAM
					case 0x42: JAM_();									break; // JAM
					case 0x43: INT_OP(CPL, A);							break; // COM A
					case 0x44: INT_OP(LSR, A);							break; // LSR A
					case 0x45: JAM_();									break; // JAM
					case 0x46: INT_OP(ROR, A);							break; // ROR A
					case 0x47: INT_OP(ASR, A);							break; // ASR A
					case 0x48: INT_OP(ASL, A);							break; // ASL A
					case 0x49: INT_OP(ROL, A);							break; // ROL A
					case 0x4A: INT_OP(DEC8, A);							break; // DEC A
					case 0x4B: JAM_();									break; // JAM
					case 0x4C: INT_OP(INC8, A);							break; // INC A
					case 0x4D: INT_OP(TST, A);							break; // TST A
					case 0x4E: JAM_();									break; // JAM
					case 0x4F: INT_OP(CLR, A);							break; // CLR A
					case 0x50: INT_OP(NEG8, B);							break; // NEG B
					case 0x51: JAM_();									break; // JAM
					case 0x52: JAM_();									break; // JAM
					case 0x53: INT_OP(CPL, B);							break; // COM B
					case 0x54: INT_OP(LSR, B);							break; // LSR B
					case 0x55: JAM_();									break; // JAM
					case 0x56: INT_OP(ROR, B);							break; // ROR B
					case 0x57: INT_OP(ASR, B);							break; // ASR B
					case 0x58: INT_OP(ASL, B);							break; // ASL B
					case 0x59: INT_OP(ROL, B);							break; // ROL B
					case 0x5A: INT_OP(DEC8, B);							break; // DEC B
					case 0x5B: JAM_();									break; // JAM
					case 0x5C: INT_OP(INC8, B);							break; // INC B
					case 0x5D: INT_OP(TST, B);							break; // TST B
					case 0x5E: JAM_();									break; // JAM
					case 0x5F: INT_OP(CLR, B);							break; // CLR B
					case 0x60: REG_OP(TR, H, B);						break; // LD H, B
					case 0x61: REG_OP(TR, H, C);						break; // LD H, C
					case 0x62: REG_OP(TR, H, D);						break; // LD H, D
					case 0x63: REG_OP(TR, H, E);						break; // LD H, E
					case 0x64: REG_OP(TR, H, H);						break; // LD H, H
					case 0x65: REG_OP(TR, H, L);						break; // LD H, L
					case 0x66: REG_OP_IND(TR, H, L, H);					break; // LD H, (HL)
					case 0x67: REG_OP(TR, H, A);						break; // LD H, A
					case 0x68: REG_OP(TR, L, B);						break; // LD L, B
					case 0x69: REG_OP(TR, L, C);						break; // LD L, C
					case 0x6A: REG_OP(TR, L, D);						break; // LD L, D
					case 0x6B: REG_OP(TR, L, E);						break; // LD L, E
					case 0x6C: REG_OP(TR, L, H);						break; // LD L, H
					case 0x6D: REG_OP(TR, L, L);						break; // LD L, L
					case 0x6E: REG_OP_IND(TR, L, L, H);					break; // LD L, (HL)
					case 0x6F: REG_OP(TR, L, A);						break; // LD L, A
					case 0x70: LD_8_IND(L, H, B);						break; // LD (HL), B
					case 0x71: LD_8_IND(L, H, C);						break; // LD (HL), C
					case 0x72: LD_8_IND(L, H, D);						break; // LD (HL), D
					case 0x73: LD_8_IND(L, H, E);						break; // LD (HL), E
					case 0x74: LD_8_IND(L, H, H);						break; // LD (HL), H
					case 0x75: LD_8_IND(L, H, L);						break; // LD (HL), L
					case 0x76: HALT_();									break; // HALT
					case 0x77: LD_8_IND(L, H, A);						break; // LD (HL), A
					case 0x78: REG_OP(TR, A, B);						break; // LD A, B
					case 0x79: REG_OP(TR, A, C);						break; // LD A, C
					case 0x7A: REG_OP(TR, A, D);						break; // LD A, D
					case 0x7B: REG_OP(TR, A, E);						break; // LD A, E
					case 0x7C: REG_OP(TR, A, H);						break; // LD A, H
					case 0x7D: REG_OP(TR, A, L);						break; // LD A, L
					case 0x7E: REG_OP_IND(TR, A, L, H);					break; // LD A, (HL)
					case 0x7F: REG_OP(TR, A, A);						break; // LD A, A
					case 0x80: REG_OP(ADD8, A, B);						break; // ADD A, B
					case 0x81: REG_OP(ADD8, A, C);						break; // ADD A, C
					case 0x82: REG_OP(ADD8, A, D);						break; // ADD A, D
					case 0x83: REG_OP(ADD8, A, E);						break; // ADD A, E
					case 0x84: REG_OP(ADD8, A, H);						break; // ADD A, H
					case 0x85: REG_OP(ADD8, A, L);						break; // ADD A, L
					case 0x86: REG_OP_IND(ADD8, A, L, H);				break; // ADD A, (HL)
					case 0x87: REG_OP(ADD8, A, A);						break; // ADD A, A
					case 0x88: REG_OP(ADC8, A, B);						break; // ADC A, B
					case 0x89: REG_OP(ADC8, A, C);						break; // ADC A, C
					case 0x8A: REG_OP(ADC8, A, D);						break; // ADC A, D
					case 0x8B: REG_OP(ADC8, A, E);						break; // ADC A, E
					case 0x8C: REG_OP(ADC8, A, H);						break; // ADC A, H
					case 0x8D: REG_OP(ADC8, A, L);						break; // ADC A, L
					case 0x8E: REG_OP_IND(ADC8, A, L, H);				break; // ADC A, (HL)
					case 0x8F: REG_OP(ADC8, A, A);						break; // ADC A, A
					case 0x90: REG_OP(SUB8, A, B);						break; // SUB A, B
					case 0x91: REG_OP(SUB8, A, C);						break; // SUB A, C
					case 0x92: REG_OP(SUB8, A, D);						break; // SUB A, D
					case 0x93: REG_OP(SUB8, A, E);						break; // SUB A, E
					case 0x94: REG_OP(SUB8, A, H);						break; // SUB A, H
					case 0x95: REG_OP(SUB8, A, L);						break; // SUB A, L
					case 0x96: REG_OP_IND(SUB8, A, L, H);				break; // SUB A, (HL)
					case 0x97: REG_OP(SUB8, A, A);						break; // SUB A, A
					case 0x98: REG_OP(SBC8, A, B);						break; // SBC A, B
					case 0x99: REG_OP(SBC8, A, C);						break; // SBC A, C
					case 0x9A: REG_OP(SBC8, A, D);						break; // SBC A, D
					case 0x9B: REG_OP(SBC8, A, E);						break; // SBC A, E
					case 0x9C: REG_OP(SBC8, A, H);						break; // SBC A, H
					case 0x9D: REG_OP(SBC8, A, L);						break; // SBC A, L
					case 0x9E: REG_OP_IND(SBC8, A, L, H);				break; // SBC A, (HL)
					case 0x9F: REG_OP(SBC8, A, A);						break; // SBC A, A
					case 0xA0: REG_OP(AND8, A, B);						break; // AND A, B
					case 0xA1: REG_OP(AND8, A, C);						break; // AND A, C
					case 0xA2: REG_OP(AND8, A, D);						break; // AND A, D
					case 0xA3: REG_OP(AND8, A, E);						break; // AND A, E
					case 0xA4: REG_OP(AND8, A, H);						break; // AND A, H
					case 0xA5: REG_OP(AND8, A, L);						break; // AND A, L
					case 0xA6: REG_OP_IND(AND8, A, L, H);				break; // AND A, (HL)
					case 0xA7: REG_OP(AND8, A, A);						break; // AND A, A
					case 0xA8: REG_OP(XOR8, A, B);						break; // XOR A, B
					case 0xA9: REG_OP(XOR8, A, C);						break; // XOR A, C
					case 0xAA: REG_OP(XOR8, A, D);						break; // XOR A, D
					case 0xAB: REG_OP(XOR8, A, E);						break; // XOR A, E
					case 0xAC: REG_OP(XOR8, A, H);						break; // XOR A, H
					case 0xAD: REG_OP(XOR8, A, L);						break; // XOR A, L
					case 0xAE: REG_OP_IND(XOR8, A, L, H);				break; // XOR A, (HL)
					case 0xAF: REG_OP(XOR8, A, A);						break; // XOR A, A
					case 0xB0: REG_OP(OR8, A, B);						break; // OR A, B
					case 0xB1: REG_OP(OR8, A, C);						break; // OR A, C
					case 0xB2: REG_OP(OR8, A, D);						break; // OR A, D
					case 0xB3: REG_OP(OR8, A, E);						break; // OR A, E
					case 0xB4: REG_OP(OR8, A, H);						break; // OR A, H
					case 0xB5: REG_OP(OR8, A, L);						break; // OR A, L
					case 0xB6: REG_OP_IND(OR8, A, L, H);				break; // OR A, (HL)
					case 0xB7: REG_OP(OR8, A, A);						break; // OR A, A
					case 0xB8: REG_OP(CP8, A, B);						break; // CP A, B
					case 0xB9: REG_OP(CP8, A, C);						break; // CP A, C
					case 0xBA: REG_OP(CP8, A, D);						break; // CP A, D
					case 0xBB: REG_OP(CP8, A, E);						break; // CP A, E
					case 0xBC: REG_OP(CP8, A, H);						break; // CP A, H
					case 0xBD: REG_OP(CP8, A, L);						break; // CP A, L
					case 0xBE: REG_OP_IND(CP8, A, L, H);				break; // CP A, (HL)
					case 0xBF: REG_OP(CP8, A, A);						break; // CP A, A
					case 0xC0: RET_COND(!FlagZ);						break; // Ret NZ
					case 0xC1: POP_(C, B);								break; // POP BC
					case 0xC2: JP_COND(!FlagZ);							break; // JP NZ
					case 0xC3: JP_COND(true);							break; // JP
					case 0xC4: CALL_COND(!FlagZ);						break; // CALL NZ
					case 0xC5: PUSH_(C, B);								break; // PUSH BC
					case 0xC6: REG_OP_IND_INC(ADD8, A, PCl, PCh);		break; // ADD A, n
					case 0xC7: RST_(0);									break; // RST 0
					case 0xC8: RET_COND(FlagZ);							break; // RET Z
					case 0xC9: RET_();									break; // RET
					case 0xCA: JP_COND(FlagZ);							break; // JP Z
					case 0xCB: PREFIX_();								break; // PREFIX
					case 0xCC: CALL_COND(FlagZ);						break; // CALL Z
					case 0xCD: CALL_COND(true);							break; // CALL
					case 0xCE: REG_OP_IND_INC(ADC8, A, PCl, PCh);		break; // ADC A, n
					case 0xCF: RST_(0x08);								break; // RST 0x08
					case 0xD0: RET_COND(!FlagC);						break; // Ret NC
					case 0xD1: POP_(E, D);								break; // POP DE
					case 0xD2: JP_COND(!FlagC);							break; // JP NC
					case 0xD3: JAM_();									break; // JAM
					case 0xD4: CALL_COND(!FlagC);						break; // CALL NC
					case 0xD5: PUSH_(E, D);								break; // PUSH DE
					case 0xD6: REG_OP_IND_INC(SUB8, A, PCl, PCh);		break; // SUB A, n
					case 0xD7: RST_(0x10);								break; // RST 0x10
					case 0xD8: RET_COND(FlagC);							break; // RET C
					case 0xD9: RETI_();									break; // RETI
					case 0xDA: JP_COND(FlagC);							break; // JP C
					case 0xDB: JAM_();									break; // JAM
					case 0xDC: CALL_COND(FlagC);						break; // CALL C
					case 0xDD: JAM_();									break; // JAM
					case 0xDE: REG_OP_IND_INC(SBC8, A, PCl, PCh);		break; // SBC A, n
					case 0xDF: RST_(0x18);								break; // RST 0x18
					case 0xE0: LD_FF_IND_8(PCl, PCh, A);				break; // LD(n), A
					case 0xE1: POP_(L, H);								break; // POP HL
					case 0xE2: LD_FFC_IND_8(PCl, PCh, A);				break; // LD(C), A
					case 0xE3: JAM_();									break; // JAM
					case 0xE4: JAM_();                                  break; // JAM
					case 0xE5: PUSH_(L, H);								break; // PUSH HL
					case 0xE6: REG_OP_IND_INC(AND8, A, PCl, PCh);		break; // AND A, n
					case 0xE7: RST_(0x20);								break; // RST 0x20
					case 0xE8: ADD_SP();								break; // ADD SP,n
					case 0xE9: JP_HL();									break; // JP (HL)
					case 0xEA: LD_FF_IND_16(PCl, PCh, A);				break; // LD(nn), A
					case 0xEB: JAM_();									break; // JAM
					case 0xEC: JAM_();									break; // JAM
					case 0xED: JAM_();									break; // JAM
					case 0xEE: REG_OP_IND_INC(XOR8, A, PCl, PCh);		break; // XOR A, n
					case 0xEF: RST_(0x28);								break; // RST 0x28
					case 0xF0: LD_8_IND_FF(A, PCl, PCh);				break; // A, LD(n)
					case 0xF1: POP_(F, A);								break; // POP AF
					case 0xF2: LD_8_IND_FFC(A, PCl, PCh);				break; // A, LD(C)
					case 0xF3: DI_();									break; // DI
					case 0xF4: JAM_();									break; // JAM
					case 0xF5: PUSH_(F, A);								break; // PUSH AF
					case 0xF6: REG_OP_IND_INC(OR8, A, PCl, PCh);		break; // OR A, n
					case 0xF7: RST_(0x30);								break; // RST 0x30
					case 0xF8: LD_HL_SPn();								break; // LD HL, SP+n
					case 0xF9: LD_SP_HL();								break; // LD, SP, HL
					case 0xFA: LD_16_IND_FF(A, PCl, PCh);				break; // A, LD(nn)
					case 0xFB: EI_();									break; // EI
					case 0xFC: JAM_();									break; // JAM
					case 0xFD: JAM_();									break; // JAM
					case 0xFE: REG_OP_IND_INC(CP8, A, PCl, PCh);		break; // XOR A, n
					case 0xFF: RST_(0x38);								break; // RST 0x38
				}
			}
		}
	}
}