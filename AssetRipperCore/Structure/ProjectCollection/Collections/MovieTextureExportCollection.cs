using AssetRipper.Converters.Project.Exporter;
using AssetRipper.Parser.Classes;
using AssetRipper.Parser.Classes.Object;

namespace AssetRipper.Structure.ProjectCollection.Collections
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
