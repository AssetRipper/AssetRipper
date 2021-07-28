using AssetRipper.Classes;
using AssetRipper.Classes.Object;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;

namespace AssetRipper.Project.Exporters
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObject asset)
		{
			return new TextAssetExportCollection(this, (TextAsset)asset);
		}
	}
}
