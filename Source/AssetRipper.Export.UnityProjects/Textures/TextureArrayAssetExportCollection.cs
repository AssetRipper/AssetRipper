using AssetRipper.Assets;
using AssetRipper.Export.Configuration;

namespace AssetRipper.Export.UnityProjects.Textures;

public sealed class TextureArrayAssetExportCollection : AssetExportCollection<IUnityObjectBase>
{
	public TextureArrayAssetExportCollection(TextureArrayAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
	{
	}

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		return ((TextureArrayAssetExporter)AssetExporter).ImageExportFormat.GetFileExtension();
	}

	protected override IUnityObjectBase CreateImporter(IExportContainer container)
	{
		return ImporterFactory.GenerateTextureImporter(container, Asset);
	}
}
