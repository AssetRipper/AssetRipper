using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Platforms;
using System.IO;

namespace AssetRipper.Processing.Addressables;

public class AddressablesProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Processing Addressables");

		string? catalogPath = FindCatalog(gameData.PlatformStructure);
		if (catalogPath == null)
		{
			Logger.Info(LogCategory.Processing, "Addressables catalog not found.");
			return;
		}

		Logger.Info(LogCategory.Processing, $"Found Addressables catalog at {catalogPath}");

		string json = gameData.PlatformStructure!.FileSystem.File.ReadAllText(catalogPath);
		AddressablesCatalog? catalog = AddressablesCatalogParser.ParseJson(json);
		if (catalog == null)
		{
			Logger.Error(LogCategory.Processing, "Failed to parse Addressables catalog.");
			return;
		}

		Logger.Info(LogCategory.Processing, $"Successfully parsed catalog with {catalog.InternalIds?.Length ?? 0} internal IDs.");

		IMonoBehaviour? settings = null;
		List<IMonoBehaviour> groups = new();

		foreach (IUnityObjectBase asset in gameData.GameBundle.FetchAssets())
		{
			if (asset is IMonoBehaviour monoBehaviour)
			{
				if (monoBehaviour.IsAddressableAssetSettings())
				{
					settings = monoBehaviour;
				}
				else if (monoBehaviour.IsAddressableAssetGroup())
				{
					groups.Add(monoBehaviour);
				}
			}
		}

		if (settings != null)
		{
			ReconstructSettings(settings, groups);
		}

		RemapReferences(gameData);
	}

	private static void RemapReferences(GameData gameData)
	{
		foreach (IUnityObjectBase asset in gameData.GameBundle.FetchAssets())
		{
			if (asset is IMonoBehaviour monoBehaviour)
			{
				SerializableStructure? structure = monoBehaviour.LoadStructure();
				if (structure != null)
				{
					RemapStructure(structure, gameData);
				}
			}
		}
	}

	private static void RemapStructure(SerializableStructure structure, GameData gameData)
	{
		foreach (var field in structure.Fields)
		{
			if (field.CValue is SerializableStructure childStructure)
			{
				if (childStructure.Type.Name == "AssetReference" || childStructure.ContainsField("m_AssetGUID"))
				{
					if (childStructure.TryGetField("m_AssetGUID", out SerializableValue guidField))
					{
						// Remap original GUID to AssetRipper generated GUID if possible
						// This is a placeholder for actual remapping logic
					}
				}
				else
				{
					RemapStructure(childStructure, gameData);
				}
			}
			else if (field.AsAssetArray != null)
			{
				foreach (var element in field.AsAssetArray)
				{
					if (element is SerializableStructure elementStructure)
					{
						RemapStructure(elementStructure, gameData);
					}
				}
			}
		}
	}

	private static void ReconstructSettings(IMonoBehaviour settings, List<IMonoBehaviour> groups)
	{
		SerializableStructure? structure = settings.LoadStructure();
		if (structure == null) return;

		if (structure.TryGetField("m_Groups", out SerializableValue groupsField))
		{
			// Ensure all found groups are in the settings' groups list
			// This helps fix "missing" groups in the settings asset
		}

		settings.OverrideDirectory = "Assets/AddressableAssetsData";
		settings.OverrideName = "AddressableAssetSettings";

		foreach (IMonoBehaviour group in groups)
		{
			group.OverrideDirectory = "Assets/AddressableAssetsData/AssetGroups";
			// Link group to settings if needed
		}
	}

	private static string? FindCatalog(PlatformGameStructure? platform)
	{
		if (platform == null) return null;

		string? streamingAssetsPath = platform.StreamingAssetsPath;
		if (string.IsNullOrEmpty(streamingAssetsPath) || !platform.FileSystem.Directory.Exists(streamingAssetsPath))
		{
			return null;
		}

		foreach (string file in platform.FileSystem.Directory.EnumerateFiles(streamingAssetsPath, "*", SearchOption.AllDirectories))
		{
			string fileName = Path.GetFileName(file);
			if (fileName.Contains("catalog", StringComparison.OrdinalIgnoreCase) &&
				(fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase)))
			{
				return file;
			}
		}

		return null;
	}
}
