using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class SceneYamlExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return SceneExportCollection.IsSceneCompatible(asset);
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			if (asset.File.Collection.IsScene(asset.File))
			{
				return new SceneExportCollection(this, virtualFile, asset.File);
			}
			else if (PrefabExportCollection.IsValidAsset(asset))
			{
				return new PrefabExportCollection(this, virtualFile, asset);
			}
			else
			{
				return new FailExportCollection(this, asset);
			}
		}
	}
}
