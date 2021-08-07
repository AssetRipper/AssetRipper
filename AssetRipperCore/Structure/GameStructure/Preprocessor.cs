using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Structure.GameStructure
{
	internal static class Preprocessor
	{
		private static readonly string tempFolder;
		public const string ZipExtension = ".zip";
		public const string ApkExtension = ".apk";
		public const string XapkExtension = ".xapk";
		public const int NumberOfRandomCharacters = 10;

		static Preprocessor()
		{
			tempFolder = DirectoryUtils.CombineWithExecutingDirectory("temp");
			if(DirectoryUtils.Exists(tempFolder))
				DirectoryUtils.Delete(tempFolder, true);
			DirectoryUtils.CreateDirectory(tempFolder);
		}

		internal static List<string> Process(IEnumerable<string> paths)
		{
			List<string> result = new List<string>();
			foreach(string path in paths)
			{
				switch (GetFileExtension(path))
				{
					case ZipExtension:
					case ApkExtension:
						result.Add(ExtractZip(path));
						break;
					case XapkExtension:
						result.Add(ExtractXapk(path));
						break;
					default:
						result.Add(path);
						break;
				}
			}
			return result;
		}

		private static string ExtractZip(string zipFilePath)
		{
			string outputDirectory = GetNewRandomTempFolder();
			DecompressZipArchive(zipFilePath, outputDirectory);
			return outputDirectory;
		}

		private static string ExtractXapk(string xapkFilePath)
		{
			string intermediateDirectory = GetNewRandomTempFolder();
			string outputDirectory = GetNewRandomTempFolder();
			DecompressZipArchive(xapkFilePath, intermediateDirectory);
			foreach(var filePath in DirectoryUtils.GetFiles(intermediateDirectory))
			{
				if (GetFileExtension(filePath) == ApkExtension)
					DecompressZipArchive(filePath, outputDirectory);
			}
			return outputDirectory;
		}

		private static void DecompressZipArchive(string zipFilePath, string outputDirectory)
		{
			Logger.Log(LogType.Info, LogCategory.Import, $"Decompressing files...{Environment.NewLine}\tFrom: {zipFilePath}{Environment.NewLine}\tTo: {outputDirectory}");
			using (ZipFile zipFile = new ZipFile(zipFilePath))
			{
				foreach (ZipEntry entry in zipFile)
				{
					string internalPath = entry.Name;

					using (var zipInputStream = zipFile.GetInputStream(entry))
					{
						using (var unzippedFileStream = new MemoryStream())
						{
							int size = 0;
							byte[] buffer = new byte[4096];
							while (true)
							{
								size = zipInputStream.Read(buffer, 0, buffer.Length);
								if (size > 0)
									unzippedFileStream.Write(buffer, 0, size);
								else
									break;
							}
							string fullFilePath = Path.Combine(outputDirectory, internalPath);
							string fullDirectoryPath = (new FileInfo(fullFilePath)).Directory.FullName;
							DirectoryUtils.CreateDirectory(fullDirectoryPath);
							File.WriteAllBytes(fullFilePath, unzippedFileStream.ToArray());
						}
					}
				}
			}
		}

		private static string GetNewRandomTempFolder() => Path.Combine(tempFolder, GuidUtils.GetNewGuidString(NumberOfRandomCharacters));

		private static string GetFileExtension(string path)
		{
			if (FileUtils.Exists(path))
				return Path.GetExtension(path);
			else
				return null;
		}
	}
}
