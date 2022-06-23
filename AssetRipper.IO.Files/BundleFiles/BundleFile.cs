using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.BundleFiles.Header;
using AssetRipper.IO.Files.BundleFiles.Parser;
using AssetRipper.IO.Files.Entries;
using AssetRipper.IO.Files.Streams.MultiFile;
using System.IO;

namespace AssetRipper.IO.Files.BundleFiles
{
	public sealed class BundleFile : FileList
	{
		public BundleHeader Header { get; }
		public BundleMetadata Metadata { get; }

		internal BundleFile(BundleFileScheme scheme) : base(scheme.NameOrigin)
		{
			if (scheme == null)
			{
				throw new ArgumentNullException(nameof(scheme));
			}

			Header = scheme.Header;
			Metadata = scheme.Metadata;
		}

		public static bool IsBundleFile(string filePath) => IsBundleFile(MultiFileStream.OpenRead(filePath));
		public static bool IsBundleFile(byte[] buffer, int offset, int size) => IsBundleFile(new MemoryStream(buffer, offset, size, false));
		public static bool IsBundleFile(Stream stream) => BundleHeader.IsBundleHeader(new EndianReader(stream, EndianType.BigEndian));

		public static BundleFileScheme LoadScheme(string filePath)
		{
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			using Stream stream = MultiFileStream.OpenRead(filePath);
			return ReadScheme(stream, filePath, fileName);
		}

		public static BundleFileScheme ReadScheme(byte[] buffer, string filePath, string fileName) => BundleFileScheme.ReadScheme(buffer, filePath, fileName);
		public static BundleFileScheme ReadScheme(Stream stream, string filePath, string fileName) => BundleFileScheme.ReadScheme(stream, filePath, fileName);

	}
}
