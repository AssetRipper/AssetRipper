using AssetRipper.Export.Modules.Textures;
using AssetRipper.TextureDecoder.Rgb.Formats;
using System.Runtime.CompilerServices;

namespace AssetRipper.Tests;

public class DirectBitmapTests
{
	[Test]
	public void FlipXReversesEveryRowInEveryLayer()
	{
		DirectBitmap<ColorRGBA<byte>, byte> bitmap = new(2, 2, 2, CreatePixelData(0, 1, 2, 3, 4, 5, 6, 7));

		bitmap.FlipX();

		Assert.That(bitmap.Bits.ToArray(), Is.EqualTo(CreatePixelData(1, 0, 3, 2, 5, 4, 7, 6)));
	}

	[Test]
	public void TransposeSwapsEachOffDiagonalPairOnce()
	{
		DirectBitmap<ColorRGBA<byte>, byte> bitmap = new(2, 2, 1, CreatePixelData(0, 1, 2, 3));

		bitmap.Transpose();

		Assert.That(bitmap.Bits.ToArray(), Is.EqualTo(CreatePixelData(0, 2, 1, 3)));
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
