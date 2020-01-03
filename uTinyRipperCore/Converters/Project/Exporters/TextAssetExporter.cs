using uTinyRipper.Project;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new TextAssetExportCollection(this, (TextAsset)asset);
		}
	}
}
