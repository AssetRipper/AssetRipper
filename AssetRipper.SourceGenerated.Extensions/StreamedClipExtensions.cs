using AssetRipper.SourceGenerated.Subclasses.StreamedClip;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class StreamedClipExtensions
	{
		public static bool IsSet(this IStreamedClip clip) => clip.Data.Length > 0;
	}
}
