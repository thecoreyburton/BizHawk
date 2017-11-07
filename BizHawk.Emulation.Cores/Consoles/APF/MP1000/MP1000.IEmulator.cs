using BizHawk.Common.NumberExtensions;
using BizHawk.Emulation.Common;
using System;
using System.Collections.Generic;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	public partial class MP1000 : IEmulator, IVideoProvider, ISoundProvider
	{
		public IEmulatorServiceProvider ServiceProvider { get; }

		public ControllerDefinition ControllerDefinition => _controllerDeck.Definition;

		//Maria related variables
		public int cycle;
		public int cpu_cycle;
		public bool cpu_is_haltable;
		public bool cpu_is_halted;
		public bool cpu_halt_pending;
		public bool cpu_resume_pending;

		// input state of controllers and console
		public byte p1_state;
		public byte p2_state;
		public byte p1_fire;
		public byte p2_fire;
		public byte p1_fire_2x;
		public byte p2_fire_2x;
		public byte con_state;
		public bool left_toggle;
		public bool right_toggle;
		public bool left_was_pressed;
		public bool right_was_pressed;

		public void FrameAdvance(IController controller, bool render, bool rendersound)
		{
			if (_tracer.Enabled)
			{
				cpu.TraceCallback = s => _tracer.Put(s);
			}
			else
			{
				cpu.TraceCallback = null;
			}

			_frame++;

			if (controller.IsPressed("Power"))
			{
				HardReset();
			}

			_islag = true;

			GetControllerState(controller);
			GetConsoleState(controller);
			
			mc6847.RunFrame();

			if (_islag)
			{
				_lagcount++;
			}
		}

		// 4 ppu cycles in  a cpu cycle
		public void RunCPUCycle()
		{
			cpu_cycle++;

			if (cpu_cycle == 4)
			{
				cpu.ExecuteOne();
				cpu_cycle = 0;
			}
		}

		public void GetControllerState(IController controller)
		{
			InputCallbacks.Call();

			p1_state = _controllerDeck.ReadPort1(controller);
			p2_state = _controllerDeck.ReadPort2(controller);
			p1_fire = _controllerDeck.ReadFire1(controller);
			p2_fire = _controllerDeck.ReadFire2(controller);
			p1_fire_2x = _controllerDeck.ReadFire1_2x(controller);
			p2_fire_2x = _controllerDeck.ReadFire2_2x(controller);
		}

		public void GetConsoleState(IController controller)
		{
			byte result = 0;

			if (controller.IsPressed("Toggle Right Difficulty"))
			{
				if (!right_was_pressed)
				{
					right_toggle = !right_toggle;
				}
				right_was_pressed = true;
				result |= (byte)((right_toggle ? 1 : 0) << 7);
			}
			else
			{
				right_was_pressed = false;
				result |= (byte)((right_toggle ? 1 : 0) << 7);
			}

			if (controller.IsPressed("Toggle Left Difficulty"))
			{
				if (!left_was_pressed)
				{
					left_toggle = !left_toggle;
				}
				left_was_pressed = true;
				result |= (byte)((left_toggle ? 1 : 0) << 6);
			}
			else
			{
				left_was_pressed = false;
				result |= (byte)((left_toggle ? 1 : 0) << 6);
			}

			if (!controller.IsPressed("Pause"))
			{
				result |= (1 << 3);
			}
			if (!controller.IsPressed("Select"))
			{
				result |= (1 << 1);
			}
			if (!controller.IsPressed("Reset"))
			{
				result |= 1;
			}

			con_state = result;
		}

		public int Frame => _frame;

		public string SystemId => "A78"; 

		public bool DeterministicEmulation { get; set; }

		public void ResetCounters()
		{
			_frame = 0;
			_lagcount = 0;
			_islag = false;
		}

		public CoreComm CoreComm { get; }

		public void Dispose()
		{
			mc6821 = null;
			mc6847 = null;
		}


		#region Video provider

		public int _frameHz = 60;
		public int _screen_width = 320;
		public int _screen_height = 263;
		public int _vblanklines = 20;

		public int[] _vidbuffer;

		public int[] GetVideoBuffer()
		{
			if (_syncSettings.Filter != "None")
			{
				apply_filter();
			}
			return _vidbuffer;
		}

		public int VirtualWidth => 320;
		public int VirtualHeight => _screen_height - _vblanklines;
		public int BufferWidth => 320;
		public int BufferHeight => _screen_height - _vblanklines;
		public int BackgroundColor => unchecked((int)0xff000000);
		public int VsyncNumerator => _frameHz;
		public int VsyncDenominator => 1;

		public void apply_filter()
		{

		}

		public static Dictionary<string, string> ValidFilterTypes = new Dictionary<string, string>
		{
			{ "None",  "None"},
			{ "NTSC",  "NTSC"},
			{ "Pal",  "Pal"}
		};

		#endregion

		#region Sound provider

		private int _spf;
		
		public bool CanProvideAsync => false;

		public void SetSyncMode(SyncSoundMode mode)
		{
			if (mode != SyncSoundMode.Sync)
			{
				throw new InvalidOperationException("Only Sync mode is supported.");
			}
		}

		public SyncSoundMode SyncMode => SyncSoundMode.Sync;

		public void GetSamplesSync(out short[] samples, out int nsamp)
		{
			short[] ret = new short[_spf * 2];
			
			nsamp = _spf;

			samples = ret;
		}

		public void GetSamplesAsync(short[] samples)
		{
			throw new NotSupportedException("Async is not available");
		}

		public void DiscardSamples()
		{

		}

		#endregion

	}
}
