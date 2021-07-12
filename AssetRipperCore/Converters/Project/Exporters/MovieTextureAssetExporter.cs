using AssetRipper.Project;
using AssetRipper.Classes;
using AssetRipper.SerializedFiles;

namespace AssetRipper.Converters
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new MovieTextureExportCollection(this, (MovieTexture)asset);
		}
	}
}
