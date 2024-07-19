using AssetRipper.Assets;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Export.PrimaryContent.Scripts;

public sealed class ScriptContentExtractor : IContentExtractor
{
	public IAssemblyManager AssemblyManager { get; }
	private bool first = true;
	public ScriptContentExtractor(IAssemblyManager assemblyManager)
	{
		AssemblyManager = assemblyManager;
	}

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		if (asset is IMonoScript)
		{
			exportCollection = first ? new ScriptExportCollection(this) : EmptyExportCollection.Instance;
			first = false;
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}
}
