using AssetRipper.Core;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Library.Configuration;
using AssetRipper.SourceGenerated.Classes.ClassID_49;
using System.IO;

namespace AssetRipper.Library.Exporters.Miscellaneous
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

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new TextAssetExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			TaskManager.AddTask(File.WriteAllBytesAsync(path, ((ITextAsset)asset).Script_C49.Data));
			return true;
		}
	}
}
