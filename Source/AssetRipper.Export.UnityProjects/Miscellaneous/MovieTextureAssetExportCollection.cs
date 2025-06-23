using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_152;

namespace AssetRipper.Export.UnityProjects.Miscellaneous;

public sealed class MovieTextureAssetExportCollection : AssetExportCollection<IMovieTexture>
{
	public MovieTextureAssetExportCollection(MovieTextureAssetExporter assetExporter, IMovieTexture asset) : base(assetExporter, asset)
	{
	}

	protected override string GetExportExtension(IUnityObjectBase asset) => "ogv";
}
