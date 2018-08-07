using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.AssetExporters.Mono;

namespace UtinyRipper
{
	internal class PCGameStructure : PlatformGameStructure
	{
		public PCGameStructure(FileCollection collection, string rootPath):
			base(collection)
		{
			if(string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			m_root = new DirectoryInfo(rootPath);
			if(!m_root.Exists)
			{
				throw new Exception($"Directory '{rootPath}' doesn't exist");
			}

			m_dataPath = GetDataPCDirectory(m_root);
			if (m_dataPath == null)
			{
				throw new Exception($"Data directory hasn't been found");
			}

			DirectoryInfo managedDirectory = new DirectoryInfo(ManagedPath);
			if (!managedDirectory.Exists)
			{
				throw new Exception($"Managed directory hasn't been found");
			}

			foreach(FileInfo assemblyFile in managedDirectory.EnumerateFiles())
			{
				if(AssemblyManager.IsAssembly(assemblyFile.Name))
				{
					if(MonoManager.IsMonoAssembly(assemblyFile.Name))
					{
						m_fileCollection.AssemblyManager.ScriptingBackEnd = ScriptingBackEnd.Mono;
					}
					else
					{
						m_fileCollection.AssemblyManager.ScriptingBackEnd = ScriptingBackEnd.Il2Cpp;
					}
					break;
				}
			}
		}

		public static bool IsPCStructure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(path);
			if(!dinfo.Exists)
			{
				return false;
			}
			return IsRootPCDirectory(dinfo);
		}

		private static bool IsRootPCDirectory(DirectoryInfo rootDiectory)
		{
			return GetDataPCDirectory(rootDiectory) != null;
		}
		
		private static string GetDataPCDirectory(DirectoryInfo rootDiectory)
		{
			foreach (FileInfo finfo in rootDiectory.EnumerateFiles())
			{
				if (finfo.Extension == ExeExtension)
				{
					string exeName = Path.GetFileNameWithoutExtension(finfo.Name);
					string dataFolder = $"{exeName}_{DataPostfix}";
					string dataDirectory = Path.Combine(rootDiectory.FullName, dataFolder);
					if (Directory.Exists(dataDirectory))
					{
						return dataDirectory;
					}
				}
			}
			return null;
		}

		public override string Name => m_root.Name;
		public override string MainDataPath => m_dataPath;
		public override IEnumerable<string> DataPathes => new string[] { MainDataPath };

		private const string DataPostfix = "Data";
		private const string ResourcesName = "Resources";

		private const string ExeExtension = ".exe";

		private readonly DirectoryInfo m_root;
		private readonly string m_dataPath;
		private readonly string m_managedPath;
	}
}
