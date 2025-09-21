using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests;

public static class FileStreamTests
{
	[TestCase(BundleVersion.BF_260_340, "3.4.0")]
	[TestCase(BundleVersion.BF_350_4x, "4.2.0")]
	[TestCase(BundleVersion.BF_520a1, "5.2.0a1")]
	[TestCase(BundleVersion.BF_520aunk, "5.2.0a10")]
	[TestCase(BundleVersion.BF_520_x, "5.3.0f1")]
	[TestCase(BundleVersion.BF_520_x, "2018.4.40f1")]
	[TestCase(BundleVersion.BF_LargeFilesSupport, "2022.1.0f1")]
	[TestCase(BundleVersion.BF_LargeFilesSupport, "2022.1.10f1")]
	[TestCase(BundleVersion.BF_2022_2, "2022.2.0f1")]
	public static void ReadIsSymmetricToWriteForEmptyBundle(BundleVersion bundleVersion, string unityVersion)
	{
		FileStreamBundleScheme scheme = new();
		FileStreamBundleFile bundle = MakeEmptyBundle(bundleVersion, unityVersion);
		using SmartStream stream = SmartStream.CreateMemory();

		bundle.Write(stream);
		long positionAfterWrite = stream.Position;
		stream.Position = 0;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(positionAfterWrite, Is.GreaterThan(0));
			Assert.That(scheme.CanRead(stream));
		}

		FileStreamBundleFile readBundle = scheme.Read(stream, bundle.FilePath, bundle.Name);
		long positionAfterRead = stream.Position;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(readBundle.Header, Is.EqualTo(bundle.Header));
			Assert.That(positionAfterRead, Is.EqualTo(positionAfterWrite));
		}
	}

	[Test]
	public static void CompressTypeWorks()
	{
		FileStreamBundleHeader header = new();
		header.Flags = BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd;
		Assert.That(header.CompressionType, Is.EqualTo(CompressionType.None));
		header.CompressionType = CompressionType.Lzma;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(header.CompressionType, Is.EqualTo(CompressionType.Lzma));
			Assert.That(header.Flags, Is.EqualTo(BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd | BundleFlags.CompressionBit1));
		}
		header.CompressionType = CompressionType.Lz4;
		using (Assert.EnterMultipleScope())
		{
			Assert.That(header.CompressionType, Is.EqualTo(CompressionType.Lz4));
			Assert.That(header.Flags, Is.EqualTo(BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd | BundleFlags.CompressionBit2));
		}
	}

	private static FileStreamBundleFile MakeEmptyBundle(BundleVersion bundleVersion, string unityVersion)
	{
		FileStreamBundleFile bundle = new();
		FileStreamBundleHeader header = bundle.Header;
		header.Version = bundleVersion;
		header.UnityWebBundleVersion = "5.x.x";
		header.UnityWebMinimumRevision = unityVersion;
		return bundle;
	}
}
