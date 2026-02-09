using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_156;
using AssetRipper.SourceGenerated.Extensions;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using System.Numerics;

namespace AssetRipper.Export.Modules.Models;

public static class GlbTerrainBuilder
{
	public static SceneBuilder Build(ITerrainData terrain)
	{
		SceneBuilder sceneBuilder = new();
		AddTerrainToSceneBuilder(sceneBuilder, terrain);
		return sceneBuilder;
	}

	private static void AddTerrainToSceneBuilder(SceneBuilder sceneBuilder, ITerrainData terrain)
	{
		MaterialBuilder material = new MaterialBuilder("DefaultMaterial");
		MeshBuilder<VertexPosition, VertexTexture1, VertexEmpty> meshBuilder = new();
		PrimitiveBuilder<MaterialBuilder, VertexPosition, VertexTexture1, VertexEmpty> primitiveBuilder = meshBuilder.UsePrimitive(material);

		int tw = Math.Max(terrain.Heightmap.Width, terrain.Heightmap.Resolution);
		int th = Math.Max(terrain.Heightmap.Height, terrain.Heightmap.Resolution);

		Vector3 meshScale = terrain.Heightmap.Scale.CastToStruct() * new Vector3(-1, 1, 1);
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
				vertices[y * w + x] = new VertexBuilder<VertexPosition, VertexTexture1, VertexEmpty>(new VertexPosition(pos), new VertexTexture1(uv));
			}
		}

		for (int y = 0; y < h - 1; y++)
		{
			for (int x = 0; x < w - 1; x++)
			{
				primitiveBuilder.AddTriangle(
					vertices[y * w + x],
					vertices[y * w + x + 1],
					vertices[(y + 1) * w + x]);
				primitiveBuilder.AddTriangle(
					vertices[(y + 1) * w + x],
					vertices[y * w + x + 1],
					vertices[(y + 1) * w + x + 1]);
			}
		}

		NodeBuilder nodeBuilder = new NodeBuilder(terrain.Name);
		sceneBuilder.AddRigidMesh(meshBuilder, nodeBuilder);
	}

	private static float[,] GetHeights(ITerrainData terrain)
	{
		int width = Math.Max(terrain.Heightmap.Width, terrain.Heightmap.Resolution);
		int height = Math.Max(terrain.Heightmap.Height, terrain.Heightmap.Resolution);
		AssetList<short> heights = terrain.Heightmap.Heights;
		float[,] result = new float[width, height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				result[x, y] = (float)heights[x + y * width] / short.MaxValue;
			}
		}
		return result;
	}
}
