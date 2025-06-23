using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public static class Vector2fExtensions
{
	public static void Scale(this Vector2f vector, float scalar)
	{
		vector.X *= scalar;
		vector.Y *= scalar;
	}

	public static void CopyValues(this Vector2f vector, Vector2 source)
	{
		vector.X = source.X;
		vector.Y = source.Y;
	}

	public static Vector2 CastToStruct(this Vector2f vector)
	{
		return new Vector2(vector.X, vector.Y);
	}

	public static void SetOne(this Vector2f vector)
	{
		vector.X = 1.0f;
		vector.Y = 1.0f;
	}
}
