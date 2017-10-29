namespace BizHawk.Emulation.Common.Cores.MC6800
{
	public partial class MC6800
	{
		private void INT_OP_IND(ushort operation, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						RD, Z, src_l, src_h,
						IDLE,
						operation, Z,
						IDLE,
						WR, src_l, src_h, Z,
						IDLE,					
						IDLE,						
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

		private void I_INT_OP_IND(ushort operation, ushort src_l, ushort src_h)
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

		private void I_JP_IND(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{RD, ALU, PCl, PCh,
						TR_16, PCl, PCh, src_l, src_h,
						ADDS, PCl, PCh, ALU,
						OP };
		}

		private void BIT_OP_IND(ushort operation, ushort bit, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,	
						RD, Z, src_l, src_h,
						IDLE,
						operation, bit, Z,
						IDLE,
						WR, src_l, src_h, Z,
						IDLE,
						IDLE,
						IDLE,
						OP };
		}

		private void BIT_TE_IND(ushort operation, ushort bit, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						RD, Z, src_l, src_h,
						IDLE,
						operation, bit, Z,
						IDLE,
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

		private void REG_OP_IND(ushort operation, ushort dest, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						RD, Z, src_l, src_h,
						IDLE,
						operation, dest, Z,
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

		private void LD_8_IND_INC(ushort dest_l, ushort dest_h, ushort src)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						WR, dest_l, dest_h, src,
						IDLE,
						INC16, dest_l, dest_h,
						IDLE,
						OP };
		}

		private void LD_IND_8_INC(ushort dest, ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						RD, dest, src_l, src_h,
						IDLE,
						INC16, src_l, src_h,
						IDLE,
						OP };
		}

		private void LD_8_IND_ZP(ushort dest_l, ushort dest_h, ushort src)
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

		private void LD_IND_8_ZP(ushort dest, ushort src_l, ushort src_h)
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

		private void LD_8_IND(ushort dest_l, ushort dest_h, ushort src)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,
						WR, dest_l, dest_h, src,
						IDLE,
						IDLE,						
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

		private void INC_8_IND(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,					
						RD, Z, src_l, src_h,
						IDLE,
						INC8, Z,
						IDLE,
						WR,  src_l, src_h, Z,
						IDLE,
						IDLE,
						IDLE,
						OP };
		}

		private void DEC_8_IND(ushort src_l, ushort src_h)
		{
			cur_instr = new ushort[]
						{IDLE,
						IDLE,
						IDLE,	
						RD, Z, src_l, src_h,
						IDLE,
						DEC8, Z,
						IDLE,
						WR, src_l, src_h, Z,
						IDLE,
						IDLE,
						IDLE,
						OP };
		}
	}
}
