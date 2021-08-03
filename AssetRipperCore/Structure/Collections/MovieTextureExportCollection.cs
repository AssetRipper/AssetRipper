using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Object;

namespace AssetRipper.Core.Structure.Collections
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
