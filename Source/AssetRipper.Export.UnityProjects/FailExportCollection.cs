using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Logging;
using AssetRipper.IO.Files.SerializedFiles;

namespace AssetRipper.Export.UnityProjects;

public class FailExportCollection : IExportCollection
{
	public FailExportCollection(IAssetExporter assetExporter, IUnityObjectBase asset)
	{
		AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
		m_asset = asset ?? throw new ArgumentNullException(nameof(asset));
	}

	public bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		Logger.Log(LogType.Warning, LogCategory.Export, $"Unable to export asset {Name}");
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

	public UnityGuid GetExportGUID(IUnityObjectBase _)
	{
		throw new NotSupportedException();
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
	public IEnumerable<IUnityObjectBase> Assets
	{
		get { yield return m_asset; }
	}
	public string Name => m_asset.GetBestName();

	private readonly IUnityObjectBase m_asset;
}
