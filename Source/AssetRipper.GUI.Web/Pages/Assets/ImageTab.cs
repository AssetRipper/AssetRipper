using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Terrains;
using AssetRipper.Export.UnityProjects.Textures;
using AssetRipper.Export.UnityProjects.Utils;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.TextureDecoder.Rgb.Formats;
using DirectBitmap = AssetRipper.Export.UnityProjects.Utils.DirectBitmap<AssetRipper.TextureDecoder.Rgb.Formats.ColorBGRA32, byte>;

namespace AssetRipper.GUI.Web.Pages.Assets;

internal sealed class ImageTab : HtmlTab
{
	private readonly DirectBitmap<ColorBGRA32, byte> bitmap;

	public override string DisplayName => Localization.AssetTabImage;

	public override string HtmlName => "image";

	public override bool Enabled => bitmap != default;

	public ImageTab(IUnityObjectBase asset)
	{
		bitmap = GetImageBitmap(asset);
	}

	public override void Write(TextWriter writer)
	{
		MemoryStream stream = new();
		bitmap.SaveAsPng(stream);
		string sourcePath = $"data:image/png;base64,{stream.ToArray().ToBase64String()}";

		// Click on image to save
		using (new A(writer).WithHref(sourcePath).WithDownload("extracted_image").End())
		{
			new Img(writer).WithSrc(sourcePath).WithStyle("object-fit:contain; width:100%; height:100%").Close();
		}
	}

	private static DirectBitmap GetImageBitmap(IUnityObjectBase asset)
	{
		return asset switch
		{
			ITexture2D texture => TextureToBitmap(texture),
			SpriteInformationObject spriteInformationObject => TextureToBitmap(spriteInformationObject.Texture),
			ISprite sprite => SpriteToBitmap(sprite),
			ITerrainData terrainData => TerrainHeatmapExporter.GetBitmap(terrainData),
			_ => default,
		};

		static DirectBitmap TextureToBitmap(ITexture2D texture)
		{
			return TextureConverter.TryConvertToBitmap(texture, out DirectBitmap bitmap) ? bitmap : default;
		}

		static DirectBitmap SpriteToBitmap(ISprite sprite)
		{
			return sprite.TryGetTexture() is { } spriteTexture ? TextureToBitmap(spriteTexture) : default;
		}
	}
}
