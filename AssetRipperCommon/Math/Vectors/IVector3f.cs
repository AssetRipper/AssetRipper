using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Math.Vectors
{
	public interface IVector3f : IAsset
	{
		float X { get; set; }
		float Y { get; set; }
		float Z { get; set; }
	}

	public static class Vector3fExtensions
	{
		public static void CopyValuesFrom(this IVector3f instance, IVector3f source)
		{
			instance.X = source.X;
			instance.Y = source.Y;
			instance.Z = source.Z;
		}

		public static void Reset(this IVector3f instance)
		{
			instance.X = 0;
			instance.Y = 0;
			instance.Z = 0;
		}
	}
}
