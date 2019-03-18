using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.AssetExporters
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new TextAssetExportCollection(this, (TextAsset)asset);
		}
	}
}
