using AssetRipper.Assets;
using AssetRipper.Import.Project.Collections;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class MovieTextureAssetExportCollection : AssetExportCollection
	{
		public MovieTextureAssetExportCollection(MovieTextureAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "ogv";
	}
}
