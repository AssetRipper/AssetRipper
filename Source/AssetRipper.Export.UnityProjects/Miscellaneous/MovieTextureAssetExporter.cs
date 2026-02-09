using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_152;

namespace AssetRipper.Export.UnityProjects.Miscellaneous;

public sealed class MovieTextureAssetExporter : BinaryAssetExporter
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IMovieTexture texture && IsValidData(texture.MovieData))
		{
			exportCollection = new MovieTextureAssetExportCollection(this, texture);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		fileSystem.File.WriteAllBytes(path, ((IMovieTexture)asset).MovieData!);
		return true;
	}
}
