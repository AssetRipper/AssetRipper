using AssetRipper.Parser.Classes;
using AssetRipper.Parser.Classes.Object;
using AssetRipper.Parser.Files.SerializedFile;
using AssetRipper.Structure.ProjectCollection.Collections;

namespace AssetRipper.Converters.Project.Exporter
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new MovieTextureExportCollection(this, (MovieTexture)asset);
		}
	}
}
