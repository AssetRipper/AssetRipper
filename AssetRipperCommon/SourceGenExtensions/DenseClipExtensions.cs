using AssetRipper.SourceGenerated.Subclasses.DenseClip;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class DenseClipExtensions
	{
		public static bool IsSet(this IDenseClip clip) => clip.SampleArray.Length > 0;
	}
}
