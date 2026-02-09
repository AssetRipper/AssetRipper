using AssetRipper.Assets;
using AssetRipper.Export.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_49;

namespace AssetRipper.Export.UnityProjects.Miscellaneous;

public sealed class TextAssetExporter : BinaryAssetExporter
{
	public TextExportMode ExportMode { get; }
	public TextAssetExporter(FullConfiguration configuration)
	{
		ExportMode = configuration.ExportSettings.TextExportMode;
	}

	public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is ITextAsset textAsset && !textAsset.Script_C49.IsEmpty)
		{
			exportCollection = new TextAssetExportCollection(this, textAsset);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public override bool Export(IExportContainer container, IUnityObjectBase asset, string path, FileSystem fileSystem)
	{
		fileSystem.File.WriteAllBytes(path, ((ITextAsset)asset).Script_C49.Data);
		return true;
	}
}
