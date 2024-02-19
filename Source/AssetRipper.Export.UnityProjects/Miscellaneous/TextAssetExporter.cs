using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_49;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		public TextExportMode ExportMode { get; }
		public TextAssetExporter(LibraryConfiguration configuration)
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

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			using FileStream stream = File.OpenWrite(path);
			stream.Write(((ITextAsset)asset).Script_C49.Data);
			stream.Flush();
			return true;
		}
	}
}
