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
		public bool halted;
		public bool stopped;
		public bool jammed;
		public int LY;

		public void FetchInstruction(byte opcode)
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
				case 0x32: POP_(A);									break; // PUL A
				case 0x33: POP_(B);									break; // PUL B
				case 0x34: DEC_16(SPl, SPh);						break; // DES
				case 0x35: TR_16_(SPl, SPh, Ixl, Ixh);				break; // TXS
				case 0x36: PUSH_(A);								break; // PSH A
				case 0x37: PUSH_(B);								break; // PSH B
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
				case 0x60: I_INT_OP_IND(NEG8, Ixl, Ixh);			break; // NEG (Ix + n)
				case 0x61: JAM_();									break; // JAM
				case 0x62: JAM_();									break; // JAM
				case 0x63: I_INT_OP_IND(CPL, Ixl, Ixh);				break; // COM (Ix + n)
				case 0x64: I_INT_OP_IND(LSR, Ixl, Ixh);				break; // LSR (Ix + n)
				case 0x65: JAM_();									break; // JAM
				case 0x66: I_INT_OP_IND(ROR, Ixl, Ixh);				break; // ROR (Ix + n)
				case 0x67: I_INT_OP_IND(ASR, Ixl, Ixh);				break; // ASR (Ix + n)
				case 0x68: I_INT_OP_IND(ASL, Ixl, Ixh);				break; // ASL (Ix + n)
				case 0x69: I_INT_OP_IND(ROL, Ixl, Ixh);				break; // ROL (Ix + n)
				case 0x6A: I_INT_OP_IND(DEC8, Ixl, Ixh);			break; // DEC (Ix + n)
				case 0x6B: JAM_();									break; // JAM
				case 0x6C: I_INT_OP_IND(INC8, Ixl, Ixh);			break; // INC (Ix + n)
				case 0x6D: I_INT_OP_IND(TST, Ixl, Ixh);				break; // TST (Ix + n)
				case 0x6E: I_JP_IND(Ixl, Ixh);						break; // JMP (Ix + n)
				case 0x6F: I_INT_OP_IND(CLR, Ixl, Ixh);				break; // CLR (Ix + n)
				case 0x70: INT_OP_EXT(NEG8);						break; // NEG (nn)
				case 0x71: JAM_();									break; // JAM
				case 0x72: JAM_();									break; // JAM
				case 0x73: INT_OP_EXT(CPL);							break; // COM (nn)
				case 0x74: INT_OP_EXT(LSR);							break; // LSR (nn)
				case 0x75: JAM_();									break; // JAM
				case 0x76: INT_OP_EXT(ROR);							break; // ROR (nn)
				case 0x77: INT_OP_EXT(ASR);							break; // ASR (nn)
				case 0x78: INT_OP_EXT(ASL);							break; // ASL (nn)
				case 0x79: INT_OP_EXT(ROL);							break; // ROL (nn)
				case 0x7A: INT_OP_EXT(DEC8);						break; // DEC (nn)
				case 0x7B: JAM_();									break; // JAM
				case 0x7C: INT_OP_EXT(INC8);						break; // INC (nn)
				case 0x7D: INT_OP_EXT(TST);							break; // TST (nn)
				case 0x7E: JP_EXT();								break; // JMP (nn)
				case 0x7F: INT_OP_EXT(CLR);							break; // CLR (nn)
				case 0x80: REG_OP_IMM_INC(SUB8, A, PCl, PCh);		break; // SUB A, n
				case 0x81: REG_OP_IMM_INC(CP8, A, PCl, PCh);		break; // CMP A, n
				case 0x82: REG_OP_IMM_INC(SBC8, A, PCl, PCh);		break; // SBC A, n
				case 0x83: JAM_();									break; // JAM
				case 0x84: REG_OP_IMM_INC(AND8, A, PCl, PCh);		break; // AND A, n
				case 0x85: REG_OP_IMM_INC(BIT8, A, PCl, PCh);		break; // BIT A, n
				case 0x86: LD_IND_8_INC(A, PCl, PCh);				break; // LD A, n
				case 0x87: LD_8_IND_INC(PCl, PCh, A);				break; // ST A, n
				case 0x88: REG_OP_IMM_INC(XOR8, A, PCl, PCh);		break; // XOR A, n
				case 0x89: REG_OP_IMM_INC(ADC8, A, PCl, PCh);		break; // ADC A, n
				case 0x8A: REG_OP_IMM_INC(OR8, A, PCl, PCh);		break; // OR A, n
				case 0x8B: REG_OP_IMM_INC(ADD8, A, PCl, PCh);		break; // ADD A, n
				case 0x8C: CP_16_IMM(Ixl, Ixh);						break; // CMP nn, Ix
				case 0x8D: BRS();									break; // BRS
				case 0x8E: LD_IMM_16(SPl, SPh, PCl, PCh);			break; // LD SP, nn
				case 0x8F: LD_16_IMM(PCl, PCh, SPl, SPh);			break; // LD nn, SP
				case 0x90: REG_OP_ZP(SUB8, A, PCl, PCh);			break; // SUB A, zp(n)
				case 0x91: REG_OP_ZP(CP8, A, PCl, PCh);				break; // CMP A, zp(n)
				case 0x92: REG_OP_ZP(SBC8, A, PCl, PCh);			break; // SBC A, zp(n)
				case 0x93: JAM_();									break; // JAM
				case 0x94: REG_OP_ZP(AND8, A, PCl, PCh);			break; // AND A, zp(n)
				case 0x95: REG_OP_ZP(BIT8, A, PCl, PCh);			break; // BIT A, zp(n)
				case 0x96: LD_IND_8_ZP(A, PCl, PCh);				break; // LD A, zp(n)
				case 0x97: LD_8_IND_ZP(PCl, PCh, A);				break; // ST A, zp(n)
				case 0x98: REG_OP_ZP(XOR8, A, PCl, PCh);			break; // XOR A, zp(n)
				case 0x99: REG_OP_ZP(ADC8, A, PCl, PCh);			break; // ADC A, zp(n)
				case 0x9A: REG_OP_ZP(OR8, A, PCl, PCh);				break; // OR A, zp(n)
				case 0x9B: REG_OP_ZP(ADD8, A, PCl, PCh);			break; // ADD A, zp(n)
				case 0x9C: CP_16_ZP(Ixl, Ixh);						break; // CMP nn, Ix
				case 0x9D: JAM_();									break; // JAM
				case 0x9E: LD_ZP_16(SPl, SPh, PCl, PCh);			break; // LD SP, zp(n)
				case 0x9F: LD_16_ZP(PCl, PCh, SPl, SPh);			break; // LD zp(n), SP
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