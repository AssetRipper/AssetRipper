using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.Import.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;

namespace AssetRipper.Export.UnityProjects
{
	public sealed class YamlStreamedAssetExporter : YamlExporterBase
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is IMesh or ITexture2D or ITexture3D or ITexture2DArray or ICubemapArray;
			//Note: ICubeMap inherits from ITexture2D
		}

		public override IExportCollection CreateCollection(TemporaryAssetCollection virtualFile, IUnityObjectBase asset)
		{
			return new YamlStreamedAssetExportCollection(this, asset);
		}
	}
}
