using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Library.Exporters.Meshes;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.IO;
using System.Numerics;
using System.Text;

namespace AssetRipper.Library.Exporters.Terrains
{
	public sealed class TerrainMeshExporter : BinaryAssetExporter
	{
		public override bool IsHandle(IUnityObjectBase asset)
		{
			return asset is ITerrainData;
		}

		public override IExportCollection CreateCollection(VirtualSerializedFile virtualFile, IUnityObjectBase asset)
		{
			return new GlbExportCollection(this, asset);
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
			SceneBuilder sceneBuilder = new SceneBuilder();
			AddTerrainToSceneBuilder(sceneBuilder, terrain);
			return sceneBuilder.ToGltf2().WriteGLB().ToArray();
		}
		
		private static void AddTerrainToSceneBuilder(SceneBuilder sceneBuilder, ITerrainData terrain)
		{
			MaterialBuilder material = new MaterialBuilder("DefaultMaterial");
			MeshBuilder<VertexPosition, VertexTexture1, VertexEmpty> meshBuilder = new();
			PrimitiveBuilder<MaterialBuilder, VertexPosition, VertexTexture1, VertexEmpty> primitiveBuilder = meshBuilder.UsePrimitive(material);

			int tw = Math.Max(terrain.Heightmap_C156.Width, terrain.Heightmap_C156.Resolution);
			int th = Math.Max(terrain.Heightmap_C156.Height, terrain.Heightmap_C156.Resolution);
			
			Vector3 meshScale = terrain.Heightmap_C156.Scale.CastToStruct() * new Vector3(-1, 1, 1);
			Vector2 uvScale = new Vector2(1 / (tw - 1), 1 / (th - 1));

			int w = th;
			int h = tw;

			int startX = 0;
			int startY = 0;

			float[,] tData = GetHeights(terrain);
			
			VertexBuilder<VertexPosition, VertexTexture1, VertexEmpty>[] vertices = new VertexBuilder<VertexPosition, VertexTexture1, VertexEmpty>[w * h];
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					Vector3 pos = new Vector3(-(startY + y), tData[startX + x, startY + y], startX + x) * meshScale;
					Vector2 uv = new Vector2(x, y) * uvScale;
					vertices[(y * w) + x] = new VertexBuilder<VertexPosition, VertexTexture1, VertexEmpty>(new VertexPosition(pos), new VertexTexture1(uv));
				}
			}
			
			for (int y = 0; y < h - 1; y++)
			{
				for (int x = 0; x < w - 1; x++)
				{
					primitiveBuilder.AddTriangle(
						vertices[(y * w) + x],
						vertices[(y * w) + x + 1],
						vertices[((y + 1) * w) + x]);
					primitiveBuilder.AddTriangle(
						vertices[((y + 1) * w) + x],
						vertices[(y * w) + x + 1],
						vertices[((y + 1) * w) + x + 1]);
				}
			}

			NodeBuilder nodeBuilder = new NodeBuilder(terrain.NameString);
			sceneBuilder.AddRigidMesh(meshBuilder, nodeBuilder);
		}

		private static float[,] GetHeights(ITerrainData terrain)
		{
			int width = Math.Max(terrain.Heightmap_C156.Width, terrain.Heightmap_C156.Resolution);
			int height = Math.Max(terrain.Heightmap_C156.Height, terrain.Heightmap_C156.Resolution);
			short[] heights = terrain.Heightmap_C156.Heights;
			float[,] result = new float[width, height];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					result[x, y] = (float)heights[x + (y * width)] / short.MaxValue;
				}
			}
			return result;
		}
	}
}
