using AssetRipper.Core.Logging;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Raw;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Web;
using AssetRipper.IO.Files.ResourceFiles;
using System;

namespace AssetRipper.FileAnalyzer
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("No arguments");
			}
			else
			{
				Logger.Add(new ConsoleLogger(true));
				//Analyzer.LoadFiles(args);
				LoadFiles(args);
			}
			Console.ReadKey();
			return;
		}

		private static void LoadFiles(string[] files)
		{
			foreach (string file in files)
			{
				LoadFile(file);
			}
		}

		private static void LoadFile(string fullName)
		{
			Logger.Info(fullName);
			Logger.BlankLine();
			try
			{
				File file = SchemeReader.LoadFile(fullName);
				LogFileInfo(file);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}

		private static void LogFileInfo(File file, int indent = 0)
		{
			string indentionString = new string('\t', indent);
			Logger.Info($"{indentionString}{file.GetType().Name}");
			Logger.Info($"{indentionString}File path: {file.FilePath}");
			Logger.Info($"{indentionString}Name: {file.Name}");
			Logger.Info($"{indentionString}Name (fixed): {file.NameFixed}");
			switch (file)
			{
				case FileStreamBundleFile fileStreamBundleFile:
					LogFileInfo(fileStreamBundleFile, indent);
					break;
				case RawBundleFile rawBundleFile:
					LogFileInfo(rawBundleFile, indent);
					break;
				case WebBundleFile webBundleFile:
					LogFileInfo(webBundleFile, indent);
					break;
				case FileContainer fileList:
					LogFileListInfo(fileList, indent);
					break;
				default:
					break;
			}
		}

		private static void LogFileInfo(WebBundleFile file, int indent = 0)
		{
			string indentionString = new string('\t', indent);
			WebBundleHeader header = file.Header;
			Logger.Info($"{indentionString}Bundle version: {(int)header.Version}");
			Logger.Info($"{indentionString}Unity version: {header.UnityWebMinimumRevision}");
			LogFileListInfo(file, indent);
		}

		private static void LogFileInfo(RawBundleFile file, int indent = 0)
		{
			string indentionString = new string('\t', indent);
			RawBundleHeader header = file.Header;
			Logger.Info($"{indentionString}Bundle version: {(int)header.Version}");
			Logger.Info($"{indentionString}Unity version: {header.UnityWebMinimumRevision}");
			LogFileListInfo(file, indent);
		}

		private static void LogFileInfo(FileStreamBundleFile file, int indent = 0)
		{
			string indentionString = new string('\t', indent);
			FileStreamBundleHeader header = file.Header;
			Logger.Info($"{indentionString}Bundle version: {(int)header.Version}");
			Logger.Info($"{indentionString}Unity version: {header.UnityWebMinimumRevision}");
			Logger.Info($"{indentionString}Compression type: {header.CompressionType}");
			LogFileListInfo(file, indent);
		}

		private static void LogFileListInfo(FileContainer fileList, int indent)
		{
			string indentionString = new string('\t', indent);
			if (fileList.ResourceFiles.Count > 0)
			{
				Logger.Info($"{indentionString}Resource Files:");
				foreach (ResourceFile file in fileList.ResourceFiles)
				{
					LogFileInfo(file, indent + 1);
					Logger.BlankLine();
				}
			}
			if (fileList.FileLists.Count > 0)
			{
				Logger.Info($"{indentionString}File Containers:");
				foreach (FileContainer nestedList in fileList.FileLists)
				{
					LogFileInfo(nestedList, indent + 1);
					Logger.BlankLine();
				}
			}
		}
	}
}
