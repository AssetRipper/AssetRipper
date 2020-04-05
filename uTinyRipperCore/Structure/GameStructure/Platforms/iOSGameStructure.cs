using System;
using System.Collections.Generic;
using System.IO;

namespace uTinyRipper
{
	internal sealed class iOSGameStructure : PlatformGameStructure
	{
		public iOSGameStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			m_root = new DirectoryInfo(DirectoryUtils.ToLongPath(rootPath));
			if (!m_root.Exists)
			{
				throw new Exception($"Root directory '{rootPath}' doesn't exist");
			}

			if (!GetDataiOSDirectory(m_root, out string dataPath, out string name))
			{
				throw new Exception($"Data directory wasn't found");
			}
			Name = name;
			DataPathes = new string[] { dataPath };

			DirectoryInfo dataDirectory = new DirectoryInfo(DirectoryUtils.ToLongPath(dataPath));

			Dictionary<string, string> files = new Dictionary<string, string>();
			CollectGameFiles(dataDirectory, files);
			CollectiOSStreamingAssets(dataDirectory, files);
			Files = files;

			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			CollectMainAssemblies(dataDirectory, assemblies);
			Assemblies = assemblies;
		}

		public static bool IsiOSStructure(string path)
		{
			DirectoryInfo root = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
			if (!root.Exists)
			{
				return false;
			}

			return GetDataiOSDirectory(root, out string _, out string _);
		}

		private static bool GetDataiOSDirectory(DirectoryInfo rootDirectory, out string dataPath, out string appName)
		{
			dataPath = null;
			appName = null;

			string payloadPath = Path.Combine(rootDirectory.FullName, PayloadName);
			DirectoryInfo payloadDirectory = new DirectoryInfo(payloadPath);
			if (!payloadDirectory.Exists)
			{
				return false;
			}

			foreach (DirectoryInfo dinfo in payloadDirectory.EnumerateDirectories())
			{
				if(dinfo.Name.EndsWith(AppExtension, StringComparison.Ordinal))
				{
					appName = dinfo.Name.Substring(0, dinfo.Name.Length - AppExtension.Length);
					dataPath = Path.Combine(dinfo.FullName, DataFolderName);
					if (DirectoryUtils.Exists(dataPath))
					{
						return true;
					}
				}
			}

			dataPath = null;
			appName = null;
			return false;
		}

		private void CollectiOSStreamingAssets(DirectoryInfo root, IDictionary<string, string> files)
		{
			string streamingPath = Path.Combine(root.FullName, iOSStreamingName);
			DirectoryInfo streamingDirectory = new DirectoryInfo(streamingPath);
			if (streamingDirectory.Exists)
			{
				CollectAssetBundlesRecursivly(root, files);
			}
		}

		public override string Name { get; }
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

		private const string iOSStreamingName = "Raw";

		private const string PayloadName = "Payload";
		private const string AppExtension = ".app";

		private readonly DirectoryInfo m_root;
	}
}