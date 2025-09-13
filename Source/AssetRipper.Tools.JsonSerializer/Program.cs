using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

internal static class Program
{
	private static readonly string outputDirectory = Path.Join(AppContext.BaseDirectory, "Output");

	static void Main(string[] args)
	{
		Directory.CreateDirectory(outputDirectory);
		if (args.Length == 0)
		{
			Console.WriteLine("No arguments");
		}
		else
		{
			LoadFiles(args);
		}
		Console.WriteLine("Done!");
		Console.ReadKey();
		return;
	}

	private static void LoadFiles(string[] files)
	{
		foreach (string file in files)
		{
			LoadFile(file);
		}
	}

	private static void LoadFile(string fullName)
	{
		Console.WriteLine(fullName);
#if !DEBUG
		try
#endif
		{
			FileBase file = SchemeReader.LoadFile(fullName, LocalFileSystem.Instance);
			if (file is SerializedFile serializedFile)
			{
				ExtractJson(serializedFile);
			}
			else if (file is FileContainer container)
			{
				file.ReadContents();
				foreach (SerializedFile serializedFile1 in container.FetchSerializedFiles())
				{
					ExtractJson(serializedFile1);
				}
			}
			else
			{
				Console.WriteLine($"File is {file.GetType()}");
			}
		}
#if !DEBUG
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
#endif
	}

	private static void ExtractJson(SerializedFile file)
	{
		GameBundle bundle = new();
		SerializedAssetCollection collection = bundle.AddCollectionFromSerializedFile(file, new JsonAssetFactory());
		bundle.InitializeAllDependencyLists();
		ExtractJson(file, collection);
	}

	private static void ExtractJson(SerializedFile file, SerializedAssetCollection collection)
	{
		JsonArray array = new();
		JsonObject root = new()
			{
				{ "Version", file.Version.ToString() },
				{ "Assets", array }
			};
		foreach ((_, IUnityObjectBase asset) in collection.Assets)
		{
			JsonObject assetObject = new();
			array.Add((JsonNode)assetObject);
			assetObject.Add("PathID", asset.PathID);
			assetObject.Add("ClassID", asset.ClassID);
			assetObject.Add("Fields", ((JsonAsset)asset).Contents);
			//Note: this assigns assetObject as the parent of Contents.
			//Normally, this would be a cause for concern, but the asset won't be used after this.
		}
		using FileStream stream = File.Create(Path.Join(outputDirectory, $"{file.NameFixed}.json"));
		System.Text.Json.JsonSerializer.Serialize(stream, root, JsonObjectSerializerContext.Default.JsonObject);
	}
}
