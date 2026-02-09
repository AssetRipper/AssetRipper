using AssetRipper.SourceGenerated.Subclasses.Vector4f;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public static class Vector4fExtensions
{
	public static void Scale(this IVector4f vector, float scalar)
	{
		vector.X *= scalar;
		vector.Y *= scalar;
		vector.Z *= scalar;
		vector.W *= scalar;
	}

	public static Vector4 CastToStruct(this IVector4f vector)
	{
		return new Vector4(vector.X, vector.Y, vector.Z, vector.W);
	}

	public static void CopyValues(this IVector4f vector, Vector4 source)
	{
		vector.X = source.X;
		vector.Y = source.Y;
		vector.Z = source.Z;
		vector.W = source.W;
	}
}
