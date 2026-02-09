using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.LOD;

namespace AssetRipper.SourceGenerated.Extensions;

public static class LODExtensions
{
	public static LODFadeMode GetFadeMode(this ILOD lod)
	{
		return (LODFadeMode)lod.FadeMode;
	}
}
