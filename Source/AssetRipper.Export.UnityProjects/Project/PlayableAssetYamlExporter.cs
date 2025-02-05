using AssetRipper.Assets;
using AssetRipper.Processing.Playable;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Export.UnityProjects.Project;

public class PlayableAssetYamlExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		switch (asset.MainAsset)
		{
			case PlayableAssetGroup playableAssetGroup:
				exportCollection = new PlayableAssetExportCollection(this, playableAssetGroup);
				return true;
			default:
				exportCollection = null;
				return false;
		}
	}

	private sealed class PlayableAssetExportCollection : AssetsExportCollection<IMonoBehaviour>
	{
		public PlayableAssetGroup Group { get; }
		public PlayableAssetExportCollection(PlayableAssetYamlExporter exporter, PlayableAssetGroup group) : base(exporter, group.Root)
		{
			Group = group;
			AddAssets(group.Children);
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return "playable";
		}

		public override IEnumerable<IUnityObjectBase> Assets => base.Assets.Prepend(Group);

		public override IEnumerable<IUnityObjectBase> ExportableAssets => base.Assets;
	}
}
