using AssetRipper.Parser.Classes.Font;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Structure.ProjectCollection.Collections;

namespace AssetRipper.Converters.Project.Exporter
{
	public sealed class FontAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(Object asset, ExportOptions options)
		{
			Font font = (Font)asset;
			return font.IsValidData;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new FontExportCollection(this, (Font)asset);
		}
	}
}
