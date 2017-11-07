using System;
using BizHawk.Emulation.Common;
using BizHawk.Common.NumberExtensions;
using BizHawk.Common;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	// Emulates the MC6847 graphics chip
	public class MC6847
	{
		public MP1000 Core { get; set; }

		public int[] _palette;

		// the graphics chip can directly access memory
		public Func<ushort, byte> ReadMemory;

		public int cycle;
		public int scanline;

		// variables for drawing a pixel
		int color;
		int local_GFX_index;
		int temp_palette;
		int temp_bit_0;
		int temp_bit_1;
		int disp_mode;
		int pixel;

		// each frame contains 263 scanlines
		// each scanline consists of 454 ppu cycles

		public void RunFrame()
		{
			scanline = 0;
			Core.Maria_regs[8] = 0x80; // indicates VBlank state

			// we start off in VBlank for 20 scanlines
			while (scanline < 20)
			{
				Core.RunCPUCycle();
				cycle++;

				if (cycle == 454)
				{
					scanline++;
					cycle = 0;
				}
			}

			Core.Maria_regs[8] = 0; // we have now left VBLank

			for (int i=0; i<454;i++)
			{
				Core.RunCPUCycle();
			}

			scanline++;
			cycle = 0;

			// Now proceed with the remaining scanlines
			// the first one is a pre-render line, since we didn't actually put any data into the buffer yet
			while (scanline < Core._screen_height)
			{				
				
				Core.RunCPUCycle();

				//////////////////////////////////////////////
				// Drawing Start
				//////////////////////////////////////////////

				if (cycle >=133 && cycle < 453  && scanline > 20)
				{
					pixel = cycle - 133;

					disp_mode = Core.Maria_regs[0x1C] & 0x3;

					if (disp_mode == 0)
					{
						// direct read, nothing to do
					}
					else if (disp_mode == 2) // note: 1 is not used
					{
						// there is a trick here to be aware of.
						// the renderer has no concept of objects, as it only has information on each pixel
						// but objects are specified in groups of 8 pixels. 
						// however, since objects can only be placed in 160 resolution
						// we can pick bits based on whether the current pixel is even or odd
						temp_palette = color & 0x10;
						temp_bit_0 = 0;
						temp_bit_1 = 0;

						if (pixel % 2 == 0)
						{
							temp_bit_1 = color & 2;
							temp_bit_0 = (color & 8) >> 3;
						}
						else
						{
							temp_bit_1 = (color & 1) << 1;
							temp_bit_0 = (color & 4) >> 2;
						}

						color = temp_palette + temp_bit_1 + temp_bit_0;
					}
					else
					{
						// same as above, we can use the pixel index to pick the bits out
						if (pixel % 2 == 0)
						{
							color &= 0x1E;
						}
						else
						{
							color = (color & 0x1C) + ((color & 1) << 1);
						}
					}
				}

				//////////////////////////////////////////////
				// Drawing End
				//////////////////////////////////////////////

				cycle++;

				if (cycle == 454)
				{
					scanline++;

					cycle = 0;
				}
			}
		}

		public void Reset()
		{

		}


		public void SyncState(Serializer ser)
		{
			ser.BeginSection("Maria");

			ser.EndSection();
		}
	}
}
