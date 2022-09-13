using AssetRipper.Core.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class Texture2DExtensions
	{
		public static long GetCompleteImageSize(this ITexture2D texture)
		{
			return System.Math.Max(texture.CompleteImageSize_C28_Int32, texture.CompleteImageSize_C28_UInt32);
		}

		public static bool CheckAssetIntegrity(this ITexture2D texture)
		{
			if (!texture.ImageData_C28.IsNullOrEmpty())
			{
				return true;
			}
			else if (texture.StreamData_C28 is not null)
			{
				return texture.StreamData_C28.CheckIntegrity(texture.SerializedFile);
			}
			else
			{
				return false;
			}
		}

		public static byte[] GetImageData(this ITexture2D texture)
		{
			byte[] data = texture.ImageData_C28;

			if (!data.IsNullOrEmpty())
			{
				return texture.ImageData_C28;
			}
			else if (texture.StreamData_C28 is not null && texture.StreamData_C28.IsSet())
			{
				data = texture.StreamData_C28.GetContent(texture.SerializedFile);
			}

			data ??= Array.Empty<byte>();

			if (IsSwapBytes(texture.SerializedFile.Platform, texture.Format_C28E))
			{
				for (int i = 0; i < data.Length; i += 2)
				{
					(data[i], data[i + 1]) = (data[i + 1], data[i]);
				}
			}

			return data;
		}

		public static bool IsSwapBytes(Parser.Files.BuildTarget platform, TextureFormat format)
		{
			if (platform == Parser.Files.BuildTarget.XBox360)
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
