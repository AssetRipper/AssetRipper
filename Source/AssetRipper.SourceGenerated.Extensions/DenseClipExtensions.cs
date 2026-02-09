using AssetRipper.SourceGenerated.Subclasses.DenseClip;

namespace AssetRipper.SourceGenerated.Extensions;

public static class DenseClipExtensions
{
	public static bool IsSet(this IDenseClip clip) => clip.SampleArray.Count > 0;
}
