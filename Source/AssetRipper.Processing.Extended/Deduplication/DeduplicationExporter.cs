using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects;
using AssetRipper.IO.Files;

namespace AssetRipper.Processing.Extended.Deduplication;

/// <summary>
/// Redirects redundant assets to their canonical copies at export time.
/// </summary>
public sealed class DeduplicationExporter(DeduplicationMap Map) : IAssetExporter
{
	AssetType IAssetExporter.ToExportType(IUnityObjectBase asset) => throw new NotSupportedException();

	bool IAssetExporter.ToUnknownExportType(Type type, out AssetType assetType)
	{
		assetType = default;
		return false;
	}

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (Map.TryGetCanonical(asset, out IUnityObjectBase? canonical))
		{
			exportCollection = new DeduplicationExportCollection(asset, canonical);
			return true;
		}

		exportCollection = null;
		return false;
	}
}
