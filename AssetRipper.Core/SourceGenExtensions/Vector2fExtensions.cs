using AssetRipper.SourceGenerated.Subclasses.Vector2f;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class Vector2fExtensions
	{
		public static void Scale(this IVector2f vector, float scalar)
		{
			vector.X *= scalar;
			vector.Y *= scalar;
		}
	}
}
