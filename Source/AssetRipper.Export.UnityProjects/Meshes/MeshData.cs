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
		Vector2[]? UV2,
		Vector2[]? UV3,
		Vector2[]? UV4,
		Vector2[]? UV5,
		Vector2[]? UV6,
		Vector2[]? UV7,
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
						if (UV2 != null && UV2.Length == Vertices.Length)
						{
							//TODO: Not implemented yet. Defines a vertex with up to 8 UV channels.
							//meshType |= GlbMeshType.TextureN;
							meshType |= GlbMeshType.Texture2;
						}
						else
						{
							meshType |= GlbMeshType.Texture2;
						}
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
		public BoneWeight4 TryGetSkinAtIndex(uint index)
		{
			BoneWeight4 s = TryGetAtIndex(Skin, index);
			if (s == default || s.AnyWeightsNegative)
			{
				//Invalid bone weights, set to a valid default.
				return new BoneWeight4(.25f, .25f, .25f, .25f, 0, 0, 0, 0);
			}
			else if (s.Sum != 1)
			{
				return s.NormalizeWeights();
			}
			else
			{
				return s;
			}
		}

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
				out Vector2[]? uv2,
				out Vector2[]? uv3,
				out Vector2[]? uv4,
				out Vector2[]? uv5,
				out Vector2[]? uv6,
				out Vector2[]? uv7,
				out _, //bindpose
				out uint[] processedIndexBuffer);

			if (vertices is null)
			{
				meshData = default;
				return false;
			}
			else
			{
				meshData = new MeshData(vertices, normals, tangents, colors, uv0, uv1, uv2, uv3, uv4, uv5, uv6, uv7, skin, processedIndexBuffer, mesh);
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
