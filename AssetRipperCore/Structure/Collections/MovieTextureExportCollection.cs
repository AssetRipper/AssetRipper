using AssetRipper.Project.Exporters;
using AssetRipper.Classes;
using AssetRipper.Classes.Object;

namespace AssetRipper.Structure.Collections
{
	public sealed class MovieTextureExportCollection : AssetExportCollection
	{
		public MovieTextureExportCollection(IAssetExporter assetExporter, MovieTexture asset) : base(assetExporter, asset) { }

		protected override string GetExportExtension(Object asset)
		{
			return "ogv";
		}
	}
}
