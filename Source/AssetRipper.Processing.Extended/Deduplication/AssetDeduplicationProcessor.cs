using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Import.Logging;
using AssetRipper.Processing.Textures;

namespace AssetRipper.Processing.Extended.Deduplication;

/// <summary>
/// Finds assets that Unity duplicated across asset bundles and elects one canonical copy
/// for each group. The duplicates are not modified; the export redirects references to the
/// canonical copy via <see cref="DeduplicationExporter"/>.
/// </summary>
public sealed class AssetDeduplicationProcessor : IAssetProcessor
{
	/// <summary>
	/// The types documented as safe to deduplicate. Anything else is left alone.
	/// </summary>
	private static readonly HashSet<string> SupportedTypes =
	[
		"MonoScript",
		"Shader",
		"ComputeShader",
		"AudioClip",
		"TextAsset",
		"Mesh",
		"Texture2D",
	];

	private readonly DeduplicationMap map;

	public AssetDeduplicationProcessor(DeduplicationMap map)
	{
		ArgumentNullException.ThrowIfNull(map);
		this.map = map;
	}

	public void Process(GameData gameData)
	{
		Dictionary<string, List<IUnityObjectBase>> groups = [];

		foreach (IUnityObjectBase asset in gameData.GameBundle.FetchAssets())
		{
			if (!IsCandidate(asset))
			{
				continue;
			}

			string hash;
			try
			{
				hash = AssetContentHasher.Compute(asset);
			}
			catch (Exception ex)
			{
				// An asset that cannot be serialized simply is not a dedup candidate.
				Logger.Warning(LogCategory.Processing, $"Deduplication: could not hash '{asset.GetBestName()}' ({asset.ClassName}): {ex.Message}");
				continue;
			}

			if (!groups.TryGetValue(hash, out List<IUnityObjectBase>? group))
			{
				group = [];
				groups.Add(hash, group);
			}
			group.Add(asset);
		}

		int redundant = 0;
		int groupCount = 0;
		foreach (List<IUnityObjectBase> group in groups.Values)
		{
			if (group.Count < 2)
			{
				continue;
			}

			// Only deduplicate across collections. Two identical assets inside one collection
			// are Unity's own business and may be referenced positionally.
			if (group.Select(static a => a.Collection).Distinct().Count() < 2)
			{
				continue;
			}

			IUnityObjectBase canonical = ElectCanonical(group);
			groupCount++;
			foreach (IUnityObjectBase asset in group)
			{
				if (!ReferenceEquals(asset, canonical))
				{
					map.Add(asset, canonical);
					redundant++;
				}
			}
		}

		Logger.Info(LogCategory.Processing, $"Deduplication: {groupCount} duplicate groups, {redundant} redundant copies redirected.");
	}

	private static bool IsCandidate(IUnityObjectBase asset)
	{
		if (!SupportedTypes.Contains(asset.ClassName))
		{
			return false;
		}

		// Textures backing sprites are excluded: the sprite machinery owns them and
		// redirecting the texture alone would desynchronize the sprite group.
		if (asset.MainAsset is SpriteInformationObject)
		{
			return false;
		}

		// An asset already claimed by a group is not independently redirectable.
		return asset.MainAsset is null || ReferenceEquals(asset.MainAsset, asset);
	}

	/// <summary>
	/// Prefers a copy outside a serialized bundle so the survivor keeps a stable path,
	/// then orders deterministically so repeated runs elect the same asset.
	/// </summary>
	private static IUnityObjectBase ElectCanonical(List<IUnityObjectBase> group)
	{
		return group
			.OrderBy(static a => a.Collection.Bundle is SerializedBundle ? 1 : 0)
			.ThenBy(static a => a.Collection.Name, StringComparer.Ordinal)
			.ThenBy(static a => a.PathID)
			.First();
	}
}
