using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Project
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
