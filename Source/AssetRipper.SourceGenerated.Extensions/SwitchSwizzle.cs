using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Enums;
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
	
	private static int CeilDivide(int a, int b)
	{
		return (a + b - 1) / b;
	}
	
	public static byte[] Unswizzle(ITexture2D texture, byte[] data)
	{
		TextureFormat realFormat = GetCorrectedSwitchTextureFormat(texture.Format_C28E);
		
		// Format is unsupported, we back out
		if (GetTextureFormatBlockSize(realFormat) is not { } blockSize)
		{
			return data;
		}

		byte[] newData = new byte[data.Length];
		
		int blockHeight = GetBlockHeightByPlatformBlob(texture.PlatformBlob_C28);
		
		Size paddedSize = GetPaddedTextureSize(texture.Width_C28, texture.Height_C28, blockSize.Width, blockSize.Height, blockHeight);

		int blockCountX = CeilDivide(paddedSize.Width, blockSize.Width);
		int blockCountY = CeilDivide(paddedSize.Height, blockSize.Height);

		int gobCountX = blockCountX / GobXTexelCount;
		int gobCountY = blockCountY / GobYTexelCount;

		int srcPos = 0;
		for (int i = 0; i < gobCountY / blockHeight; i++)
		{
			for (int j = 0; j < gobCountX; j++)
			{
				for (int k = 0; k < blockHeight; k++)
				{
					for (int l = 0; l < BlocksInGob; l++)
					{
						int gobX = GobXPoses[l];
						int gobY = GobYPoses[l];
						int gobDstX = j * GobXTexelCount + gobX;
						int gobDstY = (i * blockHeight + k) * GobYTexelCount + gobY;
						int gobDstLinPos = gobDstY * blockCountX * TexelByteSize + gobDstX * TexelByteSize;

						Array.Copy(data, srcPos, newData, gobDstLinPos, TexelByteSize);

						srcPos += TexelByteSize;
					}
				}
			}
		}

		return newData;
	}

	private static Size? GetTextureFormatBlockSize(TextureFormat textureFormat) 
	{
		return textureFormat switch
		{
			TextureFormat.Alpha8 => new Size(16, 1),
			TextureFormat.ARGB4444 => new Size(8, 1),
			TextureFormat.RGBA32 => new Size(4, 1),
			TextureFormat.ARGB32 => new Size(4, 1),
			TextureFormat.RGB565 => new Size(8, 1),
			TextureFormat.R16 => new Size(8, 1),
			TextureFormat.DXT1 => new Size(8, 4),
			TextureFormat.DXT5 => new Size(4, 4),
			TextureFormat.RGBA4444 => new Size(8, 1),
			TextureFormat.BC6H => new Size(4, 4),
			TextureFormat.BC7 => new Size(4, 4),
			TextureFormat.BC4 => new Size(8, 4),
			TextureFormat.BC5 => new Size(4, 4),
			TextureFormat.ASTC_RGB_4x4 => new Size(4, 4),
			TextureFormat.ASTC_RGB_5x5 => new Size(5, 5),
			TextureFormat.ASTC_RGB_6x6 => new Size(6, 6),
			TextureFormat.ASTC_RGB_8x8 => new Size(8, 8),
			TextureFormat.ASTC_RGB_10x10 => new Size(10, 10),
			TextureFormat.ASTC_RGB_12x12 => new Size(12, 12),
			TextureFormat.ASTC_RGBA_4x4 => new Size(4, 4),
			TextureFormat.ASTC_RGBA_5x5 => new Size(5, 5),
			TextureFormat.ASTC_RGBA_6x6 => new Size(6, 6),
			TextureFormat.ASTC_RGBA_8x8 => new Size(8, 8),
			TextureFormat.ASTC_RGBA_10x10 => new Size(10, 10),
			TextureFormat.ASTC_RGBA_12x12 => new Size(12, 12),
			TextureFormat.RG16 => new Size(8, 1),
			TextureFormat.R8 => new Size(16, 1),
			_ => null,
		};
	}
	
	private static Size GetPaddedTextureSize(int width, int height, int blockWidth, int blockHeight, int gobsPerBlock)
	{
		width = CeilDivide(width, blockWidth * GobXTexelCount) * blockWidth * GobXTexelCount;
		height = CeilDivide(height, blockHeight * GobYTexelCount * gobsPerBlock) * blockHeight * GobYTexelCount * gobsPerBlock;
		return new Size(width, height);
	}

	private static int GetBlockHeightByPlatformBlob(byte[] platformBlob)
	{
		return 1 << BitConverter.ToInt32(platformBlob, 8);
	}

	private static TextureFormat GetCorrectedSwitchTextureFormat(TextureFormat format)
	{
		return format switch
		{
			TextureFormat.RGB24 => TextureFormat.RGBA32,
			_ => format
		};
	}
}
