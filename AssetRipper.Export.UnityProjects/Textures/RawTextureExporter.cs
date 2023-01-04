using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Export.UnityProjects.Textures
{
	internal class RawTextureExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is ITexture2D;
		}

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
		{
			File.WriteAllBytes(path, ((ITexture2D)asset).GetImageData());
			return true;
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new RawTextureExportCollection(this, (ITexture2D)asset);
		}
	}
}
