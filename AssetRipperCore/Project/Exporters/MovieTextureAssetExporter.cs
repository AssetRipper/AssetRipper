using AssetRipper.Classes;
using AssetRipper.Classes.Object;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Structure.Collections;

namespace AssetRipper.Project.Exporters
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new MovieTextureExportCollection(this, (MovieTexture)asset);
		}
	}
}
