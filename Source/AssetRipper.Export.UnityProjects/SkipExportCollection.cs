using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Export.UnityProjects;

public sealed class SkipExportCollection : IExportCollection
{
	public SkipExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset)
	{
		AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
		m_asset = asset ?? throw new ArgumentNullException(nameof(asset));
	}

	public bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		return false;
	}

	public bool Contains(IUnityObjectBase asset)
	{
		return asset == m_asset;
	}

	public long GetExportID(IExportContainer container, IUnityObjectBase asset)
	{
		if (asset == m_asset)
		{
			return ExportIdHandler.GetMainExportID(m_asset);
		}
		throw new ArgumentException(null, nameof(asset));
	}

	public MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		if (isLocal)
		{
			throw new ArgumentException(null, nameof(isLocal));
		}

		long exportId = GetExportID(container, asset);
		AssetType type = AssetExporter.ToExportType(asset);
		return new MetaPtr(exportId, UnityGuid.MissingReference, type);
	}

	public IAssetExporter AssetExporter { get; }
	public AssetCollection File => m_asset.Collection;
	public TransferInstructionFlags Flags => File.Flags;
	public IEnumerable<IUnityObjectBase> Assets => Enumerable.Empty<IUnityObjectBase>();
	public string Name => m_asset.GetType().Name;

	private readonly IUnityObjectBase m_asset;
}
