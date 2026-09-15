using AssetRipper.Assets;
using AssetRipper.Import.Logging;
using AssetRipper.Processing.Extended.Deduplication;
using AssetRipper.Processing.Prefabs;

namespace AssetRipper.Processing.Extended.Outlining;

/// <summary>
/// Collapses generated prefab hierarchies that are structurally identical, so a repeated
/// prefab yields one asset instead of one per collection.
/// </summary>
/// <remarks>
/// <para>
/// This is the root-level half of prefab outlining. It runs AFTER <see cref="PrefabProcessor"/>
/// because it operates on the hierarchies that processor generates.
/// </para>
/// <para>
/// Extracting repeated sub-hierarchies from inside scenes is NOT done here. That requires
/// rewriting scene hierarchies into prefab instances, which conflicts with
/// <see cref="PrefabProcessor"/>'s assumption that each GameObject belongs to exactly one
/// hierarchy.
/// </para>
/// </remarks>
public sealed class PrefabOutliningProcessor : IAssetProcessor
{
	private readonly DeduplicationMap map;

	public PrefabOutliningProcessor(DeduplicationMap map)
	{
		ArgumentNullException.ThrowIfNull(map);
		this.map = map;
	}

	public void Process(GameData gameData)
	{
		Dictionary<string, List<PrefabHierarchyObject>> groups = [];
		foreach (PrefabHierarchyObject hierarchy in gameData.GameBundle.FetchAssets().OfType<PrefabHierarchyObject>())
		{
			string signature = HierarchySignature.Compute(hierarchy.Root);
			if (!groups.TryGetValue(signature, out List<PrefabHierarchyObject>? group))
			{
				group = [];
				groups.Add(signature, group);
			}
			group.Add(hierarchy);
		}

		int outlined = 0;
		int groupCount = 0;
		int rejected = 0;

		foreach (List<PrefabHierarchyObject> group in groups.Values)
		{
			// PrefabProcessor puts every generated hierarchy in one shared collection, so the
			// hierarchy's own collection says nothing. Group membership is judged by where the
			// root GameObject actually came from.
			if (group.Count < 2 || group.Select(static h => h.Root.Collection).Distinct().Count() < 2)
			{
				continue;
			}

			PrefabHierarchyObject canonical = group
				.OrderBy(static h => h.Root.Collection.Name, StringComparer.Ordinal)
				.ThenBy(static h => h.Root.PathID)
				.First();

			bool anyOutlined = false;
			foreach (PrefabHierarchyObject duplicate in group)
			{
				if (ReferenceEquals(duplicate, canonical))
				{
					continue;
				}

				List<(IUnityObjectBase Duplicate, IUnityObjectBase Canonical)> pairs = [];
				if (!HierarchySignature.TryPair(duplicate.Root, canonical.Root, pairs))
				{
					// Equal signatures but divergent structure. Discard rather than guess.
					rejected++;
					continue;
				}

				pairs.Add((duplicate.Prefab, canonical.Prefab));
				pairs.Add((duplicate, canonical));

				foreach ((IUnityObjectBase from, IUnityObjectBase to) in pairs)
				{
					if (!ReferenceEquals(from, to))
					{
						map.Add(from, to);
					}
				}

				outlined++;
				anyOutlined = true;
			}

			if (anyOutlined)
			{
				groupCount++;
			}
		}

		Logger.Info(LogCategory.Processing, $"Prefab outlining: {groupCount} repeated hierarchies, {outlined} duplicates collapsed, {rejected} rejected on structure mismatch.");
	}
}
