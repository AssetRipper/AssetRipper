using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.Metadata;
using AssetRipper.IO.Endian;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_89;
using AssetRipper.SourceGenerated.Extensions;
using System.Diagnostics;

namespace AssetRipper.Tools.RawTextureExtractor;

internal static class Program
{
	private static readonly string outputDirectory = Path.Join(AppContext.BaseDirectory, "Output");

	static void Main(string[] args)
	{
		if (Directory.Exists(outputDirectory))
		{
			Directory.Delete(outputDirectory, true);
		}
		Directory.CreateDirectory(outputDirectory);
		if (args.Length == 0)
		{
			Console.WriteLine("No arguments");
		}
		else
		{
			LoadFiles(args);
			Console.WriteLine("Done!");
		}
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
				GameBundle bundle = new();
				SerializedAssetCollection collection = bundle.AddCollectionFromSerializedFile(serializedFile, new TextureAssetFactory());
				bundle.InitializeAllDependencyLists();
				Extract(collection);
			}
			else if (file is FileContainer container)
			{
				file.ReadContents();
				SerializedBundle serializedBundle = SerializedBundle.FromFileContainer(container, new TextureAssetFactory());
				foreach (AssetCollection collection in serializedBundle.FetchAssetCollections())
				{
					Extract(collection);
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

	private static void Extract(AssetCollection collection)
	{
		const string jsonExtension = ".json";

		string collectionOutputPath = Path.Join(GetReversedName(collection).Reverse().ToArray());
		Directory.CreateDirectory(collectionOutputPath);
		foreach (ITexture2D texture in collection.OfType<ITexture2D>())
		{
			byte[] data = texture.GetImageData();
			if (data.Length > 0)
			{
				string originalName = texture.Name;
				string name = originalName.Length > 0
					? FileSystem.FixInvalidFileNameCharacters(originalName)
					: $"{texture.ClassName}_{ToValidString(texture.PathID)}";
				Debug.Assert(name.Length > 0);
				string uniqueName = LocalFileSystem.Instance.GetUniqueName(collectionOutputPath, name, FileSystem.MaxFileNameLength - jsonExtension.Length);
				string dataFilePath = Path.Join(collectionOutputPath, uniqueName);
				string infoFilePath = dataFilePath + jsonExtension;
				File.WriteAllBytes(dataFilePath, data);
				string text = $$"""
					{
						"Type" : "{{texture.ClassName}}",
						"Format" : "{{texture.Format_C28E}}",
						"FileSize" : {{data.Length}},
						"ImageSize" : {{texture.CompleteImageSize}},
						"ImageCount" : {{texture.ImageCount_C28}},
						"Mips" : {{texture.Mips.ToJson()}},
						"Width" : {{texture.Width_C28}},
						"Height" : {{texture.Height_C28}}
					}
					""";
				File.WriteAllText(infoFilePath, text);
			}
		}
	}

	private static IEnumerable<string> GetReversedName(AssetCollection collection)
	{
		yield return collection.Name;
		Bundle? bundle = collection.Bundle;
		while (bundle is not null and not GameBundle)
		{
			yield return bundle.Name;
			bundle = bundle.Parent;
		}
		yield return outputDirectory;
	}

	private static string ToValidString(long value)
	{
		if (value >= 0)
		{
			return value.ToString();
		}
		else if (value == long.MinValue)
		{
			return $"N{value.ToString().AsSpan(1)}";
		}
		else
		{
			return (-value).ToString();
		}
	}

	private static string ToJson(this bool value)
	{
		return value ? "true" : "false";
	}

	private sealed class TextureAssetFactory : AssetFactoryBase
	{
		public override IUnityObjectBase? ReadAsset(AssetInfo assetInfo, ReadOnlyArraySegment<byte> assetData, SerializedType? assetType)
		{
			IUnityObjectBase? asset = CreateAsset(assetInfo);
			if (asset is not null)
			{
				EndianSpanReader reader = new EndianSpanReader(assetData, asset.Collection.EndianType);
				return TryReadAsset(ref reader, asset.Collection.Flags, asset);
			}
			else
			{
				return null;
			}
		}

		private static IUnityObjectBase? CreateAsset(AssetInfo assetInfo)
		{
			return (ClassIDType)assetInfo.ClassID switch
			{
				ClassIDType.Texture2D => Texture2D.Create(assetInfo),
				ClassIDType.Cubemap => Cubemap.Create(assetInfo),
				_ => null
			};
		}

		private static IUnityObjectBase? TryReadAsset(ref EndianSpanReader reader, TransferInstructionFlags flags, IUnityObjectBase asset)
		{
			try
			{
				asset.Read(ref reader, flags);
				if (reader.Position != reader.Length)
				{
					Console.WriteLine($"Read {reader.Position} but expected {reader.Length} for asset type {(ClassIDType)asset.ClassID}. V: {asset.Collection.Version} P: {asset.Collection.Platform} N: {asset.Collection.Name} Path: {asset.Collection.FilePath}");
					return null;
				}
				else
				{
					return asset;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during reading of asset type {(ClassIDType)asset.ClassID}. V: {asset.Collection.Version} P: {asset.Collection.Platform} N: {asset.Collection.Name} Path: {asset.Collection.FilePath}\n{ex}");
				return null;
			}
		}
	}
}
