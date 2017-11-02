using System;
using System.ComponentModel;

using Newtonsoft.Json;

using BizHawk.Common;
using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	public partial class MP1000 : IEmulator, IStatable, ISettable<MP1000.MP1000Settings, MP1000.MP1000SyncSettings>
	{
		public MP1000Settings GetSettings()
		{
			return _settings.Clone();
		}

		public MP1000SyncSettings GetSyncSettings()
		{
			return _syncSettings.Clone();
		}

		public bool PutSettings(MP1000Settings o)
		{
			_settings = o;
			return false;
		}

		public bool PutSyncSettings(MP1000SyncSettings o)
		{
			bool ret = MP1000SyncSettings.NeedsReboot(_syncSettings, o);
			_syncSettings = o;
			return ret;
		}

		private MP1000Settings _settings = new MP1000Settings();
		public MP1000SyncSettings _syncSettings = new MP1000SyncSettings();

		public class MP1000Settings
		{
			public MP1000Settings Clone()
			{
				return (MP1000Settings)MemberwiseClone();
			}
		}

		public class MP1000SyncSettings
		{
			private string _port1 = MP1000ControllerDeck.DefaultControllerName;
			private string _port2 = MP1000ControllerDeck.DefaultControllerName;
			private string _Filter = "None";

			[JsonIgnore]
			public string Filter
			{
				get { return _Filter; }
				set
				{
					_Filter = value;
				}
			}

			[JsonIgnore]
			public string Port1
			{
				get { return _port1; }
				set
				{
					if (!MP1000ControllerDeck.ValidControllerTypes.ContainsKey(value))
					{
						throw new InvalidOperationException("Invalid controller type: " + value);
					}

					_port1 = value;
				}
			}

			[JsonIgnore]
			public string Port2
			{
				get { return _port2; }
				set
				{
					if (!MP1000ControllerDeck.ValidControllerTypes.ContainsKey(value))
					{
						throw new InvalidOperationException("Invalid controller type: " + value);
					}

					_port2 = value;
				}
			}

			public MP1000SyncSettings Clone()
			{
				return (MP1000SyncSettings)MemberwiseClone();
			}

			public static bool NeedsReboot(MP1000SyncSettings x, MP1000SyncSettings y)
			{
				return !DeepEquality.DeepEquals(x, y);
			}
		}
	}
}
