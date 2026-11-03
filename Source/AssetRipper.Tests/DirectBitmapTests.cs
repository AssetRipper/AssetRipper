using AssetRipper.Export.Modules.Textures;
using AssetRipper.TextureDecoder.Rgb.Formats;
using System.Runtime.CompilerServices;

namespace AssetRipper.Tests;

public class DirectBitmapTests
{
	[Test]
	public async Task FlipXReversesEveryRowInEveryLayer()
	{
		DirectBitmap<ColorRGBA<byte>, byte> bitmap = new(2, 2, 2, CreatePixelData(0, 1, 2, 3, 4, 5, 6, 7));

		bitmap.FlipX();

		await Assert.That(bitmap.Bits.ToArray().SequenceEqual(CreatePixelData(1, 0, 3, 2, 5, 4, 7, 6))).IsTrue();
	}

	[Test]
	public async Task TransposeSwapsEachOffDiagonalPairOnce()
	{
		DirectBitmap<ColorRGBA<byte>, byte> bitmap = new(2, 2, 1, CreatePixelData(0, 1, 2, 3));

		bitmap.Transpose();

		await Assert.That(bitmap.Bits.ToArray().SequenceEqual(CreatePixelData(0, 2, 1, 3))).IsTrue();
	}

	private static byte[] CreatePixelData(params byte[] pixelValues)
	{
		byte[] data = new byte[pixelValues.Length * Unsafe.SizeOf<ColorRGBA<byte>>()];
		for (int i = 0; i < pixelValues.Length; i++)
		{
			data.AsSpan(i * Unsafe.SizeOf<ColorRGBA<byte>>(), Unsafe.SizeOf<ColorRGBA<byte>>()).Fill(pixelValues[i]);
		}
		return data;
	}
}