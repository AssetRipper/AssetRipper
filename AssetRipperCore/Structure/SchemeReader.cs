using AssetRipper.Core.IO.Smart;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files.ArchiveFiles;
using AssetRipper.Core.Parser.Files.BundleFile;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.Schemes;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.WebFiles;
using System.IO;

namespace AssetRipper.Core.Structure
{
	internal static class SchemeReader
	{
		/// <summary>Returns some information about the file including its type</summary>
		public static FileScheme LoadScheme(string filePath, string fileName)
		{
			using (SmartStream stream = SmartStream.OpenRead(filePath))
			{
				return ReadScheme(stream, filePath, fileName);
			}
		}

		public static FileScheme ReadScheme(byte[] buffer, string filePath, string fileName)
		{
			using (MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length, false))
			{
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
		}

		public static FileScheme ReadScheme(SmartStream stream, string filePath, string fileName)
		{
			if (BundleFile.IsBundleFile(stream))
			{
				Logger.SendStatusChange($"Parsing Bundle {fileName}");
				return BundleFileScheme.ReadScheme(stream, filePath, fileName);
			}
			if (ArchiveFile.IsArchiveFile(stream))
			{
				Logger.SendStatusChange($"Parsing Archive File {fileName}");
				return ArchiveFile.ReadScheme(stream, filePath, fileName);
			}
			if (WebFile.IsWebFile(stream))
			{
				Logger.SendStatusChange($"Parsing Web File {fileName}");
				return WebFile.ReadScheme(stream, filePath);
			}
			if (SerializedFile.IsSerializedFile(stream))
			{
				Logger.SendStatusChange($"Parsing Serialized File {fileName}");
				return SerializedFile.ReadScheme(stream, filePath, fileName);
			}
			
			Logger.SendStatusChange($"Parsing Resource File {fileName}");
			return ResourceFile.ReadScheme(stream, filePath, fileName);
		}
	}
}
