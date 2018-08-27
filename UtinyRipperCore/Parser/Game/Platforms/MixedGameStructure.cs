using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UtinyRipper
{
	internal class MixedGameStructure : PlatformGameStructure
	{
		public MixedGameStructure(FileCollection collection, IEnumerable<string> pathes) :
			base(collection)
		{
			Dictionary<string, string> files = new Dictionary<string, string>();
			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			HashSet<string> dataPathes = new HashSet<string>();
			foreach (string path in pathes)
			{
				if (FileMultiStream.Exists(path))
				{
					string name = FileMultiStream.GetFileName(path);
					files.Add(name, path);
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
			SetScriptingBackend();
			Name = Files.First().Key;
		}

		private void CollectFromDirectory(DirectoryInfo root, IDictionary<string, string> files, IDictionary<string, string> assemblies, ISet<string> dataPathes)
		{
			int count = files.Count;
			CollectGameFiles(root, files);
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

		public override string Name { get; }
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

	}
}
