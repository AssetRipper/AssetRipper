using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IVariableBoneCountWeights : IAsset
	{
		uint[] Data { get; set; }
	}
}
