using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.Playable;

public class PlayableProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Processing Playable Assets");
		ProcessedAssetCollection collection = gameData.AddNewProcessedCollection("Generated Playable Asset Groups");
		foreach (IMonoBehaviour monoBehaviour in gameData.GameBundle.FetchAssets().OfType<IMonoBehaviour>())
		{
			if (monoBehaviour.IsTimelineAsset())
			{
				PlayableAssetGroup group = collection.CreateAsset(-1, monoBehaviour, static (assetInfo, root) => new PlayableAssetGroup(assetInfo, root));
				group.Children.AddRange(FindChildren(monoBehaviour));
				group.SetMainAssets();
			}
		}
	}

	private static IEnumerable<IMonoBehaviour> FindChildren(IMonoBehaviour root)
	{
		SerializableStructure? structure = LoadStructure(root);
		if (structure is null)
		{
			return [];
		}

		HashSet<IMonoBehaviour> children = [];
		if (structure.TryGetField("m_Tracks", out SerializableValue tracks))
		{
			foreach (IPPtr pptr in tracks.AsAssetArray.Cast<IPPtr>())
			{
				if (!root.Collection.TryGetAsset(pptr.FileID, pptr.PathID, out IMonoBehaviour? child))
				{
					continue;
				}

				children.Add(child);

				SerializableStructure? childStructure = LoadStructure(child);
				if (childStructure is null)
				{
					continue;
				}

				if (childStructure.TryGetField("m_Clips", out SerializableValue clips))
				{
					foreach (SerializableStructure clip in clips.AsAssetArray.Cast<SerializableStructure>())
					{
						if (clip.TryGetField("m_Asset", out SerializableValue asset) && root.Collection.TryGetAsset(asset.AsPPtr.FileID, asset.AsPPtr.PathID, out IMonoBehaviour? clipAsset))
						{
							children.Add(clipAsset);
						}
					}
				}

				if (childStructure.TryGetField("m_Markers", out SerializableValue markers) && markers.AsStructure.TryGetField("m_Objects", out SerializableValue objects))
				{
					foreach (IPPtr markerPtr in objects.AsAssetArray.Cast<IPPtr>())
					{
						if (root.Collection.TryGetAsset(markerPtr.FileID, markerPtr.PathID, out IMonoBehaviour? marker))
						{
							children.Add(marker);
						}
					}
				}
			}
		}
		if (structure.TryGetField("m_MarkerTrack", out SerializableValue markerTrack))
		{
			if (root.Collection.TryGetAsset(markerTrack.AsPPtr.FileID, markerTrack.AsPPtr.PathID, out IMonoBehaviour? child))
			{
				children.Add(child);
			}
		}

		return children;
	}

	private static SerializableStructure? LoadStructure(IMonoBehaviour monoBehaviour)
	{
		if (monoBehaviour.Structure is SerializableStructure structure)
		{
			return structure;
		}
		return (monoBehaviour.Structure as UnloadedStructure)?.LoadStructure();
	}
}
