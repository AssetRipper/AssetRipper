using AssetRipper.Assets.Export.Yaml;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.IO.Endian;

namespace AssetRipper.Assets.IO
{
	public interface IAsset : IEndianSpanReadable, IAssetWritable, IYamlExportable
	{
	}
}
