using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Library.Exporters.Miscellaneous
{
	public sealed class MovieTextureAssetExportCollection : AssetExportCollection
	{
		public MovieTextureAssetExportCollection(MovieTextureAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "ogv";
	}
}
