using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;

namespace AssetRipper.Assets.IO
{
	public interface IAsset : IAssetReadable, IAssetWritable, IYamlExportable
	{
	}
}
