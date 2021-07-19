using AssetRipper.Classes;
using AssetRipper.Project;
using AssetRipper.SerializedFiles;

namespace AssetRipper.Converters
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new TextAssetExportCollection(this, (TextAsset)asset);
		}
	}
}
