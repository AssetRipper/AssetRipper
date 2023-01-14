using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class TextAssetExporter : BinaryAssetExporter
	{
		public TextExportMode ExportMode { get; }
		public TextAssetExporter(LibraryConfiguration configuration)
		{
			ExportMode = configuration.TextExportMode;
		}

		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is ITextAsset textAsset && !textAsset.Script_C49.Data.IsNullOrEmpty();
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new TextAssetExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			File.WriteAllBytes(path, ((ITextAsset)asset).Script_C49.Data);
			return true;
		}
	}
}
