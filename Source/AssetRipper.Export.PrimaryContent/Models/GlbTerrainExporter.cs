using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using SharpGLTF.Scenes;

namespace AssetRipper.Export.PrimaryContent.Models;

public sealed class GlbTerrainExporter : IContentExtractor
{
	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out ExportCollectionBase? exportCollection)
	{
		if (asset is ITerrainData)
		{
			exportCollection = new GlbExportCollection(this, asset);
			return true;
		}
		else
		{
			exportCollection = null;
			return false;
		}
	}

	public bool Export(IUnityObjectBase asset, string path)
	{
		byte[] data = ExportTerrainToGlb((ITerrainData)asset);
		if (data.Length == 0)
		{
			return false;
		}
		File.WriteAllBytes(path, data);
		return true;
	}

	private static byte[] ExportTerrainToGlb(ITerrainData terrain)
	{
		SceneBuilder sceneBuilder = GlbTerrainBuilder.Build(terrain);
		return sceneBuilder.ToGltf2().WriteGLB().ToArray();
	}
}
