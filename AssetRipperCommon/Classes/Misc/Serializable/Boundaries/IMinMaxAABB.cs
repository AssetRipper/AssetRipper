using AssetRipper.Core.Math.Vectors;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public interface IMinMaxAABB
	{
		IVector3f Min { get; }
		IVector3f Max { get; }
	}
}
