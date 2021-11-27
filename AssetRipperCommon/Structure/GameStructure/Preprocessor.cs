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
		public const string ZipExtension = ".zip";
		public const string ApkExtension = ".apk";
		public const string XapkExtension = ".xapk";
		public const string VpkExtension = ".vpk";

		internal static List<string> Process(IEnumerable<string> paths)
		{
			List<string> result = new List<string>();
			foreach(string path in paths)
			{
				switch (GetFileExtension(path))
				{
					case ZipExtension:
					case ApkExtension:
					case VpkExtension:
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
			string outputDirectory = TempFolderManager.CreateNewRandomTempFolder();
			DecompressZipArchive(zipFilePath, outputDirectory);
			return outputDirectory;
		}

		private static string ExtractXapk(string xapkFilePath)
		{
			string intermediateDirectory = TempFolderManager.CreateNewRandomTempFolder();
			string outputDirectory = TempFolderManager.CreateNewRandomTempFolder();
			DecompressZipArchive(xapkFilePath, intermediateDirectory);
			foreach(var filePath in Directory.GetFiles(intermediateDirectory))
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
	}
}
