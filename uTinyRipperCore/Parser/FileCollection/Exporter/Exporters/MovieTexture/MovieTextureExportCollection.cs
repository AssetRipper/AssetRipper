using uTinyRipper.Classes;

namespace uTinyRipper.AssetExporters
{
	public sealed class MovieTextureExportCollection : AssetExportCollection
	{
		public MovieTextureExportCollection(IAssetExporter assetExporter, MovieTexture asset) :
			base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(Object asset)
		{
			return "ogv";
		}
	}
}
