using AssetRipper.SourceGenerated.Subclasses.ConstantClip;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ConstantClipExtensions
{
	public static bool IsSet(this IConstantClip clip) => clip.Data.Count > 0;
}
