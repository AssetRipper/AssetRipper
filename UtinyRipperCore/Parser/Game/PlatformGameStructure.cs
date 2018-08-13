using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UtinyRipper.AssetExporters;

namespace UtinyRipper
{
	public abstract class PlatformGameStructure : IGameStructure
	{
		public PlatformGameStructure(FileCollection collection)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			m_fileCollection = collection;
		}

		protected static string GetPresentFileName(string rootPath, string fileName)
		{
			if (RunetimeUtils.IsRunningOnMono)
			{
				if (Directory.Exists(rootPath))
				{
					fileName = Path.GetFileName(fileName).ToLower(CultureInfo.InvariantCulture);
					DirectoryInfo root = new DirectoryInfo(rootPath);
					foreach (FileInfo file in root.EnumerateFiles())
					{
						if (file.Name.ToLower(CultureInfo.InvariantCulture) == fileName)
						{
							return file.Name;
						}
					}
				}
			}
			return fileName;
		}

		protected static string GetPresentDirectoryName(string rootPath, string directoryName)
		{
			if (RunetimeUtils.IsRunningOnMono)
			{
				if (Directory.Exists(rootPath))
				{
					directoryName = Path.GetFileName(directoryName).ToLower(CultureInfo.InvariantCulture);
					DirectoryInfo root = new DirectoryInfo(rootPath);
					foreach (DirectoryInfo directory in root.EnumerateDirectories())
					{
						if (directory.Name.ToLower(CultureInfo.InvariantCulture) == directoryName)
						{
							return directory.Name;
						}
					}
				}
			}
			return directoryName;
		}

		private static bool IsDefaultResource(string fileName)
		{
			return fileName == DefaultResourceName1 || fileName == DefaultResourceName2;
		}

		private static bool IsBuiltinExtra(string fileName)
		{
			return fileName == BuiltinExtraName1 || fileName == BuiltinExtraName2;
		}

		public virtual IEnumerable<string> FetchFiles()
		{
			string filePath = Path.Combine(MainDataPath, MainDataName);
			if (File.Exists(filePath))
			{
				yield return filePath;
			}

			filePath = Path.Combine(MainDataPath, GlobalGameManagerName);
			if (File.Exists(filePath))
			{
				yield return filePath;
			}

			foreach(string dataPath in DataPathes)
			{
				DirectoryInfo dataDirectory = new DirectoryInfo(dataPath);
				foreach (FileInfo levelFile in dataDirectory.EnumerateFiles())
				{
					if (m_levelName.IsMatch(levelFile.Name))
					{
						yield return levelFile.FullName;
					}
				}

				string streamingPath = Path.Combine(dataPath, StreamingName);
				DirectoryInfo streamingDirectory = new DirectoryInfo(streamingPath);
				if (streamingDirectory.Exists)
				{
					foreach (string path in FetchAssetBundles(streamingDirectory))
					{
						yield return path;
					}
				}
			}
		}

		public virtual IEnumerable<string> FetchAssemblies()
		{
			DirectoryInfo managedDirectory = new DirectoryInfo(ManagedPath);
			foreach (FileInfo assemblyFile in managedDirectory.EnumerateFiles())
			{
				if (AssemblyManager.IsAssembly(assemblyFile.Name))
				{
					yield return assemblyFile.FullName;
				}
			}
		}

		public virtual bool RequestDependency(string dependency)
		{
			foreach (string dataPath in DataPathes)
			{
				string filePath = Path.Combine(dataPath, dependency);
				if (FileMultiStream.Exists(filePath))
				{
					m_fileCollection.Load(filePath);
					return true;
				}

				if (dependency.StartsWith(LibraryFolder, StringComparison.Ordinal))
				{
					dependency = dependency.Substring(LibraryFolder.Length + 1);
				}
				if (IsDefaultResource(dependency))
				{
					if (LoadEngineDependency(dataPath, DefaultResourceName1, dependency))
					{
						return true;
					}
					if (LoadEngineDependency(dataPath, DefaultResourceName2, dependency))
					{
						return true;
					}
				}
				else if (IsBuiltinExtra(dependency))
				{
					if (LoadEngineDependency(dataPath, BuiltinExtraName1, dependency))
					{
						return true;
					}
					if (LoadEngineDependency(dataPath, BuiltinExtraName2, dependency))
					{
						return true;
					}
				}
			}
			return false;
		}

		public virtual bool RequestAssembly(string assembly)
		{
			foreach(string assemblyPath in FetchAssemblies())
			{
				string fileName = Path.GetFileName(assemblyPath);
				if (fileName == assembly)
				{
					m_fileCollection.LoadAssembly(assemblyPath);
					return true;
				}

				fileName = Path.GetFileNameWithoutExtension(fileName);
				if (fileName == assembly)
				{
					m_fileCollection.LoadAssembly(assemblyPath);
					return true;
				}
			}
			
			return false;
		}
		
		protected IEnumerable<string> FetchAssetBundles(DirectoryInfo root)
		{
			foreach(FileInfo file in root.EnumerateFiles())
			{
				if(file.Extension == AssetBundleExtension)
				{
					yield return file.FullName;
				}
			}
			foreach(DirectoryInfo directory in root.EnumerateDirectories())
			{
				foreach(string path in FetchAssetBundles(directory))
				{
					yield return path;
				}
			}
		}

		private bool LoadEngineDependency(string path, string dependency, string originName)
		{
			string filePath = Path.Combine(path, dependency);
			if (File.Exists(filePath))
			{
				m_fileCollection.LoadSerializedFile(filePath, originName);
				return true;
			}

			string resourcePath = Path.Combine(path, ResourceName);
			filePath = Path.Combine(resourcePath, dependency);
			if (File.Exists(filePath))
			{
				m_fileCollection.LoadSerializedFile(filePath, originName);
				return true;
			}
			return false;
		}

		public abstract string Name { get; }
		public abstract string MainDataPath { get; }
		public abstract IEnumerable<string> DataPathes { get; }
		public string ManagedPath => Path.Combine(MainDataPath, ManagedName);
		protected string ResourcePath => Path.Combine(MainDataPath, ResourceName);

		protected const string ManagedName = "Managed";
		protected const string ResourceName = "Resources";
		protected const string StreamingName = "StreamingAssets";

		protected const string MainDataName = "mainData";
		protected const string GlobalGameManagerName = "globalgamemanagers";
		protected const string LevelPrefix = "level";

		protected const string AssetBundleExtension = ".unity3d";

		private const string LibraryFolder = "library";
		private const string DefaultResourceName1 = "unity default resources";
		private const string DefaultResourceName2 = "unity_default_resources";
		private const string BuiltinExtraName1 = "unity builtin extra";
		private const string BuiltinExtraName2 = "unity_builtin_extra";

		protected readonly FileCollection m_fileCollection;
		protected readonly Regex m_levelName = new Regex("level[0-9]{1,3}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
