using System.Runtime.InteropServices;
using System;

namespace BizHawk.Emulation.Common.Cores.MC6800
{
	public partial class MC6800
	{
		// registers

		public static ushort PCl = 0;
		public static ushort PCh = 1;
		public static ushort SPl = 2;
		public static ushort SPh = 3;
		public static ushort Ixl = 4;
		public static ushort Ixh = 5;
		public static ushort A = 6;
		public static ushort B = 7;
		public static ushort P = 8;
		public static ushort ALU = 9;
		public static ushort W = 10;
		public static ushort Z = 11;

		public ushort[] Regs = new ushort[16];

		public bool FlagC
		{
			get { return (Regs[8] & 0x01) != 0; }
			set { Regs[8] = (ushort)((Regs[8] & ~0x01) | (value ? 0x01 : 0x00)); }
		}

		public bool FlagV
		{
			get { return (Regs[8] & 0x02) != 0; }
			set { Regs[8] = (ushort)((Regs[8] & ~0x02) | (value ? 0x02 : 0x00)); }
		}

		public bool FlagZ
		{
			get { return (Regs[8] & 0x04) != 0; }
			set { Regs[8] = (ushort)((Regs[8] & ~0x04) | (value ? 0x04 : 0x00)); }
		}

		public bool FlagN
		{
			get { return (Regs[8] & 0x08) != 0; }
			set { Regs[8] = (ushort)((Regs[8] & ~0x08) | (value ? 0x08 : 0x00)); }
		}

		public bool FlagI
		{
			get { return (Regs[8] & 0x10) != 0; }
			set { Regs[8] = (ushort)((Regs[8] & ~0x10) | (value ? 0x10 : 0x00)); }
		}

		public bool FlagH
		{
			get { return (Regs[8] & 0x20) != 0; }
			set { Regs[8] = (ushort)((Regs[8] & ~0x20) | (value ? 0x20 : 0x00)); }
		}

		public ushort RegPC
		{
			get { return (ushort)(Regs[0] | (Regs[1] << 8)); }
			set
			{
				Regs[0] = (ushort)(value & 0xFF);
				Regs[1] = (ushort)((value >> 8) & 0xFF);
			}
		}

		private void ResetRegisters()
		{
			for (int i=0; i < 16; i++)
			{
				Regs[i] = 0;
			}
		}

	}
}