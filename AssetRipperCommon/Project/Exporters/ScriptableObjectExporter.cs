using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;

namespace AssetRipper.Core.Project.Exporters
{
	public class ScriptableObjectExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IMonoBehaviour;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
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
