using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests;

public class FileStreamTests
{
	[Test]
	[Arguments(BundleVersion.BF_260_340, "3.4.0")]
	[Arguments(BundleVersion.BF_350_4x, "4.2.0")]
	[Arguments(BundleVersion.BF_520a1, "5.2.0a1")]
	[Arguments(BundleVersion.BF_520aunk, "5.2.0a10")]
	[Arguments(BundleVersion.BF_520_x, "5.3.0f1")]
	[Arguments(BundleVersion.BF_520_x, "2018.4.40f1")]
	[Arguments(BundleVersion.BF_LargeFilesSupport, "2022.1.0f1")]
	[Arguments(BundleVersion.BF_LargeFilesSupport, "2022.1.10f1")]
	[Arguments(BundleVersion.BF_2022_2, "2022.2.0f1")]
	public async Task ReadIsSymmetricToWriteForEmptyBundle(BundleVersion bundleVersion, string unityVersion)
	{
		FileStreamBundleScheme scheme = new();
		FileStreamBundleFile bundle = MakeEmptyBundle(bundleVersion, unityVersion);
		using SmartStream stream = SmartStream.CreateMemory();

		bundle.Write(stream);
		long positionAfterWrite = stream.Position;
		stream.Position = 0;

		{
			await Assert.That(positionAfterWrite).IsGreaterThan(0);
			await Assert.That(scheme.CanRead(stream)).IsTrue();
		}

		FileStreamBundleFile readBundle = scheme.Read(stream, bundle.FilePath, bundle.Name);
		long positionAfterRead = stream.Position;

		{
			await Assert.That(readBundle.Header).IsEqualTo(bundle.Header);
			await Assert.That(positionAfterRead).IsEqualTo(positionAfterWrite);
		}
	}

	[Test]
	public async Task CompressTypeWorks()
	{
		FileStreamBundleHeader header = new();
		header.Flags = BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd;
		await Assert.That(header.CompressionType).IsEqualTo(CompressionType.None);
		header.CompressionType = CompressionType.Lzma;

		{
			await Assert.That(header.CompressionType).IsEqualTo(CompressionType.Lzma);
			await Assert.That(header.Flags).IsEqualTo(BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd | BundleFlags.CompressionBit1);
		}
		header.CompressionType = CompressionType.Lz4;

		{
			await Assert.That(header.CompressionType).IsEqualTo(CompressionType.Lz4);
			await Assert.That(header.Flags).IsEqualTo(BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd | BundleFlags.CompressionBit2);
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