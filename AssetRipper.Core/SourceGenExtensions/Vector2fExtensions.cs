using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using System.Numerics;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class Vector2fExtensions
	{
		public static void Scale(this IVector2f vector, float scalar)
		{
			vector.X *= scalar;
			vector.Y *= scalar;
		}

		public static Vector2 CastToStruct(this IVector2f vector)
		{
			return new Vector2(vector.X, vector.Y);
		}
	}
}
