using AssetRipper.Core.Classes.Sprite;
using AssetRipper.Core.IO;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.SourceGenerated.Subclasses.SpriteRenderData;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using AssetRipper.SourceGenerated.Subclasses.Vector2f;
using AssetRipper.SourceGenerated.Subclasses.Vector3f;
using System.Collections.Generic;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class SpriteRenderDataExtensions
	{
		//Notes:
		//
		// TextureRect:
		// Actual sprite rectangle inside atlas texture (or in original texture for non atlas sprite)
		// It is a retangle of cropped image if tight mode is used. Otherwise, its size matches the original size
		//
		// TextureRectOffset:
		// Offset of actual (cropped) sprite rectangle relative to Sprite.Rect .
		// Unity crops rectangle to save atlas space if tight mode is used. So final atlas image is a cropped version
		// of a rectangle, developer specified in original texture.
		// In other words, this value show how much Unity cropped the Sprite.Rect from bottom-left corner

		public static bool IsPacked(this ISpriteRenderData spriteRenderData) => (spriteRenderData.SettingsRaw & 1) != 0;

		public static SpritePackingMode GetPackingMode(this ISpriteRenderData spriteRenderData)
		{
			return (SpritePackingMode)((spriteRenderData.SettingsRaw >> 1) & 1);
		}

		public static SpritePackingRotation GetPackingRotation(this ISpriteRenderData spriteRenderData)
		{
			return (SpritePackingRotation)((spriteRenderData.SettingsRaw >> 2) & 0xF);
		}

		public static SpriteMeshType GetMeshType(this ISpriteRenderData spriteRenderData)
		{
			return (SpriteMeshType)((spriteRenderData.SettingsRaw >> 6) & 0x1);
		}

		public static void GenerateOutline(this ISpriteRenderData spriteRenderData, UnityVersion version, AssetList<AssetList<Vector2f_3_5_0_f5>> outlines)
		{
			outlines.Clear();
			if (spriteRenderData.Has_VertexData() && spriteRenderData.SubMeshes!.Count != 0)
			{
				for (int i = 0; i < spriteRenderData.SubMeshes.Count; i++)
				{
					Vector3f_3_5_0_f5[] vertices = spriteRenderData.VertexData.GenerateVertices(version, spriteRenderData.SubMeshes[i]);
					List<Vector2f[]> vectorArrayList = spriteRenderData.VertexDataToOutline(vertices, spriteRenderData.SubMeshes[i]);
					outlines.AddRanges(vectorArrayList);
				}
			}
			else if (spriteRenderData.Has_Vertices() && spriteRenderData.Vertices.Count != 0)
			{
				List<Vector2f[]> vectorArrayList = spriteRenderData.VerticesToOutline();
				outlines.Capacity = vectorArrayList.Count;
				outlines.AddRanges(vectorArrayList);
			}
		}

		private static List<Vector2f[]> VerticesToOutline(this ISpriteRenderData spriteRenderData)
		{
			Vector3f_3_5_0_f5[] vertices = new Vector3f_3_5_0_f5[spriteRenderData.Vertices.Count];
			for (int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = spriteRenderData.Vertices[i].Pos;
			}

			Vector3i[] triangles = new Vector3i[spriteRenderData.Indices.Length / 3];
			for (int i = 0, j = 0; i < triangles.Length; i++)
			{
				int x = spriteRenderData.Indices[j++];
				int y = spriteRenderData.Indices[j++];
				int z = spriteRenderData.Indices[j++];
				triangles[i] = new Vector3i(x, y, z);
			}

			MeshOutlineGenerator outlineGenerator = new MeshOutlineGenerator(vertices, triangles);
			return outlineGenerator.GenerateOutlines();
		}

		private static List<Vector2f[]> VertexDataToOutline(this ISpriteRenderData spriteRenderData, Vector3f_3_5_0_f5[] vertices, ISubMesh submesh)
		{
			Vector3i[] triangles = new Vector3i[submesh.IndexCount / 3];
			for (int o = (int)submesh.FirstByte, ti = 0; ti < triangles.Length; o += 6, ti++)
			{
				int x = BitConverter.ToUInt16(spriteRenderData.IndexBuffer, o + 0);
				int y = BitConverter.ToUInt16(spriteRenderData.IndexBuffer, o + 2);
				int z = BitConverter.ToUInt16(spriteRenderData.IndexBuffer, o + 4);
				triangles[ti] = new Vector3i(x, y, z);
			}
			MeshOutlineGenerator outlineGenerator = new MeshOutlineGenerator(vertices, triangles);
			return outlineGenerator.GenerateOutlines();
		}

		private static void AddRanges(this AssetList<AssetList<Vector2f_3_5_0_f5>> instance, List<Vector2f[]> vectorArrayList)
		{
			foreach (Vector2f[] vectorArray in vectorArrayList)
			{
				AssetList<Vector2f_3_5_0_f5> assetList = new AssetList<Vector2f_3_5_0_f5>(vectorArray.Length);
				instance.Add(assetList);
				foreach (Vector2f v in vectorArray)
				{
					assetList.Add((Vector2f_3_5_0_f5)v);
				}
			}
		}
	}
}
