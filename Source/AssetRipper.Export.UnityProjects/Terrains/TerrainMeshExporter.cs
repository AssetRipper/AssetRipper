using AssetRipper.Assets;
using AssetRipper.Export.Modules.Models;
using AssetRipper.Export.UnityProjects.Meshes;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using SharpGLTF.Scenes;

namespace AssetRipper.Export.UnityProjects.Terrains
{
	public sealed class TerrainMeshExporter : BinaryAssetExporter
	{
		public override bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
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

		public override bool Export(IExportContainer container, IUnityObjectBase asset, string path)
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
}
