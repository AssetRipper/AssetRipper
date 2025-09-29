using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Enums;
using BuildTarget = AssetRipper.IO.Files.BuildTarget;

namespace AssetRipper.SourceGenerated.Extensions;

public static class Texture2DExtensions
{
	extension(ITexture2D texture)
	{
		public int CompleteImageSize
		{
			get
			{
				if (texture.Has_CompleteImageSize_C28_UInt32())
				{
					return (int)texture.CompleteImageSize_C28_UInt32;//No texture is larger than 2GB
				}
				else
				{
					return texture.CompleteImageSize_C28_Int32;
				}
			}
		}

		/// <summary>
		/// Get the CompleteImageSize, or a calculated value if necessary
		/// </summary>
		/// <remarks>
		/// <see href="https://github.com/AssetRipper/AssetRipper/issues/1789"/>
		/// </remarks>
		public int ActualImageSize
		{
			get
			{
				int completeImageSize = texture.CompleteImageSize;
				if (completeImageSize > 0)
				{
					return completeImageSize;
				}

				// Normally, the complete image size is non-zero, so this only applies in certain situations.
				// One way this can happen is for server builds of a game.
				// In server builds, textures are often 1 pixel and have an image size equal to be zero.
				// However, they still contain a small amount of texture data, so the image size is actual non-zero.
				// We calculate the actual value below.

				int dataLength = texture.ImageDataLength;
				int count = texture.ImageCount_C28;
				if (count > 1 && dataLength % count == 0)
				{
					return dataLength / count;
				}
				else
				{
					return dataLength;
				}
			}
		}

		public bool Mips => texture.MipMap_C28 || texture.MipCount_C28 > 0;

		public bool CheckAssetIntegrity()
		{
			if (!texture.ImageData_C28.IsNullOrEmpty())
			{
				return true;
			}
			else if (texture.StreamData_C28 is not null)
			{
				return texture.StreamData_C28.CheckIntegrity(texture.Collection);
			}
			else
			{
				return false;
			}
		}

		public byte[] GetImageData()
		{
			byte[] data = texture.ImageData_C28;

			bool swapBytes = IsSwapBytes(texture.Collection.Platform, texture.Format_C28E);
			bool switchSwizzled = texture is { PlatformBlob_C28: { Length: >= 12 }, Collection.Platform: BuildTarget.Switch };

			if (data.Length != 0)
			{
				if (swapBytes)
				{
					// Need to copy the data to avoid modifying the original
					data = data.AsSpan().ToArray();
				}
			}
			else if (texture.StreamData_C28 is not null && texture.StreamData_C28.IsSet())
			{
				data = texture.StreamData_C28.GetContent(texture.Collection);
			}

			if (swapBytes)
			{
				for (int i = 0; i < data.Length; i += 2)
				{
					(data[i], data[i + 1]) = (data[i + 1], data[i]);
				}
			}

			if (switchSwizzled)
			{
				return SwitchSwizzle.Unswizzle(texture, data);
			}

			return data;
		}

		public int ImageDataLength
		{
			get
			{
				if (texture.ImageData_C28.Length != 0)
				{
					return texture.ImageData_C28.Length;
				}
				else if (texture.StreamData_C28 is not null && texture.StreamData_C28.IsSet())
				{
					return (int)texture.StreamData_C28.Size;
				}
				else
				{
					return 0;
				}
			}
		}
	}

	private static bool IsSwapBytes(IO.Files.BuildTarget platform, TextureFormat format)
	{
		if (platform == IO.Files.BuildTarget.XBox360)
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
