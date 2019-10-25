using System;
using System.IO;

namespace uTinyRipper.WebFiles
{
	public sealed class WebFile : FileList
	{
		internal WebFile(IFileCollection collection, WebFileScheme scheme)
		{
			if (scheme == null)
			{
				throw new ArgumentNullException(nameof(scheme));
			}

			Header = scheme.Header;
			Metadata = scheme.Metadata;
		}

		public static bool IsWebFile(string webPath)
		{
			if (!FileMultiStream.Exists(webPath))
			{
				throw new Exception($"Web at path '{webPath}' doesn't exist");
			}

			using (Stream stream = FileMultiStream.OpenRead(webPath))
			{
				return IsWebFile(stream);
			}
		}

		public static bool IsWebFile(Stream stream)
		{
			return IsWebFile(stream, stream.Position, stream.Length - stream.Position);
		}

		public static bool IsWebFile(Stream stream, long offset, long size)
		{
			using (PartialStream bundleStream = new PartialStream(stream, offset, size))
			{
				using (EndianReader reader = new EndianReader(bundleStream, EndianType.BigEndian))
				{
					return WebHeader.IsWebHeader(reader);
				}
			}
		}

		public static WebFileScheme LoadScheme(string filePath)
		{
			if (!FileUtils.Exists(filePath))
			{
				throw new Exception($"Web file at path '{filePath}' doesn't exist");
			}
			string fileName = Path.GetFileNameWithoutExtension(filePath);
			using (SmartStream stream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(stream, 0, stream.Length, filePath, fileName);
			}
		}

		public static WebFileScheme ReadScheme(SmartStream stream, long offset, long size, string filePath, string fileName)
		{
			return WebFileScheme.ReadScheme(stream, offset, size, filePath, fileName);
		}

		public WebHeader Header { get; private set; }
		public WebMetadata Metadata { get; private set; }

		private readonly string m_filePath;
	}
}
