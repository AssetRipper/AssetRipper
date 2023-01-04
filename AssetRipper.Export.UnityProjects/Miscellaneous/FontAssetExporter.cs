using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_128;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class FontAssetExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IFont font && IsValidData(font.FontData_C128);
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new FontAssetExportCollection(this, asset);
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			TaskManager.AddTask(File.WriteAllBytesAsync(path, ((IFont)asset).FontData_C128));
			return true;
		}
	}
}
