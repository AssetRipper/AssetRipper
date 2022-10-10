using AssetRipper.Core.Math.Vectors;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public interface IAABBi
	{
		IVector3i Center { get; }
		IVector3i Extent { get; }
	}
}