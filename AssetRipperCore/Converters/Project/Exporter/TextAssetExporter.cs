using AssetRipper.Parser.Classes;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Structure.ProjectCollection.Collections;

namespace AssetRipper.Converters.Project.Exporter
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new TextAssetExportCollection(this, (TextAsset)asset);
		}
	}
}
