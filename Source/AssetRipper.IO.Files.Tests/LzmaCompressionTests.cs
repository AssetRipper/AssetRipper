using AssetRipper.IO.Files.BundleFiles;

namespace AssetRipper.IO.Files.Tests;

public class LzmaCompressionTests
{
	[Test]
	public async Task LzmaStreamSymmetryTest()
	{
		const int UncompressedSize = 4075;
		byte[] randomData = new Random(42).NextBytes(UncompressedSize);

		MemoryStream uncompressedStream = new MemoryStream(randomData);
		MemoryStream compressedStream = new();
		long compressedSize = LzmaCompression.CompressLzmaStream(uncompressedStream, UncompressedSize, compressedStream);
		await Assert.That(uncompressedStream.Position).IsEqualTo(UncompressedSize);

		compressedStream.Position = 0;
		MemoryStream decompressedStream = new();
		LzmaCompression.DecompressLzmaStream(compressedStream, compressedSize, decompressedStream, UncompressedSize);
		byte[] decompressedData = decompressedStream.ToArray();

		await Assert.That(decompressedData.SequenceEqual(randomData)).IsTrue();
	}

	[Test]
	public async Task LzmaStreamSymmetryTestForPartialInputStream()
	{
		const int UncompressedSize = 4077;
		const int InitialOffset = 34;
		const int TrailingCount = 50;
		byte[] randomData = new Random(42).NextBytes(InitialOffset + UncompressedSize + TrailingCount);

		MemoryStream uncompressedStream = new MemoryStream(randomData);
		uncompressedStream.Position += InitialOffset;

		MemoryStream compressedStream = new();
		long compressedSize = LzmaCompression.CompressLzmaStream(uncompressedStream, UncompressedSize, compressedStream);
		await Assert.That(uncompressedStream.Position).IsEqualTo(InitialOffset + UncompressedSize);

		compressedStream.Position = 0;
		MemoryStream decompressedStream = new();
		LzmaCompression.DecompressLzmaStream(compressedStream, compressedSize, decompressedStream, UncompressedSize);
		byte[] decompressedData = decompressedStream.ToArray();

		await Assert.That(decompressedData.AsSpan().SequenceEqual(randomData.AsSpan(InitialOffset, UncompressedSize))).IsTrue();
	}

	[Test]
	public async Task LzmaSizeStreamSymmetryTest()
	{
		const int UncompressedSize = 4067;
		byte[] randomData = new Random(42).NextBytes(UncompressedSize);

		MemoryStream uncompressedStream = new MemoryStream(randomData);
		MemoryStream compressedStream = new();
		long compressedSize = LzmaCompression.CompressLzmaSizeStream(uncompressedStream, UncompressedSize, compressedStream);
		await Assert.That(uncompressedStream.Position).IsEqualTo(UncompressedSize);

		compressedStream.Position = 0;
		MemoryStream decompressedStream = new();
		LzmaCompression.DecompressLzmaSizeStream(compressedStream, compressedSize, decompressedStream);
		byte[] decompressedData = decompressedStream.ToArray();

		await Assert.That(decompressedData.SequenceEqual(randomData)).IsTrue();
	}

	[Test]
	public async Task LzmaSizeStreamSymmetryTestForPartialInputStream()
	{
		const int UncompressedSize = 4087;
		const int InitialOffset = 44;
		const int TrailingCount = 40;
		byte[] randomData = new Random(42).NextBytes(InitialOffset + UncompressedSize + TrailingCount);

		MemoryStream uncompressedStream = new MemoryStream(randomData);
		uncompressedStream.Position += InitialOffset;

		MemoryStream compressedStream = new();
		long compressedSize = LzmaCompression.CompressLzmaSizeStream(uncompressedStream, UncompressedSize, compressedStream);
		await Assert.That(uncompressedStream.Position).IsEqualTo(InitialOffset + UncompressedSize);

		compressedStream.Position = 0;
		MemoryStream decompressedStream = new();
		LzmaCompression.DecompressLzmaSizeStream(compressedStream, compressedSize, decompressedStream);
		byte[] decompressedData = decompressedStream.ToArray();

		await Assert.That(decompressedData.AsSpan().SequenceEqual(randomData.AsSpan(InitialOffset, UncompressedSize))).IsTrue();
	}
}
