using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Import.Logging;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Textures;

public class TextureAssetExporter : BinaryAssetExporter
{
	public ImageExportFormat ImageExportFormat { get; }
	private SpriteExportMode SpriteExportMode { get; }
	public bool PreferOriginalTextureExtension { get; }
	private bool ExportSprites => SpriteExportMode is not SpriteExportMode.Yaml;

	public TextureAssetExporter(FullConfiguration configuration)
	{
		ImageExportFormat = configuration.ExportSettings.ImageExportFormat;
		SpriteExportMode = configuration.ExportSettings.SpriteExportMode;
		PreferOriginalTextureExtension = configuration.ExportSettings.PreferOriginalTextureExtension;
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset.MainAsset is SpriteInformationObject spriteInformationObject && (ExportSprites || asset is not ISprite))
		{
			ITexture2D texture = spriteInformationObject.Texture;
			// Dynamic font atlases can be serialized before they have any pixels.
			// Preserve these as native assets rather than attempting a zero-sized bitmap.
			exportCollection = texture.Width_C28 == 0 && texture.Height_C28 == 0
				&& texture.ImageData_C28.Length == 0 && (texture.StreamData_C28?.Size ?? 0) == 0
				&& spriteInformationObject.Sprites.Count == 0
				? new EmptyTextureExportCollection(spriteInformationObject)
				: new TextureExportCollection(this, spriteInformationObject, ExportSprites);
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
		ITexture2D texture = (ITexture2D)asset;
		if (!texture.CheckAssetIntegrity())
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{texture.Name}' because resources file '{texture.StreamData_C28?.Path}' hasn't been found");
			return false;
		}

		if (TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap))
		{
			using Stream stream = fileSystem.File.Create(path);
			bitmap.Save(stream, texture.GetTextureExportFormat(PreferOriginalTextureExtension, ImageExportFormat));
			return true;
		}
		else
		{
			Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to convert '{texture.Name}' to bitmap");
			return false;
		}
	}

	private sealed class EmptyTextureExportCollection : AssetsExportCollection<ITexture2D>
	{
		public EmptyTextureExportCollection(SpriteInformationObject spriteInformationObject)
			: base(new DefaultYamlExporter(), spriteInformationObject.Texture)
		{
			AddAsset(spriteInformationObject);
		}

		protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
		{
			return AssetExporter.Export(container, Asset, filePath, fileSystem);
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => "asset";
	}
}
