using AssetRipper.SourceGenerated.Classes.ClassID_188;

namespace AssetRipper.SourceGenerated.Extensions;

public static class CubemapArrayExtensions
{
	public static byte[] GetImageData(this ICubemapArray texture)
	{
		if (texture.ImageData_C188.Length > 0)
		{
			return texture.ImageData_C188;
		}
		else if (texture.Has_StreamData_C188() && texture.StreamData_C188.IsSet())
		{
			return texture.StreamData_C188.GetContent(texture.Collection);
		}
		else
		{
			return Array.Empty<byte>();
		}
	}

	public static bool CheckAssetIntegrity(this ICubemapArray texture)
	{
		if (texture.ImageData_C188.Length > 0)
		{
			return true;
		}
		else if (texture.Has_StreamData_C188())
		{
			return texture.StreamData_C188.CheckIntegrity(texture.Collection);
		}
		else
		{
			return false;
		}
	}
}
