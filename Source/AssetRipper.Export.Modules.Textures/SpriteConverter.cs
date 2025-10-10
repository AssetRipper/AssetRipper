using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.Modules.Textures;

public static class SpriteConverter
{
	public static bool Supported(ISprite sprite)
	{
		if (sprite.TryGetTexture() is { } texture)
		{
			return texture.CheckAssetIntegrity();
		}
		else if (sprite.SpriteAtlasP is { } atlas)
		{
			return false;
		}
		else
		{
			return false;
		}
	}

	public static bool TryConvertToBitmap(ISprite sprite, out DirectBitmap bitmap)
	{
		if (sprite.TryGetTexture() is { } texture)
		{
			if (!TextureConverter.TryConvertToBitmap(texture, out DirectBitmap textureBitmap))
			{
				return ReturnFalse(out bitmap);
			}

			bitmap = textureBitmap;
			return true;
		}
		else if (sprite.SpriteAtlasP is { } atlas)
		{
			return ReturnFalse(out bitmap);
		}
		else
		{
			return ReturnFalse(out bitmap);
		}
	}

	private static bool ReturnFalse(out DirectBitmap bitmap)
	{
		bitmap = DirectBitmap.Empty;
		return false;
	}
}
