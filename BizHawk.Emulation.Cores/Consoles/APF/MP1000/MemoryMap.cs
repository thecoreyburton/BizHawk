using System;

using BizHawk.Common.BufferExtensions;
using BizHawk.Emulation.Common;

/*
0000 - 03FF = RAM
0400 - 2000 = RAM mirrors
2000 - 2003 = MC6821
2004 - 3FFF = MC6821 mirrors
4000 - 5FFF = BIOS
6000 - 6003 = Expansion Regs (unused here)
6004 - 63FF = Expansion Regs mirror
6400 - 67FF = I/O interface
6800 - 77FF = 4k ROM cart
7800 - 7FFF = ROM expansion
8000 - 9FFF = 8k ROM cart
A000 - BFFF = RAM
C000 - DFFF = RAM expansion
E000 - FFEF = Unused
FFF0 - FFFF = MC6800 vectors
*/

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	public partial class MP1000
	{
		public byte ReadMemory(ushort addr)
		{
			MemoryCallbacks.CallReads(addr);
			
			if (addr < 0x2000)
			{
				return RAM_LO[addr & 0x3FF];
			}
			else if (addr < 0x4000)
			{
				return mc6821.Regs[addr & 3];
			}
			else if (addr < 0x6000)
			{
				return _bios[addr - 0x4000];
			}
			else if (addr < 0x6400)
			{
				return 0xFF;
			}
			else if (addr < 0x6800)
			{
				return 0xFF;
			}
			else if (addr < 0x7800)
			{
				return _rom[addr - 0x6800];
			}
			else if (addr < 0x8000)
			{
				return _rom[addr - 0x7800];
			}
			else if (addr < 0xA000)
			{
				return _rom[addr - 0x8000];
			}
			else if (addr < 0xC000)
			{
				return RAM_HI[addr - 0xA000];
			}
			else if (addr < 0xE000)
			{
				return RAM_HI[addr - 0xC000];
			}
			else if (addr < 0xFFF0)
			{
				return 0xFF;
			}
			else
			{
				return 0xFF;
			}
		}

		public void WriteMemory(ushort addr, byte value)
		{
			MemoryCallbacks.CallWrites(addr);

			if (addr < 0x2000)
			{
				RAM_LO[addr & 0x3FF] = value;
			}
			else if (addr < 0x4000)
			{
				mc6821.Regs[addr & 3] =value;
			}
			else if (addr < 0x6000)
			{
				// Read Only
			}
			else if (addr < 0x6400)
			{
				// Read Only
			}
			else if (addr < 0x6800)
			{
				// Read Only
			}
			else if (addr < 0x7800)
			{
				// Read Only
			}
			else if (addr < 0x8000)
			{
				// Read Only
			}
			else if (addr < 0xA000)
			{
				// Read Only
			}
			else if (addr < 0xC000)
			{
				RAM_HI[addr - 0xA000] = value;
			}
			else if (addr < 0xE000)
			{
				RAM_HI[addr - 0xC000] = value;
			}
			else if (addr < 0xFFF0)
			{
				// Read Only
			}
			else
			{
				// Read Only
			}
		}
	}
}
