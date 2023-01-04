using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Import.Project.Exporters
{
	public class ScriptableObjectExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IMonoBehaviour;
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			IMonoBehaviour monoBehaviour = (IMonoBehaviour)asset;
			if (monoBehaviour.IsScriptableObject())
			{
				return new AssetExportCollection(this, asset);
			}
			else
			{
				// such MonoBehaviours as StateMachineBehaviour in AnimatorController
				return new EmptyExportCollection();
			}
		}
	}
}
