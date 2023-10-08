using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Export.UnityProjects.Meshes
{
	internal readonly record struct MeshData(
		Vector3[] Vertices,
		Vector3[]? Normals,
		Vector4[]? Tangents,
		ColorFloat[]? Colors,
		Vector2[]? UV0,
		Vector2[]? UV1,
		BoneWeight4[]? Skin,
		uint[] ProcessedIndexBuffer,
		IMesh Mesh)
	{
		public GlbMeshType MeshType
		{
			get
			{
				GlbMeshType meshType = default;

				if (Normals != null && Normals.Length == Vertices.Length)
				{
					if (Tangents != null && Tangents.Length == Vertices.Length)
					{
						meshType |= GlbMeshType.PositionNormalTangent;
					}
					else
					{
						meshType |= GlbMeshType.PositionNormal;
					}
				}

				if (UV0 != null && UV0.Length == Vertices.Length)
				{
					if (UV1 != null && UV1.Length == Vertices.Length)
					{
						meshType |= GlbMeshType.Texture2;
					}
					else
					{
						meshType |= GlbMeshType.Texture1;
					}
				}

				if (Colors != null && Colors.Length == Vertices.Length)
				{
					meshType |= GlbMeshType.Color1;
				}

				if (Skin != null && Skin.Length == Vertices.Length)
				{
					meshType |= GlbMeshType.Joints4;
				}

				return meshType;
			}
		}
		
		public Vector3 TryGetVertexAtIndex(uint index) => Vertices[index];
		public Vector3 TryGetNormalAtIndex(uint index) => TryGetAtIndex(Normals, index);
		public Vector4 TryGetTangentAtIndex(uint index)
		{
			Vector4 v = TryGetAtIndex(Tangents, index);
			//Unity documentation claims W should always be 1 or -1, but it's not always the case.
			return v.W switch
			{
				-1 or 1 => v,
				< 0 => new Vector4(v.X, v.Y, v.Z, -1),
				_ => new Vector4(v.X, v.Y, v.Z, 1)
			};
		}
		public ColorFloat TryGetColorAtIndex(uint index) => TryGetAtIndex(Colors, index);
		public Vector2 TryGetUV0AtIndex(uint index) => FlipY(TryGetAtIndex(UV0, index));
		public Vector2 TryGetUV1AtIndex(uint index) => FlipY(TryGetAtIndex(UV1, index));
		public BoneWeight4 TryGetSkinAtIndex(uint index) => TryGetAtIndex(Skin, index);
		private static Vector2 FlipY(Vector2 uv) => new Vector2(uv.X, 1 - uv.Y);

		public static bool TryMakeFromMesh(IMesh mesh, out MeshData meshData)
		{
			mesh.ReadData(
				out Vector3[]? vertices,
				out Vector3[]? normals,
				out Vector4[]? tangents,
				out ColorFloat[]? colors,
				out BoneWeight4[]? skin,
				out Vector2[]? uv0,
				out Vector2[]? uv1,
				out Vector2[]? _, //uv2
				out Vector2[]? _, //uv3
				out Vector2[]? _, //uv4
				out Vector2[]? _, //uv5
				out Vector2[]? _, //uv6
				out Vector2[]? _, //uv7
				out _, //bindpose
				out uint[] processedIndexBuffer);

			if (vertices is null)
			{
				meshData = default;
				return false;
			}
			else
			{
				meshData = new MeshData(vertices, normals, tangents, colors, uv0, uv1, skin, processedIndexBuffer, mesh);
				return true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		private static T TryGetAtIndex<T>(T[]? array, uint index) where T : struct
		{
			return array is null ? default : array[index];
		}
	}
}
