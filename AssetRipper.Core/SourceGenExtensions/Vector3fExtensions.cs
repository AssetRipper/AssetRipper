using AssetRipper.SourceGenerated.Subclasses.Vector3f;

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
	}
}
