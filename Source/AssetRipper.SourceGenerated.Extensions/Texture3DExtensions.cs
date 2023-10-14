using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class Texture3DExtensions
	{
		public static byte[] GetImageData(this ITexture3D texture)
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

		public static bool CheckAssetIntegrity(this ITexture3D texture)
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

		public static TextureFormat GetTextureFormat(this ITexture3D texture)
		{
			if (texture.Has_Format_Int32())
			{
				return texture.Format_Int32E;
			}
			else
			{
				return texture.Format_UInt32E;
			}
		}

		public static TextureUsageMode GetLightmapFormat(this ITexture3D texture)
		{
			if (texture.Has_LightmapFormat())
			{
				return texture.LightmapFormatE;
			}
			else
			{
				return texture.UsageModeE;
			}
		}

		public static int GetCompleteImageSize(this ITexture3D texture)
		{
			if (texture.Has_DataSize())
			{
				return (int)texture.DataSize;
			}
			else
			{
				return texture.CompleteImageSize;
			}
		}

		public static int GetMipCount(this ITexture3D texture)
		{
			if (texture.Has_MipCount())
			{
				return texture.MipCount;
			}
			else
			{
				if (texture.MipMap)
				{
					int maxSide = Math.Max(texture.Width, texture.Height);
					return Convert.ToInt32(Math.Log2(maxSide));
				}
				else
				{
					return 1;
				}
			}
		}
	}
}
