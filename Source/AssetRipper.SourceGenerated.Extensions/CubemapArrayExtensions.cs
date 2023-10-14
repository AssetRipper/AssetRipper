using AssetRipper.SourceGenerated.Classes.ClassID_188;

namespace AssetRipper.SourceGenerated.Extensions;

public static class CubemapArrayExtensions
{
	public static byte[] GetImageData(this ICubemapArray texture)
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

	public static bool CheckAssetIntegrity(this ICubemapArray texture)
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
}
