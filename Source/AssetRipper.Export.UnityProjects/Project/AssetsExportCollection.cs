using AssetRipper.Assets;
using AssetRipper.Assets.Collections;

namespace AssetRipper.Export.UnityProjects.Project;

/// <summary>
/// A collection of assets that are exported together.
/// </summary>
/// <typeparam name="T">The type of <see cref="AssetExportCollection{T}.Asset"/>.</typeparam>
public abstract class AssetsExportCollection<T> : AssetExportCollection<T> where T : IUnityObjectBase
{
	public AssetsExportCollection(IAssetExporter assetExporter, T asset) : base(assetExporter, asset)
	{
		m_file = asset.Collection;
	}

	public override bool Contains(IUnityObjectBase asset)
	{
		return base.Contains(asset) || m_exportIDs.ContainsKey(asset);
	}

	public override long GetExportID(IExportContainer container, IUnityObjectBase asset)
	{
		if (asset.AssetInfo == Asset.AssetInfo)
		{
			return base.GetExportID(container, asset);
		}
		return m_exportIDs[asset];
	}

	protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		return AssetExporter.Export(container, ExportableAssets, filePath, fileSystem);
	}

	public override IEnumerable<IUnityObjectBase> Assets
	{
		get
		{
			foreach (IUnityObjectBase asset in base.Assets)
			{
				m_file = asset.Collection;
				yield return asset;
			}
			foreach ((IUnityObjectBase asset, long _) in m_exportIDs)
			{
				m_file = asset.Collection;
				yield return asset;
			}
		}
	}

	protected virtual long GenerateExportID(IUnityObjectBase asset)
	{
		return ExportIdHandler.GetRandomExportId(asset, ContainsID);
	}

	/// <summary>
	/// Add an asset to this export collection.
	/// </summary>
	/// <param name="asset">The asset to be added to this export collection.</param>
	/// <returns><see langword="true"/> if the <paramref name="asset"/> is added to the <see cref="AssetsExportCollection"/> object; <see langword="false"/> if the <paramref name="asset"/> is already present.</returns>
	protected bool AddAsset(IUnityObjectBase? asset)
	{
		if (asset is null || asset == (IUnityObjectBase)Asset)
		{
			return false;
		}

		long exportID = GenerateExportID(asset);
		if (m_exportIDs.TryAdd(asset, exportID))
		{
			m_exportAssetIds.Add(exportID);
			return true;
		}
		else
		{
			return false;
		}
	}

	protected void AddAssets(IEnumerable<IUnityObjectBase?> assets)
	{
		foreach (IUnityObjectBase? asset in assets)
		{
			AddAsset(asset);
		}
	}

	private bool ContainsID(long id)
	{
		return m_exportAssetIds.Contains(id);
	}

	public override AssetCollection File => m_file;
	protected AssetCollection m_file;

	/// <summary>
	/// A one-to-one dictionary of export id's
	/// </summary>
	private readonly Dictionary<IUnityObjectBase, long> m_exportIDs = new();

	private readonly HashSet<long> m_exportAssetIds = new();
}
