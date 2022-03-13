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
		private const string ZipExtension = ".zip";
		private const string ApkExtension = ".apk";
		private const string ObbExtension = ".obb";
		private const string XapkExtension = ".xapk";
		private const string VpkExtension = ".vpk"; //PS Vita
		private const string IpaExtension = ".ipa"; //iOS App Store Package
		private const uint ZipNormalMagic = 0x04034B50;
		private const uint ZipEmptyMagic = 0x06054B50;
		private const uint ZipSpannedMagic = 0x08074B50;

		internal static List<string> Process(IEnumerable<string> paths)
		{
			List<string> result = new List<string>();
			foreach (string path in paths)
			{
				switch (GetFileExtension(path))
				{
					case ZipExtension:
					case ApkExtension:
					case ObbExtension:
					case VpkExtension:
					case IpaExtension:
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
			if (!HasCompatibleMagic(zipFilePath))
				return zipFilePath;

			string outputDirectory = TempFolderManager.CreateNewRandomTempFolder();
			DecompressZipArchive(zipFilePath, outputDirectory);
			return outputDirectory;
		}

		private static string ExtractXapk(string xapkFilePath)
		{
			if (!HasCompatibleMagic(xapkFilePath))
				return xapkFilePath;

			string intermediateDirectory = TempFolderManager.CreateNewRandomTempFolder();
			string outputDirectory = TempFolderManager.CreateNewRandomTempFolder();
			DecompressZipArchive(xapkFilePath, intermediateDirectory);
			foreach (var filePath in Directory.GetFiles(intermediateDirectory))
			{
				if (GetFileExtension(filePath) == ApkExtension)
					DecompressZipArchive(filePath, outputDirectory);
			}
			return outputDirectory;
		}

		private static void DecompressZipArchive(string zipFilePath, string outputDirectory)
		{
			Logger.Info(LogCategory.Import, $"Decompressing files...{Environment.NewLine}\tFrom: {zipFilePath}{Environment.NewLine}\tTo: {outputDirectory}");
			FastZip zipper = new FastZip();
			zipper.ExtractZip(zipFilePath, outputDirectory, null);
		}

		private static string GetFileExtension(string path)
		{
			if (File.Exists(path))
				return Path.GetExtension(path);
			else
				return null;
		}

		private static bool HasCompatibleMagic(string path)
		{
			uint magic = GetMagicNumber(path);
			return magic == ZipNormalMagic || magic == ZipEmptyMagic || magic == ZipSpannedMagic;
		}

		private static uint GetMagicNumber(string path)
		{
			return new BinaryReader(File.OpenRead(path)).ReadUInt32();
		}
	}
}
