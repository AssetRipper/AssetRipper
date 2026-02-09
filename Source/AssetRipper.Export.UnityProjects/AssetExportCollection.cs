using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_1034;

namespace AssetRipper.Export.UnityProjects;

public class AssetExportCollection<T> : ExportCollection where T : IUnityObjectBase
{
	public AssetExportCollection(IAssetExporter assetExporter, T asset)
	{
		AssetExporter = assetExporter ?? throw new ArgumentNullException(nameof(assetExporter));
		Asset = asset ?? throw new ArgumentNullException(nameof(asset));
	}

	public override bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		string subPath = fileSystem.Path.Join(projectDirectory, FileSystem.FixInvalidPathCharacters(Asset.GetBestDirectory()));
		string fileName = GetUniqueFileName(Asset, subPath, fileSystem);

		fileSystem.Directory.Create(subPath);

		string filePath = fileSystem.Path.Join(subPath, fileName);
		bool result = ExportInner(container, filePath, projectDirectory, fileSystem);
		if (result)
		{
			Meta meta = new Meta(GUID, CreateImporter(container));
			ExportMeta(container, meta, filePath, fileSystem);
			return true;
		}
		return false;
	}

	public override bool Contains(IUnityObjectBase asset)
	{
		return Asset.AssetInfo == asset.AssetInfo;
	}

	public override long GetExportID(IExportContainer container, IUnityObjectBase asset)
	{
		if (asset.AssetInfo == Asset.AssetInfo)
		{
			return ExportIdHandler.GetMainExportID(Asset);
		}
		throw new ArgumentException(null, nameof(asset));
	}

	public override MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		long exportID = GetExportID(container, asset);
		return isLocal ?
			new MetaPtr(exportID) :
			new MetaPtr(exportID, GUID, AssetExporter.ToExportType(Asset));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="container"></param>
	/// <param name="filePath">The full path to the exported asset destination</param>
	/// <param name="dirPath">The full path to the project export directory</param>
	/// <returns>True if export was successful, false otherwise</returns>
	protected virtual bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		return AssetExporter.Export(container, Asset, filePath, fileSystem);
	}

	protected virtual IUnityObjectBase CreateImporter(IExportContainer container)
	{
		INativeFormatImporter importer = NativeFormatImporter.Create(container.File, container.ExportVersion);
		importer.MainObjectFileID = GetExportID(container, Asset);
		if (importer.Has_AssetBundleName_R() && Asset.AssetBundleName is not null)
		{
			importer.AssetBundleName_R = Asset.AssetBundleName;
		}
		return importer;
	}

	public override UnityGuid GUID { get; } = UnityGuid.NewGuid();
	public override IAssetExporter AssetExporter { get; }
	public override AssetCollection File => Asset.Collection;
	public override IEnumerable<IUnityObjectBase> Assets
	{
		get { yield return Asset; }
	}
	public override string Name => Asset.GetBestName();
	public T Asset { get; }
}
