using AssetRipper.SourceGenerated.Subclasses.Vector3f;
using System.Numerics;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class Vector3fExtensions
	{
		public static void Scale(this IVector3f vector, float scalar)
		{
			vector.X *= scalar;
			vector.Y *= scalar;
			vector.Z *= scalar;
		}

		public static Vector3 CastToStruct(this IVector3f vector)
		{
			return new Vector3(vector.X, vector.Y, vector.Z);
		}
	}
}
