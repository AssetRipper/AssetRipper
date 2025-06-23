using AssetRipper.SourceGenerated.Subclasses.Quaternionf;
using System.Numerics;

namespace AssetRipper.SourceGenerated.Extensions;

public static class QuaternionfExtensions
{
	public static Quaternion CastToStruct(this IQuaternionf vector)
	{
		return new Quaternion(vector.X, vector.Y, vector.Z, vector.W);
	}

	public static void CopyValues(this IQuaternionf vector, Quaternion source)
	{
		vector.X = source.X;
		vector.Y = source.Y;
		vector.Z = source.Z;
		vector.W = source.W;
	}

	public static void SetIdentity(this IQuaternionf vector) => vector.CopyValues(Quaternion.Identity);
}
