using AssetRipper.Core.Classes.ParticleSystem.Trigger;
using AssetRipper.SourceGenerated.Subclasses.TriggerModule;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class TriggerModuleExtensions
	{
		public static void SetToDefault(this ITriggerModule module)
		{
			module.Inside = (int)TriggerAction.Kill;
			module.RadiusScale = 1;
		}

		public static TriggerAction GetInside(this ITriggerModule module)
		{
			return (TriggerAction)module.Inside;
		}

		public static TriggerAction GetOutside(this ITriggerModule module)
		{
			return (TriggerAction)module.Outside;
		}

		public static TriggerAction GetEnter(this ITriggerModule module)
		{
			return (TriggerAction)module.Enter;
		}

		public static TriggerAction GetExit(this ITriggerModule module)
		{
			return (TriggerAction)module.Exit;
		}
	}
}
