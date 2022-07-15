using AssetRipper.Core.Classes.SpriteRenderer;
using AssetRipper.SourceGenerated.Classes.ClassID_331;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SpriteMaskExtensions
	{
		public static SpriteSortPoint GetSpriteSortPoint(this ISpriteMask mask)
		{
			return (SpriteSortPoint)mask.SpriteSortPoint_C331;
		}
	}
}
