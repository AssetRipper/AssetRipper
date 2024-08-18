using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Extensions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AssetRipper.Export.Modules.Models
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

				if (HasNormals)
				{
					if (HasTangents)
					{
						meshType |= GlbMeshType.PositionNormalTangent;
					}
					else
					{
						meshType |= GlbMeshType.PositionNormal;
					}
				}

				meshType |= UVCount switch
				{
					0 => default,
					1 => GlbMeshType.Texture1,
					2 => GlbMeshType.Texture2,
					_ => GlbMeshType.TextureN,
				};

				if (HasColors)
				{
					meshType |= GlbMeshType.Color1;
				}

				if (HasSkin)
				{
					meshType |= GlbMeshType.Joints4;
				}

				return meshType;
			}
		}

		public bool HasNormals => Normals != null && Normals.Length == Vertices.Length;

		public bool HasTangents => Tangents != null && Tangents.Length == Vertices.Length;

		public bool HasColors => Colors != null && Colors.Length == Vertices.Length;

		public bool HasSkin => Skin != null && Skin.Length == Vertices.Length;

		public int UVCount
		{
			get
			{
				if (UV0 is null || UV0.Length != Vertices.Length)
				{
					return 0;
				}
				else if (UV1 is null || UV1.Length != Vertices.Length)
				{
					return 1;
				}
				else if (UV2 is null || UV2.Length != Vertices.Length)
				{
					return 2;
				}
				else if (UV3 is null || UV3.Length != Vertices.Length)
				{
					return 3;
				}
				else if (UV4 is null || UV4.Length != Vertices.Length)
				{
					return 4;
				}
				else if (UV5 is null || UV5.Length != Vertices.Length)
				{
					return 5;
				}
				else if (UV6 is null || UV6.Length != Vertices.Length)
				{
					return 6;
				}
				else if (UV7 is null || UV7.Length != Vertices.Length)
				{
					return 7;
				}
				else
				{
					return 8;
				}
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
		public Vector2 TryGetUV2AtIndex(uint index) => FlipY(TryGetAtIndex(UV2, index));
		public Vector2 TryGetUV3AtIndex(uint index) => FlipY(TryGetAtIndex(UV3, index));
		public Vector2 TryGetUV4AtIndex(uint index) => FlipY(TryGetAtIndex(UV4, index));
		public Vector2 TryGetUV5AtIndex(uint index) => FlipY(TryGetAtIndex(UV5, index));
		public Vector2 TryGetUV6AtIndex(uint index) => FlipY(TryGetAtIndex(UV6, index));
		public Vector2 TryGetUV7AtIndex(uint index) => FlipY(TryGetAtIndex(UV7, index));
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
