using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_28;

namespace AssetRipper.Export.UnityProjects.Textures;

/// <summary>
/// Preserves textures without pixels as native Unity assets.
/// </summary>
internal sealed class EmptyTextureExportCollection : AssetsExportCollection<ITexture2D>
{
	public EmptyTextureExportCollection(SpriteInformationObject spriteInformationObject)
		: base(new DefaultYamlExporter(), spriteInformationObject.Texture)
	{
		AddAsset(spriteInformationObject);
	}

	protected override bool ExportInner(IExportContainer container, string filePath, string dirPath, FileSystem fileSystem)
	{
		return AssetExporter.Export(container, Asset, filePath, fileSystem);
	}

	protected override string GetExportExtension(IUnityObjectBase asset) => "asset";
}
