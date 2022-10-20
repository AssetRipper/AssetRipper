using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.Metadata;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Structure;
using AssetRipper.Core.Structure.Assembly.Managers;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.Tools.DependenceGrapher.Filters;
using System.CommandLine;

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

			rootCommand.SetHandler((List<string> filesToExport, string? outputFile, string? name, string? className, int classID, long pathID, bool verbose) =>
			{
				if (filesToExport.Count == 0)
				{
					Console.WriteLine("No files were specified for analysis.");
					return;
				}

				List<IAssetFilter> filters = CreateFilterList(name, className, classID, pathID);
				if (string.IsNullOrEmpty(outputFile))
				{
					outputFile = Path.Combine(AppContext.BaseDirectory, "output.txt");
				}
				using FileStream stream = System.IO.File.Create(outputFile);
				using TextWriter writer = new StreamWriter(stream);
				LoadFiles(filesToExport, writer, filters, verbose);
				writer.Flush();
				Console.WriteLine("Done!");
			},
			filesToExportOption, outputOption, nameOption, classNameOption, classIDOption, pathIDOption, verboseOption);

			rootCommand.Invoke(args);
		}

		private static void LoadFiles(IReadOnlyList<string> files, TextWriter writer, List<IAssetFilter> filters, bool verbose)
		{
			GameAssetFactory factory = new GameAssetFactory(new BaseManager(s => { }));
			foreach (string file in files)
			{
				LoadFile(file, factory, writer, filters, verbose);
			}
		}

		private static void LoadFile(string fullName, GameAssetFactory factory, TextWriter writer, List<IAssetFilter> filters, bool verbose)
		{
#if !DEBUG
			try
#endif
			{
				IO.Files.File file = SchemeReader.LoadFile(fullName);
				if (file is SerializedFile serializedFile)
				{
					writer.WriteLine($"Dependencies of serialized file [{serializedFile.NameFixed}]:");
					writer.WriteLine();
					ExtractDependencies(serializedFile, factory, writer, filters, verbose);
				}
				else if (file is FileContainer container)
				{
					file.ReadContents();
					foreach (SerializedFile serializedFile1 in container.FetchSerializedFiles())
					{
						writer.WriteLine($"Dependencies of serialized file [{serializedFile1.NameFixed}] in bundle [{container.NameFixed}]:");
						writer.WriteLine();
						ExtractDependencies(serializedFile1, factory, writer, filters, verbose);
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

		private static void ExtractDependencies(SerializedFile file, GameAssetFactory factory, TextWriter writer, List<IAssetFilter> filters, bool verbose)
		{
			GameBundle bundle = new();
			
			SerializedAssetCollection collection = bundle.AddCollectionFromSerializedFile(file, factory);
			bundle.InitializeAllDependencyLists();
			ExtractDependencies(file, collection, writer, filters, verbose);
		}

		private static void ExtractDependencies(SerializedFile file, SerializedAssetCollection collection, TextWriter writer, List<IAssetFilter> filters, bool verbose)
		{
			Dictionary<string, LinkedList<(string, IUnityObjectBase)>> results = new();
			foreach (IUnityObjectBase asset in collection)
			{
				if (filters.All(filter => filter.IsAcceptable(asset)))
				{
					foreach ((FieldName fieldName, PPtr<IUnityObjectBase> pptr) in asset.FetchDependencies())
					{
						if (pptr.FileID > 0)
						{
							FileIdentifier identifier = file.Dependencies[pptr.FileID - 1];
							results.GetOrAdd(identifier.GetFilePath()).AddLast((fieldName.ToString(), asset));
						}
					}
				}
			}
			foreach ((string referenceFileName, LinkedList<(string, IUnityObjectBase)> list) in results)
			{
				writer.WriteLine($"* {referenceFileName}:");
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
