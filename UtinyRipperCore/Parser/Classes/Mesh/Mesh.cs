using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Meshes;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	/// <summary>
	/// LodMesh previously
	/// </summary>
	public sealed class Mesh : NamedObject
	{
		public Mesh(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}
		
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public static bool IsReadLODData(Version version)
		{
			return version.IsLess(2, 0, 0);
		}
		/// <summary>
		/// 2.0.0 to 3.5.0 exclusive
		/// </summary>
		public static bool IsReadIndicesUsage(Version version)
		{
			return version.IsGreaterEqual(2) && version.IsLess(3, 5);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadIndexBuffer(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadSubMeshes(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// Greater than 4.1.0a
		/// </summary>
		public static bool IsReadBlendShapes(Version version)
		{
			return version.IsGreater(4, 1, 0, VersionType.Alpha);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadBoneNameHashes(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadMeshCompression(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 4.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadStreamCompression(Version version)
		{
			return version.IsGreaterEqual(4) && version.IsLess(5);
		}
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool IsReadIsReadable(Version version)
		{
			return version.IsGreaterEqual(4);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadIndexFormat(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// Less than 3.5.1
		/// </summary>
		public static bool IsReadVertices(Version version)
		{
			return version.IsLess(3, 5, 1);
		}
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool IsReadSkin(Version version)
		{
			return version.IsLess(2018, 2);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool IsReadBindPoses(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// Less than 3.5.1
		/// </summary>
		public static bool IsReadUV(Version version)
		{
			return version.IsLess(3, 5, 1);
		}
		/// <summary>
		/// 1.6.0 to 3.5.0 inclusive
		/// </summary>
		public static bool IsReadUV1(Version version)
		{
			return version.IsGreaterEqual(1, 6) && version.IsLessEqual(3, 5, 0);
		}
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool IsReadTangentSpace(Version version)
		{
			return version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0 to 3.5.0 inclusive
		/// </summary>
		public static bool IsReadTangents(Version version)
		{
			return version.IsGreaterEqual(2, 6) && version.IsLessEqual(3, 5, 0);
		}
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsReadVertexData(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadAlign(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadCompressedMesh(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.5.0 and less
		/// </summary>
		public static bool IsReadColors(Version version)
		{
			return version.IsLessEqual(3, 5, 0);
		}
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool IsReadCollisionTriangles(Version version)
		{
			return version.IsLess(3, 5);
		}
		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool IsReadMeshUsageFlags(Version version)
		{
			return version.IsGreaterEqual(2, 5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadCollision(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadMeshMetrics(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}

		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		private static bool IsReadIndexBufferFirst(Version version)
		{
			return version.IsLess(2, 6);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsReadBindPosesFirst(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.5.1 and greater
		/// </summary>
		private static bool IsReadOnlyVertexData(Version version)
		{
			return version.IsGreaterEqual(3, 5, 1);
		}
		/// <summary>
		/// 2017.3.1p1 and greater
		/// </summary>
		private static bool IsReadIndexFormatCondition(Version version)
		{
			return version.IsLess(2017, 3, 1, VersionType.Patch);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
#warning update version:
				return 8;
			}

			if (version.IsGreater(2018, 2))
			{
				return 9;
			}
#warning unknown
			if (version.IsGreater(4, 0, 0, VersionType.Beta, 1))
			{
				return 8;
			}
			if (version.IsGreaterEqual(4, 0, 0))
			{
				return 7;
			}
			if (version.IsGreaterEqual(3, 5))
			{
				return 6;
			}
			if (version.IsGreaterEqual(3))
			{
				return 5;
			}
#warning unknown
			if (version.IsGreater(2, 6, 0, VersionType.Beta))
			{
				return 4;
			}
			if (version.IsGreaterEqual(2, 6))
			{
				return 3;
			}
			if (version.IsGreaterEqual(2))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadLODData(stream.Version))
			{
				m_LODData = stream.ReadArray<LOD>();
			}
			if (IsReadIndicesUsage(stream.Version))
			{
				Use16bitIndices = stream.ReadInt32() > 0;
			}
			if (IsReadIndexBuffer(stream.Version))
			{
				if (IsReadIndexBufferFirst(stream.Version))
				{
					m_indexBuffer = stream.ReadByteArray();
					stream.AlignStream(AlignType.Align4);
				}
			}
			if (IsReadSubMeshes(stream.Version))
			{
				m_subMeshes = stream.ReadArray<SubMesh>();
			}
			
			if(IsReadBlendShapes(stream.Version))
			{
				Shapes.Read(stream);
			}
			if (IsReadBindPosesFirst(stream.Version))
			{
				m_bindPoses = stream.ReadArray<Matrix4x4f>();
			}
			if (IsReadBoneNameHashes(stream.Version))
			{
				m_boneNameHashes = stream.ReadUInt32Array();
				RootBoneNameHash = stream.ReadUInt32();
			}

			if (IsReadMeshCompression(stream.Version))
			{
				MeshCompression = stream.ReadByte();
			}
			if(IsReadStreamCompression(stream.Version))
			{
				StreamCompression = stream.ReadByte();
			}
			if(IsReadIsReadable(stream.Version))
			{
				IsReadable = stream.ReadBoolean();
				KeepVertices = stream.ReadBoolean();
				KeepIndices = stream.ReadBoolean();
			}
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadIndexFormat(stream.Version))
			{
				if (IsReadIndexFormatCondition(stream.Version))
				{
					if(MeshCompression == 0)
					{
						IndexFormat = stream.ReadInt32();
					}
				}
				else
				{
					IndexFormat = stream.ReadInt32();
				}
			}

			if (IsReadIndexBuffer(stream.Version))
			{
				if (!IsReadIndexBufferFirst(stream.Version))
				{
					m_indexBuffer = stream.ReadByteArray();
					stream.AlignStream(AlignType.Align4);
				}
			}
			
			if (IsReadVertices(stream.Version))
			{
				if (IsReadVertexData(stream.Version))
				{
					if(MeshCompression != 0)
					{
						m_vertices = stream.ReadArray<Vector3f>();
					}
				}
				else
				{
					m_vertices = stream.ReadArray<Vector3f>();
				}
			}

			if(IsReadSkin(stream.Version))
			{
				m_skin = stream.ReadArray<BoneWeights4>();
			}
			if (IsReadBindPoses(stream.Version))
			{
				if (!IsReadBindPosesFirst(stream.Version))
				{
					m_bindPoses = stream.ReadArray<Matrix4x4f>();
				}
			}
			
			if (IsReadVertexData(stream.Version))
			{
				if (IsReadOnlyVertexData(stream.Version))
				{
					VertexData.Read(stream);
				}
				else
				{
					if (MeshCompression == 0)
					{
						VertexData.Read(stream);
					}
					else
					{
						m_UV = stream.ReadArray<Vector2f>();
						m_UV1 = stream.ReadArray<Vector2f>();
						m_tangents = stream.ReadArray<Vector4f>();
						m_normals = stream.ReadArray<Vector3f>();
						m_colors = stream.ReadArray<ColorRGBA32>();
					}
				}
			}
			else
			{
				m_UV = stream.ReadArray<Vector2f>();
				if (IsReadUV1(stream.Version))
				{
					m_UV1 = stream.ReadArray<Vector2f>();
				}
				if (IsReadTangentSpace(stream.Version))
				{
					m_tangentSpace = stream.ReadArray<Tangent>();
				}
				else
				{
					m_tangents = stream.ReadArray<Vector4f>();
					m_normals = stream.ReadArray<Vector3f>();
				}
			}
			if (IsReadAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadCompressedMesh(stream.Version))
			{
				CompressedMesh.Read(stream);
			}

			LocalAABB.Read(stream);
			if (IsReadColors(stream.Version))
			{
				if (!IsReadVertexData(stream.Version))
				{
					m_colors = stream.ReadArray<ColorRGBA32>();
				}
			}
			if (IsReadCollisionTriangles(stream.Version))
			{
				m_collisionTriangles = stream.ReadUInt32Array();
				CollisionVertexCount = stream.ReadInt32();
			}
			if (IsReadMeshUsageFlags(stream.Version))
			{
				MeshUsageFlags = stream.ReadInt32();
			}
			
			if(IsReadCollision(stream.Version))
			{
				CollisionData.Read(stream);
			}
			if(IsReadMeshMetrics(stream.Version))
			{
				m_meshMetrics = new float[2];
				m_meshMetrics[0] = stream.ReadSingle();
				m_meshMetrics[1] = stream.ReadSingle();
			}
		}
		
		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_SubMeshes", GetSubMeshes(container.Version).ExportYAML(container));
			node.Add("m_Shapes", Shapes.ExportYAML(container));
			node.Add("m_BindPose", IsReadBindPoses(container.Version) ? BindPoses.ExportYAML(container) : YAMLSequenceNode.Empty);
#warning TODO?
			node.Add("m_BoneNames", YAMLSequenceNode.Empty);
			node.Add("m_BoneNameHashes", IsReadBoneNameHashes(container.Version) ? BoneNameHashes.ExportYAML(false) : YAMLSequenceNode.Empty);
#warning TODO?
			node.Add("m_RootBoneName", YAMLScalarNode.Empty);
			node.Add("m_RootBoneNameHash", RootBoneNameHash);
			node.Add("m_MeshCompression", MeshCompression);
			node.Add("m_IsReadable", IsReadable);
			node.Add("m_KeepVertices", KeepVertices);
			node.Add("m_KeepIndices", KeepIndices);
			node.Add("m_IndexBuffer", GetIndexBuffer(container.Version).ExportYAML());
			node.Add("m_Skin", IsReadSkin(container.Version) ? Skin.ExportYAML(container) : YAMLSequenceNode.Empty);
			node.Add("m_VertexData", GetVertexData(container.Version).ExportYAML(container));
			node.Add("m_CompressedMesh", CompressedMesh.ExportYAML(container));
			node.Add("m_LocalAABB", LocalAABB.ExportYAML(container));
			node.Add("m_MeshUsageFlags", MeshUsageFlags);
			if (IsReadCollision(container.Version))
			{
				node.Add("m_BakedConvexCollisionMesh", CollisionData.BakedConvexCollisionMesh.ExportYAML());
				node.Add("m_BakedTriangleCollisionMesh", CollisionData.BakedTriangleCollisionMesh.ExportYAML());
			}
			else
			{
				node.Add("m_BakedConvexCollisionMesh", ArrayExtensions.EmptyBytes.ExportYAML());
				node.Add("m_BakedTriangleCollisionMesh", ArrayExtensions.EmptyBytes.ExportYAML());
			}
#warning ???
			node.Add("m_MeshOptimized", 0);
			
			return node;
		}

		private IReadOnlyList<SubMesh> GetSubMeshes(Version version)
		{
			return IsReadSubMeshes(version) ? SubMeshes : new SubMesh[0];
		}

		private IReadOnlyList<byte> GetIndexBuffer(Version version)
		{
			return IsReadIndexBuffer(version) ? IndexBuffer : new byte[0];
		}

		private VertexData GetVertexData(Version version)
		{
			if (IsReadVertexData(version))
			{
				if (IsReadOnlyVertexData(version))
				{
					return VertexData;
				}
				else
				{
					if (MeshCompression == 0)
					{
						return VertexData;
					}
					else
					{
						return new VertexData(version, Vertices, Normals, Colors, UV, UV1, Tangents);
					}
				}
			}
			else
			{
				return new VertexData(version, Vertices, Normals, Colors, UV, UV1, Tangents);
			}
		}

		public IReadOnlyList<LOD> LODData => m_LODData;
		public IReadOnlyList<Vector3f> Vertices => m_vertices;
		public IReadOnlyList<Vector2f> UV => m_UV;
		public IReadOnlyList<Vector2f> UV1 => m_UV1;
		public IReadOnlyList<Tangent> TangentSpace => m_tangentSpace;
		public IReadOnlyList<Vector4f> Tangents => m_tangents;
		public IReadOnlyList<Vector3f> Normals => m_normals;
		public IReadOnlyList<ColorRGBA32> Colors => m_colors;
		public IReadOnlyList<uint> CollisionTriangles => m_collisionTriangles;

		public IReadOnlyList<byte> IndexBuffer => m_indexBuffer;
		public IReadOnlyList<SubMesh> SubMeshes => m_subMeshes;
		public IReadOnlyList<Matrix4x4f> BindPoses => m_bindPoses;
		public IReadOnlyList<uint> BoneNameHashes => m_boneNameHashes;
		public IReadOnlyList<BoneWeights4> Skin => m_skin;
		/// <summary>
		/// Newer versions always use 16bit indecies
		/// </summary>
		public bool Use16bitIndices { get; private set; }
		public uint RootBoneNameHash { get; private set; }
		public byte MeshCompression { get; private set; }
		public byte StreamCompression { get; private set; }
		public bool IsReadable { get; private set; }
		public bool KeepVertices { get; private set; }
		public bool KeepIndices { get; private set; }
		public int IndexFormat { get; private set; }
		public int CollisionVertexCount { get; private set; }
		public int MeshUsageFlags { get; private set; }

		public BlendShapeData Shapes;
		public VertexData VertexData;
		public CompressedMesh CompressedMesh;
		public AABB LocalAABB;
		public CollisionMeshData CollisionData;
		
		private LOD[] m_LODData;
		private Vector2f[] m_UV;
		private Vector2f[] m_UV1;
		private Tangent[] m_tangentSpace;
		private Vector4f[] m_tangents;
		private Vector3f[] m_normals;
		private ColorRGBA32[] m_colors;
		private uint[] m_collisionTriangles;

		private byte[] m_indexBuffer;
		private SubMesh[] m_subMeshes;
		private Matrix4x4f[] m_bindPoses;
		private uint[] m_boneNameHashes;
		private Vector3f[] m_vertices;
		private BoneWeights4[] m_skin;
		private float[] m_meshMetrics;
	}
}
