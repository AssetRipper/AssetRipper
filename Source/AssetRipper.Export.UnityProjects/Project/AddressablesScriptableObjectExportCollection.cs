using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_1034;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Export.UnityProjects.Project;

internal sealed class AddressablesScriptableObjectExportCollection : ExportCollection
{
	private readonly ScriptableObjectExporter exporter;
	private readonly AddressablesLayout.Layout layout;

	public AddressablesScriptableObjectExportCollection(ScriptableObjectExporter exporter, IMonoBehaviour asset, AddressablesLayout.Layout layout)
	{
		this.exporter = exporter;
		Asset = asset;
		this.layout = layout;
	}

	public IMonoBehaviour Asset { get; }

	public override bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		string relativeDirectory = layout.DirectoryPath.Replace('/', Path.DirectorySeparatorChar);
		string subPath = fileSystem.Path.Join(projectDirectory, relativeDirectory);
		fileSystem.Directory.Create(subPath);

		string requestedFileName = $"{FileSystem.FixInvalidFileNameCharacters(layout.FileName)}.asset";
		string uniqueFileName = GetUniqueFileName(subPath, requestedFileName, fileSystem);
		string filePath = fileSystem.Path.Join(subPath, uniqueFileName);
		if (!AssetExporter.Export(container, Asset, filePath, fileSystem))
		{
			return false;
		}

		Meta meta = new Meta(GUID, CreateImporter(container));
		ExportMeta(container, meta, filePath, fileSystem);
		return true;
	}

	public override bool Contains(IUnityObjectBase asset)
	{
		return Asset.AssetInfo == asset.AssetInfo;
	}

	public override long GetExportID(IExportContainer container, IUnityObjectBase asset)
	{
		if (asset.AssetInfo != Asset.AssetInfo)
		{
			throw new ArgumentException(null, nameof(asset));
		}

		return ExportIdHandler.GetMainExportID(Asset);
	}

	public override MetaPtr CreateExportPointer(IExportContainer container, IUnityObjectBase asset, bool isLocal)
	{
		long exportID = GetExportID(container, asset);
		return isLocal
			? new MetaPtr(exportID)
			: new MetaPtr(exportID, GUID, AssetExporter.ToExportType(Asset));
	}

	private IUnityObjectBase CreateImporter(IExportContainer container)
	{
		INativeFormatImporter importer = NativeFormatImporter.Create(container.File, container.ExportVersion);
		importer.MainObjectFileID = GetExportID(container, Asset);
		if (importer.Has_AssetBundleName_R() && Asset.AssetBundleName is not null)
		{
			importer.AssetBundleName_R = Asset.AssetBundleName;
		}
		return importer;
	}

	public override UnityGuid GUID => layout.Guid;
	public override IAssetExporter AssetExporter => exporter;
	public override AssetCollection File => Asset.Collection;
	public override IEnumerable<IUnityObjectBase> Assets
	{
		get { yield return Asset; }
	}
	public override string Name => layout.FileName;
}
