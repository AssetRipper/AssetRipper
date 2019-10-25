using System;
using System.IO;

namespace uTinyRipper.BundleFiles
{
	public sealed class BundleFile : FileList
	{
		internal BundleFile(IFileCollection collection, BundleFileScheme scheme)
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
			if (!FileMultiStream.Exists(filePath))
			{
				throw new Exception($"Bundle at path '{filePath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(filePath))
			{
				return IsBundleFile(stream);
			}
		}

		public static bool IsBundleFile(Stream stream)
		{
			return IsBundleFile(stream, stream.Position, stream.Length - stream.Position);
		}

		public static bool IsBundleFile(Stream stream, long offset, long size)
		{
			using (PartialStream bundleStream = new PartialStream(stream, offset, size))
			{
				using (EndianReader reader = new EndianReader(bundleStream, EndianType.BigEndian))
				{
					return BundleHeader.IsBundleHeader(reader);
				}
			}
		}

		public static BundleFileScheme LoadScheme(string filePath)
		{
			if (!FileUtils.Exists(filePath))
			{
				throw new Exception($"Bundle file at path '{filePath}' doesn't exist");
			}
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			using (SmartStream stream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(stream, 0, stream.Length, filePath, fileName);
			}
		}

		public static BundleFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			return BundleFileScheme.ReadScheme(stream, offset, size, filePath, fileName);
		}

		public BundleHeader Header { get; } = new BundleHeader();
		public BundleMetadata Metadata { get; private set; }

		private readonly string m_filePath;
	}
}