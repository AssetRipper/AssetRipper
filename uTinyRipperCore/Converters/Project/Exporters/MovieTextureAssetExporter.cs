using uTinyRipper.Project;
using uTinyRipper.Classes;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Object asset)
		{
			return new MovieTextureExportCollection(this, (MovieTexture)asset);
		}
	}
}
