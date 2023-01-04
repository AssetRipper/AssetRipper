using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class Texture2DArrayExtensions
	{
		public static byte[] GetImageData(this ITexture2DArray texture)
		{
			if (texture.ImageData_C187.Length > 0)
			{
				return texture.ImageData_C187;
			}
			else if (texture.Has_StreamData_C187() && texture.StreamData_C187.IsSet())
			{
				return texture.StreamData_C187.GetContent(texture.Collection);
			}
			else
			{
				return Array.Empty<byte>();
			}
		}

		public static bool CheckAssetIntegrity(this ITexture2DArray texture)
		{
			if (texture.ImageData_C187.Length > 0)
			{
				return true;
			}
			else if (texture.Has_StreamData_C187())
			{
				return texture.StreamData_C187.CheckIntegrity(texture.Collection);
			}
			else
			{
				return false;
			}
		}

		public static ColorSpace GetColorSpace(this ITexture2DArray texture)
		{
			return (ColorSpace)texture.ColorSpace_C187;
		}
	}
}
