using System;

namespace BizHawk.Emulation.Common.Cores.MC6800
{
	public partial class MC6800
	{
		// this contains the vectors of instrcution operations
		// NOTE: This list is NOT confirmed accurate for each individual cycle

		#region Implied

		private void JAM_()
		{
			cur_instr = new ushort[]
						{JAM,
						IDLE,
						IDLE,
						IDLE };
		}

		private void NOP_()
		{
			cur_instr = new ushort[]
						{IDLE,
						OP };
		}

		private void INT_OP(ushort operation, ushort src)
		{
			cur_instr = new ushort[]
						{operation, src,
						OP };
		}

		private void REG_OP(ushort operation, ushort dest, ushort src)
		{
			cur_instr = new ushort[]
						{operation, dest, src,
						OP };
		}

		private void BIT_OP(ushort operation, ushort bit, ushort src)
		{
			cur_instr = new ushort[]
						{operation, bit, src,
						OP };
		}

		private void INC_16(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{INC16,  src_l, src_h,
						IDLE,
						IDLE,
						OP };
		}

		private void DEC_16(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{DEC16, src_l, src_h,
						IDLE,
						IDLE,
						OP };
		}

		private void PUSH_(ushort src)
		{
			cur_instr = new ushort[]
						{DEC16, SPl, SPh,
						IDLE,
						WR, SPl, SPh, src,
						OP };
		}

		// NOTE: this is the only instruction that can write to P
		// but the top 2 bits of P are always 1, so instead of putting a special check for every read op
		// let's just put a special operation here specifically for P
		private void POP_(ushort src)
		{
			if (src != P)
			{
				cur_instr = new ushort[]
							{RD, src, SPl, SPh,
							IDLE,
							INC16, SPl, SPh,
							IDLE,
							OP };
			}
			else
			{
				cur_instr = new ushort[]
							{RD_P, src, SPl, SPh,
							IDLE,
							INC16, SPl, SPh,
							IDLE,
							OP };
			}
		}

		private void TR_16_(ushort dest_l, ushort dest_h, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						TR_16, dest_l, dest_h, src_l, src_h,
						IDLE,
						OP };
		}

		private void SWI_()
		{
			cur_instr = new ushort[]
						{IDLE,
						DEC16, SPl, SPh,
						WR, SPl, SPh, B,
						DEC16, SPl, SPh,
						WR, SPl, SPh, A,
						DEC16, SPl, SPh,
						WR, SPl, SPh, Ixh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, Ixl,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCl,
						ASGN, Z, 0xFA,
						ASGN, W, 0xFF,
						RD, PCl, Z, W,
						INC16, Z, W,
						RD, PCh, Z, W,
						OP };
		}

		private void WAI_()
		{
			cur_instr = new ushort[]
						{IDLE,
						DEC16, SPl, SPh,
						WR, SPl, SPh, B,
						DEC16, SPl, SPh,
						WR, SPl, SPh, A,
						DEC16, SPl, SPh,
						WR, SPl, SPh, Ixh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, Ixl,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCl,
						WAI };
		}

		private void RET_()
		{
			cur_instr = new ushort[]
						{RD, PCl, SPl, SPh,
						INC16, SPl, SPh,
						RD, PCh, SPl, SPh,
						INC16, SPl, SPh,
						OP };
		}

		private void RETI_()
		{
			cur_instr = new ushort[]
						{RD, PCl, SPl, SPh,
						INC16, SPl, SPh,
						RD, PCh, SPl, SPh,
						INC16, SPl, SPh,
						RD, Ixl, SPl, SPh,
						INC16, SPl, SPh,
						RD, Ixh, SPl, SPh,
						INC16, SPl, SPh,
						RD, A, SPl, SPh,
						INC16, SPl, SPh,
						RD, B, SPl, SPh,
						INC16, SPl, SPh,
						OP };
		}

		#endregion

		#region Direct

		private void LD_8_ZP(ushort dest_l, ushort dest_h, ushort src)
		{
			cur_instr = new ushort[]
						{IDLE,
						RD, Z, dest_l, dest_h,
						INC16, dest_l, dest_h,
						ASGN, W, 0,
						WR, Z, W, src,
						IDLE,
						OP  };
		}

		private void LD_ZP_8(ushort dest, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						RD, Z, src_l, src_h,
						INC16, src_l, src_h,
						ASGN, W, 0,
						RD, ALU, Z, W,
						TR, dest, ALU,
						IDLE,
						OP };
		}

		private void LD_ZP_16(ushort dest_l, ushort dest_h, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						RD, Z, src_l, src_h,
						IDLE,
						INC16, src_l, src_h,
						IDLE,
						ASGN, W, 0,
						RD, SPl, Z, W,
						INC16, Z, W,
						RD, SPh, Z, W,
						OP };
		}

		private void LD_16_ZP(ushort dest_l, ushort dest_h, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						RD, Z, dest_l, dest_h,
						IDLE,
						INC16, dest_l, dest_h,
						IDLE,
						ASGN, W, 0,
						WR, Z, W, SPl,
						INC16, Z, W,
						WR, Z, W, SPh,
						OP };
		}

		private void CP_16_ZP(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						ASGN, ALU2, 0,
						RD, Z, ALU, ALU2,
						INC16, ALU, ALU2,
						RD, W, ALU, ALU2,
						CP16, Z, W, src_l, src_h,
						IDLE,
						OP };
		}

		private void REG_OP_ZP(ushort operation, ushort dest, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						RD, Z, src_l, src_h,
						INC16, src_l, src_h,
						ASGN, W, 0,
						RD, ALU, Z, W,
						operation, dest, ALU,
						IDLE,
						OP };
		}

		#endregion

		#region Relative

		private void JR_COND(bool cond)
		{
			if (cond)
			{
				cur_instr = new ushort[]
							{RD, ALU, PCl, PCh,
							INC16, PCl, PCh,
							ADDS, PCl, PCh, ALU,
							OP };
			}
			else
			{
				cur_instr = new ushort[]
							{RD, ALU, PCl, PCh,
							INC16, PCl, PCh,
							IDLE,
							OP };
			}
		}

		private void BRS()
		{

			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCl,
						ADDS, PCl, PCh, ALU,
						OP };
		}

		#endregion

		#region Indexed

		private void LD_8_INDX(ushort src)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						TR_16, Z, W, Ixl, Ixh,
						ADDS, Z, W, ALU,
						WR, Z, W, src,
						OP };
		}

		private void LD_INDX_8(ushort dest)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						TR_16, Z, W, Ixl, Ixh,
						ADDS, Z, W, ALU,
						RD, dest, Z, W,
						OP };
		}

		private void INT_OP_INDX(ushort operation, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						TR_16, Z, W, src_l, src_h,
						ADDS, Z, W, ALU,
						RD, ALU, Z, W,
						operation, ALU,
						WR, Z, W, ALU,
						OP };
		}

		private void JP_INDX(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						TR_16, PCl, PCh, src_l, src_h,
						ADDS, PCl, PCh, ALU,
						OP };
		}

		private void REG_OP_INDX(ushort operation, ushort dest)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						TR_16, Z, W, Ixl, Ixh,
						ADDS, Z, W, ALU,
						RD, ALU, Z, W,
						operation, dest, ALU,
						OP };
		}
		
		private void JSR_INDX()
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCl,
						TR_16, PCl, PCh, Ixl, Ixh,
						ADDS, PCl, PCh, ALU,
						OP };
		}

		private void CP_16_INDX()
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						TR_16, Z, W, Ixl, Ixh,
						ADDS, Z, W, ALU,
						RD, ALU2, Z, W,
						INC16, W, Z,
						RD, ALU, Z, W,
						CP16, ALU2, ALU, Ixl, Ixh,
						OP };
		}

		private void LD_INDX_16(ushort dest_l, ushort dest_h)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						TR_16, Z, W, Ixl, Ixh,
						ADDS, Z, W, ALU,
						RD, dest_l, Z, W,
						INC16, W, Z,
						RD, dest_h, Z, W,
						OP };
		}

		private void LD_16_INDX(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						INC16, PCl, PCh,
						TR_16, Z, W, Ixl, Ixh,
						ADDS, Z, W, ALU,
						WR, Z, W, src_l,
						INC16, W, Z,
						WR, Z, W, src_h,
						OP };
		}

		#endregion

		#region Extended

		private void JSR_EXT()
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCh,
						DEC16, SPl, SPh,
						WR, SPl, SPh, PCl,
						TR_16, PCl, PCh, Z, W,
						OP };
		}

		private void LD_8_EXT(ushort src)
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						WR, Z, W, src,
						OP };
		}

		private void LD_EXT_8(ushort dest)
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						RD, dest, Z, W,
						OP };
		}

		private void CP_16_EXT()
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						RD, ALU2, Z, W,
						INC16, W, Z,
						RD, ALU, Z, W,
						CP16, ALU2, ALU, Ixl, Ixh,
						OP };
		}

		private void LD_EXT_16(ushort dest_l, ushort dest_h)
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						RD, dest_l, Z, W,
						INC16, W, Z,
						RD, dest_h, Z, W,
						OP };
		}

		private void LD_16_EXT(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						WR, Z, W, src_l,
						INC16, W, Z,
						WR, Z, W, src_h,
						OP };
		}

		private void REG_OP_EXT(ushort operation, ushort dest)
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						RD, ALU, Z, W,
						operation, dest, ALU,
						IDLE,
						OP };
		}

		private void INT_OP_EXT(ushort operation)
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						RD, ALU, Z, W,
						operation, ALU,
						WR, Z, W, ALU,
						OP };
		}

		private void JP_EXT()
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						TR_16, PCl, PCh, Z, W,
						OP };
		}

		#endregion

		#region Immediate

		private void LD_8_IMM(ushort dest_l, ushort dest_h, ushort src)
		{
			cur_instr = new ushort[]
						{WR, dest_l, dest_h, src,
						INC16, dest_l, dest_h,
						IDLE,
						OP };
		}

		private void LD_IMM_8(ushort dest, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{RD, dest, src_l, src_h,
						INC16, src_l, src_h,
						IDLE,
						OP };
		}

		private void CP_16_IMM(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{RD, Z, PCl, PCh,
						INC16, PCl, PCh,
						RD, W, PCl, PCh,
						INC16, PCl, PCh,
						CP16, Z, W, src_l, src_h,
						IDLE,
						OP };
		}

		private void REG_OP_IMM(ushort operation, ushort dest, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						RD, Z, src_l, src_h,
						IDLE,
						operation, dest, Z,
						INC16, src_l, src_h,
						OP };
		}


		private void LD_IMM_16(ushort dest_l, ushort dest_h, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						RD, dest_l, src_l, src_h,
						IDLE,
						INC16, src_l, src_h,
						IDLE,
						RD, dest_h, src_l, src_h,
						IDLE,
						INC16, src_l, src_h,
						IDLE,
						OP };
		}

		private void LD_16_IMM(ushort dest_l, ushort dest_h, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						WR, dest_l, dest_h, src_l,
						IDLE,
						INC16, dest_l, src_h,
						IDLE,
						WR, dest_l, dest_h, src_h,
						IDLE,
						INC16, dest_l, dest_h,
						IDLE,
						OP };
		}

		#endregion
	}
}
