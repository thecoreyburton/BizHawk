using System;

using BizHawk.Common.BufferExtensions;
using BizHawk.Emulation.Common;
using BizHawk.Emulation.Common.Cores.MC6800;
using BizHawk.Common.NumberExtensions;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	[Core(
		"MP1000",
		"",
		isPorted: false,
		isReleased: true)]
	[ServiceNotApplicable(typeof(ISettable<,>), typeof(IDriveLight))]
	public partial class MP1000 : IEmulator, ISaveRam, IDebuggable, IStatable, IInputPollable,
		IRegionable, IBoardInfo, ISettable<MP1000.MP1000Settings, MP1000.MP1000SyncSettings>
	{
		// this register selects between 2600 and 7800 mode in the A7800
		// however, we already have a 2600 emulator so this core will only be loading A7800 games
		// furthermore, the location of the register is in the same place as TIA registers (0x0-0x1F)
		// any writes to this location before the register is 'locked' will go to the register and not the TIA

		// memory domains
		public byte[] Maria_regs = new byte[0x20];
		public byte[] RAM_LO = new byte[0x400];
		public byte[] RAM_HI = new byte[0x1000];
		public byte[] RAM_6532 = new byte[0x80];
		public byte[] hs_bios_mem = new byte[0x800];

		public readonly byte[] _rom;
		public readonly byte[] _hsbios;
		public readonly byte[] _bios;
		public readonly byte[] _hsram = new byte[2048];

		private int _frame = 0;

		public string s_mapper;
		public int cart_RAM = 0;

		private readonly ITraceable _tracer;

		public MC6800 cpu;
		public MC6847 mc6847;
		public MC6821 mc6821;
		public Pokey pokey;

		public MP1000(CoreComm comm, GameInfo game, byte[] rom, string gameDbFn, object settings, object syncSettings)
		{
			var ser = new BasicServiceProvider(this);

			mc6847 = new MC6847();
			mc6821 = new MC6821();
			pokey = new Pokey();

			cpu = new MC6800
			{
				ReadMemory = ReadMemory,
				WriteMemory = WriteMemory,
				PeekMemory = ReadMemory,
				DummyReadMemory = ReadMemory,
				OnExecFetch = ExecFetch
			};

			mc6847 = new MC6847
			{
				ReadMemory = ReadMemory
			};

			CoreComm = comm;

			_settings = (MP1000Settings)settings ?? new MP1000Settings();
			_syncSettings = (MP1000SyncSettings)syncSettings ?? new MP1000SyncSettings();
			_controllerDeck = new MP1000ControllerDeck(_syncSettings.Port1, _syncSettings.Port2);

			byte[] ntscBios = comm.CoreFileProvider.GetFirmware("A78", "Bios_NTSC", false, "The game will not run if the correct region BIOS is not available.");

			byte[] header = new byte[128];
			bool is_header = false;

			if (rom.Length % 1024 == 128)
			{
				Console.WriteLine("128 byte header detected");
				byte[] newrom = new byte[rom.Length - 128];
				is_header = true;
				Buffer.BlockCopy(rom, 0, header, 0, 128);
				Buffer.BlockCopy(rom, 128, newrom, 0, newrom.Length);
				rom = newrom;
			}

			// look up hash in gamedb to see what mapper to use
			// if none found default is zero
			// also check for PAL region
			string hash_md5 = null;
			s_mapper = null;
			hash_md5 = "md5:" + rom.HashMD5(0, rom.Length);

			var gi = Database.CheckDatabase(hash_md5);

			if (gi != null)
			{
				var dict = gi.GetOptionsDict();

				if (dict.ContainsKey("board"))
				{
					s_mapper = dict["board"];
				}
				else
				{
					throw new Exception("No Board selected for this game");
				}

				// check if the game uses pokey or RAM
				if (dict.ContainsKey("RAM"))
				{
					int.TryParse(dict["RAM"], out cart_RAM);
					Console.WriteLine(cart_RAM);
				}
			}
			else if (is_header)
			{
				Console.WriteLine("ROM not in DB, inferring mapper info from header");

				byte cart_1 = header[0x35];
				byte cart_2 = header[0x36];

				if (cart_2.Bit(1))
				{
					if (cart_2.Bit(3))
					{
						s_mapper = "2";
					}
					else
					{
						s_mapper = "1";
					}					
				}
				else
				{
					s_mapper = "0";
				}
			}
			else
			{
				throw new Exception("ROM not in gamedb and has no header");
			}

			_rom = rom;

			if (_bios == null)
			{
				throw new MissingFirmwareException("The BIOS corresponding to the region of the game you loaded is required to run Atari 7800 games.");
			}

			mc6847.Core = this;
			mc6821.Core = this;
			pokey.Core = this;

			ser.Register<IVideoProvider>(this);
			ser.Register<ISoundProvider>(this);
			ServiceProvider = ser;

			_tracer = new TraceBuffer { Header = cpu.TraceHeader };
			ser.Register<ITraceable>(_tracer);

			SetupMemoryDomains();
			ser.Register<IDisassemblable>(cpu);
			HardReset();
		}

		public string BoardName => _rom.Length > 0x4000 ? "8K" : "4K";

		public DisplayType Region => DisplayType.NTSC;

		private readonly MP1000ControllerDeck _controllerDeck;

		private void HardReset()
		{
			cpu.Reset();
			cpu.SetCallbacks(ReadMemory, ReadMemory, ReadMemory, WriteMemory);

			mc6847.Reset();
			mc6821.Reset();
			pokey.Reset();
			
			Maria_regs = new byte[0x20];
			RAM_LO = new byte[0x1000];

			cpu_cycle = 0;

			_vidbuffer = new int[VirtualWidth * VirtualHeight];

			_spf = (_frameHz > 55) ? 740 : 880;
		}

		private void ExecFetch(ushort addr)
		{
			MemoryCallbacks.CallExecutes(addr);
		}

		public static readonly int[] NTSCPalette =
		{
			0x000000, 0x2e2e2e, 0x3c3c3c, 0x595959,
			0x777777, 0x838383, 0xa0a0a0, 0xb7b7b7,
			0xcdcdcd, 0xd8d8d8, 0xdddddd, 0xe0e0e0,
			0xeaeaea, 0xf0f0f0, 0xf6f6f6, 0xffffff,

			0x412000, 0x542800, 0x763706, 0x984f0f,
			0xbb6818, 0xd78016, 0xff911d, 0xffab1d,
			0xffc51d, 0xffd03b, 0xffd84c, 0xffe651,
			0xfff456, 0xfff977, 0xffff98, 0xffffab,

			0x451904, 0x721e11, 0x9f241e, 0xb33a20,
			0xc85122, 0xe36920, 0xfc811e, 0xff8c25,
			0xff982c, 0xffae38, 0xffc455, 0xffc559,
			0xffc66d, 0xffd587, 0xffe4a1, 0xffe6ab,

			0x5f1f0e, 0x7a240d, 0x9c2c0f, 0xb02f0e,
			0xbf3624, 0xd34e2a, 0xe7623e, 0xf36e4a,
			0xfd7854, 0xff8a6a, 0xff987c, 0xffa48b,
			0xffb39e, 0xffc2b2, 0xffd0c3, 0xffdad0,

			0x4a1704, 0x7e1a0d, 0xb21d17, 0xc82119,
			0xdf251c, 0xec3b38, 0xfa5255, 0xfc6161,
			0xff7063, 0xff7f7e, 0xff8f8f, 0xff9d9e,
			0xffabad, 0xffb9bd, 0xffc7ce, 0xffcade,

			0x490136, 0x66014b, 0x80035f, 0x951874,
			0xaa2d89, 0xba3d99, 0xca4da9, 0xd75ab6,
			0xe467c3, 0xef72ce, 0xfb7eda, 0xff8de1,
			0xff9de5, 0xffa5e7, 0xffafea, 0xffb8ec,

			0x48036c, 0x5c0488, 0x650d91, 0x7b23a7,
			0x933bbf, 0x9d45c9, 0xa74fd3, 0xb25ade,
			0xbd65e9, 0xc56df1, 0xce76fa, 0xd583ff,
			0xda90ff, 0xde9cff, 0xe2a9ff, 0xe6b6ff,

			0x051e81, 0x0626a5, 0x082fca, 0x263dd4,
			0x444cde, 0x4f5aec, 0x5a68ff, 0x6575ff,
			0x7183ff, 0x8091ff, 0x90a0ff, 0x97a9ff,
			0x9fb2ff, 0xafbeff, 0xc0cbff, 0xcdd3ff,

			0x0b0779, 0x201c8e, 0x3531a3, 0x4642b4,
			0x5753c5, 0x615dcf, 0x6d69db, 0x7b77e9,
			0x8985f7, 0x918dff, 0x9c98ff, 0xa7a4ff,
			0xb2afff, 0xbbb8ff, 0xc3c1ff, 0xd3d1ff,

			0x1d295a, 0x1d3876, 0x1d4892, 0x1d5cac,
			0x1d71c6, 0x3286cf, 0x489bd9, 0x4ea8ec,
			0x55b6ff, 0x70c7ff, 0x8cd8ff, 0x93dbff,
			0x9bdfff, 0xafe4ff, 0xc3e9ff, 0xcfedff,

			0x014b59, 0x015d6e, 0x016f84, 0x01849c,
			0x0199b5, 0x01abca, 0x01bcde, 0x01d0f5,
			0x1adcff, 0x3ee1ff, 0x64e7ff, 0x76eaff,
			0x8bedff, 0x9aefff, 0xb1f3ff, 0xc7f6ff,

			0x004800, 0x005400, 0x036b03, 0x0e760e,
			0x188018, 0x279227, 0x36a436, 0x4eb94e,
			0x51cd51, 0x72da72, 0x7ce47c, 0x85ed85,
			0xa2ffa2, 0xb5ffb5, 0xc8ffc8, 0xd0ffd0,

			0x164000, 0x1c5300, 0x236600, 0x287800,
			0x2e8c00, 0x3a980c, 0x47a519, 0x51af23,
			0x5cba2e, 0x71cf43, 0x85e357, 0x8deb5f,
			0x97f569, 0xa4ff97, 0xb9ff97, 0xb9ff97,

			0x2c3500, 0x384400, 0x445200, 0x495600,
			0x607100, 0x6c7f00, 0x798d0a, 0x8b9f1c,
			0x9eb22f, 0xabbf3c, 0xb8cc49, 0xc2d653,
			0xcde153, 0xdbef6c, 0xe8fc79, 0xf2ffab,

			0x463a09, 0x4d3f09, 0x544509, 0x6c5809,
			0x907609, 0xab8b0a, 0xc1a120, 0xd0b02f,
			0xdebe3d, 0xe6c645, 0xedcd4c, 0xf6da65,
			0xfde67d, 0xfff2a2, 0xfff9c5, 0xfff9d4,

			0x401a02, 0x581f05, 0x702408, 0x8d3a13,
			0xab511f, 0xb56427, 0xbf7730, 0xd0853a,
			0xe19344, 0xeda04e, 0xf9ad58, 0xfcb75c,
			0xffc160, 0xffc671, 0xffcb83, 0xffd498
		};
	}
}
