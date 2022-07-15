using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Core.Project.Exporters
{
	public class ScriptableObjectExporter : YamlExporterBase
	{
		public override bool IsHandle(Interfaces.IUnityObjectBase asset)
		{
			return asset is IMonoBehaviour;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, Interfaces.IUnityObjectBase asset)
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
