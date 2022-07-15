using AssetRipper.SourceGenerated.Subclasses.Vector4f;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class Vector4fExtensions
	{
		public static void Scale(this IVector4f vector, float scalar)
		{
			vector.X *= scalar;
			vector.Y *= scalar;
			vector.Z *= scalar;
			vector.W *= scalar;
		}
	}
}
