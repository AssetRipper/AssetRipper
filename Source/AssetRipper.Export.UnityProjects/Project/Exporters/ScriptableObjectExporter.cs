using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Project.Exporters
{
	public class ScriptableObjectExporter : YamlExporterBase
	{
		private IExportCollection CreateCollection(IMonoBehaviour monoBehaviour)
		{
			if (monoBehaviour.IsScriptableObject())
			{
				return new AssetExportCollection<IMonoBehaviour>(this, monoBehaviour);
			}
			else
			{
				// such MonoBehaviours as StateMachineBehaviour in AnimatorController
				return new EmptyExportCollection();
			}
		}

		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			exportCollection = asset switch
			{
				IMonoBehaviour monoBehaviour => CreateCollection(monoBehaviour),
				_ => null,
			};
			return exportCollection is not null;
		}
	}
}
