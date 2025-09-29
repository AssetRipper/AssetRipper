using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Enums;
using System.Drawing;

namespace AssetRipper.SourceGenerated.Extensions;

public class SwitchSwizzle
{
	private const int GOB_X_TEXEL_COUNT = 4;
	private const int GOB_Y_TEXEL_COUNT = 8;
	private const int TEXEL_BYTE_SIZE = 16;
	
	/// <summary>
	/// In this case, "block" refers to a compressed texture block, not a gob one.
	/// </summary>
	private const int BLOCKS_IN_GOB = GOB_X_TEXEL_COUNT * GOB_Y_TEXEL_COUNT;
	
	private static readonly int[] GOB_X_POSES = new int[]
	{
		0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3, 2, 2, 3, 3
	};
	
	private static readonly int[] GOB_Y_POSES = new int[]
	{
		0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7, 0, 1, 0, 1, 2, 3, 2, 3, 4, 5, 4, 5, 6, 7, 6, 7
	};
	
	private readonly Size originalSize;
	private readonly Size paddedSize;
	private readonly Size blockSize;
	private readonly int gobsPerBlock;
	private readonly TextureFormat realFormat;

	public SwitchSwizzle(ITexture2D texture)
	{
		originalSize = new Size(texture.Width_C28, texture.Height_C28);
		realFormat = GetCorrectedSwitchTextureFormat(texture.Format_C28E);
		gobsPerBlock = GetBlockHeightByPlatformBlob(texture.PlatformBlob_C28);

		blockSize = GetTextureFormatBlockSize(realFormat);
		paddedSize = GetPaddedTextureSize(originalSize.Width, originalSize.Height, blockSize.Width, blockSize.Height, gobsPerBlock);
	}
	
	private static int CeilDivide(int a, int b)
	{
		return (a + b - 1) / b;
	}
	
	public byte[] PreprocessDeswizzle(byte[] rawData, out TextureFormat format, out int width, out int height)
	{
		format = realFormat;
		width = paddedSize.Width;
		height = paddedSize.Height;
		return Unswizzle(rawData, paddedSize, blockSize, gobsPerBlock);
	}
	
	public static byte[] Unswizzle(byte[] data, Size imageSize, Size blockSize, int blockHeight)
	{
		byte[] newData = new byte[data.Length];

		int width = imageSize.Width;
		int height = imageSize.Height;

		int blockCountX = CeilDivide(width, blockSize.Width);
		int blockCountY = CeilDivide(height, blockSize.Height);

		int gobCountX = blockCountX / GOB_X_TEXEL_COUNT;
		int gobCountY = blockCountY / GOB_Y_TEXEL_COUNT;

		int srcPos = 0;
		for (int i = 0; i < gobCountY / blockHeight; i++)
		{
			for (int j = 0; j < gobCountX; j++)
			{
				for (int k = 0; k < blockHeight; k++)
				{
					for (int l = 0; l < BLOCKS_IN_GOB; l++)
					{
						int gobX = GOB_X_POSES[l];
						int gobY = GOB_Y_POSES[l];
						int gobDstX = j * GOB_X_TEXEL_COUNT + gobX;
						int gobDstY = (i * blockHeight + k) * GOB_Y_TEXEL_COUNT + gobY;
						int gobDstLinPos = gobDstY * blockCountX * TEXEL_BYTE_SIZE + gobDstX * TEXEL_BYTE_SIZE;

						Array.Copy(data, srcPos, newData, gobDstLinPos, TEXEL_BYTE_SIZE);

						srcPos += TEXEL_BYTE_SIZE;
					}
				}
			}
		}

		return newData;
	}

	private static Size GetTextureFormatBlockSize(TextureFormat textureFormat)
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
			_ => throw new NotImplementedException(),
		};
	}
	
	public static Size GetPaddedTextureSize(int width, int height, int blockWidth, int blockHeight, int gobsPerBlock)
	{
		width = CeilDivide(width, blockWidth * GOB_X_TEXEL_COUNT) * blockWidth * GOB_X_TEXEL_COUNT;
		height = CeilDivide(height, blockHeight * GOB_Y_TEXEL_COUNT * gobsPerBlock) * blockHeight * GOB_Y_TEXEL_COUNT * gobsPerBlock;
		return new Size(width, height);
	}
	
	public static int GetBlockHeightByBlockSize(Size blockSize, int imageHeight)
	{
		int blockHeight = CeilDivide(imageHeight, blockSize.Height);
		int heightAndHalf = blockHeight + blockHeight / 2;
		
		return heightAndHalf switch
		{
			>= 128 => 16,
			>= 64 => 8,
			>= 32 => 4,
			>= 16 => 2,
			_ => 1
		};
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
