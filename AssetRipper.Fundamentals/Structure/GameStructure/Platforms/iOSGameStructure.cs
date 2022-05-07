﻿using System;
using System.Collections.Generic;
using System.IO;

namespace AssetRipper.Core.Structure.GameStructure.Platforms
{
	internal sealed class iOSGameStructure : PlatformGameStructure
	{
		public iOSGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(nameof(rootPath));
			}
			m_root = new DirectoryInfo(rootPath);
			if (!m_root.Exists)
			{
				throw new Exception($"Root directory '{rootPath}' doesn't exist");
			}

			if (!GetDataiOSDirectory(m_root, out string dataPath, out string appPath, out string name))
			{
				throw new Exception($"Data directory wasn't found");
			}

			Name = name;
			RootPath = rootPath;
			GameDataPath = dataPath;
			StreamingAssetsPath = Path.Combine(m_root.FullName, iOSStreamingName);
			ResourcesPath = Path.Combine(dataPath, ResourcesName);
			ManagedPath = Path.Combine(dataPath, ManagedName);
			UnityPlayerPath = null;
			UnityVersion = GetUnityVersionFromDataDirectory(GameDataPath);
			Il2CppGameAssemblyPath = Path.Combine(appPath, name);
			Il2CppMetaDataPath = Path.Combine(ManagedPath, MetadataName, DefaultGlobalMetadataName);

			if (HasIl2CppFiles())
				Backend = Assembly.ScriptingBackend.Il2Cpp;
			else if (HasMonoAssemblies(ManagedPath))
				Backend = Assembly.ScriptingBackend.Mono;
			else
				Backend = Assembly.ScriptingBackend.Unknown;

			DataPaths = new string[] { dataPath };
		}

		public static bool IsiOSStructure(string path)
		{
			DirectoryInfo root = new DirectoryInfo(path);
			if (!root.Exists)
			{
				return false;
			}

			return GetDataiOSDirectory(root, out string _, out string _, out string _);
		}

		private static bool GetDataiOSDirectory(DirectoryInfo rootDirectory, out string dataPath, out string appPath, out string appName)
		{
			dataPath = null;
			appPath = null;
			appName = null;

			string payloadPath = Path.Combine(rootDirectory.FullName, PayloadName);
			DirectoryInfo payloadDirectory = new DirectoryInfo(payloadPath);
			if (!payloadDirectory.Exists)
			{
				return false;
			}

			foreach (DirectoryInfo dinfo in payloadDirectory.EnumerateDirectories())
			{
				if (dinfo.Name.EndsWith(AppExtension, StringComparison.Ordinal))
				{
					appPath = dinfo.FullName;
					appName = dinfo.Name.Substring(0, dinfo.Name.Length - AppExtension.Length);
					dataPath = Path.Combine(dinfo.FullName, DataFolderName);
					if (Directory.Exists(dataPath))
					{
						return true;
					}
				}
			}

			dataPath = null;
			appPath = null;
			appName = null;
			return false;
		}

		private void CollectiOSStreamingAssets(DirectoryInfo root, IDictionary<string, string> files)
		{
			string streamingPath = Path.Combine(root.FullName, iOSStreamingName);
			DirectoryInfo streamingDirectory = new DirectoryInfo(streamingPath);
			if (streamingDirectory.Exists)
			{
				CollectAssetBundlesRecursively(root, files);
			}
		}


		private const string iOSStreamingName = "Raw";

		private const string PayloadName = "Payload";
		private const string AppExtension = ".app";
	}
}