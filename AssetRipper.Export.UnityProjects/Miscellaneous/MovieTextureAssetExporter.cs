using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_152;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class MovieTextureAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IMovieTexture texture && IsValidData(texture.MovieData_C152);
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new MovieTextureAssetExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			File.WriteAllBytes(path, ((IMovieTexture)asset).MovieData_C152!);
			return true;
		}
	}
}
