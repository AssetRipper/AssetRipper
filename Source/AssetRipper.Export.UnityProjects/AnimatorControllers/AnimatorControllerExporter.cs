using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_91;

namespace AssetRipper.Export.UnityProjects.AnimatorControllers;

public sealed class AnimatorControllerExporter : YamlExporterBase
{
	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = asset.MainAsset switch
		{
			IAnimatorController controller => new AnimatorControllerExportCollection(this, controller),
			_ => null,
		};
		return exportCollection is not null;
	}
}
