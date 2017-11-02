using System;
using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	public partial class MP1000 : ISaveRam
	{
		public byte[] CloneSaveRam()
		{
			return (byte[])_hsram.Clone();
		}

		public void StoreSaveRam(byte[] data)
		{
			Buffer.BlockCopy(data, 0, _hsram, 0, data.Length);
		}

		public bool SaveRamModified
		{
			get 
			{
				return false;
			}	
		}
	}
}
