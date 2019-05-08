using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
				if (FileMultiStream.Exists(path))
				{
					string name = FileMultiStream.GetFileName(path);
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

			if (files.Count == 0)
			{
				throw new Exception("No files has been found");
			}

			DataPathes = dataPathes.ToArray();
			Files = files;
			Assemblies = assemblies;
			Name = Files.First().Key;
		}

		private IEnumerable<string> SelectUniquePathes(IEnumerable<string> pathes)
		{
			return pathes.Select(t => FileMultiStream.GetFilePath(t)).Distinct();
		}

		private void CollectFromDirectory(DirectoryInfo root, IDictionary<string, string> files, IDictionary<string, string> assemblies, ISet<string> dataPathes)
		{
			int count = files.Count;
			CollectGameFiles(root, files);
			CollectWebFiles(root, files);
			CollectAssetBundles(root, files);
			CollectAssemblies(root, assemblies);
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
					case WebGLStructure.DataExtension:
					case WebGLStructure.DataGzExtension:
						{
							string name = Path.GetFileNameWithoutExtension(levelFile.Name);
							AddFile(files, name, levelFile.FullName);
						}
						break;

					case WebGLStructure.UnityWebExtension:
						{
							if(levelFile.Name.EndsWith(WebGLStructure.DataWebExtension, StringComparison.Ordinal))
							{
								string name = levelFile.Name.Substring(0, levelFile.Name.Length - WebGLStructure.DataWebExtension.Length);
								AddFile(files, name, levelFile.FullName);
							}
						}
						break;
				}
			}
		}

		public override string Name { get; }
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

	}
}
