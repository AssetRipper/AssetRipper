using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Export.UnityProjects.Project.Exporters;
using AssetRipper.SourceGenerated.Classes.ClassID_28;

namespace AssetRipper.Export.UnityProjects.Textures
{
	internal class RawTextureExportCollection : AssetExportCollection
	{
		public RawTextureExportCollection(IAssetExporter assetExporter, ITexture2D asset) : base(assetExporter, asset)
		{
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			return ((ITexture2D)asset).Format_C28E.ToString();
		}
	}
}
