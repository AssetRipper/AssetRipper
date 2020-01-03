using System;
using System.IO;
using uTinyRipper.BundleFiles;

namespace uTinyRipper
{
	public sealed class BundleFile : FileList
	{
		internal BundleFile(BundleFileScheme scheme):
			base(scheme.NameOrigin)
		{
			if (scheme == null)
			{
				throw new ArgumentNullException(nameof(scheme));
			}

			Header = scheme.Header;
			Metadata = scheme.Metadata;
		}

		public static bool IsBundleFile(string filePath)
		{
			using (Stream stream = MultiFileStream.OpenRead(filePath))
			{
				return IsBundleFile(stream);
			}
		}

		public static bool IsBundleFile(byte[] buffer, int offset, int size)
		{
			using (MemoryStream stream = new MemoryStream(buffer, offset, size, false))
			{
				return IsBundleFile(stream);
			}
		}

		public static bool IsBundleFile(Stream stream)
		{
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				return BundleHeader.IsBundleHeader(reader);
			}
		}

		public static BundleFileScheme LoadScheme(string filePath)
		{
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			using (Stream stream = MultiFileStream.OpenRead(filePath))
			{
				return ReadScheme(stream, filePath, fileName);
			}
		}

		public static BundleFileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			return BundleFileScheme.ReadScheme(buffer, filePath, fileName);
		}

		public static BundleFileScheme ReadScheme(Stream stream, string filePath, string fileName)
		{
			return BundleFileScheme.ReadScheme(stream, filePath, fileName);
		}

		public BundleHeader Header { get; }
		public BundleMetadata Metadata { get; }
	}
}