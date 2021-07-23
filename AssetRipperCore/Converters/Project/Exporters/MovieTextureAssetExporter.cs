using AssetRipper.Parser.Classes;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;

namespace AssetRipper.Converters.Project.Exporters
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new MovieTextureExportCollection(this, (MovieTexture)asset);
		}
	}
}
