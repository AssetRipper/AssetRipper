using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using uTinyRipper.Game;

namespace uTinyRipper
{
	internal class MixedGameStructure : PlatformGameStructure
	{
		public MixedGameStructure(IEnumerable<string> pathes)
		{
			Dictionary<string, string> files = new Dictionary<string, string>();
			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			HashSet<string> dataPathes = new HashSet<string>();
			foreach (string path in SelectUniquePathes(pathes))
			{
				if (MultiFileStream.Exists(path))
				{
					string name = MultiFileStream.GetFileName(path);
					AddFile(files, name, path);
					string directory = Path.GetDirectoryName(path);
					dataPathes.Add(directory);
				}
				else if (DirectoryUtils.Exists(path))
				{
					DirectoryInfo directory = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
					CollectFromDirectory(directory, files, assemblies, dataPathes);
				}
				else
				{
					throw new Exception($"Neither file nor directory at '{path}' exists");
				}
			}

			DataPathes = dataPathes.ToArray();
			Files = files;
			Assemblies = assemblies;
			Name = Files.Count == 0 ? string.Empty : Files.First().Key;
		}

		private IEnumerable<string> SelectUniquePathes(IEnumerable<string> pathes)
		{
			return pathes.Select(t => MultiFileStream.GetFilePath(t)).Distinct();
		}

		private void CollectFromDirectory(DirectoryInfo root, IDictionary<string, string> files, IDictionary<string, string> assemblies, ISet<string> dataPathes)
		{
			int count = files.Count;
			CollectSerializedGameFiles(root, files);
			CollectWebFiles(root, files);
			CollectAssetBundles(root, files);
			CollectAssembliesSafe(root, assemblies);
			if (files.Count != count)
			{
				dataPathes.Add(root.FullName);
			}

			foreach (DirectoryInfo subDirectory in root.EnumerateDirectories())
			{
				CollectFromDirectory(subDirectory, files, assemblies, dataPathes);
			}
		}

		private void CollectWebFiles(DirectoryInfo root, IDictionary<string, string> files)
		{
			foreach (FileInfo levelFile in root.EnumerateFiles())
			{
				string extension = Path.GetExtension(levelFile.Name);
				switch(extension)
				{
					case WebGLGameStructure.DataExtension:
					case WebGLGameStructure.DataGzExtension:
						{
							string name = Path.GetFileNameWithoutExtension(levelFile.Name);
							AddFile(files, name, levelFile.FullName);
						}
						break;

					case WebGLGameStructure.UnityWebExtension:
						{
							if(levelFile.Name.EndsWith(WebGLGameStructure.DataWebExtension, StringComparison.Ordinal))
							{
								string name = levelFile.Name.Substring(0, levelFile.Name.Length - WebGLGameStructure.DataWebExtension.Length);
								AddFile(files, name, levelFile.FullName);
							}
						}
						break;
				}
			}
		}

		private void CollectAssembliesSafe(DirectoryInfo root, IDictionary<string, string> assemblies)
		{
			foreach (FileInfo file in root.EnumerateFiles())
			{
				if (AssemblyManager.IsAssembly(file.Name))
				{
					if (assemblies.ContainsKey(file.Name))
					{
						Logger.Log(LogType.Warning, LogCategory.Import, $"Duplicate assemblies found: '{assemblies[file.Name]}' & '{file.FullName}'");
					}
					else
					{
						assemblies.Add(file.Name, file.FullName);
					}
				}
			}
		}

		public override string Name { get; }
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

	}
}
