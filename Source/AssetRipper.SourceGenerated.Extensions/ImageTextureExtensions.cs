using AssetRipper.SourceGenerated.Classes.ClassID_189;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ImageTextureExtensions
{
	public static byte[] GetImageData(this IImageTexture texture)
	{
		if (texture.ImageData_C189.Length > 0)
		{
			return texture.ImageData_C189;
		}
		else if (texture.Has_StreamData_C189() && texture.StreamData_C189.IsSet())
		{
			return texture.StreamData_C189.GetContent(texture.Collection);
		}
		else
		{
			return [];
		}
	}

	public static bool CheckAssetIntegrity(this IImageTexture texture)
	{
		if (texture.ImageData_C189.Length > 0)
		{
			return true;
		}
		else if (texture.Has_StreamData_C189())
		{
			return texture.StreamData_C189.CheckIntegrity(texture.Collection);
		}
		else
		{
			return false;
		}
	}
}
