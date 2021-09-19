using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.Configuration;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		private TextExportMode exportMode;
		public TextAssetExporter(CoreConfiguration configuration)
		{
			exportMode = configuration.TextExportMode;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new TextAssetExportCollection(this, (TextAsset)asset, exportMode);
		}
	}
}
