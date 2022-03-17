using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Parser.Files;
using System;

namespace AssetRipper.Core.Classes.Texture2D
{
	public interface ITexture2D : INamedObject //, IHasImageData
	{
		byte[] ImageData { get; set; }
		TextureFormat TextureFormat { get; set; }
		int Width { get; set; }
		int Height { get; set; }
		/// <summary>
		/// Actually int before 2020 and uint afterwards
		/// </summary>
		long CompleteImageSize { get; set; }
		/// <summary>
		/// Formerly bool MipMap
		/// </summary>
		int MipCount { get; set; }
		TextureDimension TextureDimension { get; set; }
		TextureUsageMode LightmapFormat { get; set; }
		ColorSpace ColorSpace { get; set; }
		bool StreamingMipmaps { get; set; }
		int StreamingMipmapsPriority { get; set; }
		bool AlphaIsTransparency { get; set; }
		bool IsReadable { get; set; }
		IStreamingInfo StreamData { get; }
		IGLTextureSettings TextureSettings { get; }
	}

	public static class Texture2DExtensions
	{
		public static bool CheckAssetIntegrity(this ITexture2D texture)
		{
			if (!texture.ImageData.IsNullOrEmpty())
			{
				return true;
			}
			else if (texture.StreamData != null)
			{
				return texture.StreamData.CheckIntegrity(texture.SerializedFile);
			}
			else
			{
				return false;
			}
		}

		public static byte[] GetImageData(this ITexture2D texture)
		{
			byte[] data = texture.ImageData;

			if (!data.IsNullOrEmpty())
			{
				return texture.ImageData;
			}
			else if (texture.StreamData != null && texture.StreamData.IsSet())
			{
				data = texture.StreamData.GetContent(texture.SerializedFile);
			}

			data ??= Array.Empty<byte>();

			if (IsSwapBytes(texture.SerializedFile.Platform, texture.TextureFormat))
			{
				for (int i = 0; i < data.Length; i += 2)
				{
					(data[i], data[i + 1]) = (data[i + 1], data[i]);
				}
			}

			return data;
		}

		public static bool IsSwapBytes(Platform platform, TextureFormat format)
		{
			if (platform == Platform.XBox360)
			{
				switch (format)
				{
					case TextureFormat.ARGB4444:
					case TextureFormat.RGB565:
					case TextureFormat.DXT1:
					case TextureFormat.DXT1Crunched:
					case TextureFormat.DXT3:
					case TextureFormat.DXT5:
					case TextureFormat.DXT5Crunched:
						return true;
				}
			}
			return false;
		}

	}
}
