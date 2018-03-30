//#define DEBUG_PROGRAM

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UtinyRipper.SerializedFiles;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper
{
	public class Program
	{
		public static IEnumerable<Object> FetchExportObjects(AssetCollection collection)
		{
			//yield break;
			return collection.FetchAssets();
		}

		public static void Main(string[] args)
		{
			Logger.Instance = ConsoleLogger.Instance;
			Config.IsAdvancedLog = true;
			Config.IsGenerateGUIDByContent = false;
			Config.IsExportDependencies = false;

			if (args.Length == 0)
			{
				Console.WriteLine("No arguments");
				Console.ReadKey();
				return;
			}
			foreach(string arg in args)
			{
				if(!FileMultiStream.Exists(arg))
				{
					Console.WriteLine(FileMultiStream.IsMultiFile(arg) ?
						$"File {arg} doen't has all parts for combining" :
						$"File {arg} doesn't exist", arg);
					Console.ReadKey();
					return;
				}
			}

#if !DEBUG_PROGRAM
			try
#endif
			{
				AssetCollection collection = new AssetCollection();
				LoadFiles(collection, args);

				LoadDependencies(collection, args);
				ValidateCollection(collection);

				string name = Path.GetFileNameWithoutExtension(args.First());
				string exportPath = ".\\Ripped\\" + name;
				if (Directory.Exists(exportPath))
				{
					Directory.Delete(exportPath, true);
				}
				collection.Exporter.Export(exportPath, FetchExportObjects(collection));

				Logger.Instance.Log(LogType.Info, LogCategory.General, "Finished");
			}
#if !DEBUG_PROGRAM
			catch(Exception ex)
			{
				Logger.Instance.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
#endif
			Console.ReadKey();
		}

		private static void LoadFiles(AssetCollection collection, IEnumerable<string> filePathes)
		{
			List<string> processed = new List<string>();
			foreach (string path in filePathes)
			{
				string filePath = FileMultiStream.GetFilePath(path);
				if (processed.Contains(filePath))
				{
					continue;
				}
				
				string fileName = FileMultiStream.GetFileName(path);
				using (Stream stream = FileMultiStream.OpenRead(path))
				{
					collection.Read(stream, filePath, fileName);
				}
				processed.Add(filePath);
			}
		}

		private static void ValidateCollection(AssetCollection collection)
		{
			Version[] versions = collection.Files.Select(t => t.Version).Distinct().ToArray();
			if(versions.Count() > 1)
			{
				Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Asset collection has (possibly) incompatible with each assets file versions. Here they are:");
				foreach(Version version in versions)
				{
					Logger.Instance.Log(LogType.Warning, LogCategory.Import, version.ToString());
				}
			}
		}

		private static void LoadDependencies(AssetCollection collection, IEnumerable<string> files)
		{
			HashSet<string> directories = new HashSet<string>();
			foreach (string filePath in files)
			{
				string dirPath = Path.GetDirectoryName(filePath);
				directories.Add(dirPath);
			}

			HashSet<string> processed = new HashSet<string>();
			foreach (ISerializedFile file in collection.Files)
			{
				processed.Add(file.Name);
			}

			for (int i = 0; i < collection.Files.Count; i++)
			{
				ISerializedFile serializedFile = collection.Files[i];
				foreach (FileIdentifier file in serializedFile.Dependencies)
				{
					string fileName = file.FilePath;
					if (processed.Contains(fileName))
					{
						continue;
					}
					
					LoadDependency(collection, directories, fileName);
					processed.Add(fileName);
				}
			}
		}

		private static void LoadDependency(AssetCollection collection, IReadOnlyCollection<string> directories, string fileName)
		{
			foreach (string loadName in FetchNameVariants(fileName))
			{
				bool found = TryLoadDependency(collection, directories, fileName, loadName);
				if (found)
				{
					return;
				}
			}

			Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Dependency '{fileName}' wasn't found");
		}

		private static bool TryLoadDependency(AssetCollection collection, IEnumerable<string> directories, string originalName, string loadName)
		{
			foreach (string dirPath in directories)
			{
				string path = Path.Combine(dirPath, loadName);
#if !DEBUG_PROGRAM
				try
#endif
				{
					if (FileMultiStream.Exists(path))
					{
						using (Stream stream = FileMultiStream.OpenRead(path))
						{
							collection.Read(stream, path, originalName);
						}

						Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Dependency '{path}' was loaded");
						return true;
					}
				}
#if !DEBUG_PROGRAM
				catch (Exception ex)
				{
					Logger.Instance.Log(LogType.Error, LogCategory.Import, $"Can't parse dependency file {path}");
					Logger.Instance.Log(LogType.Error, LogCategory.Debug, ex.ToString());
				}
#endif
			}
			return false;
		}
		
		private static IEnumerable<string> FetchNameVariants(string name)
		{
			yield return name;

			const string libraryFolder = "library";
			if (name.ToLower().StartsWith(libraryFolder))
			{
				string fixedName = name.Substring(libraryFolder.Length + 1);
				yield return fixedName;

				fixedName = Path.Combine("Resources", fixedName);
				yield return fixedName;
			}
		}
	}
}
