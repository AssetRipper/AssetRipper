using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Export;
using AssetRipper.SourceGenerated.Classes.ClassID_117;
using AssetRipper.SourceGenerated.Classes.ClassID_187;
using AssetRipper.SourceGenerated.Classes.ClassID_188;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_43;

namespace AssetRipper.Export.UnityProjects.Project
{
	public sealed class YamlStreamedAssetExporter : YamlExporterBase
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, TemporaryAssetCollection temporaryFile, [NotNullWhen(true)] out IExportCollection? exportCollection)
		{
			//Note: ICubeMap inherits from ITexture2D
			if (asset is IMesh or ITexture2D or ITexture3D or ITexture2DArray or ICubemapArray)
			{
				exportCollection = new YamlStreamedAssetExportCollection(this, asset);
				return true;
			}
			else
			{
				exportCollection = null;
				return false;
			}
		}
	}
}
