using AssetRipper.Assets;
using AssetRipper.Export.Configuration;

namespace AssetRipper.Export.UnityProjects.Textures;

public sealed class TextureArrayAssetExportCollection : AssetExportCollection<IUnityObjectBase>
{
	public TextureArrayAssetExportCollection(TextureArrayAssetExporter assetExporter, IUnityObjectBase asset) : base(assetExporter, asset)
	{
	}

	private new TextureArrayAssetExporter AssetExporter => (TextureArrayAssetExporter)base.AssetExporter;

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		return asset.GetTextureExtension(AssetExporter.PreferOriginalTextureExtension, AssetExporter.ImageExportFormat);
	}

	protected override IUnityObjectBase CreateImporter(IExportContainer container)
	{
		return ImporterFactory.GenerateTextureImporter(container, Asset);
	}
}
