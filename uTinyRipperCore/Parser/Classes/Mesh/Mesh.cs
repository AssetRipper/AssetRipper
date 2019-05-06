using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Meshes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
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
		public static bool IsReadUse16bitIndices(Version version)
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
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadBonesAABB(Version version)
		{
			return version.IsGreaterEqual(2019);
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
		/// Less than 3.5.0f1
		/// </summary>
		public static bool IsReadCollisionTriangles(Version version)
		{
			return version.IsLessEqual(3, 5, 0, VersionType.Beta);
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
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool IsReadMeshOptimized(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 2019.1 and greater and Not Release
		/// </summary>
		public static bool IsReadMeshOptimizationFlags(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsReadStreamData(Version version)
		{
			return version.IsGreaterEqual(2018, 3);
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
		/// Less than 2017.3.1p1
		/// </summary>
		private static bool IsReadIndexFormatCondition(Version version)
		{
			return version.IsLess(2017, 3, 1, VersionType.Patch);
		}

		private static int GetSerializedVersion(Version version)
		{
			// MeshOptimized has been extended to MeshOptimizationFlags
			if (version.IsGreaterEqual(2019))
			{
				return 10;
			}
			// Skin has been moved to VertexData
			if (version.IsGreaterEqual(2018, 2))
			{
				return 9;
			}
			if (version.IsGreaterEqual(4, 0, 0))
			{
				return 8;
			}
			// unknown (4.0.0b) version
			// return 7;
			if (version.IsGreaterEqual(3, 5))
			{
				return 6;
			}
			if (version.IsGreaterEqual(3))
			{
				return 5;
			}
			if (version.IsGreaterEqual(2, 6))
			{
				return 4;
			}
			// unknown (2.6.0b) version
			// return 3;
			if (version.IsGreaterEqual(2))
			{
				return 2;
			}
			return 1;
		}

		public string FindBlendShapeNameByCRC(uint crc)
		{
			return Shapes.FindShapeNameByCRC(File.Version, crc);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadLODData(reader.Version))
			{
				m_LODData = reader.ReadAssetArray<LOD>();
			}
			if (IsReadUse16bitIndices(reader.Version))
			{
				Use16bitIndices = reader.ReadInt32() > 0;
			}
			if (IsReadIndexBuffer(reader.Version))
			{
				if (IsReadIndexBufferFirst(reader.Version))
				{
					m_indexBuffer = reader.ReadByteArray();
					reader.AlignStream(AlignType.Align4);
				}
			}
			if (IsReadSubMeshes(reader.Version))
			{
				m_subMeshes = reader.ReadAssetArray<SubMesh>();
			}

			if (IsReadBlendShapes(reader.Version))
			{
				Shapes.Read(reader);
			}
			if (IsReadBindPosesFirst(reader.Version))
			{
				m_bindPoses = reader.ReadAssetArray<Matrix4x4f>();
			}
			if (IsReadBoneNameHashes(reader.Version))
			{
				m_boneNameHashes = reader.ReadUInt32Array();
				RootBoneNameHash = reader.ReadUInt32();
			}
			if (IsReadBonesAABB(reader.Version))
			{
				m_bonesAABB = reader.ReadAssetArray<MinMaxAABB>();
				VariableBoneCountWeights.Read(reader);
			}

			if (IsReadMeshCompression(reader.Version))
			{
				MeshCompression = (MeshCompression)reader.ReadByte();
			}
			if (IsReadStreamCompression(reader.Version))
			{
				StreamCompression = reader.ReadByte();
			}
			if (IsReadIsReadable(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
				KeepVertices = reader.ReadBoolean();
				KeepIndices = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadIndexFormat(reader.Version))
			{
				if (IsReadIndexFormatCondition(reader.Version))
				{
					if (MeshCompression == 0)
					{
						IndexFormat = (IndexFormat)reader.ReadInt32();
					}
				}
				else
				{
					IndexFormat = (IndexFormat)reader.ReadInt32();
				}
			}

			if (IsReadIndexBuffer(reader.Version))
			{
				if (!IsReadIndexBufferFirst(reader.Version))
				{
					m_indexBuffer = reader.ReadByteArray();
					reader.AlignStream(AlignType.Align4);
				}
			}

			if (IsReadVertices(reader.Version))
			{
				if (IsReadVertexData(reader.Version))
				{
					if (MeshCompression != 0)
					{
						m_vertices = reader.ReadAssetArray<Vector3f>();
					}
				}
				else
				{
					m_vertices = reader.ReadAssetArray<Vector3f>();
				}
			}

			if (IsReadSkin(reader.Version))
			{
				m_skin = reader.ReadAssetArray<BoneWeights4>();
			}
			if (IsReadBindPoses(reader.Version))
			{
				if (!IsReadBindPosesFirst(reader.Version))
				{
					m_bindPoses = reader.ReadAssetArray<Matrix4x4f>();
				}
			}

			if (IsReadVertexData(reader.Version))
			{
				if (IsReadOnlyVertexData(reader.Version))
				{
					VertexData.Read(reader);
				}
				else
				{
					if (MeshCompression == 0)
					{
						VertexData.Read(reader);
					}
					else
					{
						m_UV = reader.ReadAssetArray<Vector2f>();
						m_UV1 = reader.ReadAssetArray<Vector2f>();
						m_tangents = reader.ReadAssetArray<Vector4f>();
						m_normals = reader.ReadAssetArray<Vector3f>();
						m_colors = reader.ReadAssetArray<ColorRGBA32>();
					}
				}
			}
			else
			{
				m_UV = reader.ReadAssetArray<Vector2f>();
				if (IsReadUV1(reader.Version))
				{
					m_UV1 = reader.ReadAssetArray<Vector2f>();
				}
				if (IsReadTangentSpace(reader.Version))
				{
					m_tangentSpace = reader.ReadAssetArray<Tangent>();
				}
				else
				{
					m_tangents = reader.ReadAssetArray<Vector4f>();
					m_normals = reader.ReadAssetArray<Vector3f>();
				}
			}
			if (IsReadAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadCompressedMesh(reader.Version))
			{
				CompressedMesh.Read(reader);
			}

			LocalAABB.Read(reader);
			if (IsReadColors(reader.Version))
			{
				if (!IsReadVertexData(reader.Version))
				{
					m_colors = reader.ReadAssetArray<ColorRGBA32>();
				}
			}
			if (IsReadCollisionTriangles(reader.Version))
			{
				m_collisionTriangles = reader.ReadUInt32Array();
				CollisionVertexCount = reader.ReadInt32();
			}
			if (IsReadMeshUsageFlags(reader.Version))
			{
				MeshUsageFlags = reader.ReadInt32();
			}

			if (IsReadCollision(reader.Version))
			{
				CollisionData.Read(reader);
			}
			if (IsReadMeshMetrics(reader.Version))
			{
				m_meshMetrics = new float[2];
				m_meshMetrics[0] = reader.ReadSingle();
				m_meshMetrics[1] = reader.ReadSingle();
			}
#if UNIVERSAL
			if (IsReadMeshOptimized(reader.Version, reader.Flags))
			{
				MeshOptimizationFlags = reader.ReadBoolean() ? MeshOptimizationFlags.Everything : 0;
			}
			else if (IsReadMeshOptimizationFlags(reader.Version, reader.Flags))
			{
				MeshOptimizationFlags = (MeshOptimizationFlags)reader.ReadInt32();
			}
#endif
			if (IsReadStreamData(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
				StreamData.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(SubMeshesName, GetSubMeshes(container.Version).ExportYAML(container));
			node.Add(ShapesName, GetShapes(container.Version).ExportYAML(container));
			node.Add(BindPoseName, GetBindPoses(container.Version).ExportYAML(container));
			node.Add(BoneNameHashesName, GetBoneNameHashes(container.Version).ExportYAML(true));
			node.Add(RootBoneNameHashName, RootBoneNameHash);
			if (IsReadBonesAABB(container.ExportVersion))
			{
				node.Add(BonesAABBName, GetBonesAABB(container.Version).ExportYAML(container));
				node.Add(VariableBoneCountWeightsName, GetVariableBoneCountWeights(container.Version).ExportYAML(container));
			}
			node.Add(MeshCompressionName, (byte)MeshCompression);
			node.Add(IsReadableName, IsReadable);
			node.Add(KeepVerticesName, KeepVertices);
			node.Add(KeepIndicesName, KeepIndices);
			node.Add(IndexFormatName, (int)IndexFormat);
			node.Add(IndexBufferName, GetIndexBuffer(container.Version, container.Platform).ExportYAML());
			node.Add(SkinName, GetSkin(container.Version).ExportYAML(container));
			node.Add(VertexDataName, GetVertexData(container.Version).ExportYAML(container));
			node.Add(CompressedMeshName, CompressedMesh.ExportYAML(container));
			node.Add(LocalAABBName, LocalAABB.ExportYAML(container));
			node.Add(MeshUsageFlagsName, MeshUsageFlags);
			node.Add(BakedConvexCollisionMeshName, GetBakedConvexCollisionMesh(container.Version).ExportYAML());
			node.Add(BakedTriangleCollisionMeshName, GetBakedTriangleCollisionMesh(container.Version).ExportYAML());
			if (IsReadMeshOptimizationFlags(container.ExportVersion, container.ExportFlags))
			{
				node.Add(MeshOptimizationFlagsName, (int)GetMeshOptimizationFlags(container.Version, container.Flags));
			}
			else
			{
				node.Add(MeshOptimizedName, GetMeshOptimized(container.Version, container.Flags));
			}
			if (IsReadStreamData(container.ExportVersion))
			{
				StreamingInfo streamData = new StreamingInfo(true);
				node.Add(StreamDataName, streamData.ExportYAML(container));
			}

			return node;
		}

		private IReadOnlyList<SubMesh> GetSubMeshes(Version version)
		{
			return IsReadSubMeshes(version) ? SubMeshes : new SubMesh[0];
		}
		private BlendShapeData GetShapes(Version version)
		{
			return IsReadBlendShapes(version) ? Shapes : new BlendShapeData(true);
		}
		private IReadOnlyList<Matrix4x4f> GetBindPoses(Version version)
		{
			return IsReadBindPoses(version) ? BindPoses : new Matrix4x4f[0];
		}
		private IReadOnlyList<uint> GetBoneNameHashes(Version version)
		{
			return IsReadBoneNameHashes(version) ? BoneNameHashes : new uint[0];
		}
		private IReadOnlyList<MinMaxAABB> GetBonesAABB(Version version)
		{
			return IsReadBonesAABB(version) ? BonesAABB : new MinMaxAABB[0];
		}
		private VariableBoneCountWeights GetVariableBoneCountWeights(Version version)
		{
			return IsReadBonesAABB(version) ? VariableBoneCountWeights : new VariableBoneCountWeights(true);
		}
		private IReadOnlyList<byte> GetIndexBuffer(Version version, Platform platform)
		{
			if (IsReadIndexBuffer(version))
			{
				if (platform == Platform.XBox360)
				{
					AlignType align = (IsReadUse16bitIndices(version) && Use16bitIndices) ? AlignType.Align2 : AlignType.Align4;
					return m_indexBuffer.SwapBytes(align);
				}
				return IndexBuffer;
			}
			return ArrayExtensions.EmptyBytes;
		}

		private IReadOnlyList<BoneWeights4> GetSkin(Version version)
		{
			if (IsReadSkin(version))
			{
				return Skin;
			}
			else
			{
				return GetVertexData(version).GenerateSkin(version);
			}
		}

		private VertexData GetVertexData(Version version)
		{
			if (IsReadVertexData(version))
			{
				if (IsReadOnlyVertexData(version))
				{
					if (IsReadStreamData(version) && StreamData.IsValid)
					{
						byte[] data = StreamData.GetContent(File);
						if (data == null)
						{
							Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{ValidName}' because resources file '{StreamData.Path}' wasn't found");
							return VertexData;
						}
						return new VertexData(VertexData, data);
					}

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
						return new VertexData(Vertices, Normals, Colors, UV, UV1, Tangents);
					}
				}
			}
			else
			{
				return new VertexData(Vertices, Normals, Colors, UV, UV1, Tangents);
			}
		}

		private IReadOnlyList<byte> GetBakedConvexCollisionMesh(Version version)
		{
			return IsReadCollision(version) ? CollisionData.BakedConvexCollisionMesh : new byte[0];
		}
		private IReadOnlyList<byte> GetBakedTriangleCollisionMesh(Version version)
		{
			return IsReadCollision(version) ? CollisionData.BakedTriangleCollisionMesh : new byte[0];
		}
		private bool GetMeshOptimized(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadMeshOptimized(version, flags))
			{
				return MeshOptimizationFlags == 0 ? false : true;
			}
#endif
			return false;
		}
		private MeshOptimizationFlags GetMeshOptimizationFlags(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadMeshOptimizationFlags(version, flags))
			{
				return MeshOptimizationFlags;
			}
#endif
			return MeshOptimizationFlags.Everything;
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
		public IReadOnlyList<MinMaxAABB> BonesAABB => m_bonesAABB;
		public MeshCompression MeshCompression { get; private set; }
		public byte StreamCompression { get; private set; }
		public bool IsReadable { get; private set; }
		public bool KeepVertices { get; private set; }
		public bool KeepIndices { get; private set; }
		public IndexFormat IndexFormat { get; private set; }
		public int CollisionVertexCount { get; private set; }
		public int MeshUsageFlags { get; private set; }
#if UNIVERSAL
		public MeshOptimizationFlags MeshOptimizationFlags { get; private set; }
#endif

		public const string SubMeshesName = "m_SubMeshes";
		public const string ShapesName = "m_Shapes";
		public const string BindPoseName = "m_BindPose";
		public const string BoneNamesName = "m_BoneNames";
		public const string BoneNameHashesName = "m_BoneNameHashes";
		public const string RootBoneNameName = "m_RootBoneName";
		public const string RootBoneNameHashName = "m_RootBoneNameHash";
		public const string BonesAABBName = "m_BonesAABB";
		public const string VariableBoneCountWeightsName = "m_VariableBoneCountWeights";
		public const string MeshCompressionName = "m_MeshCompression";
		public const string IsReadableName = "m_IsReadable";
		public const string KeepVerticesName = "m_KeepVertices";
		public const string KeepIndicesName = "m_KeepIndices";
		public const string IndexFormatName = "m_IndexFormat";
		public const string IndexBufferName = "m_IndexBuffer";
		public const string SkinName = "m_Skin";
		public const string VertexDataName = "m_VertexData";
		public const string CompressedMeshName = "m_CompressedMesh";
		public const string LocalAABBName = "m_LocalAABB";
		public const string MeshUsageFlagsName = "m_MeshUsageFlags";
		public const string BakedConvexCollisionMeshName = "m_BakedConvexCollisionMesh";
		public const string BakedTriangleCollisionMeshName = "m_BakedTriangleCollisionMesh";
		public const string MeshOptimizedName = "m_MeshOptimized";
		public const string MeshOptimizationFlagsName = "m_MeshOptimizationFlags";
		public const string StreamDataName = "m_StreamData";

		public BlendShapeData Shapes;
		public VariableBoneCountWeights VariableBoneCountWeights;
		public VertexData VertexData;
		public CompressedMesh CompressedMesh;
		public AABB LocalAABB;
		public CollisionMeshData CollisionData;
		public StreamingInfo StreamData;

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
		private MinMaxAABB[] m_bonesAABB;
		private Vector3f[] m_vertices;
		private BoneWeights4[] m_skin;
		private float[] m_meshMetrics;
	}
}
