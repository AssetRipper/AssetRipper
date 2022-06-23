using AssetRipper.IO.Files.ArchiveFiles;
using AssetRipper.IO.Files.BundleFiles;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.Schemes;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.Streams.Smart;
using AssetRipper.IO.Files.WebFiles;
using System.IO;

namespace AssetRipper.IO.Files
{
	public static class SchemeReader
	{
		/// <summary>Returns some information about the file including its type</summary>
		public static FileScheme LoadScheme(string filePath, string fileName)
		{
			using SmartStream stream = SmartStream.OpenRead(filePath);
			return ReadScheme(stream, filePath, fileName);
		}

		public static FileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			using MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length, false);
			if (BundleFile.IsBundleFile(stream))
			{
				return BundleFileScheme.ReadScheme(buffer, filePath, fileName);
			}
			if (ArchiveFile.IsArchiveFile(stream))
			{
				return ArchiveFile.ReadScheme(buffer, filePath, fileName);
			}
			if (WebFile.IsWebFile(stream))
			{
				return WebFile.ReadScheme(buffer, filePath);
			}
			if (SerializedFile.IsSerializedFile(stream))
			{
				return SerializedFile.ReadScheme(buffer, filePath, fileName);
			}
			return ResourceFile.ReadScheme(buffer, filePath, fileName);
		}

		public static FileScheme ReadScheme(SmartStream stream, string filePath, string fileName)
		{
			if (BundleFile.IsBundleFile(stream))
			{
				return BundleFileScheme.ReadScheme(stream, filePath, fileName);
			}
			if (ArchiveFile.IsArchiveFile(stream))
			{
				return ArchiveFile.ReadScheme(stream, filePath, fileName);
			}
			if (WebFile.IsWebFile(stream))
			{
				return WebFile.ReadScheme(stream, filePath);
			}
			if (SerializedFile.IsSerializedFile(stream))
			{
				return SerializedFile.ReadScheme(stream, filePath, fileName);
			}

			return ResourceFile.ReadScheme(stream, filePath, fileName);
		}
	}
}
