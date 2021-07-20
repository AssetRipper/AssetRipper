using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;

namespace AssetRipper.Parser.IO.Asset
{
	public interface IAsset : IAssetReadable, IAssetWritable, IYAMLExportable
	{
	}
}
