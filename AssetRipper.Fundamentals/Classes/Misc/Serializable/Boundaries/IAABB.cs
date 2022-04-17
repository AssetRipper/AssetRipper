using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public interface IAABB : IAsset
	{
		IVector3f Center { get; }
		IVector3f Extent { get; }
	}

	public static class AABBExtensions
	{
		public static void CopyValuesFrom(this IAABB instance, IAABB source)
		{
			instance.Center.CopyValuesFrom(source.Center);
			instance.Extent.CopyValuesFrom(source.Extent);
		}

		public static void CopyValuesFrom(this IAABB instance, IVector3f center, IVector3f extent)
		{
			instance.Center.CopyValuesFrom(center);
			instance.Extent.CopyValuesFrom(extent);
		}

		public static void Reset(this IAABB instance)
		{
			instance.Center.Reset();
			instance.Extent.Reset();
		}
	}
}
