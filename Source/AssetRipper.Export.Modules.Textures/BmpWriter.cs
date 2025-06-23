using System.Runtime.CompilerServices;

namespace AssetRipper.Export.Modules.Textures;

/// <summary>
/// This is really fast, even in comparison to System.Drawing<br/>
/// It could be even faster if it didn't have to flip the pixels in the y direction
/// </summary>
/// <remarks>
/// <see href="https://en.wikipedia.org/wiki/BMP_file_format"/>
/// </remarks>
internal static class BmpWriter
{
	/*
	static byte[] bmpData = // All values are little-endian
	{
		//14 bytes
		0x42, 0x4D,             // Signature 'BM'
		0x9a, 0x00, 0x00, 0x00, // Size: 154 bytes
		0x00, 0x00,             // Unused
		0x00, 0x00,             // Unused
		0x7a, 0x00, 0x00, 0x00, // Offset to image data, ie 122

		//108 bytes
		0x6c, 0x00, 0x00, 0x00, // DIB header size (108 bytes)
		0x04, 0x00, 0x00, 0x00, // Width (4px)
		0x02, 0x00, 0x00, 0x00, // Height (2px)
		0x01, 0x00,             // Planes (1)
		0x20, 0x00,             // Bits per pixel (32)
		0x03, 0x00, 0x00, 0x00, // Format (bitfield = use bitfields | no compression)
		0x20, 0x00, 0x00, 0x00, // Image raw size (32 bytes)
		0x13, 0x0B, 0x00, 0x00, // Horizontal print resolution (2835 = 72dpi * 39.3701)
		0x13, 0x0B, 0x00, 0x00, // Vertical print resolution (2835 = 72dpi * 39.3701)
		0x00, 0x00, 0x00, 0x00, // Colors in palette (none)
		0x00, 0x00, 0x00, 0x00, // Important colors (0 = all)
		0x00, 0x00, 0xFF, 0x00, // R bitmask (00FF0000)
		0x00, 0xFF, 0x00, 0x00, // G bitmask (0000FF00)
		0xFF, 0x00, 0x00, 0x00, // B bitmask (000000FF)
		0x00, 0x00, 0x00, 0xFF, // A bitmask (FF000000)
		0x42, 0x47, 0x52, 0x73, // sRGB color space
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Unused R entry for color space
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Unused G entry for color space
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Unused B entry for color space
		0x00, 0x00, 0x00, 0x00, // Unused Gamma X entry for color space
		0x00, 0x00, 0x00, 0x00, // Unused Gamma Y entry for color space
		0x00, 0x00, 0x00, 0x00, // Unused Gamma Z entry for color space

		// Image data: 32 bytes
		0xFF, 0x00, 0x00, 0x7F, // Bottom left pixel
		0x00, 0xFF, 0x00, 0x7F,
		0x00, 0x00, 0xFF, 0x7F,
		0xFF, 0xFF, 0xFF, 0x7F, // Bottom right pixel
		0xFF, 0x00, 0x00, 0xFF, // Top left pixel
		0x00, 0xFF, 0x00, 0xFF,
		0x00, 0x00, 0xFF, 0xFF,
		0xFF, 0xFF, 0xFF, 0xFF  // Top right pixel
	};
	*/

	public static void WriteBmp(byte[] bgra32Data, int width, int height, Stream stream, bool flip = true)
	{
		ArgumentNullException.ThrowIfNull(bgra32Data);
		ArgumentNullException.ThrowIfNull(stream);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
		ThrowIfIncorrectLength(bgra32Data, width, height);

		using BinaryWriter writer = new BinaryWriter(stream);

		//14 bytes
		writer.WriteBytes(0x42, 0x4D); // Signature 'BM'
		writer.Write(GetTotalSize(width, height)); // Size of the file
		writer.WriteZeroBytes(4); // 2 unused shorts
		writer.Write((uint)(14 + (16 + 92))); // offset to image data

		//16 bytes
		writer.Write((uint)108); // DIB header size (108 bytes)
		writer.Write(width); // Width
		writer.Write(height); // Height
		writer.Write((ushort)1); // Planes (1)
		writer.Write((ushort)32); // Bits per pixel (32)

		//92 bytes
		writer.WriteBytes(0x03, 0x00, 0x00, 0x00); // Format (bitfield = use bitfields | no compression)
		writer.Write(GetRawImageSize(width, height)); // Image raw size in bytes
		writer.WriteBytes(0x13, 0x0B, 0x00, 0x00); // Horizontal print resolution (2835 = 72dpi * 39.3701)
		writer.WriteBytes(0x13, 0x0B, 0x00, 0x00); // Vertical print resolution (2835 = 72dpi * 39.3701)
		writer.WriteZeroBytes(4); // Colors in palette (none)
		writer.WriteZeroBytes(4); // Important colors (0 = all)
		writer.WriteBytes(0x00, 0x00, 0xFF, 0x00); // R bitmask (00FF0000)
		writer.WriteBytes(0x00, 0xFF, 0x00, 0x00); // G bitmask (0000FF00)
		writer.WriteBytes(0xFF, 0x00, 0x00, 0x00); // B bitmask (000000FF)
		writer.WriteBytes(0x00, 0x00, 0x00, 0xFF); // A bitmask (FF000000)
		writer.WriteBytes(0x42, 0x47, 0x52, 0x73); // sRGB color space
		writer.WriteZeroBytes(12); // Unused R entry for color space
		writer.WriteZeroBytes(12); // Unused G entry for color space
		writer.WriteZeroBytes(12); // Unused B entry for color space
		writer.WriteZeroBytes(4); // Unused Gamma X entry for color space
		writer.WriteZeroBytes(4); // Unused Gamma Y entry for color space
		writer.WriteZeroBytes(4); // Unused Gamma Z entry for color space

		if (flip)
		{
			writer.WriteFlippedY(bgra32Data, width, height);
		}
		else
		{
			writer.Write(bgra32Data.AsSpan());
		}

		static void ThrowIfIncorrectLength(byte[] bgra32Data, int width, int height, [CallerArgumentExpression(nameof(bgra32Data))] string? paramName = null)
		{
			if (bgra32Data.Length != GetRawImageSize(width, height))
			{
				throw new ArgumentException("Length must match 4 * width * height", paramName);
			}
		}
	}

	private static void WriteFlippedY(this BinaryWriter writer, byte[] data, int width, int height)
	{
		for (int r = height - 1; r >= 0; r--)
		{
			writer.Write(data.AsSpan(r * width * 4, width * 4));
		}
	}

	private static void WriteBytes(this BinaryWriter writer, byte byte0, byte byte1)
	{
		writer.Write(byte0);
		writer.Write(byte1);
	}

	private static void WriteBytes(this BinaryWriter writer, byte byte0, byte byte1, byte byte2, byte byte3)
	{
		writer.Write(byte0);
		writer.Write(byte1);
		writer.Write(byte2);
		writer.Write(byte3);
	}

	private static void WriteZeroBytes(this BinaryWriter writer, int count)
	{
		for (int i = 0; i < count; i++)
		{
			writer.Write((byte)0);
		}
	}

	private static int GetTotalSize(int width, int height)
	{
		return 14 + 108 + GetRawImageSize(width, height);
	}

	private static int GetRawImageSize(int width, int height)
	{
		return 4 * width * height;
	}
}
