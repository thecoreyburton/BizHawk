using System;
using System.Collections.Generic;
using System.Linq;

using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	public partial class MP1000
	{
		private IMemoryDomains MemoryDomains;

		public void SetupMemoryDomains()
		{
			var domains = new List<MemoryDomain>
			{
				new MemoryDomainDelegate(
					"Main RAM_LO",
					RAM_LO.Length,
					MemoryDomain.Endian.Little,
					addr => RAM_LO[addr],
					(addr, value) => RAM_LO[addr] = value,
					1),
				new MemoryDomainDelegate(
					"TMain RAM_HI",
					RAM_HI.Length,
					MemoryDomain.Endian.Little,
					addr => RAM_HI[addr],
					(addr, value) => RAM_HI[addr] = value,
					1),
				new MemoryDomainDelegate(
					"Maria Registers",
					Maria_regs.Length,
					MemoryDomain.Endian.Little,
					addr => Maria_regs[addr],
					(addr, value) => Maria_regs[addr] = value,
					1),
				new MemoryDomainDelegate(
					"6532 RAM",
					RAM_6532.Length,
					MemoryDomain.Endian.Little,
					addr => RAM_6532[addr],
					(addr, value) => RAM_6532[addr] = value,
					1),
				new MemoryDomainDelegate(
					"System Bus",
					0X10000,
					MemoryDomain.Endian.Little,
					addr => PeekSystemBus(addr),
					(addr, value) => PokeSystemBus(addr, value),
					1)
			};

			MemoryDomains = new MemoryDomainList(domains);
			(ServiceProvider as BasicServiceProvider).Register<IMemoryDomains>(MemoryDomains);
		}

		private byte PeekSystemBus(long addr)
		{
			ushort addr2 = (ushort)(addr & 0xFFFF);
			return ReadMemory(addr2);
		}

		private void PokeSystemBus(long addr, byte value)
		{
			ushort addr2 = (ushort)(addr & 0xFFFF);
			WriteMemory(addr2, value);
		}
	}
}
