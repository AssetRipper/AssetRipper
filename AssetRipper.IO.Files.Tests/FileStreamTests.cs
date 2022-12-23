using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.Streams.Smart;

namespace AssetRipper.IO.Files.Tests
{
	public class FileStreamTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		[Ignore("Write not yet implemented")]
		public void ReadIsSymmetricToWrite()
		{
			FileStreamBundleScheme scheme = new();
			FileStreamBundleFile bundle = MakeBundle();
			using SmartStream stream = SmartStream.CreateMemory();

			bundle.Write(stream);
			long positionAfterWrite = stream.Position;
			stream.Position = 0;
			Assert.Multiple(() =>
			{
				Assert.That(positionAfterWrite, Is.GreaterThan(0));
				Assert.That(scheme.CanRead(stream));
			});

			FileStreamBundleFile readBundle = scheme.Read(stream, bundle.FilePath, bundle.Name);
			long positionAfterRead = stream.Position;
			Assert.Multiple(() =>
			{
				Assert.That(readBundle.Header, Is.EqualTo(bundle.Header));
				Assert.That(positionAfterRead, Is.EqualTo(positionAfterWrite));
			});
		}

		[Test]
		public void CompressTypeWorks()
		{
			FileStreamBundleHeader header = new();
			header.Flags = BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd;
			Assert.That(header.CompressionType, Is.EqualTo(CompressionType.None));
			header.CompressionType = CompressionType.Lzma;
			Assert.Multiple(() =>
			{
				Assert.That(header.CompressionType, Is.EqualTo(CompressionType.Lzma));
				Assert.That(header.Flags, Is.EqualTo(BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd | BundleFlags.CompressionBit1));
			});
			header.CompressionType = CompressionType.Lz4;
			Assert.Multiple(() =>
			{
				Assert.That(header.CompressionType, Is.EqualTo(CompressionType.Lz4));
				Assert.That(header.Flags, Is.EqualTo(BundleFlags.BlockInfoNeedPaddingAtStart | BundleFlags.BlocksInfoAtTheEnd | BundleFlags.CompressionBit2));
			});
		}

		private static FileStreamBundleFile MakeBundle() 
		{
			FileStreamBundleFile bundle = new();
			FileStreamBundleHeader header = bundle.Header;
			header.Version = BundleVersion.BF_LargeFilesSupport;
			header.UnityWebBundleVersion = "5.x.x";
			header.UnityWebMinimumRevision = "2022.1.0f1";
			return bundle;
		}
	}
}
