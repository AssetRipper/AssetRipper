using AssetRipper.SourceGenerated.Subclasses.PackingParameters;
using AssetRipper.SourceGenerated.Subclasses.PackingSettings;

namespace AssetRipper.SourceGenerated.Extensions;

public static class PackingSettingsExtensions
{
	public static void Initialize(this IPackingSettings settings)
	{
		settings.Padding = 2;
		settings.BlockOffset = 1;
		settings.AllowAlphaSplitting = false;
		settings.EnableRotation = true;
		settings.EnableTightPacking = true;
		settings.EnableAlphaDilation = true;
	}

	public static void Initialize(this IPackingParameters settings) //maybe rename to PackingSettings?
	{
		settings.Padding = 2;
		settings.PaddingPower = 2; //todo: rename to Padding
		settings.BlockOffset = 1;
		settings.AllowAlphaSplitting = 0;
		settings.EnableRotation = 1;
		settings.EnableTightPacking = 1;
	}
}
