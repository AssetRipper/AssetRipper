using AssetRipper.SourceGenerated.Subclasses.StreamedClip;

namespace AssetRipper.SourceGenerated.Extensions;

public static class StreamedClipExtensions
{
	public static bool IsSet(this IStreamedClip clip) => clip.Data.Count > 0;

	public static int CurveCount(this IStreamedClip clip)
	{
		return clip.Has_CurveCount_UInt32() ? (int)clip.CurveCount_UInt32 : clip.CurveCount_UInt16 + clip.DiscreteCurveCount;
	}
}
