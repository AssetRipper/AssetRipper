using AssetRipper.SourceGenerated.Subclasses.TriggerModule;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TriggerModuleExtensions
{
	public enum TriggerAction
	{
		Ignore = 0,
		Kill = 1,
		Callback = 2,
	}

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
