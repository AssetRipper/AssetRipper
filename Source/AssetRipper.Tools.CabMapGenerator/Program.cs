using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using System.Text.Json;

namespace AssetRipper.Tools.CabMapGenerator;

internal static class Program
{
	static void Main(string[] args)
	{
		Arguments? arguments = Arguments.Parse(args);
		if (arguments is null)
		{
			return;
		}

		if (arguments.FilesToExport is null or { Length: 0 })
		{
			Console.WriteLine("No files were specified for analysis.");
			return;
		}

		if (string.IsNullOrEmpty(arguments.OutputFile))
		{
			arguments.OutputFile = Path.Join(AppContext.BaseDirectory, "cabmap.json");
		}
		using FileStream stream = File.Create(arguments.OutputFile);
		Dictionary<string, string> map = new();
		LoadFiles(GetAllFilePaths(arguments.FilesToExport), map);
		JsonSerializer.Serialize(stream, map, DictionarySerializerContext.Default.DictionaryStringString);
		Console.WriteLine("Done!");
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
			FileBase file = SchemeReader.LoadFile(fullName, LocalFileSystem.Instance);
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
