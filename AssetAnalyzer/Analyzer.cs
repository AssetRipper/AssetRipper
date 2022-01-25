using AssetRipper.Core.IO.FileReading;
using AssetRipper.Core.Logging;
using System;
using System.IO;

namespace AssetAnalyzer
{
	public static class Analyzer
	{
		public static void LoadFiles(string[] files)
		{
			foreach (string file in files)
				LoadFile(file);
		}
		public static void LoadFile(string fullName)
		{
			Logger.Info(fullName);
			try
			{
				var reader = new FileReader(fullName);
				switch (reader.FileType)
				{
					case FileType.AssetsFile:
						Logger.Info("AssetsFile");
						LoadAssetsFile(reader);
						break;
					case FileType.BundleFile:
						Logger.Info("BundleFile");
						LoadBundleFile(reader);
						break;
					case FileType.WebFile:
						Logger.Info("WebFile");
						LoadWebFile(reader);
						break;
					default:
						Logger.Info("Not a rippable file");
						reader.Dispose();
						break;
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}
		private static void LoadAssetsFile(FileReader reader)
		{
			Logger.Info($"Loading {reader.FileName}");
			try
			{
				var assetsFile = new SerializedFile(reader);
				if (assetsFile.IsVersionStripped)
					Logger.Info("\tUnity version: stripped");
				else
					Logger.Info($"\tUnity version: {assetsFile.unityVersion}");
				Logger.Info($"\tSerialied version: {(int)assetsFile.header.m_Version}");
				Logger.Info($"\tEndianess: {(assetsFile.header.m_Endianess == 0 ? "Little Endian" : "Big Endian")}");

				if (assetsFile.m_Externals.Count > 0)
					Logger.Info("\tShared files:");

				foreach (var sharedFile in assetsFile.m_Externals)
				{
					Logger.Info($"\t\t{sharedFile.fileName}");
					Logger.Info($"\t\t\tGUID: {sharedFile.guid}");
					Logger.Info($"\t\t\tType: {sharedFile.type}");
				}
			}
			catch (Exception e)
			{
				Logger.Error($"Error while reading assets file {reader.FileName}", e);
			}
			finally
			{
				reader.Dispose();
			}
		}
		private static void LoadBundleFile(FileReader reader)
		{
			Logger.Info("Loading " + reader.FileName);
			try
			{
				var bundleFile = new BundleFile(reader);
				Logger.Info($"\tSignature: {bundleFile.m_Header.signature}");
				Logger.Info($"\tBundle version: {bundleFile.m_Header.version}");
				Logger.Info($"\tUnity version: {bundleFile.m_Header.unityRevision}");//real unity version
				Logger.Info(string.Format("\tFlags: 0x{0:X4}", bundleFile.m_Header.flags));
				Logger.Info($"\tSub Files:");
				foreach (var file in bundleFile.fileList)
				{
					Logger.Info($"\t\t{file.fileName}");
				}
			}
			catch (Exception e)
			{
				Logger.Error($"Error while reading bundle file {reader.FileName}:", e);
			}
			finally
			{
				reader.Dispose();
			}
		}
		private static void LoadWebFile(FileReader reader)
		{
			Logger.Info("Loading " + reader.FileName);
			try
			{
				var webFile = new WebFile(reader);
				foreach (var file in webFile.fileList)
				{
					var dummyPath = Path.Combine(Path.GetDirectoryName(reader.FullPath), file.fileName);
					var subReader = new FileReader(dummyPath, file.stream);
					switch (subReader.FileType)
					{
						case FileType.AssetsFile:
							LoadAssetsFile(subReader);
							break;
						case FileType.BundleFile:
							LoadBundleFile(subReader);
							break;
						case FileType.WebFile:
							LoadWebFile(subReader);
							break;
						case FileType.ResourceFile:
							Logger.Info("Resource File");
							break;
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error($"Error while reading web file {reader.FileName}", e);
			}
			finally
			{
				reader.Dispose();
			}
		}
	}
}
