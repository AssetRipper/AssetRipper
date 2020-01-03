using uTinyRipper.Classes.Meshes;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Misc;
using uTinyRipper;

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

		public static int ToSerializedVersion(Version version)
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
			// MeshTopology == 1 has become deprecated
			// unknown alpha/beta version
			if (version.IsGreaterEqual(4, 0, 0, VersionType.Beta))
			{
				return 8;
			}
			// unknown conversion
			if (version.IsGreaterEqual(4))
			{
				return 7;
			}
			// VertexData has been added
			if (version.IsGreaterEqual(3, 5))
			{
				return 6;
			}
			// SubMesh's vertex range (FirstVertex, VertexCount and LocalAABB) has been added
			if (version.IsGreaterEqual(3))
			{
				return 5;
			}
			// MeshCompression has been added
			// unknown alpha/beta version
			if (version.IsGreaterEqual(2, 6, 0, VersionType.Beta))
			{
				return 4;
			}
			// TangentSpace has been replaced by Tangents and Normals
			if (version.IsGreaterEqual(2, 6))
			{
				return 3;
			}
			// LODData has been replaced by IndexBuffer and SubMeshes
			if (version.IsGreaterEqual(2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public static bool HasLODData(Version version) => version.IsLess(2, 0, 0);
		/// <summary>
		/// 2.0.0 to 3.5.0 exclusive
		/// </summary>
		public static bool HasUse16bitIndices(Version version) => version.IsLess(3, 5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasBlendChannels(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// Greater than 4.1.0a
		/// </summary>
		public static bool HasBlendShapes(Version version) => version.IsGreater(4, 1, 0, VersionType.Alpha);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasBoneNameHashes(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasBonesAABB(Version version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasMeshCompression(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 4.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasStreamCompression(Version version) => version.IsGreaterEqual(4) && version.IsLess(5);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasIsReadable(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasIndexFormat(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool HasSkin(Version version) => version.IsLess(2018, 2);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool HasBindPose(Version version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 1.6.0 and greater
		/// </summary>
		public static bool HasUV1(Version version) => version.IsGreaterEqual(1, 6);
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool HasTangentSpace(Version version) => version.IsLess(2, 6);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasVertexData(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 3.5.1 and greater
		/// </summary>
		public static bool IsOnlyVertexData(Version version) => version.IsGreaterEqual(3, 5, 1);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasCompressedMesh(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// Less than 3.5.0f1
		/// </summary>
		public static bool HasCollisionTriangles(Version version) => version.IsLessEqual(3, 5, 0, VersionType.Beta);
		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool HasMeshUsageFlags(Version version) => version.IsGreaterEqual(2, 5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasCollision(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasMeshMetrics(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasMeshOptimization(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasStreamData(Version version) => version.IsGreaterEqual(2018, 3);

		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		private static bool IsIndexBufferFirst(Version version) => version.IsLess(2, 6);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsBindPoseFirst(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlignVertex(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlignFlags(Version version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// Less than 2017.3.1p1
		/// </summary>
		private static bool IsIndexFormatCondition(Version version) => version.IsLess(2017, 3, 1, VersionType.Patch);
		/// <summary>
		/// 2019.1 and greater and Not Release
		/// </summary>
		private static bool IsMeshOptimizationFlags(Version version) => version.IsGreaterEqual(2019);

		public bool CheckAssetIntegrity()
		{
			if (HasStreamData(File.Version))
			{
				if (VertexData.IsSet)
				{
					return StreamData.CheckIntegrity(File);
				}
			}
			return true;
		}

		public string FindBlendShapeNameByCRC(uint crc)
		{
			if (HasBlendChannels(File.Version))
			{
				return Shapes.FindShapeNameByCRC(crc);
			}
			else
			{
				foreach (BlendShape blendShape in BlendShapes)
				{
					if (blendShape.IsCRCMatch(crc))
					{
						return blendShape.Name;
					}
				}
				return null;
			}
		}

		// TEMP: argument
		public bool Is16BitIndices(Version version)
		{
			if (HasLODData(version))
			{
				return true;
			}
			else if (HasUse16bitIndices(version))
			{
				return Use16BitIndices != 0;
			}
			else if (HasIndexFormat(version))
			{
				return IndexFormat == IndexFormat.UInt16;
			}
			return true;
		}

		public override Object Convert(IExportContainer container)
		{
			return MeshConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasLODData(reader.Version))
			{
				LODData = reader.ReadAssetArray<LOD>();
			}
			else
			{
				if (HasUse16bitIndices(reader.Version))
				{
					Use16BitIndices = reader.ReadUInt32();
				}
				if (IsIndexBufferFirst(reader.Version))
				{
					IndexBuffer = reader.ReadByteArray();
					reader.AlignStream();
				}
				SubMeshes = reader.ReadAssetArray<SubMesh>();
			}

			if (HasBlendShapes(reader.Version))
			{
				if (HasBlendChannels(reader.Version))
				{
					Shapes.Read(reader);
				}
				else
				{
					BlendShapes = reader.ReadAssetArray<BlendShape>();
					reader.AlignStream();
					ShapeVertices = reader.ReadAssetArray<BlendShapeVertex>();
				}
			}
			if (HasBindPose(reader.Version) && IsBindPoseFirst(reader.Version))
			{
				BindPose = reader.ReadAssetArray<Matrix4x4f>();
			}
			if (HasBoneNameHashes(reader.Version))
			{
				BoneNameHashes = reader.ReadUInt32Array();
				RootBoneNameHash = reader.ReadUInt32();
			}
			if (HasBonesAABB(reader.Version))
			{
				BonesAABB = reader.ReadAssetArray<MinMaxAABB>();
				VariableBoneCountWeights.Read(reader);
			}

			if (HasMeshCompression(reader.Version))
			{
				MeshCompression = (MeshCompression)reader.ReadByte();
			}
			if (HasStreamCompression(reader.Version))
			{
				StreamCompression = reader.ReadByte();
			}
			if (HasIsReadable(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
				KeepVertices = reader.ReadBoolean();
				KeepIndices = reader.ReadBoolean();
			}
			if (IsAlignFlags(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasIndexFormat(reader.Version))
			{
				if (IsIndexFormatCondition(reader.Version))
				{
					if (MeshCompression == MeshCompression.Off)
					{
						IndexFormat = (IndexFormat)reader.ReadInt32();
					}
				}
				else
				{
					IndexFormat = (IndexFormat)reader.ReadInt32();
				}
			}

			if (!HasLODData(reader.Version) && !IsIndexBufferFirst(reader.Version))
			{
				IndexBuffer = reader.ReadByteArray();
				reader.AlignStream();
			}

			if (HasVertexData(reader.Version))
			{
				if (!IsOnlyVertexData(reader.Version))
				{
					if (MeshCompression != MeshCompression.Off)
					{
						Vertices = reader.ReadAssetArray<Vector3f>();
					}
				}
			}
			else
			{
				Vertices = reader.ReadAssetArray<Vector3f>();
			}

			if (HasSkin(reader.Version))
			{
				Skin = reader.ReadAssetArray<BoneWeights4>();
			}
			if (HasBindPose(reader.Version) && !IsBindPoseFirst(reader.Version))
			{
				BindPose = reader.ReadAssetArray<Matrix4x4f>();
			}

			if (HasVertexData(reader.Version))
			{
				if (IsOnlyVertexData(reader.Version))
				{
					VertexData.Read(reader);
				}
				else
				{
					if (MeshCompression == MeshCompression.Off)
					{
						VertexData.Read(reader);
					}
					else
					{
						UV = reader.ReadAssetArray<Vector2f>();
						UV1 = reader.ReadAssetArray<Vector2f>();
						Tangents = reader.ReadAssetArray<Vector4f>();
						Normals = reader.ReadAssetArray<Vector3f>();
						Colors = reader.ReadAssetArray<ColorRGBA32>();
					}
				}
			}
			else
			{
				UV = reader.ReadAssetArray<Vector2f>();
				if (HasUV1(reader.Version))
				{
					UV1 = reader.ReadAssetArray<Vector2f>();
				}
				if (HasTangentSpace(reader.Version))
				{
					TangentSpace = reader.ReadAssetArray<Tangent>();
				}
				else
				{
					Tangents = reader.ReadAssetArray<Vector4f>();
					Normals = reader.ReadAssetArray<Vector3f>();
				}
			}
			if (IsAlignVertex(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasCompressedMesh(reader.Version))
			{
				CompressedMesh.Read(reader);
			}

			LocalAABB.Read(reader);
			if (!HasVertexData(reader.Version))
			{
				Colors = reader.ReadAssetArray<ColorRGBA32>();
			}
			if (HasCollisionTriangles(reader.Version))
			{
				CollisionTriangles = reader.ReadUInt32Array();
				CollisionVertexCount = reader.ReadInt32();
			}
			if (HasMeshUsageFlags(reader.Version))
			{
				MeshUsageFlags = reader.ReadInt32();
			}

			if (HasCollision(reader.Version))
			{
				CollisionData.Read(reader);
			}
			if (HasMeshMetrics(reader.Version))
			{
				MeshMetrics = new float[2];
				MeshMetrics[0] = reader.ReadSingle();
				MeshMetrics[1] = reader.ReadSingle();
			}
#if UNIVERSAL
			if (HasMeshOptimization(reader.Version, reader.Flags))
			{
				if (IsMeshOptimizationFlags(reader.Version))
				{
					MeshOptimizationFlags = (MeshOptimizationFlags)reader.ReadInt32();
				}
				else
				{
					MeshOptimized = reader.ReadBoolean();
				}
			}
#endif
			if (HasStreamData(reader.Version))
			{
				reader.AlignStream();
				StreamData.Read(reader);
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasLODData(writer.Version))
			{
				LODData.Write(writer);
			}
			else
			{
				if (HasUse16bitIndices(writer.Version))
				{
					writer.Write(Use16BitIndices);
				}
				if (IsIndexBufferFirst(writer.Version))
				{
					IndexBuffer.Write(writer);
					writer.AlignStream();
				}
				SubMeshes.Write(writer);
			}

			if (HasBlendShapes(writer.Version))
			{
				if (HasBlendChannels(writer.Version))
				{
					Shapes.Write(writer);
				}
				else
				{
					BlendShapes.Write(writer);
					writer.AlignStream();
					ShapeVertices.Write(writer);
				}
			}
			if (HasBindPose(writer.Version))
			{
				if (IsBindPoseFirst(writer.Version))
				{
					BindPose.Write(writer);
				}
			}
			if (HasBoneNameHashes(writer.Version))
			{
				BoneNameHashes.Write(writer);
				writer.Write(RootBoneNameHash);
			}
			if (HasBonesAABB(writer.Version))
			{
				BonesAABB.Write(writer);
				VariableBoneCountWeights.Write(writer);
			}

			if (HasMeshCompression(writer.Version))
			{
				writer.Write((byte)MeshCompression);
			}
			if (HasStreamCompression(writer.Version))
			{
				writer.Write(StreamCompression);
			}
			if (HasIsReadable(writer.Version))
			{
				writer.Write(IsReadable);
				writer.Write(KeepVertices);
				writer.Write(KeepIndices);
			}
			if (IsAlignFlags(writer.Version))
			{
				writer.AlignStream();
			}

			if (HasIndexFormat(writer.Version))
			{
				if (IsIndexFormatCondition(writer.Version))
				{
					if (MeshCompression == MeshCompression.Off)
					{
						writer.Write((int)IndexFormat);
					}
				}
				else
				{
					writer.Write((int)IndexFormat);
				}
			}

			if (!HasLODData(writer.Version))
			{
				if (!IsIndexBufferFirst(writer.Version))
				{
					IndexBuffer.Write(writer);
					writer.AlignStream();
				}
			}

			if (HasVertexData(writer.Version))
			{
				if (!IsOnlyVertexData(writer.Version))
				{
					if (MeshCompression != MeshCompression.Off)
					{
						Vertices.Write(writer);
					}
				}
			}
			else
			{
				Vertices.Write(writer);
			}

			if (HasSkin(writer.Version))
			{
				Skin.Write(writer);
			}
			if (HasBindPose(writer.Version))
			{
				if (!IsBindPoseFirst(writer.Version))
				{
					BindPose.Write(writer);
				}
			}

			if (HasVertexData(writer.Version))
			{
				if (IsOnlyVertexData(writer.Version))
				{
					VertexData.Write(writer);
				}
				else
				{
					if (MeshCompression == MeshCompression.Off)
					{
						VertexData.Write(writer);
					}
					else
					{
						UV.Write(writer);
						UV1.Write(writer);
						Tangents.Write(writer);
						Normals.Write(writer);
						Colors.Write(writer);
					}
				}
			}
			else
			{
				UV.Write(writer);
				if (HasUV1(writer.Version))
				{
					UV1.Write(writer);
				}
				if (HasTangentSpace(writer.Version))
				{
					TangentSpace.Write(writer);
				}
				else
				{
					Tangents.Write(writer);
					Normals.Write(writer);
				}
			}
			if (IsAlignVertex(writer.Version))
			{
				writer.AlignStream();
			}

			if (HasCompressedMesh(writer.Version))
			{
				CompressedMesh.Write(writer);
			}

			LocalAABB.Write(writer);
			if (!HasVertexData(writer.Version))
			{
				Colors.Write(writer);
			}
			if (HasCollisionTriangles(writer.Version))
			{
				CollisionTriangles.Write(writer);
				writer.Write(CollisionVertexCount);
			}
			if (HasMeshUsageFlags(writer.Version))
			{
				writer.Write(MeshUsageFlags);
			}

			if (HasCollision(writer.Version))
			{
				CollisionData.Write(writer);
			}
			if (HasMeshMetrics(writer.Version))
			{
				writer.Write(MeshMetrics[0]);
				writer.Write(MeshMetrics[1]);
			}
#if UNIVERSAL
			if (HasMeshOptimization(writer.Version, writer.Flags))
			{
				if (IsMeshOptimizationFlags(writer.Version))
				{
					writer.Write((int)MeshOptimizationFlags);
				}
				else
				{
					writer.Write(MeshOptimized);
				}
			}
#endif
			if (HasStreamData(writer.Version))
			{
				writer.AlignStream();
				StreamData.Write(writer);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (HasLODData(container.ExportVersion))
			{
				node.Add(LODDataName, LODData.ExportYAML(container));
			}
			else
			{
				if (HasUse16bitIndices(container.ExportVersion))
				{
					node.Add(Use16BitIndicesName, Use16BitIndices);
				}
				if (IsIndexBufferFirst(container.ExportVersion))
				{
					node.Add(IndexBufferName, IndexBuffer.ExportYAML());
				}
				node.Add(SubMeshesName, SubMeshes.ExportYAML(container));
			}

			if (HasBlendShapes(container.ExportVersion))
			{
				if (HasBlendChannels(container.ExportVersion))
				{
					node.Add(ShapesName, Shapes.ExportYAML(container));
				}
				else
				{
					node.Add(ShapesName, BlendShapes.ExportYAML(container));
					node.Add(ShapeVerticesName, ShapeVertices.ExportYAML(container));
				}
			}
			if (HasBindPose(container.ExportVersion))
			{
				if (IsBindPoseFirst(container.ExportVersion))
				{
					node.Add(BindPoseName, BindPose.ExportYAML(container));
				}
			}
			if (HasBoneNameHashes(container.ExportVersion))
			{
				node.Add(BoneNameHashesName, BoneNameHashes.ExportYAML(true));
				node.Add(RootBoneNameHashName, RootBoneNameHash);
			}
			if (HasBonesAABB(container.ExportVersion))
			{
				node.Add(BonesAABBName, BonesAABB.ExportYAML(container));
				node.Add(VariableBoneCountWeightsName, VariableBoneCountWeights.ExportYAML(container));
			}

			if (HasMeshCompression(container.ExportVersion))
			{
				node.Add(MeshCompressionName, (byte)MeshCompression);
			}
			if (HasStreamCompression(container.ExportVersion))
			{
				node.Add(StreamCompressionName, StreamCompression);
			}
			if (HasIsReadable(container.ExportVersion))
			{
				node.Add(IsReadableName, IsReadable);
				node.Add(KeepVerticesName, KeepVertices);
				node.Add(KeepIndicesName, KeepIndices);
			}

			if (HasIndexFormat(container.ExportVersion))
			{
				node.Add(IndexFormatName, (int)IndexFormat);
			}

			if (!HasLODData(container.ExportVersion))
			{
				if (!IsIndexBufferFirst(container.ExportVersion))
				{
					node.Add(IndexBufferName, IndexBuffer.ExportYAML());
				}
			}

			if (HasVertexData(container.ExportVersion))
			{
				if (!IsOnlyVertexData(container.ExportVersion))
				{
					if (MeshCompression != MeshCompression.Off)
					{
						node.Add(VerticesName, Vertices.ExportYAML(container));
					}
				}
			}
			else
			{
				node.Add(VerticesName, Vertices.ExportYAML(container));
			}

			if (HasSkin(container.ExportVersion))
			{
				node.Add(SkinName, Skin.ExportYAML(container));
			}
			if (HasBindPose(container.ExportVersion))
			{
				if (!IsBindPoseFirst(container.ExportVersion))
				{
					node.Add(BindPoseName, BindPose.ExportYAML(container));
				}
			}

			if (HasVertexData(container.ExportVersion))
			{
				if (IsOnlyVertexData(container.ExportVersion))
				{
					node.Add(VertexDataName, VertexData.ExportYAML(container));
				}
				else
				{
					if (MeshCompression == MeshCompression.Off)
					{
						node.Add(VertexDataName, VertexData.ExportYAML(container));
					}
					else
					{
						node.Add(UVName, UV.ExportYAML(container));
						node.Add(UV1Name, UV1.ExportYAML(container));
						node.Add(TangentsName, Tangents.ExportYAML(container));
						node.Add(NormalsName, Normals.ExportYAML(container));
						node.Add(ColorsName, Colors.ExportYAML(container));
					}
				}
			}
			else
			{
				node.Add(UVName, UV.ExportYAML(container));
				if (HasUV1(container.ExportVersion))
				{
					node.Add(UV1Name, UV1.ExportYAML(container));
				}
				if (HasTangentSpace(container.ExportVersion))
				{
					node.Add(TangentSpaceName, Tangents.ExportYAML(container));
				}
				else
				{
					node.Add(TangentsName, Tangents.ExportYAML(container));
					node.Add(NormalsName, Normals.ExportYAML(container));
				}
			}

			if (HasCompressedMesh(container.ExportVersion))
			{
				node.Add(CompressedMeshName, CompressedMesh.ExportYAML(container));
			}

			node.Add(LocalAABBName, LocalAABB.ExportYAML(container));
			if (!HasVertexData(container.ExportVersion))
			{
				node.Add(ColorsName, Colors.ExportYAML(container));
			}
			if (HasCollisionTriangles(container.ExportVersion))
			{
				node.Add(CollisionTrianglesName, CollisionTriangles.ExportYAML(true));
				node.Add(CollisionVertexCountName, CollisionVertexCount);
			}
			if (HasMeshUsageFlags(container.ExportVersion))
			{
				node.Add(MeshUsageFlagsName, MeshUsageFlags);
			}

			if (HasCollision(container.ExportVersion))
			{
				node.Add(BakedConvexCollisionMeshName, CollisionData.BakedConvexCollisionMesh.ExportYAML());
				node.Add(BakedTriangleCollisionMeshName, CollisionData.BakedTriangleCollisionMesh.ExportYAML());
			}
			if (HasMeshMetrics(container.ExportVersion))
			{
				node.Add(MeshMetricsName + "[0]", MeshMetrics[0]);
				node.Add(MeshMetricsName + "[1]", MeshMetrics[1]);
			}
			if (HasMeshOptimization(container.ExportVersion, container.ExportFlags))
			{
				if (IsMeshOptimizationFlags(container.ExportVersion))
				{
					node.Add(MeshOptimizationFlagsName, (int)MeshOptimizationFlags);
				}
				else
				{
					node.Add(MeshOptimizedName, MeshOptimized);
				}
			}
			if (HasStreamData(container.ExportVersion))
			{
				StreamingInfo streamData = new StreamingInfo(true);
				node.Add(StreamDataName, streamData.ExportYAML(container));
			}
			return node;
		}

		public byte[] GetChannelsData()
		{
			if (HasStreamData(File.Version) && StreamData.IsSet)
			{
				return StreamData.GetContent(File);
			}
			else
			{
				return VertexData.Data;
			}
		}

		public LOD[] LODData { get; set; }
		public uint Use16BitIndices
		{
			get => IndexFormat == IndexFormat.UInt16 ? 1U : 0U;
			set => IndexFormat = value == 0 ? IndexFormat.UInt32 : IndexFormat.UInt16;
		}
		public byte[] IndexBuffer { get; set; }
		public SubMesh[] SubMeshes { get; set; }
		/// <summary>
		/// Shapes - real name
		/// </summary>
		public BlendShape[] BlendShapes
		{
			get => Shapes.Shapes;
			set => Shapes.Shapes = value;
		}
		public BlendShapeVertex[] ShapeVertices
		{
			get => Shapes.Vertices;
			set => Shapes.Vertices = value;
		}
		public Matrix4x4f[] BindPose { get; set; }
		public uint[] BoneNameHashes { get; set; }
		public uint RootBoneNameHash { get; set; }
		public MinMaxAABB[] BonesAABB { get; set; }
		public MeshCompression MeshCompression { get; set; }
		public byte StreamCompression { get; set; }
		public bool IsReadable { get; set; }
		public bool KeepVertices { get; set; }
		public bool KeepIndices { get; set; }
		public IndexFormat IndexFormat { get; set; }
		public Vector3f[] Vertices { get; set; }
		public BoneWeights4[] Skin { get; set; }
		public Vector2f[] UV { get; set; }
		public Vector2f[] UV1 { get; set; }
		public Tangent[] TangentSpace { get; set; }
		public Vector4f[] Tangents { get; set; }
		public Vector3f[] Normals { get; set; }
		public ColorRGBA32[] Colors { get; set; }
		public uint[] CollisionTriangles { get; set; }
		public int CollisionVertexCount { get; set; }
		public int MeshUsageFlags { get; set; }
		public float[] MeshMetrics { get; set; }

#if UNIVERSAL
		public bool MeshOptimized
		{
			get => MeshOptimizationFlags == MeshOptimizationFlags.Everything;
			set => MeshOptimizationFlags = value ? MeshOptimizationFlags.Everything : MeshOptimizationFlags.PolygonOrder;
		}
		public MeshOptimizationFlags MeshOptimizationFlags { get; set; }
#else
		public bool MeshOptimized => true;
		public MeshOptimizationFlags MeshOptimizationFlags => MeshOptimizationFlags.Everything;
#endif

		public const string LODDataName = "m_LODData";
		public const string Use16BitIndicesName = "m_Use16BitIndices";
		public const string IndexBufferName = "m_IndexBuffer";
		public const string SubMeshesName = "m_SubMeshes";
		public const string ShapesName = "m_Shapes";
		public const string ShapeVerticesName = "m_ShapeVertices";
		public const string BindPoseName = "m_BindPose";
		public const string BoneNameHashesName = "m_BoneNameHashes";
		public const string RootBoneNameHashName = "m_RootBoneNameHash";
		public const string BonesAABBName = "m_BonesAABB";
		public const string VariableBoneCountWeightsName = "m_VariableBoneCountWeights";
		public const string MeshCompressionName = "m_MeshCompression";
		public const string StreamCompressionName = "m_StreamCompression";
		public const string IsReadableName = "m_IsReadable";
		public const string KeepVerticesName = "m_KeepVertices";
		public const string KeepIndicesName = "m_KeepIndices";
		public const string IndexFormatName = "m_IndexFormat";
		public const string VerticesName = "m_Vertices";
		public const string SkinName = "m_Skin";
		public const string UVName = "m_UV";
		public const string UV1Name = "m_UV1";
		public const string TangentSpaceName = "m_TangentSpace";
		public const string TangentsName = "m_Tangents";
		public const string NormalsName = "m_Normals";
		public const string ColorsName = "m_Colors";
		public const string VertexDataName = "m_VertexData";
		public const string CompressedMeshName = "m_CompressedMesh";
		public const string LocalAABBName = "m_LocalAABB";
		public const string CollisionTrianglesName = "m_CollisionTriangles";
		public const string CollisionVertexCountName = "m_CollisionVertexCount";
		public const string MeshUsageFlagsName = "m_MeshUsageFlags";
		public const string BakedConvexCollisionMeshName = "m_BakedConvexCollisionMesh";
		public const string BakedTriangleCollisionMeshName = "m_BakedTriangleCollisionMesh";
		public const string MeshMetricsName = "m_MeshMetrics";
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
	}
}
