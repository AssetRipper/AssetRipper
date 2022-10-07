using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Yaml;

namespace AssetRipper.Assets;

/// <summary>
/// The artificial base class for all generated Unity classes which inherit from Object.
/// </summary>
public abstract class UnityObjectBase : UnityAssetBase, IUnityObjectBase
{
	protected UnityObjectBase(AssetInfo assetInfo)
	{
		AssetInfo = assetInfo;
	}

	public AssetInfo AssetInfo { get; }
	public AssetCollection Collection => AssetInfo.Collection;
	public int ClassID => AssetInfo.ClassID;
	public long PathID => AssetInfo.PathID;
	public virtual string ClassName => GetType().Name;

	public YamlDocument ExportYamlDocument(IExportContainer container)
	{
		YamlDocument document = new();
		YamlMappingNode root = document.CreateMappingRoot();
		root.Tag = ClassID.ToString();
		root.Anchor = container.GetExportID(this).ToString();
		root.Add(ClassName, ExportYaml(container));
		return document;
	}
}
