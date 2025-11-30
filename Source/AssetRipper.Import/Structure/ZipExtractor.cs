using AssetRipper.Import.Logging;
using AssetRipper.IO.Files;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;

namespace AssetRipper.Import.Structure;

internal static class ZipExtractor
{
	private const string ZipExtension = ".zip";
	private const string ApkExtension = ".apk";
	private const string ApksExtension = ".apks";
	private const string ApkPlusExtension = ".apk+";
	private const string ObbExtension = ".obb";
	private const string XapkExtension = ".xapk";
	private const string VpkExtension = ".vpk"; //PS Vita
	private const string IpaExtension = ".ipa"; //iOS App Store Package
	private const uint ZipNormalMagic = 0x04034B50;
	private const uint ZipEmptyMagic = 0x06054B50;
	private const uint ZipSpannedMagic = 0x08074B50;

	public static List<string> Process(IEnumerable<string> paths, FileSystem fileSystem)
	{
		List<string> result = [];
		foreach (string path in paths)
		{
			switch (GetFileExtension(path, fileSystem))
			{
				case ZipExtension:
				case ApkExtension:
				case ObbExtension:
				case VpkExtension:
				case IpaExtension:
					result.Add(ExtractZip(path, fileSystem));
					break;
				case ApksExtension:
				case ApkPlusExtension:
				case XapkExtension:
					result.Add(ExtractXapk(path, fileSystem));
					break;
				default:
					result.Add(path);
					break;
			}
		}
		return result;
	}

	private static string ExtractZip(string zipFilePath, FileSystem fileSystem)
	{
		if (!HasCompatibleMagic(zipFilePath, fileSystem))
		{
			return zipFilePath;
		}

		string outputDirectory = fileSystem.Directory.CreateTemporary();
		DecompressZipArchive(zipFilePath, outputDirectory, fileSystem);
		return outputDirectory;
	}

	private static string ExtractXapk(string xapkFilePath, FileSystem fileSystem)
	{
		if (!HasCompatibleMagic(xapkFilePath, fileSystem))
		{
			return xapkFilePath;
		}

		string intermediateDirectory = fileSystem.Directory.CreateTemporary();
		string outputDirectory = fileSystem.Directory.CreateTemporary();
		DecompressZipArchive(xapkFilePath, intermediateDirectory, fileSystem);
		foreach (string filePath in fileSystem.Directory.GetFiles(intermediateDirectory))
		{
			if (GetFileExtension(filePath, fileSystem) == ApkExtension)
			{
				DecompressZipArchive(filePath, outputDirectory, fileSystem);
			}
		}
		return outputDirectory;
	}

	private static void DecompressZipArchive(string zipFilePath, string outputDirectory, FileSystem fileSystem)
	{
		Logger.Info(LogCategory.Import, $"Decompressing files...{Environment.NewLine}\tFrom: {zipFilePath}{Environment.NewLine}\tTo: {outputDirectory}");
		using Stream stream = fileSystem.File.OpenRead(zipFilePath);
		using ZipArchive archive = ZipArchive.Open(stream);
		foreach (ZipArchiveEntry entry in archive.Entries)
		{
			WriteEntryToDirectory(entry, outputDirectory, fileSystem);
		}
	}

	private static void WriteEntryToDirectory(ZipArchiveEntry entry, string outputDirectory, FileSystem fileSystem)
	{
		string filePath;
		string fullOutputDirectory = fileSystem.Path.GetFullPath(outputDirectory);

		if (!fileSystem.Directory.Exists(fullOutputDirectory))
		{
			throw new ExtractionException($"Directory does not exist to extract to: {fullOutputDirectory}");
		}

		string fileName = fileSystem.Path.GetFileName(entry.Key ?? throw new NullReferenceException("Entry Key is null")) ?? throw new NullReferenceException("File is null");
		fileName = FileSystem.FixInvalidFileNameCharacters(fileName);

		string directory = fileSystem.Path.GetDirectoryName(entry.Key ?? throw new NullReferenceException("Entry Key is null")) ?? throw new NullReferenceException("Directory is null");
		string fullDirectory = fileSystem.Path.GetFullPath(fileSystem.Path.Join(fullOutputDirectory, directory));

		if (!fileSystem.Directory.Exists(fullDirectory))
		{
			if (!fullDirectory.StartsWith(fullOutputDirectory, StringComparison.Ordinal))
			{
				throw new ExtractionException("Entry is trying to create a directory outside of the destination directory.");
			}

			fileSystem.Directory.Create(fullDirectory);
		}
		filePath = fileSystem.Path.Join(fullDirectory, fileName);

		if (!entry.IsDirectory)
		{
			filePath = fileSystem.Path.GetFullPath(filePath);

			if (!filePath.StartsWith(fullOutputDirectory,StringComparison.Ordinal))
			{
				throw new ExtractionException("Entry is trying to write a file outside of the destination directory.");
			}

			using Stream stream = fileSystem.File.Create(filePath);
			entry.WriteTo(stream);
		}
		else if (!fileSystem.Directory.Exists(filePath))
		{
			fileSystem.Directory.Create(filePath);
		}
	}

	private static string? GetFileExtension(string path, FileSystem fileSystem)
	{
		if (fileSystem.File.Exists(path))
		{
			return fileSystem.Path.GetExtension(path);
		}
		else
		{
			return null;
		}
	}

	private static bool HasCompatibleMagic(string path, FileSystem fileSystem)
	{
		uint magic = GetMagicNumber(path, fileSystem);
		return magic == ZipNormalMagic || magic == ZipEmptyMagic || magic == ZipSpannedMagic;
	}

	private static uint GetMagicNumber(string path, FileSystem fileSystem)
	{
		using Stream stream = fileSystem.File.OpenRead(path);
		return new BinaryReader(stream).ReadUInt32();
	}
}
