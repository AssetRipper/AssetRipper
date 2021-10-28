using AssetRipper.Core.Classes;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class DefaultYamlAssetExporter : YamlAssetExporterBase
	{
		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, UnityObjectBase asset)
		{
			if (SceneExportCollection.IsSceneCompatible(asset))
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
			else
			{
				switch (asset.ClassID)
				{
					case ClassIDType.AnimatorController:
						return new AnimatorControllerExportCollection(this, virtualFile, asset);

					case ClassIDType.TimeManager:
					case ClassIDType.AudioManager:
					case ClassIDType.InputManager:
					case ClassIDType.Physics2DSettings:
					case ClassIDType.GraphicsSettings:
					case ClassIDType.QualitySettings:
					case ClassIDType.PhysicsManager:
					case ClassIDType.TagManager:
					case ClassIDType.NavMeshProjectSettings:
					case ClassIDType.NetworkManager:
					case ClassIDType.ClusterInputManager:
					case ClassIDType.UnityConnectSettings:
						return new ManagerExportCollection(this, asset);
					case ClassIDType.BuildSettings:
						return new BuildSettingsExportCollection(this, virtualFile, asset);

					case ClassIDType.MonoBehaviour:
						{
							MonoBehaviour monoBehaviour = (MonoBehaviour)asset;
							if (monoBehaviour.IsScriptableObject)
							{
								return new AssetExportCollection(this, asset);
							}
							else
							{
								// such MonoBehaviours as StateMachineBehaviour in AnimatorController
								return new EmptyExportCollection();
							}
						}

					default:
						return new AssetExportCollection(this, asset);
				}
			}
		}
	}
}
