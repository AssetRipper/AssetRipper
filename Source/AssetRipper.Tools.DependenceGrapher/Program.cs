using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.AssetCreation;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.Tools.DependenceGrapher.Filters;
using System.CommandLine;
using System.Text.Json;

namespace AssetRipper.Tools.DependenceGrapher
{
	internal static class Program
	{
		/// <summary>
		/// This is chosen as the default because class ID numbers are always non-negative.
		/// </summary>
		private const int DefaultClassID = -1;
		/// <summary>
		/// This is chosen as the default because Unity treats a zero path id as being a null pointer.
		/// </summary>
		private const long DefaultPathID = 0;

		static void Main(string[] args)
		{
			RootCommand rootCommand = new() { Description = "AssetRipper Dependence Grapher" };

			Argument<List<string>> filesToExportOption = new();
			rootCommand.AddArgument(filesToExportOption);

			Option<string?> outputOption = new Option<string?>(
							aliases: new[] { "-o", "--output" },
							description: "The output file to save the information. If not specified, it will be called \"output.txt\".",
							getDefaultValue: () => null);
			rootCommand.AddOption(outputOption);

			Option<string?> cabMapOption = new Option<string?>(
							name: "--cab-map",
							description: "If provided, a cab map json file will be used to list bundle names for referenced files.",
							getDefaultValue: () => null);
			rootCommand.AddOption(cabMapOption);

			Option<string?> nameOption = new Option<string?>(
							name: "--name",
							description: "If used, only assets with this name will be analyzed for external references.",
							getDefaultValue: () => null);
			rootCommand.AddOption(nameOption);

			Option<string?> classNameOption = new Option<string?>(
							name: "--class-name",
							description: "If used, only assets with this class name will be analyzed for external references.",
							getDefaultValue: () => null);
			rootCommand.AddOption(classNameOption);

			Option<int> classIDOption = new Option<int>(
							name: "--class-id",
							description: "If used, only assets with this class ID will be analyzed for external references.",
							getDefaultValue: () => DefaultClassID);
			rootCommand.AddOption(classIDOption);

			Option<long> pathIDOption = new Option<long>(
							name: "--path-id",
							description: "If used, only assets with this PathID will be analyzed for external references.",
							getDefaultValue: () => DefaultPathID);
			rootCommand.AddOption(pathIDOption);

			Option<bool> verboseOption = new Option<bool>(
							aliases: new[] { "-v", "--verbose" },
							description: "If true, additional information will be outputted about referencing assets.",
							getDefaultValue: () => false);
			rootCommand.AddOption(verboseOption);

			rootCommand.SetHandler((List<string> filesToExport, string? outputFile, string? cabMapPath, string? name, string? className, int classID, long pathID, bool verbose) =>
			{
				if (filesToExport.Count == 0)
				{
					Console.WriteLine("No files were specified for analysis.");
					return;
				}

				Dictionary<string, string> cabMap;
				if (File.Exists(cabMapPath))
				{
					using FileStream cabMapStream = File.OpenRead(cabMapPath);
					cabMap = JsonSerializer.Deserialize(cabMapPath, DictionarySerializerContext.Default.DictionaryStringString) ?? new();
				}
				else
				{
					cabMap = new();
				}

				List<IAssetFilter> filters = CreateFilterList(name, className, classID, pathID);
				if (string.IsNullOrEmpty(outputFile))
				{
					outputFile = Path.Combine(AppContext.BaseDirectory, "output.txt");
				}
				using FileStream stream = File.Create(outputFile);
				using TextWriter writer = new StreamWriter(stream);
				LoadFiles(GetAllFilePaths(filesToExport), writer, filters, verbose, cabMap);
				writer.Flush();
				Console.WriteLine("Done!");
			},
			filesToExportOption, outputOption, cabMapOption, nameOption, classNameOption, classIDOption, pathIDOption, verboseOption);

			rootCommand.Invoke(args);
		}

		private static IEnumerable<string> GetAllFilePaths(IEnumerable<string> paths)
		{
			foreach (string path in paths)
			{
				if (File.Exists(path))
				{
					yield return path;
				}
				else if (Directory.Exists(path))
				{
					foreach (string filePath in Directory.EnumerateFiles(path))
					{
						yield return filePath;
					}
				}
				else
				{
					Console.WriteLine($"No file or directory exists at {path}");
				}
			}
		}

		private static void LoadFiles(IEnumerable<string> files, TextWriter writer, List<IAssetFilter> filters, bool verbose, Dictionary<string, string> cabMap)
		{
			GameAssetFactory factory = new GameAssetFactory(new BaseManager(s => { }));
			foreach (string file in files)
			{
				LoadFile(file, factory, writer, filters, verbose, cabMap);
			}
		}

		private static void LoadFile(string fullName, GameAssetFactory factory, TextWriter writer, List<IAssetFilter> filters, bool verbose, Dictionary<string, string> cabMap)
		{
#if !DEBUG
			try
#endif
			{
				FileBase file = SchemeReader.LoadFile(fullName);
				if (file is SerializedFile serializedFile)
				{
					writer.WriteLine($"Dependencies of serialized file [{serializedFile.NameFixed}]:");
					writer.WriteLine();
					ExtractDependencies(serializedFile, factory, writer, filters, verbose, cabMap);
				}
				else if (file is FileContainer container)
				{
					file.ReadContents();
					foreach (SerializedFile serializedFile1 in container.FetchSerializedFiles())
					{
						writer.WriteLine($"Dependencies of serialized file [{serializedFile1.NameFixed}] in bundle [{container.NameFixed}]:");
						writer.WriteLine();
						ExtractDependencies(serializedFile1, factory, writer, filters, verbose, cabMap);
					}
				}
				else
				{
					string message = $"Error: File is {file.GetType()}";
					Console.WriteLine(message);
				}
			}
#if !DEBUG
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
#endif
		}

		private static List<IAssetFilter> CreateFilterList(string? name, string? className, int classID, long pathID)
		{
			List<IAssetFilter> filters = new();
			if (!string.IsNullOrEmpty(name))
			{
				filters.Add(new NameFilter(name));
			}
			if (!string.IsNullOrEmpty(className))
			{
				filters.Add(new ClassNameFilter(className));
			}
			if (classID != DefaultClassID)
			{
				filters.Add(new ClassIDFilter(classID));
			}
			if (pathID != DefaultPathID)
			{
				filters.Add(new PathIDFilter(pathID));
			}

			return filters;
		}

		private static void ExtractDependencies(SerializedFile file, GameAssetFactory factory, TextWriter writer, List<IAssetFilter> filters, bool verbose, Dictionary<string, string> cabMap)
		{
			GameBundle bundle = new();
			
			SerializedAssetCollection collection = bundle.AddCollectionFromSerializedFile(file, factory);
			bundle.InitializeAllDependencyLists();
			ExtractDependencies(file, collection, writer, filters, verbose, cabMap);
		}

		private static void ExtractDependencies(SerializedFile file, SerializedAssetCollection collection, TextWriter writer, List<IAssetFilter> filters, bool verbose, Dictionary<string, string> cabMap)
		{
			Dictionary<string, LinkedList<(string, IUnityObjectBase)>> results = new();
			foreach (IUnityObjectBase asset in collection)
			{
				if (filters.All(filter => filter.IsAcceptable(asset)))
				{
					foreach ((string fieldName, PPtr<IUnityObjectBase> pptr) in asset.FetchDependencies())
					{
						if (pptr.FileID > 0)
						{
							FileIdentifier identifier = file.Dependencies[pptr.FileID - 1];
							results.GetOrAdd(identifier.GetFilePath()).AddLast((fieldName, asset));
						}
					}
				}
			}
			foreach ((string referenceFileName, LinkedList<(string, IUnityObjectBase)> list) in results)
			{
				if (cabMap.TryGetValue(referenceFileName, out string? bundleName))
				{
					writer.WriteLine($"* {referenceFileName} in {bundleName}:");
				}
				else
				{
					writer.WriteLine($"* {referenceFileName}:");
				}

				foreach ((string fieldName, IUnityObjectBase asset) in list)
				{
					if (verbose)
					{
						writer.WriteLine($"  - {asset.GetName()}: {fieldName} (Class: {asset.ClassID} {asset.ClassName}, PathID: {asset.PathID})");
					}
					else
					{
						writer.WriteLine($"  - {asset.GetName()}: {fieldName}");
					}
				}
				writer.WriteLine();
			}
		}
	}
}
