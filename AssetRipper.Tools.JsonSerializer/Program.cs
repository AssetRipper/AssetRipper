using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using System;
using System.Text.Json.Nodes;

namespace AssetRipper.Tools.JsonSerializer;

internal static class Program
{
	private static readonly string outputDirectory = System.IO.Path.Combine(AppContext.BaseDirectory, "Output");

	static void Main(string[] args)
	{
		System.IO.Directory.CreateDirectory(outputDirectory);
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
			GameBundle bundle = new();
			SerializedFile file = (SerializedFile)SchemeReader.LoadFile(fullName);
			SerializedAssetCollection collection = bundle.AddCollectionFromSerializedFile(file, new JsonAssetFactory());
			bundle.InitializeAllDependencyLists();
			JsonArray array = new();
			JsonObject root = new()
			{
				{ "Version", file.Version.ToString() },
				{ "Assets", array }
			};
			foreach ((_, IUnityObjectBase asset) in collection.Assets)
			{
				JsonObject assetObject = new();
				array.Add(assetObject);
				assetObject.Add("PathID", asset.PathID);
				assetObject.Add("ClassID", asset.ClassID);
				assetObject.Add("Fields", ((JsonAsset)asset).Contents);
				//Note: this assigns assetObject as the parent of Contents.
				//Normally, this would be a cause for concern, but the asset won't be used after this.
			}
			System.IO.File.WriteAllText(System.IO.Path.Combine(outputDirectory, $"{file.NameFixed}.json"), root.ToJsonString(new() { WriteIndented = true }));
		}
#if !DEBUG
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
#endif
	}
}
