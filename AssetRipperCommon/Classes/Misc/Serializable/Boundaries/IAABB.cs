using AssetRipper.Core.Math.Vectors;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public interface IAABB
	{
		IVector3f Center { get; }
		IVector3f Extent { get; }
	}
}