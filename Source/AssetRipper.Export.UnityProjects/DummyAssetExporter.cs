using AssetRipper.Assets;

namespace AssetRipper.Export.UnityProjects;

public abstract class DummyAssetExporter : IAssetExporter
{
	private static readonly EmptyDummyAssetExporter instance_empty_serialized = new(AssetType.Serialized);
	private static readonly SkipDummyAssetExporter instance_skip_serialized = new(AssetType.Serialized);
	private static readonly EmptyDummyAssetExporter instance_empty_meta = new(AssetType.Meta);
	private static readonly SkipDummyAssetExporter instance_skip_meta = new(AssetType.Meta);

	/// <summary>
	/// Setup exporting of the specified class type.
	/// </summary>
	/// <param name="classType">The class id of assets we are setting these parameters for.</param>
	/// <param name="isEmptyCollection">
	/// True: an exception will be thrown if the asset is referenced by another asset.<br/>
	/// False: any references to this asset will be replaced with a missing reference.
	/// </param>
	/// <param name="isMetaType"><see cref="AssetType.Meta"/> or <see cref="AssetType.Serialized"/>?</param>
	public static DummyAssetExporter Get(bool isEmptyCollection, bool isMetaType)
	{
		if (isEmptyCollection)
		{
			return isMetaType ? instance_empty_meta : instance_empty_serialized;
		}
		else
		{
			return isMetaType ? instance_skip_meta : instance_skip_serialized;
		}
	}

	private AssetType ExportType { get; }

	private DummyAssetExporter(AssetType exportType)
	{
		ExportType = exportType;
	}

	public abstract bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection);

	public AssetType ToExportType(IUnityObjectBase asset)
	{
		return ExportType;
	}

	public bool ToUnknownExportType(Type type, out AssetType assetType)
	{
		assetType = ExportType;
		return true;
	}

	private sealed class EmptyDummyAssetExporter(AssetType exportType) : DummyAssetExporter(exportType)
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			exportCollection = EmptyExportCollection.Instance;
			return true;
		}
	}

	private sealed class SkipDummyAssetExporter(AssetType exportType) : DummyAssetExporter(exportType)
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			exportCollection = new SkipExportCollection(this, asset);
			return true;
		}
	}
}
