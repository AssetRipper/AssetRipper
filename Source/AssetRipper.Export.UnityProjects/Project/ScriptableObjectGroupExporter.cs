using AssetRipper.Assets;
using AssetRipper.Processing.ScriptableObject;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Export.UnityProjects.Project;

public class ScriptableObjectGroupExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		switch (asset.MainAsset)
		{
			case ScriptableObjectGroup playableAssetGroup:
				exportCollection = new ScriptableObjectGroupExportCollection(this, playableAssetGroup);
				return true;
			default:
				exportCollection = null;
				return false;
		}
	}

	private sealed class ScriptableObjectGroupExportCollection : AssetsExportCollection<IMonoBehaviour>
	{
		public ScriptableObjectGroup Group { get; }
		public ScriptableObjectGroupExportCollection(ScriptableObjectGroupExporter exporter, ScriptableObjectGroup group) : base(exporter, group.Root)
		{
			Group = group;
			AddAssets(group.Children);
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return Group.FileExtension ?? base.GetExportExtension(asset);
		}

		public override IEnumerable<IUnityObjectBase> Assets => base.Assets.Prepend(Group);

		public override IEnumerable<IUnityObjectBase> ExportableAssets => base.Assets;
	}
}
