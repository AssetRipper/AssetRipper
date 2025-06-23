using AssetRipper.SourceGenerated.Classes.ClassID_187;

namespace AssetRipper.SourceGenerated.Extensions;

public static class Texture2DArrayExtensions
{
	public static byte[] GetImageData(this ITexture2DArray texture)
	{
		if (texture.ImageData.Length > 0)
		{
			return texture.ImageData;
		}
		else if (texture.Has_StreamData() && texture.StreamData.IsSet())
		{
			return texture.StreamData.GetContent(texture.Collection);
		}
		else
		{
			return Array.Empty<byte>();
		}
	}

	public static bool CheckAssetIntegrity(this ITexture2DArray texture)
	{
		if (texture.ImageData.Length > 0)
		{
			return true;
		}
		else if (texture.Has_StreamData())
		{
			return texture.StreamData.CheckIntegrity(texture.Collection);
		}
		else
		{
			return false;
		}
	}

	public static int GetCompleteImageSize(this ITexture2DArray texture)
	{
		return (int)texture.DataSize / (texture.Depth > 1 ? texture.Depth : 1);
	}
}
