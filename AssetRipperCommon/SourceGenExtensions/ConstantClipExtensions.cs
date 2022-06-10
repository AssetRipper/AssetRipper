using AssetRipper.SourceGenerated.Subclasses.ConstantClip;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ConstantClipExtensions
	{
		public static bool IsSet(this IConstantClip clip) => clip.Data.Length > 0;
	}
}
