using AssetRipper.SourceGenerated.Subclasses.Quaternionf;
using System.Numerics;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class QuaternionfExtensions
	{
		public static Quaternion CastToStruct(this IQuaternionf vector)
		{
			return new Quaternion(vector.X, vector.Y, vector.Z, vector.W);
		}
	}
}
