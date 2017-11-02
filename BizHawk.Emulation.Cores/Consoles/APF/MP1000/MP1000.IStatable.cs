using System.IO;

using BizHawk.Common;
using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	public partial class MP1000 : IStatable
	{
		public bool BinarySaveStatesPreferred => true;

		public void SaveStateText(TextWriter writer)
		{
			SyncState(new Serializer(writer));
		}

		public void LoadStateText(TextReader reader)
		{
			SyncState(new Serializer(reader));
		}

		public void SaveStateBinary(BinaryWriter bw)
		{
			SyncState(new Serializer(bw));
		}

		public void LoadStateBinary(BinaryReader br)
		{
			SyncState(new Serializer(br));
		}

		public byte[] SaveStateBinary()
		{
			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);
			SaveStateBinary(bw);
			bw.Flush();
			return ms.ToArray();
		}

		private void SyncState(Serializer ser)
		{
			byte[] core = null;
			if (ser.IsWriter)
			{
				var ms = new MemoryStream();
				ms.Close();
				core = ms.ToArray();
			}
			cpu.SyncState(ser);
			mc6847.SyncState(ser);
			mc6821.SyncState(ser);

			ser.BeginSection("MP1000");
			ser.Sync("core", ref core, false);
			ser.Sync("Lag", ref _lagcount);
			ser.Sync("Frame", ref _frame);
			ser.Sync("IsLag", ref _islag);
			_controllerDeck.SyncState(ser);

			ser.Sync("_spf", ref _spf);

			ser.Sync("Maria_regs", ref Maria_regs, false);
			ser.Sync("RAM_LO", ref RAM_LO, false);
			ser.Sync("RAM_HI", ref RAM_HI, false);
			ser.Sync("RAM_6532", ref RAM_6532, false);
			ser.Sync("hs_bios_mem", ref hs_bios_mem, false);

			ser.Sync("cycle", ref cycle);
			ser.Sync("cpu_cycle", ref cpu_cycle);
			ser.Sync("cpu_is_haltable", ref cpu_is_haltable);
			ser.Sync("cpu_is_halted", ref cpu_is_halted);
			ser.Sync("cpu_halt_pending", ref cpu_halt_pending);
			ser.Sync("cpu_resume_pending", ref cpu_resume_pending);

			ser.Sync("slow_access", ref slow_access);
			ser.Sync("slow_access", ref slow_countdown);
			ser.Sync("Cart RAM", ref cart_RAM);
			ser.Sync("left_toggle", ref left_toggle);
			ser.Sync("right_toggle", ref right_toggle);
			ser.Sync("left_was_pressed", ref left_was_pressed);
			ser.Sync("right_was_pressed", ref right_was_pressed);

		ser.EndSection();
		}
	}
}
