using AssetRipper.SourceGenerated.Classes.ClassID_188;

namespace AssetRipper.SourceGenerated.Extensions;

public static class CubemapArrayExtensions
{
	public static int GetHeight(this ICubemapArray texture)
	{
		return texture.Width;
	}

	public static int GetDepth(this ICubemapArray texture)
	{
		return texture.CubemapCount * 6;// Not sure about this
	}

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
			return [];
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

	public static int GetCompleteImageSize(this ICubemapArray texture)
	{
		int depth = texture.GetDepth();
		return depth > 0 ? (int)texture.DataSize / depth : 0;
	}
}
