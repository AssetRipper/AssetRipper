using System.IO;
using uTinyRipper.WebFiles;

namespace uTinyRipper
{
	public sealed class WebFile : FileList
	{
		internal WebFile(WebFileScheme scheme) :
			base(scheme.NameOrigin)
		{
			Header = scheme.Header;
			Metadata = scheme.Metadata;
		}

		public static bool IsWebFile(string webPath)
		{
			using (Stream stream = MultiFileStream.OpenRead(webPath))
			{
				return IsWebFile(stream);
			}
		}

	 	public static bool IsWebFile(Stream stream)
		{
			using (EndianReader reader = new EndianReader(stream, EndianType.BigEndian))
			{
				return WebHeader.IsWebHeader(reader);
			}
		}

		public static bool IsWebFile(byte[] buffer, int offset, int size)
		{
			using (MemoryStream stream = new MemoryStream(buffer, offset, size, false))
			{
				return IsWebFile(stream);
			}
		}

		public static WebFileScheme ReadScheme(byte[] buffer, string filePath)
		{
			return WebFileScheme.ReadScheme(buffer, filePath);
		}

		public static WebFileScheme ReadScheme(SmartStream stream, string filePath)
		{
			return WebFileScheme.ReadScheme(stream, filePath);
		}
		
		public WebHeader Header { get; }
		public WebMetadata Metadata { get; }
	}
}
