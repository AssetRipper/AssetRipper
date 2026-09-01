using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.Export.Modules.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_189;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.PrimaryContent.Textures;

public sealed class TextureExporter : IContentExtractor
{
	public ImageExportFormat ImageFormat { get; }
	public bool PreferOriginalTextureExtension { get; }

	public TextureExporter(ImageExportFormat imageFormat, bool preferOriginalTextureExtension)
	{
		ImageFormat = imageFormat;
		PreferOriginalTextureExtension = preferOriginalTextureExtension;
	}

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		if (asset is IImageTexture texture && texture.CheckAssetIntegrity())
		{
			exportCollection = new ImageExportCollection(this, texture);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public bool Export(IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		if (TextureConverter.TryConvertToBitmap((IImageTexture)asset, out DirectBitmap bitmap))
		{
			using Stream stream = fileSystem.File.Create(path);
			bitmap.Save(stream, asset.GetTextureExportFormat(PreferOriginalTextureExtension, ImageFormat));
			return true;
		}
		else
		{
			return false;
		}
	}

	private sealed class ImageExportCollection : SingleExportCollection<IImageTexture>
	{
		private new TextureExporter ContentExtractor => (TextureExporter)base.ContentExtractor;

		protected override string ExportExtension => Asset.GetTextureExtension(ContentExtractor.PreferOriginalTextureExtension, ContentExtractor.ImageFormat);

		public ImageExportCollection(IContentExtractor contentExtractor, IImageTexture asset) : base(contentExtractor, asset)
		{
		}
	}
}
