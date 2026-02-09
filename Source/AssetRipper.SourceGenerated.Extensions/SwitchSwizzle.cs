using AssetRipper.IO.Endian;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Enums;
using System.Diagnostics;
using System.Drawing;

namespace AssetRipper.SourceGenerated.Extensions;

public static class SwitchSwizzle
{
	private const int GobXTexelCount = 4;
	private const int GobYTexelCount = 8;
	private const int TexelByteSize = 16;

	/// <summary>
	/// In this case, "block" refers to a compressed texture block, not a gob one.
	/// </summary>
	private const int BlocksInGob = GobXTexelCount * GobYTexelCount;

	private static readonly int[] GobXPoses =
	[
		0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3
	];

	private static readonly int[] GobYPoses =
	[
		0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7, 0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7
	];

	/// <summary>
	/// Calculates the smallest integer greater than or equal to the result of dividing two integers.
	/// </summary>
	/// <param name="a">The dividend to be divided.</param>
	/// <param name="b">The divisor by which to divide. Must not be zero.</param>
	/// <returns>The smallest integer greater than or equal to <paramref name="a"/> divided by <paramref name="b"/>.</returns>
	private static int CeilDivide(int a, int b)
	{
		return (a + b - 1) / b;
	}

	public static byte[] Unswizzle(ITexture2D texture, byte[] data)
	{
		if (texture.ImageCount_C28 != 1)
		{
			return data;
		}

		TextureFormat realFormat = GetCorrectedSwitchTextureFormat(texture.Format_C28E);

		// Format is unsupported, we back out
		if (GetBlockDimensions(realFormat) is not { } blockDimensions)
		{
			return data;
		}

		int blockByteSize = GetBlockSize(realFormat);

		Debug.Assert(TexelByteSize % blockByteSize == 0, "Texel size should be a multiple of block size.");
		int blocksPerTexel = TexelByteSize / blockByteSize;

		Debug.Assert(blocksPerTexel > 0);
		Size texelDimensions = new(blockDimensions.Width * blocksPerTexel, blockDimensions.Height);

		byte[] newData = new byte[data.Length];

		Debug.Assert(texture.PlatformBlob_C28 is not null && texture.PlatformBlob_C28.Length >= 12);
		int texelHeight = GetTexelHeightByPlatformBlob(texture.PlatformBlob_C28, texture.Collection.EndianType);

		Size paddedDimensions = GetPaddedTextureDimensions(texture.Width_C28, texture.Height_C28, texelDimensions.Width, texelDimensions.Height, texelHeight);

		int texelCountX = CeilDivide(paddedDimensions.Width, texelDimensions.Width);
		int texelCountY = CeilDivide(paddedDimensions.Height, texelDimensions.Height);

		int gobCountX = texelCountX / GobXTexelCount;
		int gobCountY = texelCountY / GobYTexelCount;

		int srcPos = 0;
		for (int i = 0; i < gobCountY / texelHeight; i++)
		{
			for (int j = 0; j < gobCountX; j++)
			{
				for (int k = 0; k < texelHeight; k++)
				{
					for (int l = 0; l < BlocksInGob; l++)
					{
						int gobX = GobXPoses[l];
						int gobY = GobYPoses[l];
						int gobDstX = j * GobXTexelCount + gobX;
						int gobDstY = (i * texelHeight + k) * GobYTexelCount + gobY;
						int gobDstLinPos = gobDstY * texelCountX * TexelByteSize + gobDstX * TexelByteSize;

						data.AsSpan(srcPos, TexelByteSize).CopyTo(newData.AsSpan(gobDstLinPos, TexelByteSize));

						srcPos += TexelByteSize;
					}
				}
			}
		}

		// We need to remove padding from the data now, so that it matches the expected layout for later decoding.
		// The resulting arrays will have extra unused data at the end, but we need to leave it there
		// because TextureConverter expects the data to be at least texture.CompleteImageSize bytes long.

		byte[] result = newData;

		int srcBlockCountX = texelCountX * blocksPerTexel;
		int dstBlockCountX = CeilDivide(texture.Width_C28, blockDimensions.Width);
		if (dstBlockCountX != srcBlockCountX)
		{
			byte[] croppedData = new byte[newData.Length];
			int dstBlockCountY = CeilDivide(texture.Height_C28, blockDimensions.Height);
			for (int y = 0; y < dstBlockCountY; y++)
			{
				newData.AsSpan(y * srcBlockCountX * blockByteSize, dstBlockCountX * blockByteSize).CopyTo(croppedData.AsSpan(y * dstBlockCountX * blockByteSize));
			}

			result = croppedData;
		}

		if (realFormat != texture.Format_C28E)
		{
			// Convert RGBA to RGB
			Debug.Assert(blockByteSize % 4 == 0);
			Debug.Assert(blockDimensions.Height == 1);
			int actualBlockByteSize = blockByteSize / 4 * 3;
			for (int y = 0; y < texture.Height_C28; y++)
			{
				int srcOffset = y * texture.Width_C28 * blockByteSize;
				int dstOffset = y * texture.Width_C28 * actualBlockByteSize;
				for (int x = 0; x < texture.Width_C28; x++)
				{
					int srcIndex = srcOffset + x * blockByteSize;
					int dstIndex = dstOffset + x * actualBlockByteSize;

					// Because we're removing data, srcIndex should always be greater than or equal to dstIndex.
					// This allows us to safely copy in-place.
					Debug.Assert(srcIndex >= dstIndex);
					result.AsSpan(srcIndex, actualBlockByteSize).CopyTo(result.AsSpan(dstIndex, actualBlockByteSize));
				}
			}

			// Clear out the unused data at the end
			int newSize = texture.Width_C28 * texture.Height_C28 * actualBlockByteSize;
			result.AsSpan(newSize).Clear();
		}

		return result;
	}

	private static Size? GetBlockDimensions(TextureFormat textureFormat) => textureFormat switch
	{
		TextureFormat.Alpha8 => new Size(1, 1),
		TextureFormat.ARGB4444 => new Size(1, 1),
		TextureFormat.RGBA4444 => new Size(1, 1),
		TextureFormat.ARGB32 => new Size(1, 1),
		TextureFormat.BGRA32_14 or TextureFormat.BGRA32_37 => new Size(1, 1),
		TextureFormat.RGB565 => new Size(1, 1),
		TextureFormat.DXT1 => new Size(4, 4),
		TextureFormat.DXT5 => new Size(4, 4),
		TextureFormat.BC4 => new Size(4, 4),
		TextureFormat.BC5 => new Size(4, 4),
		TextureFormat.BC6H => new Size(4, 4),
		TextureFormat.BC7 => new Size(4, 4),
		TextureFormat.ASTC_RGB_4x4 or TextureFormat.ASTC_RGBA_4x4 or TextureFormat.ASTC_HDR_4x4 => new Size(4, 4),
		TextureFormat.ASTC_RGB_5x5 or TextureFormat.ASTC_RGBA_5x5 or TextureFormat.ASTC_HDR_5x5 => new Size(5, 5),
		TextureFormat.ASTC_RGB_6x6 or TextureFormat.ASTC_RGBA_6x6 or TextureFormat.ASTC_HDR_6x6 => new Size(6, 6),
		TextureFormat.ASTC_RGB_8x8 or TextureFormat.ASTC_RGBA_8x8 or TextureFormat.ASTC_HDR_8x8 => new Size(8, 8),
		TextureFormat.ASTC_RGB_10x10 or TextureFormat.ASTC_RGBA_10x10 or TextureFormat.ASTC_HDR_10x10 => new Size(10, 10),
		TextureFormat.ASTC_RGB_12x12 or TextureFormat.ASTC_RGBA_12x12 or TextureFormat.ASTC_HDR_12x12 => new Size(12, 12),
		TextureFormat.R8 or TextureFormat.R8_SIGNED => new Size(1, 1),
		TextureFormat.RG16 or TextureFormat.RG16_SIGNED => new Size(1, 1),
		TextureFormat.RGBA32 or TextureFormat.RGBA32_SIGNED => new Size(1, 1),
		TextureFormat.R16 or TextureFormat.R16_SIGNED or TextureFormat.RHalf => new Size(1, 1),
		TextureFormat.RG32 or TextureFormat.RG32_SIGNED or TextureFormat.RGHalf => new Size(1, 1),
		TextureFormat.RGBA64 or TextureFormat.RGBA64_SIGNED or TextureFormat.RGBAHalf => new Size(1, 1),
		TextureFormat.RFloat => new Size(1, 1),
		TextureFormat.RGFloat => new Size(1, 1),
		TextureFormat.RGBAFloat => new Size(1, 1),
		TextureFormat.ARGBFloat => new Size(1, 1),
		_ => null,
	};

	private static int GetBlockSize(TextureFormat textureFormat) => textureFormat switch
	{
		TextureFormat.Alpha8 => 1,
		TextureFormat.ARGB4444 => 2,
		TextureFormat.RGBA4444 => 2,
		TextureFormat.ARGB32 => 4,
		TextureFormat.BGRA32_14 or TextureFormat.BGRA32_37 => 4,
		TextureFormat.RGB565 => 2,
		TextureFormat.DXT1 => 8,
		TextureFormat.DXT5 => 16,
		TextureFormat.BC4 => 8,
		TextureFormat.BC5 => 16,
		TextureFormat.BC6H => 16,
		TextureFormat.BC7 => 16,
		TextureFormat.ASTC_RGB_4x4 or TextureFormat.ASTC_RGBA_4x4 or TextureFormat.ASTC_HDR_4x4 => 16,
		TextureFormat.ASTC_RGB_5x5 or TextureFormat.ASTC_RGBA_5x5 or TextureFormat.ASTC_HDR_5x5 => 16,
		TextureFormat.ASTC_RGB_6x6 or TextureFormat.ASTC_RGBA_6x6 or TextureFormat.ASTC_HDR_6x6 => 16,
		TextureFormat.ASTC_RGB_8x8 or TextureFormat.ASTC_RGBA_8x8 or TextureFormat.ASTC_HDR_8x8 => 16,
		TextureFormat.ASTC_RGB_10x10 or TextureFormat.ASTC_RGBA_10x10 or TextureFormat.ASTC_HDR_10x10 => 16,
		TextureFormat.ASTC_RGB_12x12 or TextureFormat.ASTC_RGBA_12x12 or TextureFormat.ASTC_HDR_12x12 => 16,
		TextureFormat.R8 or TextureFormat.R8_SIGNED => 1,
		TextureFormat.RG16 or TextureFormat.RG16_SIGNED => 2,
		TextureFormat.RGBA32 or TextureFormat.RGBA32_SIGNED => 4,
		TextureFormat.R16 or TextureFormat.R16_SIGNED or TextureFormat.RHalf => 2,
		TextureFormat.RG32 or TextureFormat.RG32_SIGNED or TextureFormat.RGHalf => 4,
		TextureFormat.RGBA64 or TextureFormat.RGBA64_SIGNED or TextureFormat.RGBAHalf => 8,
		TextureFormat.RFloat => 4,
		TextureFormat.RGFloat => 8,
		TextureFormat.RGBAFloat => 16,
		TextureFormat.ARGBFloat => 16,
		_ => throw new NotSupportedException(),
	};

	private static Size GetPaddedTextureDimensions(int width, int height, int blockWidth, int blockHeight, int gobsPerBlock)
	{
		width = CeilDivide(width, blockWidth * GobXTexelCount) * blockWidth * GobXTexelCount;
		height = CeilDivide(height, blockHeight * GobYTexelCount * gobsPerBlock) * blockHeight * GobYTexelCount * gobsPerBlock;
		return new Size(width, height);
	}

	private static int GetTexelHeightByPlatformBlob(byte[] platformBlob, EndianType endianType)
	{
		return 1 << new EndianSpanReader(platformBlob.AsSpan(8), endianType).ReadInt32();
	}

	private static TextureFormat GetCorrectedSwitchTextureFormat(TextureFormat format) => format switch
	{
		TextureFormat.RGB24 => TextureFormat.RGBA32,
		TextureFormat.RGB48 => TextureFormat.RGBA64,
		TextureFormat.RGB24_SIGNED => TextureFormat.RGBA32_SIGNED,
		TextureFormat.RGB48_SIGNED => TextureFormat.RGBA64_SIGNED,
		TextureFormat.BGR24 => TextureFormat.BGRA32_14,
		_ => format
	};
}
