using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Project;

public class ScriptableObjectExporter : YamlExporterBase
{
	private IExportCollection CreateCollection(IMonoBehaviour monoBehaviour)
	{
		if (monoBehaviour.IsComponentOnGameObject())
		{
			return EmptyExportCollection.Instance;
		}
		else
		{
			return new ScriptableObjectExportCollection(this, monoBehaviour);
		}
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		exportCollection = asset switch
		{
			IMonoBehaviour monoBehaviour => CreateCollection(monoBehaviour),
			_ => null,
		};
		return exportCollection is not null;
	}

	private sealed class ScriptableObjectExportCollection : AssetExportCollection<IMonoBehaviour>
	{
		public ScriptableObjectExportCollection(ScriptableObjectExporter exporter, IMonoBehaviour asset) : base(exporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			IMonoBehaviour monoBehaviour = (IMonoBehaviour)asset;
			if (monoBehaviour.IsGuiSkin())
			{
				return "guiskin";
			}
			else if (monoBehaviour.IsBrush())
			{
				return "brush";
			}
			return base.GetExportExtension(asset);
		}
	}
}
