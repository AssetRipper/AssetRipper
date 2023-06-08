using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class Texture3DExtensions
	{
		public static byte[] GetImageData(this ITexture3D texture)
		{
			if (texture.ImageData_C117.Length > 0)
			{
				return texture.ImageData_C117;
			}
			else if (texture.Has_StreamData_C117() && texture.StreamData_C117.IsSet())
			{
				return texture.StreamData_C117.GetContent(texture.Collection);
			}
			else
			{
				return Array.Empty<byte>();
			}
		}

		public static bool CheckAssetIntegrity(this ITexture3D texture)
		{
			if (texture.ImageData_C117.Length > 0)
			{
				return true;
			}
			else if (texture.Has_StreamData_C117())
			{
				return texture.StreamData_C117.CheckIntegrity(texture.Collection);
			}
			else
			{
				return false;
			}
		}

		public static TextureFormat GetTextureFormat(this ITexture3D texture)
		{
			if (texture.Has_Format_C117_Int32())
			{
				return texture.Format_C117_Int32E;
			}
			else
			{
				return texture.Format_C117_UInt32E;
			}
		}

		public static TextureUsageMode GetLightmapFormat(this ITexture3D texture)
		{
			if (texture.Has_LightmapFormat_C117())
			{
				return texture.LightmapFormat_C117E;
			}
			else
			{
				return (TextureUsageMode)texture.UsageMode_C117;
			}
		}

		public static int GetCompleteImageSize(this ITexture3D texture)
		{
			if (texture.Has_DataSize_C117())
			{
				return (int)texture.DataSize_C117;
			}
			else
			{
				return texture.CompleteImageSize_C117;
			}
		}

		public static int GetMipCount(this ITexture3D texture)
		{
			if (texture.Has_MipCount_C117())
			{
				return texture.MipCount_C117;
			}
			else
			{
				if (texture.MipMap_C117)
				{
					int maxSide = Math.Max(texture.Width_C117, texture.Height_C117);
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
