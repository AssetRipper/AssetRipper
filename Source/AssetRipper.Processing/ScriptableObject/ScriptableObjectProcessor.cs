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

		// Assets that can be a child of a group
		HashSet<IMonoBehaviour> uniqueAssets = new();

		// Assets that cannot be a child of a group
		HashSet<IMonoBehaviour> nonuniqueAssets = new();

		List<IMonoBehaviour> timelineAssets = new();
		List<IMonoBehaviour> postProcessProfiles = new();

		foreach (IMonoBehaviour monoBehaviour in gameData.GameBundle.FetchAssets().OfType<IMonoBehaviour>())
		{
			if (monoBehaviour.MainAsset is not null)
			{
			}
			else if (monoBehaviour.IsTimelineAsset())
			{
				nonuniqueAssets.Add(monoBehaviour);
				timelineAssets.Add(monoBehaviour);
			}
			else if (monoBehaviour.IsPostProcessProfile())
			{
				nonuniqueAssets.Add(monoBehaviour);
				postProcessProfiles.Add(monoBehaviour);
			}
		}

		foreach (IMonoBehaviour timelineAsset in timelineAssets)
		{
			foreach (IMonoBehaviour child in FindTimelineAssetChildren(timelineAsset))
			{
				AddChild(uniqueAssets, nonuniqueAssets, child);
			}
		}
		foreach (IMonoBehaviour postProcessProfile in postProcessProfiles)
		{
			foreach (IMonoBehaviour child in FindPostProcessProfileChildren(postProcessProfile))
			{
				AddChild(uniqueAssets, nonuniqueAssets, child);
			}
		}

		nonuniqueAssets.Clear();

		foreach (IMonoBehaviour timelineAsset in timelineAssets)
		{
			ScriptableObjectGroup group = CreateGroup(collection, timelineAsset);
			group.FileExtension = "playable";
			group.Children.AddRange(FindTimelineAssetChildren(timelineAsset).Where(uniqueAssets.Contains));
			group.SetMainAsset();
		}
		foreach (IMonoBehaviour postProcessProfile in postProcessProfiles)
		{
			ScriptableObjectGroup group = CreateGroup(collection, postProcessProfile);
			group.Children.AddRange(FindPostProcessProfileChildren(postProcessProfile).Where(uniqueAssets.Contains));
			group.SetMainAsset();
		}
	}

	private static void AddChild(HashSet<IMonoBehaviour> uniqueAssets, HashSet<IMonoBehaviour> nonuniqueAssets, IMonoBehaviour child)
	{
		if (child.MainAsset is not null)
		{
		}
		else if (nonuniqueAssets.Contains(child))
		{
		}
		else if (uniqueAssets.Add(child))
		{
		}
		else
		{
			uniqueAssets.Remove(child);
			nonuniqueAssets.Add(child);
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

				SerializableStructure? childStructure = child.LoadStructure();
				if (childStructure is null)
				{
					continue;
				}

				if (!childStructure.TryGetField("m_Parent", out SerializableValue parent))
				{
					continue;
				}

				if (root.Collection.TryGetAsset(parent.AsPPtr.FileID, parent.AsPPtr.PathID) != root)
				{
					continue;
				}

				children.Add(child);

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
