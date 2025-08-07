using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

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

	extension (ITexture3D texture)
	{
		public int Format
		{
			get
			{
				if (texture.Has_Format_Int32())
				{
					return texture.Format_Int32;
				}
				else
				{
					return unchecked((int)texture.Format_UInt32);
				}
			}
			set
			{
				if (texture.Has_Format_Int32())
				{
					texture.Format_Int32 = value;
				}
				else
				{
					texture.Format_UInt32 = unchecked((uint)value);
				}
			}
		}
	}

	public static TextureFormat GetTextureFormat(this ITexture3D texture)
	{
		if (texture.Has_Format_Int32())
		{
			return ((GraphicsFormat)texture.Format_Int32).ToTextureFormat();
		}
		else
		{
			return ((GraphicsFormat)texture.Format_UInt32).ToTextureFormat();
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
		if (texture.Has_CompleteImageSize_UInt32())
		{
			return (int)texture.CompleteImageSize_UInt32;
		}
		else
		{
			return texture.CompleteImageSize_Int32;
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
