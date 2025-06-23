using AssetRipper.Assets;
using AssetRipper.SourceGenerated.Classes.ClassID_28;

namespace AssetRipper.Export.UnityProjects.Textures;

internal class RawTextureExportCollection : AssetExportCollection<ITexture2D>
{
	public RawTextureExportCollection(IAssetExporter assetExporter, ITexture2D asset) : base(assetExporter, asset)
	{
	}

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		return Asset.Format_C28E.ToString();
	}
}
