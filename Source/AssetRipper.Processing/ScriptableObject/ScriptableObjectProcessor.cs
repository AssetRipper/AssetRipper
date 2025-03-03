using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Metadata;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.ScriptableObject;

public class ScriptableObjectProcessor : IAssetProcessor
{
	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Processing Scriptable Object Groups");
		ProcessedAssetCollection collection = gameData.AddNewProcessedCollection("Generated Scriptable Object Groups");
		foreach (IMonoBehaviour monoBehaviour in gameData.GameBundle.FetchAssets().OfType<IMonoBehaviour>())
		{
			if (monoBehaviour.IsTimelineAsset())
			{
				ScriptableObjectGroup group = CreateGroup(collection, monoBehaviour);
				group.FileExtension = "playable";
				group.Children.AddRange(FindTimelineAssetChildren(monoBehaviour));
				group.SetMainAsset();
			}
			else if (monoBehaviour.IsPostProcessProfile())
			{
				ScriptableObjectGroup group = CreateGroup(collection, monoBehaviour);
				group.Children.AddRange(FindPostProcessProfileChildren(monoBehaviour));
				group.SetMainAsset();
			}
		}
	}

	private static ScriptableObjectGroup CreateGroup(ProcessedAssetCollection collection, IMonoBehaviour root)
	{
		return collection.CreateAsset(-1, root, static (assetInfo, root) => new ScriptableObjectGroup(assetInfo, root));
	}

	private static IEnumerable<IMonoBehaviour> FindTimelineAssetChildren(IMonoBehaviour root)
	{
		SerializableStructure? structure = root.LoadStructure();
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

				SerializableStructure? childStructure = child.LoadStructure();
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

	private static IEnumerable<IMonoBehaviour> FindPostProcessProfileChildren(IMonoBehaviour root)
	{
		SerializableStructure? structure = root.LoadStructure();
		if (structure is null)
		{
			return [];
		}

		HashSet<IMonoBehaviour> children = [];
		if (structure.TryGetField("settings", out SerializableValue settings))
		{
			foreach (IPPtr pptr in settings.AsAssetArray.Cast<IPPtr>())
			{
				if (!root.Collection.TryGetAsset(pptr.FileID, pptr.PathID, out IMonoBehaviour? child))
				{
					continue;
				}

				children.Add(child);
			}
		}

		return children;
	}
}
