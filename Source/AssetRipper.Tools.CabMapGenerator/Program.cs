using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using System.CommandLine;
using System.Text.Json;

namespace AssetRipper.Tools.CabMapGenerator
{
	internal static class Program
	{
		static void Main(string[] args)
		{
			RootCommand rootCommand = new() { Description = "AssetRipper Cab Map Generator" };

			Argument<List<string>> filesToExportOption = new();
			rootCommand.AddArgument(filesToExportOption);

			Option<string?> outputOption = new Option<string?>(
							aliases: new[] { "-o", "--output" },
							description: "The output file to save the information. If not specified, it will be called \"cabmap.json\".",
							getDefaultValue: () => null);
			rootCommand.AddOption(outputOption);

			rootCommand.SetHandler((List<string> filesToExport, string? outputFile) =>
			{
				if (filesToExport.Count == 0)
				{
					Console.WriteLine("No files were specified for analysis.");
					return;
				}

				if (string.IsNullOrEmpty(outputFile))
				{
					outputFile = Path.Combine(AppContext.BaseDirectory, "cabmap.json");
				}
				using FileStream stream = File.Create(outputFile);
				Dictionary<string, string> map = new();
				LoadFiles(GetAllFilePaths(filesToExport), map);
				JsonSerializer.Serialize(stream, map, DictionarySerializerContext.Default.DictionaryStringString);
				Console.WriteLine("Done!");
			},
			filesToExportOption, outputOption);

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

		private static void LoadFiles(IEnumerable<string> files, Dictionary<string, string> map)
		{
			foreach (string file in files)
			{
				LoadFile(file, map);
			}
		}

		private static void LoadFile(string fullName, Dictionary<string, string> map)
		{
#if !DEBUG
			try
#endif
			{
				FileBase file = SchemeReader.LoadFile(fullName);
				if (file is SerializedFile serializedFile)
				{
					Console.WriteLine($"Skipping non-bundled serialized file [{serializedFile.NameFixed}]");
				}
				else if (file is FileContainer container)
				{
					file.ReadContents();
					foreach (SerializedFile serializedFile1 in container.FetchSerializedFiles())
					{
						Console.WriteLine($"Found serialized file [{serializedFile1.NameFixed}] in bundle [{container.NameFixed}]:");
						if (!map.TryAdd(serializedFile1.NameFixed, container.NameFixed))
						{
							Console.WriteLine($"Serialized file [{serializedFile1.NameFixed}] is already listed in the map.");
						}
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
	}
}
