using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.IO.Yaml;

namespace AssetRipper.Assets.IO
{
	public interface IAsset : IAssetReadable, IAssetWritable, IYamlExportable
	{
	}
}
