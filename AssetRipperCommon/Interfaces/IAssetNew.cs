using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Interfaces
{
	public interface IAssetNew : IAssetNewReadable, IAssetNewWritable, IYAMLExportableNew, IAsset
	{
	}
}
