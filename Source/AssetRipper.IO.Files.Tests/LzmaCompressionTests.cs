using AssetRipper.IO.Files.BundleFiles;

namespace AssetRipper.IO.Files.Tests;

public static class LzmaCompressionTests
{
	[Test]
	public static void LzmaStreamSymmetryTest()
	{
		const int UncompressedSize = 4075;
		byte[] randomData = RandomData.MakeRandomData(UncompressedSize);

		MemoryStream uncompressedStream = new MemoryStream(randomData);
		MemoryStream compressedStream = new();
		long compressedSize = LzmaCompression.CompressLzmaStream(uncompressedStream, UncompressedSize, compressedStream);
		Assert.That(uncompressedStream.Position, Is.EqualTo(UncompressedSize));

		compressedStream.Position = 0;
		MemoryStream decompressedStream = new();
		LzmaCompression.DecompressLzmaStream(compressedStream, compressedSize, decompressedStream, UncompressedSize);
		byte[] decompressedData = decompressedStream.ToArray();

		Assert.That(decompressedData, Is.EqualTo(randomData));
	}

	[Test]
	public static void LzmaStreamSymmetryTestForPartialInputStream()
	{
		const int UncompressedSize = 4077;
		const int InitialOffset = 34;
		const int TrailingCount = 50;
		byte[] randomData = RandomData.MakeRandomData(InitialOffset + UncompressedSize + TrailingCount);

		MemoryStream uncompressedStream = new MemoryStream(randomData);
		uncompressedStream.Position += InitialOffset;

		MemoryStream compressedStream = new();
		long compressedSize = LzmaCompression.CompressLzmaStream(uncompressedStream, UncompressedSize, compressedStream);
		Assert.That(uncompressedStream.Position, Is.EqualTo(InitialOffset + UncompressedSize));

		compressedStream.Position = 0;
		MemoryStream decompressedStream = new();
		LzmaCompression.DecompressLzmaStream(compressedStream, compressedSize, decompressedStream, UncompressedSize);
		byte[] decompressedData = decompressedStream.ToArray();

		Assert.That((ArraySegment<byte>)decompressedData, Is.EqualTo(new ArraySegment<byte>(randomData, InitialOffset, UncompressedSize)));
	}

	[Test]
	public static void LzmaSizeStreamSymmetryTest()
	{
		const int UncompressedSize = 4067;
		byte[] randomData = RandomData.MakeRandomData(UncompressedSize);

		MemoryStream uncompressedStream = new MemoryStream(randomData);
		MemoryStream compressedStream = new();
		long compressedSize = LzmaCompression.CompressLzmaSizeStream(uncompressedStream, UncompressedSize, compressedStream);
		Assert.That(uncompressedStream.Position, Is.EqualTo(UncompressedSize));

		compressedStream.Position = 0;
		MemoryStream decompressedStream = new();
		LzmaCompression.DecompressLzmaSizeStream(compressedStream, compressedSize, decompressedStream);
		byte[] decompressedData = decompressedStream.ToArray();

		Assert.That(decompressedData, Is.EqualTo(randomData));
	}

	[Test]
	public static void LzmaSizeStreamSymmetryTestForPartialInputStream()
	{
		const int UncompressedSize = 4087;
		const int InitialOffset = 44;
		const int TrailingCount = 40;
		byte[] randomData = RandomData.MakeRandomData(InitialOffset + UncompressedSize + TrailingCount);

		MemoryStream uncompressedStream = new MemoryStream(randomData);
		uncompressedStream.Position += InitialOffset;

		MemoryStream compressedStream = new();
		long compressedSize = LzmaCompression.CompressLzmaSizeStream(uncompressedStream, UncompressedSize, compressedStream);
		Assert.That(uncompressedStream.Position, Is.EqualTo(InitialOffset + UncompressedSize));

		compressedStream.Position = 0;
		MemoryStream decompressedStream = new();
		LzmaCompression.DecompressLzmaSizeStream(compressedStream, compressedSize, decompressedStream);
		byte[] decompressedData = decompressedStream.ToArray();

		Assert.That((ArraySegment<byte>)decompressedData, Is.EqualTo(new ArraySegment<byte>(randomData, InitialOffset, UncompressedSize)));
	}
}
