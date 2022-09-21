using AssetRipper.Core.Logging;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.BundleFiles.FileStream;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Raw;
using AssetRipper.IO.Files.BundleFiles.RawWeb.Web;
using AssetRipper.IO.Files.ResourceFiles;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using System;

namespace AssetRipper.Tools.FileAnalyzer
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
				file.ReadContentsRecursively();
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
				case SerializedFile serializedFile:
					LogFileInfo(serializedFile, indent);
					break;
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

		private static void LogFileInfo(SerializedFile assetsFile, int indent = 0)
		{
			string indentionString = new string('\t', indent);
			if (assetsFile.Metadata.UnityVersion.IsEqual(0, 0, 0))
			{
				Logger.Info($"{indentionString}Unity version: stripped");
			}
			else
			{
				Logger.Info($"{indentionString}Unity version: {assetsFile.Metadata.UnityVersion}");
			}

			Logger.Info($"{indentionString}Serialied version: {(int)assetsFile.Header.Version}");
			Logger.Info($"{indentionString}Endianess: {(assetsFile.EndianType)}");

			if (assetsFile.Metadata.Externals.Length > 0)
			{
				Logger.Info($"{indentionString}Shared files:");
				for (int i = 0; i < assetsFile.Metadata.Externals.Length; i++)
				{
					FileIdentifier sharedFile = assetsFile.Metadata.Externals[i];
					if (i != 0)
					{
						Logger.BlankLine();
					}
					Logger.Info($"{indentionString}\t{sharedFile.PathNameOrigin}");
					Logger.Info($"{indentionString}\tGUID: {sharedFile.Guid}");
					Logger.Info($"{indentionString}\tType: {sharedFile.Type}");
				}
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
			if (file.DirectoryInfo.Nodes.Length > 0)
			{
				Logger.Info($"{indentionString}Nodes:");
				foreach (FileStreamNode node in file.DirectoryInfo.Nodes)
				{
					Logger.Info($"{indentionString}\tFlags: {node.Flags,-14} Path: {node.Path}");
				}
			}
			LogFileListInfo(file, indent);
		}

		private static void LogFileListInfo(FileContainer fileList, int indent)
		{
			string indentionString = new string('\t', indent);
			if (fileList.SerializedFiles.Count > 0)
			{
				Logger.Info($"{indentionString}Serialized Files:");
				foreach (File file in fileList.SerializedFiles)
				{
					LogFileInfo(file, indent + 1);
					Logger.BlankLine();
				}
			}
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
