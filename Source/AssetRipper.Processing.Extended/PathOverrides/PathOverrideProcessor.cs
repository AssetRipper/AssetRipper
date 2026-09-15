using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;

namespace AssetRipper.Processing.Extended.PathOverrides;

/// <summary>
/// Applies user-supplied export destinations. Runs after <see cref="AssetRipper.Processing.Scenes.OriginalPathProcessor"/>
/// so that overrides win over the paths it assigns.
/// </summary>
public sealed class PathOverrideProcessor : IAssetProcessor
{
	private readonly PathOverrideData data;

	public PathOverrideProcessor(PathOverrideData data)
	{
		ArgumentNullException.ThrowIfNull(data);
		this.data = data;
	}

	public void Process(GameData gameData)
	{
		if (data.Files.Count == 0)
		{
			return;
		}

		Dictionary<string, AssetCollection> collectionsByName = [];
		foreach (AssetCollection collection in gameData.GameBundle.FetchAssetCollections())
		{
			// Collection names are not guaranteed unique. First one wins; warn on the rest.
			if (!collectionsByName.TryAdd(collection.Name, collection))
			{
				Logger.Warning(LogCategory.Processing, $"Path overrides: more than one collection is named '{collection.Name}'. Using the first.");
			}
		}

		int applied = 0;
		foreach ((string collectionName, Dictionary<long, string> overrides) in data.Files)
		{
			if (!collectionsByName.TryGetValue(collectionName, out AssetCollection? collection))
			{
				Logger.Warning(LogCategory.Processing, $"Path overrides: no collection named '{collectionName}'. Skipping its {overrides.Count} entries.");
				continue;
			}

			foreach ((long pathID, string path) in overrides)
			{
				IUnityObjectBase? asset = collection.TryGetAsset(pathID);
				if (asset is null)
				{
					Logger.Warning(LogCategory.Processing, $"Path overrides: '{collectionName}' has no asset with path ID {pathID}. Skipping.");
					continue;
				}

				if (string.IsNullOrEmpty(path))
				{
					Logger.Warning(LogCategory.Processing, $"Path overrides: empty path for {pathID} in '{collectionName}'. Skipping.");
					continue;
				}

				asset.OverridePath = path;
				applied++;
			}
		}

		Logger.Info(LogCategory.Processing, $"Path overrides: applied {applied}.");
	}
}
