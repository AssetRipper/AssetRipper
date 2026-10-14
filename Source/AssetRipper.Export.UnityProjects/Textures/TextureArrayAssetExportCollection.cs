using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Subclasses.TextureImporterPlatformSettings;

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
		ITextureImporter importer = ImporterFactory.GenerateTextureImporter(container, Asset);
		if (TextureArrayAssetExporter.TryGetLayerSize(Asset, out int width, out int height, out int depth)
			&& FlipbookLayout.TryCreate(width, height, depth, out FlipbookLayout layout)
			&& importer.Has_FlipbookColumns()
			&& importer.Has_FlipbookRows())
		{
			// Without these, Unity imports the whole image as a single layer.
			importer.FlipbookColumns = layout.Columns;
			importer.FlipbookRows = layout.Rows;

			// The maximum size applies to the whole image. A smaller one would scale down every layer.
			int imageSize = int.Max(width * layout.Columns, height * layout.Rows);
			importer.MaxTextureSize = int.Max(importer.MaxTextureSize, (int)System.Numerics.BitOperations.RoundUpToPowerOf2((uint)imageSize));
			foreach (ITextureImporterPlatformSettings platformSettings in importer.PlatformSettings)
			{
				platformSettings.MaxTextureSize = importer.MaxTextureSize;
			}
		}
		return importer;
	}
}
