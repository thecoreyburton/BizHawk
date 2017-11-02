using System;
using System.Collections.Generic;
using BizHawk.Emulation.Common.Cores.MC6800;
using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	public partial class MP1000 : IDebuggable
	{
		public IDictionary<string, RegisterValue> GetCpuFlagsAndRegisters()
		{
			return new Dictionary<string, RegisterValue>
			{
				["A"] = cpu.Regs[MC6800.A],
				["B"] = cpu.Regs[MC6800.B],
				["Ix"] = (cpu.Regs[MC6800.Ixh] >> 8) | cpu.Regs[MC6800.Ixl],
				["S"] = (cpu.Regs[MC6800.SPh] >> 8) | cpu.Regs[MC6800.SPl],
				["PC"] = (cpu.Regs[MC6800.PCh] >> 8) | cpu.Regs[MC6800.PCl],
				["Flag H"] = cpu.FlagH,
				["Flag I"] = cpu.FlagI,
				["Flag N"] = cpu.FlagZ,
				["Flag Z"] = cpu.FlagZ,
				["Flag V"] = cpu.FlagV,
				["Flag C"] = cpu.FlagC,
				["Flag E"] = cpu.interrupts_enabled
			};
		}

		public void SetCpuRegister(string register, int value)
		{
			switch (register)
			{
				default:
					throw new InvalidOperationException();
				case "A":
					cpu.Regs[MC6800.A] = (byte)value;
					break;
				case "B":
					cpu.Regs[MC6800.B] = (byte)value;
					break;
				case "Ixl":
					cpu.Regs[MC6800.Ixl] = (byte)value;
					break;
				case "Ixh":
					cpu.Regs[MC6800.Ixh] = (byte)value;
					break;
				case "SPl":
					cpu.Regs[MC6800.SPl] = (byte)value;
					break;
				case "SPh":
					cpu.Regs[MC6800.SPh] = (byte)value;
					break;
				case "PC":
					cpu.RegPC = (ushort)value;
					break;
				case "Flag I":
					cpu.FlagI = value > 0;
					break;
			}
		}

		public IMemoryCallbackSystem MemoryCallbacks { get; } = new MemoryCallbackSystem();

		public bool CanStep(StepType type)
		{
			return false;
		}

		[FeatureNotImplemented]
		public void Step(StepType type)
		{
			throw new NotImplementedException();
		}

		public int TotalExecutedCycles
		{
			get { return cpu.TotalExecutedCycles; }
		}
	}
}
