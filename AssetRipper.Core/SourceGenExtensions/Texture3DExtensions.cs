using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.SourceGenerated.Classes.ClassID_117;

namespace AssetRipper.Core.SourceGenExtensions
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
				return texture.StreamData_C117.GetContent(texture.SerializedFile);
			}
			else
			{
				return Array.Empty<byte>();
			}
		}

		public static ColorSpace GetColorSpace(this ITexture3D texture)
		{
			return (ColorSpace)texture.ColorSpace_C117;
		}

		public static SourceGenerated.Enums.TextureFormat GetTextureFormat(this ITexture3D texture)
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

		public static TextureDimension GetTextureDimension(this ITexture3D texture)
		{
			return (TextureDimension)texture.Dimension_C117;
		}

		public static TextureUsageMode GetLightmapFormat(this ITexture3D texture)
		{
			return (TextureUsageMode)texture.LightmapFormat_C117;
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
					int maxSide = System.Math.Max(texture.Width_C117, texture.Height_C117);
					return Convert.ToInt32(System.Math.Log(maxSide) / System.Math.Log(2));
				}
				else
				{
					return 1;
				}
			}
		}
	}
}
