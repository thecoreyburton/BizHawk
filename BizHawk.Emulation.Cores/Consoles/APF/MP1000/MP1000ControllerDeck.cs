using System;
using System.Collections.Generic;
using System.Linq;

using BizHawk.Common;
using BizHawk.Common.ReflectionExtensions;
using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.APF.MP1000
{
	public class MP1000ControllerDeck
	{
		public MP1000ControllerDeck(string controller1Name, string controller2Name)
		{
			if (!ValidControllerTypes.ContainsKey(controller1Name))
			{
				throw new InvalidOperationException("Invalid controller type: " + controller1Name);
			}

			if (!ValidControllerTypes.ContainsKey(controller2Name))
			{
				throw new InvalidOperationException("Invalid controller type: " + controller2Name);
			}

			Port1 = (IPort)Activator.CreateInstance(ValidControllerTypes[controller1Name], 1);
			Port2 = (IPort)Activator.CreateInstance(ValidControllerTypes[controller2Name], 2);

			Definition = new ControllerDefinition
			{
				Name = Port1.Definition.Name,
				BoolButtons = Port1.Definition.BoolButtons
					.Concat(Port2.Definition.BoolButtons)
					.Concat(new[]
					{
						"Power",
						"Reset",
					})
					.ToList()
			};

			Definition.FloatControls.AddRange(Port1.Definition.FloatControls);
			Definition.FloatControls.AddRange(Port2.Definition.FloatControls);

			Definition.FloatRanges.AddRange(Port1.Definition.FloatRanges);
			Definition.FloatRanges.AddRange(Port2.Definition.FloatRanges);
		}

		public byte ReadPort1(IController c)
		{
			return Port1.Read(c);
		}

		public byte ReadPort2(IController c)
		{
			return Port2.Read(c);
		}

		public byte ReadFire1(IController c)
		{
			return Port1.ReadFire(c);
		}

		public byte ReadFire2(IController c)
		{
			return Port2.ReadFire(c);
		}

		public byte ReadFire1_2x(IController c)
		{
			return Port1.ReadFire2x(c);
		}

		public byte ReadFire2_2x(IController c)
		{
			return Port2.ReadFire2x(c);
		}

		public ControllerDefinition Definition { get; }

		public void SyncState(Serializer ser)
		{
			ser.BeginSection("Port1");
			Port1.SyncState(ser);
			ser.EndSection();

			ser.BeginSection("Port2");
			Port2.SyncState(ser);
			ser.EndSection();
		}

		private readonly IPort Port1;
		private readonly IPort Port2;

		private static Dictionary<string, Type> _controllerTypes;

		public static Dictionary<string, Type> ValidControllerTypes
		{
			get
			{
				if (_controllerTypes == null)
				{
					_controllerTypes = typeof(MP1000ControllerDeck).Assembly
						.GetTypes()
						.Where(t => typeof(IPort).IsAssignableFrom(t))
						.Where(t => !t.IsAbstract && !t.IsInterface)
						.ToDictionary(tkey => tkey.DisplayName());
				}

				return _controllerTypes;
			}
		}

		public static string DefaultControllerName => typeof(StandardController).DisplayName();
	}
}
