using AssetRipper.SourceGenerated.Subclasses.Vector3Int;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public interface IAABBi
	{
		IVector3Int Center { get; }
		IVector3Int Extent { get; }
	}
}
