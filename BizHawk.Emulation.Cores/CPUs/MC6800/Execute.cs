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
				case 0x3E: WAI_();									break; // WAI
				case 0x3F: SWI_();									break; // SWI
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
				case 0x80: REG_OP_IMM(SUB8, A, PCl, PCh);			break; // SUB A, n
				case 0x81: REG_OP_IMM(CP8, A, PCl, PCh);			break; // CMP A, n
				case 0x82: REG_OP_IMM(SBC8, A, PCl, PCh);			break; // SBC A, n
				case 0x83: JAM_();									break; // JAM
				case 0x84: REG_OP_IMM(AND8, A, PCl, PCh);			break; // AND A, n
				case 0x85: REG_OP_IMM(BIT8, A, PCl, PCh);			break; // BIT A, n
				case 0x86: LD_IND_8_INC(A, PCl, PCh);				break; // LD A, n
				case 0x87: LD_8_IND_INC(PCl, PCh, A);				break; // ST A, n
				case 0x88: REG_OP_IMM(XOR8, A, PCl, PCh);			break; // XOR A, n
				case 0x89: REG_OP_IMM(ADC8, A, PCl, PCh);			break; // ADC A, n
				case 0x8A: REG_OP_IMM(OR8, A, PCl, PCh);			break; // OR A, n
				case 0x8B: REG_OP_IMM(ADD8, A, PCl, PCh);			break; // ADD A, n
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
				case 0xA0: I_REG_OP(SUB8, A);						break; // SUB A, (Ix + n)
				case 0xA1: I_REG_OP(CP8, A);						break; // CMP A, (Ix + n)
				case 0xA2: I_REG_OP(SBC8, A);						break; // SBC A, (Ix + n)
				case 0xA3: JAM_();									break; // JAM
				case 0xA4: I_REG_OP(AND8, A);						break; // AND A, (Ix + n)
				case 0xA5: I_REG_OP(BIT8, A);						break; // BIT A, (Ix + n)
				case 0xA6: LD_I_8(A);								break; // LD A, (Ix + n)
				case 0xA7: LD_8_I(A);								break; // ST A, (Ix + n)
				case 0xA8: I_REG_OP(XOR8, A);						break; // XOR A, (Ix + n)
				case 0xA9: I_REG_OP(ADC8, A);						break; // ADC A, (Ix + n)
				case 0xAA: I_REG_OP(OR8, A);						break; // OR A, (Ix + n)
				case 0xAB: I_REG_OP(ADD8, A);						break; // ADD A, (Ix + n)
				case 0xAC: CP_16_I();								break; // CMP Ix, (Ix + n)
				case 0xAD: I_JSR();									break; // JSR 
				case 0xAE: LD_I_16(SPl, SPh);						break; // LD SP, (Ix + n)
				case 0xAF: LD_16_I(SPl, SPh);						break; // LD (Ix + n), SP
				case 0xB0: REG_OP_EXT(SUB8, A);						break; // SUB A, (nn)
				case 0xB1: REG_OP_EXT(CP8, A);						break; // CMP A, (nn)
				case 0xB2: REG_OP_EXT(SBC8, A);						break; // SBC A, (nn)
				case 0xB3: JAM_();									break; // JAM
				case 0xB4: REG_OP_EXT(AND8, A);						break; // AND A, (nn)
				case 0xB5: REG_OP_EXT(BIT8, A);						break; // BIT A, (nn)
				case 0xB6: LD_8_EXT(A);								break; // LD A, (nn)
				case 0xB7: LD_8_EXT(A);								break; // ST A, (nn)
				case 0xB8: REG_OP_EXT(XOR8, A);						break; // XOR A, (nn)
				case 0xB9: REG_OP_EXT(ADC8, A);						break; // ADC A, (nn)
				case 0xBA: REG_OP_EXT(OR8, A);						break; // OR A, (nn)
				case 0xBB: REG_OP_EXT(ADD8, A);						break; // ADD A, (nn)
				case 0xBC: CP_16_EXT();								break; // CMP (nn), Ix
				case 0xBD: JSR_EXT();								break; // JSR (nn)
				case 0xBE: LD_EXT_16(SPl, SPh);						break; // LD SP, (nn)
				case 0xBF: LD_16_EXT(SPl, SPh);						break; // LD (nn), SP
				case 0xC0: REG_OP_IMM(SUB8, B, PCl, PCh);			break; // SUB B, n
				case 0xC1: REG_OP_IMM(CP8, B, PCl, PCh);			break; // CMP B, n
				case 0xC2: REG_OP_IMM(SBC8, B, PCl, PCh);			break; // SBC B, n
				case 0xC3: JAM_();									break; // JAM
				case 0xC4: REG_OP_IMM(AND8, B, PCl, PCh);			break; // AND B, n
				case 0xC5: REG_OP_IMM(BIT8, B, PCl, PCh);			break; // BIT B, n
				case 0xC6: LD_IND_8_INC(B, PCl, PCh);				break; // LD B, n
				case 0xC7: LD_8_IND_INC(PCl, PCh, B);				break; // ST B, n
				case 0xC8: REG_OP_IMM(XOR8, B, PCl, PCh);			break; // XOR B, n
				case 0xC9: REG_OP_IMM(ADC8, B, PCl, PCh);			break; // ADC B, n
				case 0xCA: REG_OP_IMM(OR8, B, PCl, PCh);			break; // OR B, n
				case 0xCB: REG_OP_IMM(ADD8, B, PCl, PCh);			break; // ADD B, n
				case 0xCC: JAM_();									break; // JAM
				case 0xCD: JAM_();									break; // JAM
				case 0xCE: LD_IMM_16(Ixl, Ixh, PCl, PCh);			break; // LD Ix, nn
				case 0xCF: LD_16_IMM(PCl, PCh, Ixl, Ixh);			break; // LD nn, Ix
				case 0xD0: REG_OP_ZP(SUB8, B, PCl, PCh);			break; // SUB B, zp(n)
				case 0xD1: REG_OP_ZP(CP8, B, PCl, PCh);				break; // CMP B, zp(n)
				case 0xD2: REG_OP_ZP(SBC8, B, PCl, PCh);			break; // SBC B, zp(n)
				case 0xD3: JAM_();									break; // JAM
				case 0xD4: REG_OP_ZP(AND8, B, PCl, PCh);			break; // AND B, zp(n)
				case 0xD5: REG_OP_ZP(BIT8, B, PCl, PCh);			break; // BIT B, zp(n)
				case 0xD6: LD_IND_8_ZP(A, PCl, PCh);				break; // LD B, zp(n)
				case 0xD7: LD_8_IND_ZP(PCl, PCh, B);				break; // ST B, zp(n)
				case 0xD8: REG_OP_ZP(XOR8, B, PCl, PCh);			break; // XOR B, zp(n)
				case 0xD9: REG_OP_ZP(ADC8, B, PCl, PCh);			break; // ADC B, zp(n)
				case 0xDA: REG_OP_ZP(OR8, B, PCl, PCh);				break; // OR B, zp(n)
				case 0xDB: REG_OP_ZP(ADD8, B, PCl, PCh);			break; // ADD B, zp(n)
				case 0xDC: JAM_();									break; // JAM
				case 0xDD: JAM_();									break; // JAM
				case 0xDE: LD_ZP_16(Ixl, Ixh, PCl, PCh);			break; // LD Ix, zp(n)
				case 0xDF: LD_16_ZP(PCl, PCh, Ixl, Ixh);			break; // LD zp(n), Ix
				case 0xE0: I_REG_OP(SUB8, B);						break; // SUB B, (Ix + n)
				case 0xE1: I_REG_OP(CP8, B);						break; // CMP B, (Ix + n)
				case 0xE2: I_REG_OP(SBC8, B);						break; // SBC B, (Ix + n)
				case 0xE3: JAM_();									break; // JAM
				case 0xE4: I_REG_OP(AND8, B);						break; // AND B, (Ix + n)
				case 0xE5: I_REG_OP(BIT8, B);						break; // BIT B, (Ix + n)
				case 0xE6: LD_I_8(B);								break; // LD B, (Ix + n)
				case 0xE7: LD_8_I(B);								break; // ST B, (Ix + n)
				case 0xE8: I_REG_OP(XOR8, B);						break; // XOR B, (Ix + n)
				case 0xE9: I_REG_OP(ADC8, B);						break; // ADC B, (Ix + n)
				case 0xEA: I_REG_OP(OR8, B);						break; // OR B, (Ix + n)
				case 0xEB: I_REG_OP(ADD8, B);						break; // ADD B, (Ix + n)
				case 0xEC: JAM_();									break; // JAM
				case 0xED: JAM_();									break; // JAM
				case 0xEE: LD_I_16(Ixl, Ixh);						break; // LD Ix, (Ix + n)
				case 0xEF: LD_16_I(Ixl, Ixh);						break; // LD (Ix + n), Ix
				case 0xF0: REG_OP_EXT(SUB8, B);						break; // SUB B, (nn)
				case 0xF1: REG_OP_EXT(CP8, B);						break; // CMP B, (nn)
				case 0xF2: REG_OP_EXT(SBC8, B);						break; // SBC B, (nn)
				case 0xF3: JAM_();									break; // JAM
				case 0xF4: REG_OP_EXT(AND8, B);						break; // AND B, (nn)
				case 0xF5: REG_OP_EXT(BIT8, B);						break; // BIT B, (nn)
				case 0xF6: LD_8_EXT(B);								break; // LD B, (nn)
				case 0xF7: LD_8_EXT(B);								break; // ST B, (nn)
				case 0xF8: REG_OP_EXT(XOR8, B);						break; // XOR B, (nn)
				case 0xF9: REG_OP_EXT(ADC8, B);						break; // ADC B, (nn)
				case 0xFA: REG_OP_EXT(OR8, B);						break; // OR B, (nn)
				case 0xFB: REG_OP_EXT(ADD8, B);						break; // ADD B, (nn)
				case 0xFC: JAM_();									break; // CMP (nn), Ix
				case 0xFD: JAM_();									break; // JSR (nn)
				case 0xFE: LD_EXT_16(Ixl, Ixh);						break; // LD Ix, (nn)
				case 0xFF: LD_16_EXT(Ixl, Ixh);						break; // LD (nn), Ix
			}
		}
	}
}