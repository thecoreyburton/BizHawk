using System;

namespace BizHawk.Emulation.Common.Cores.MC6800
{
	public partial class MC6800
	{
		public bool NMI;
		public bool NMIPending;
		public bool IRQ;
		public bool IRQPending;

		private void INTERRUPT_()
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
						ASGN, Z, 0xF8,
						ASGN, W, 0xFF,
						RD, PCl, Z, W,
						INC16, Z, W,
						RD, PCh, Z, W,
						OP };
		}

		private void NMI_()
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
						ASGN, Z, 0xFC,
						ASGN, W, 0xFF,
						RD, PCl, Z, W,
						INC16, Z, W,
						RD, PCh, Z, W,
						OP };
		}

		private void INTERRUPT_FAST()
		{
			cur_instr = new ushort[]
						{ASGN, Z, 0xF8,
						ASGN, W, 0xFF,
						RD, PCl, Z, W,
						INC16, Z, W,
						RD, PCh, Z, W,
						OP };
		}

		private void NMI_FAST()
		{
			cur_instr = new ushort[]
						{ASGN, Z, 0xFC,
						ASGN, W, 0xFF,
						RD, PCl, Z, W,
						INC16, Z, W,
						RD, PCh, Z, W,
						OP };
		}

		private static ushort[] INT_vectors = new ushort[] {0x40, 0x48, 0x50, 0x58, 0x60};

		private void ResetInterrupts()
		{
			NMI = false;
			NMIPending = false;
			IRQ = false;
			IRQPending = false;
		}
	}
}