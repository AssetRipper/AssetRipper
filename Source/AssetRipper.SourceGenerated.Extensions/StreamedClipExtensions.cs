using AssetRipper.SourceGenerated.Subclasses.StreamedClip;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class StreamedClipExtensions
	{
		public static bool IsSet(this IStreamedClip clip) => clip.Data.Count > 0;
	}
}
