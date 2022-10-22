using AssetRipper.SourceGenerated.Subclasses.AABB;
using System.Numerics;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class AABBExtensions
	{
		public static void CopyValuesFrom(this IAABB instance, IAABB source)
		{
			instance.Center.CopyValues(source.Center);
			instance.Extent.CopyValues(source.Extent);
		}

		public static void CopyValuesFrom(this IAABB instance, Vector3 center, Vector3 extent)
		{
			instance.Center.CopyValues(center);
			instance.Extent.CopyValues(extent);
		}

		public static void Reset(this IAABB instance)
		{
			instance.Center.Reset();
			instance.Extent.Reset();
		}
	}
}
