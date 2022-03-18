using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.Converters.Mesh;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.PackedBitVectors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Classes.Mesh
{
	/// <summary>
	/// LodMesh previously
	/// </summary>
	public sealed class Mesh : NamedObject, IMesh
	{
		public BlendShapeData Shapes { get; set; } = new BlendShapeData();
		public IVariableBoneCountWeights VariableBoneCountWeights { get; } = new VariableBoneCountWeights();
		public VertexData VertexData { get; set; } = new VertexData();
		public ICompressedMesh CompressedMesh { get; } = new CompressedMesh();
		public IAABB LocalAABB { get; } = new AABB();
		public byte[] BakedConvexCollisionMesh { get; set; } = Array.Empty<byte>();
		public byte[] BakedTriangleCollisionMesh { get; set; } = Array.Empty<byte>();
		public IStreamingInfo StreamData { get; } = new StreamingInfo();
		public uint Use16BitIndices
		{
			get => IndexFormat == IndexFormat.UInt16 ? 1U : 0U;
			set => IndexFormat = value == 0 ? IndexFormat.UInt32 : IndexFormat.UInt16;
		}

		#region IndexBuffer
		public byte[] RawIndexBuffer
		{
			get => m_RawIndexBuffer;
			set
			{
				m_RawIndexBuffer = value;
				ProcessedIndexBufferFromRaw();
			}
		}
		private byte[] m_RawIndexBuffer;
		public uint[] ProcessedIndexBuffer
		{
			get => m_ProcessedIndexBuffer;
			set
			{
				m_ProcessedIndexBuffer = value;
				RawIndexBufferFromProcessed();
			}
		}
		private uint[] m_ProcessedIndexBuffer;
		private ushort[] UintToUshort(uint[] array)
		{
			ushort[] result = new ushort[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				result[i] = (ushort)array[i];
			}
			return result;
		}
		private uint[] UshortToUint(ushort[] array)
		{
			uint[] result = new uint[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				result[i] = array[i];
			}
			return result;
		}
		private void ProcessedIndexBufferFromRaw()
		{
			if (Use16BitIndices != 0)
			{
				var outputBuffer = new ushort[m_RawIndexBuffer.Length / sizeof(ushort)];
				Buffer.BlockCopy(m_RawIndexBuffer, 0, outputBuffer, 0, m_RawIndexBuffer.Length);
				m_ProcessedIndexBuffer = UshortToUint(outputBuffer);
			}
			else
			{
				m_ProcessedIndexBuffer = new uint[m_RawIndexBuffer.Length / sizeof(uint)];
				Buffer.BlockCopy(m_RawIndexBuffer, 0, m_ProcessedIndexBuffer, 0, m_RawIndexBuffer.Length);
			}
		}
		private void RawIndexBufferFromProcessed()
		{
			if (Use16BitIndices != 0)
			{
				m_RawIndexBuffer = new byte[m_ProcessedIndexBuffer.Length * sizeof(ushort)];
				var inputBuffer = UintToUshort(m_ProcessedIndexBuffer);
				Buffer.BlockCopy(inputBuffer, 0, m_RawIndexBuffer, 0, m_RawIndexBuffer.Length);
			}
			else
			{
				m_RawIndexBuffer = new byte[m_ProcessedIndexBuffer.Length * sizeof(uint)];
				Buffer.BlockCopy(m_ProcessedIndexBuffer, 0, m_RawIndexBuffer, 0, m_RawIndexBuffer.Length);
			}
		}
		#endregion

		public SubMesh[] SubMeshes { get; set; }
		/// <summary>
		/// Shapes - real name
		/// </summary>
		public IMeshBlendShape[] BlendShapes
		{
			get => Shapes.Shapes;
			set => Shapes.Shapes = value;
		}
		public IBlendShapeVertex[] ShapeVertices
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
		public int VertexCount { get; private set; }
		public BoneWeights4[] Skin { get; set; }
		public Vector2f[] UV0 { get; set; }
		public Vector2f[] UV1 { get; set; }
		public Vector2f[] UV2 { get; set; }
		public Vector2f[] UV3 { get; set; }
		public Vector2f[] UV4 { get; set; }
		public Vector2f[] UV5 { get; set; }
		public Vector2f[] UV6 { get; set; }
		public Vector2f[] UV7 { get; set; }
		public Vector4f[] Tangents { get; set; }
		public Vector3f[] Normals { get; set; }
		public ColorRGBA32[] Colors { get; set; }
		public uint[] CollisionTriangles { get; set; }
		public int CollisionVertexCount { get; set; }
		public int MeshUsageFlags { get; set; }
		public float[] MeshMetrics { get; set; }
		public List<uint> Indices { get; set; } = new List<uint>();
		public List<List<uint>> Triangles { get; set; } = new List<List<uint>>();

		public Mesh(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
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
			if (version.IsGreaterEqual(4, 0, 0, UnityVersionType.Beta))
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
			if (version.IsGreaterEqual(2, 6, 0, UnityVersionType.Beta))
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

		#region Version Methods
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public static bool HasLODData(UnityVersion version) => version.IsLess(2, 0, 0);
		/// <summary>
		/// 2.0.0 to 3.5.0 exclusive
		/// </summary>
		public static bool HasUse16bitIndices(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasBlendChannels(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// Greater than 4.1.0a
		/// </summary>
		public static bool HasBlendShapes(UnityVersion version) => version.IsGreater(4, 1, 0, UnityVersionType.Alpha);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasBoneNameHashes(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasBonesAABB(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasMeshCompression(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 4.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasStreamCompression(UnityVersion version) => version.IsGreaterEqual(4) && version.IsLess(5);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasIsReadable(UnityVersion version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasIndexFormat(UnityVersion version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// Less than 2018.2
		/// </summary>
		public static bool HasSkin(UnityVersion version) => version.IsLess(2018, 2);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool HasBindPose(UnityVersion version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 1.6.0 and greater
		/// </summary>
		public static bool HasUV1(UnityVersion version) => version.IsGreaterEqual(1, 6);
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool HasTangentSpace(UnityVersion version) => version.IsLess(2, 6);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasVertexData(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 3.5.1 and greater
		/// </summary>
		public static bool IsOnlyVertexData(UnityVersion version) => version.IsGreaterEqual(3, 5, 1);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasCompressedMesh(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// Less than 3.5.0f1
		/// </summary>
		public static bool HasCollisionTriangles(UnityVersion version) => version.IsLessEqual(3, 5, 0, UnityVersionType.Beta);
		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool HasMeshUsageFlags(UnityVersion version) => version.IsGreaterEqual(2, 5);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasCollision(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasMeshMetrics(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasMeshOptimization(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool HasStreamData(UnityVersion version) => version.IsGreaterEqual(2018, 3);

		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		private static bool IsIndexBufferFirst(UnityVersion version) => version.IsLess(2, 6);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsBindPoseFirst(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlignVertex(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlignFlags(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// Less than 2017.3.1p1
		/// </summary>
		private static bool IsIndexFormatCondition(UnityVersion version) => version.IsLess(2017, 3, 1, UnityVersionType.Patch);
		/// <summary>
		/// 2019.1 and greater and Not Release
		/// </summary>
		private static bool IsMeshOptimizationFlags(UnityVersion version) => version.IsGreaterEqual(2019);
		#endregion

		public bool CheckAssetIntegrity()
		{
			if (HasStreamData(SerializedFile.Version))
			{
				if (VertexData.IsSet())
				{
					return StreamData.CheckIntegrity(SerializedFile);
				}
			}
			return true;
		}

		public string FindBlendShapeNameByCRC(uint crc)
		{
			if (HasBlendChannels(SerializedFile.Version))
			{
				return Shapes.FindShapeNameByCRC(crc);
			}
			else
			{
				foreach (MeshBlendShape blendShape in BlendShapes)
				{
					if (blendShape.IsCRCMatch(crc))
					{
						return blendShape.Name.String;
					}
				}
				return null;
			}
		}

		// TEMP: argument
		public bool Is16BitIndices(UnityVersion version)
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

		public override IUnityObjectBase ConvertLegacy(IExportContainer container)
		{
			return MeshConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasUse16bitIndices(reader.Version))
			{
				Use16BitIndices = reader.ReadUInt32();
			}
			SubMeshes = reader.ReadAssetArray<SubMesh>();

			if (HasBlendShapes(reader.Version))
			{
				if (HasBlendChannels(reader.Version))
				{
					Shapes.Read(reader);
				}
				else
				{
					BlendShapes = reader.ReadAssetArray<MeshBlendShape>();
					reader.AlignStream();
					ShapeVertices = reader.ReadAssetArray<BlendShapeVertex>();
				}
			}
			if (IsBindPoseFirst(reader.Version))
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

			MeshCompression = (MeshCompression)reader.ReadByte();

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
			reader.AlignStream();

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

			RawIndexBuffer = reader.ReadByteArray();
			reader.AlignStream();

			if (HasVertexData(reader.Version))
			{
				if (!IsOnlyVertexData(reader.Version)) //3.5.0 only
				{
					if (MeshCompression != MeshCompression.Off)
					{
						Vertices = reader.ReadAssetArray<Vector3f>();
					}
				}
			}
			else //lower than 3.5.0
			{
				Vertices = reader.ReadAssetArray<Vector3f>();
			}

			if (HasSkin(reader.Version))
			{
				Skin = reader.ReadAssetArray<BoneWeights4>();
			}
			if (!IsBindPoseFirst(reader.Version))
			{
				BindPose = reader.ReadAssetArray<Matrix4x4f>();
			}

			if (HasVertexData(reader.Version))
			{
				if (IsOnlyVertexData(reader.Version)) //3.5.1 and greater
				{
					VertexData.Read(reader);
				}
				else //3.5.0
				{
					if (MeshCompression == MeshCompression.Off)
					{
						VertexData.Read(reader);
					}
					else
					{
						UV0 = reader.ReadAssetArray<Vector2f>();
						UV1 = reader.ReadAssetArray<Vector2f>();
						Tangents = reader.ReadAssetArray<Vector4f>();
						Normals = reader.ReadAssetArray<Vector3f>();
						Colors = reader.ReadAssetArray<ColorRGBA32>();
					}
				}
			}
			else //less than 3.5.0
			{
				UV0 = reader.ReadAssetArray<Vector2f>();
				UV1 = reader.ReadAssetArray<Vector2f>();
				Tangents = reader.ReadAssetArray<Vector4f>();
				Normals = reader.ReadAssetArray<Vector3f>();
			}
			reader.AlignStream();

			CompressedMesh.Read(reader);

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
			MeshUsageFlags = reader.ReadInt32();

			if (HasCollision(reader.Version))
			{
				BakedConvexCollisionMesh = reader.ReadByteArray();
				reader.AlignStream();
				BakedTriangleCollisionMesh = reader.ReadByteArray();
				reader.AlignStream();
			}
			if (HasMeshMetrics(reader.Version))
			{
				MeshMetrics = new float[2];
				MeshMetrics[0] = reader.ReadSingle();
				MeshMetrics[1] = reader.ReadSingle();
			}
			if (HasStreamData(reader.Version))
			{
				reader.AlignStream();
				StreamData.Read(reader);
			}

			ProcessData(AssetUnityVersion);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasUse16bitIndices(writer.Version))
			{
				writer.Write(Use16BitIndices);
			}
			
			SubMeshes.Write(writer);

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
			if (IsBindPoseFirst(writer.Version))
			{
				BindPose.Write(writer);
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

			writer.Write((byte)MeshCompression);

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
			writer.AlignStream();

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

			RawIndexBuffer.Write(writer);
			writer.AlignStream();

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
			if (!IsBindPoseFirst(writer.Version))
			{
				BindPose.Write(writer);
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
						UV0.Write(writer);
						UV1.Write(writer);
						Tangents.Write(writer);
						Normals.Write(writer);
						Colors.Write(writer);
					}
				}
			}
			else
			{
				UV0.Write(writer);
				UV1.Write(writer);
				Tangents.Write(writer);
				Normals.Write(writer);
			}
			writer.AlignStream();

			CompressedMesh.Write(writer);

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
			writer.Write(MeshUsageFlags);

			if (HasCollision(writer.Version))
			{
				writer.Write(BakedConvexCollisionMesh);
				writer.AlignStream();
				writer.Write(BakedTriangleCollisionMesh);
				writer.AlignStream();
			}
			if (HasMeshMetrics(writer.Version))
			{
				writer.Write(MeshMetrics[0]);
				writer.Write(MeshMetrics[1]);
			}
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
			if (HasUse16bitIndices(container.ExportVersion))
			{
				node.Add(Use16BitIndicesName, Use16BitIndices);
			}
			node.Add(SubMeshesName, SubMeshes.ExportYAML(container));

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
			if (IsBindPoseFirst(container.ExportVersion))
			{
				node.Add(BindPoseName, BindPose.ExportYAML(container));
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

			node.Add(MeshCompressionName, (byte)MeshCompression);

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

			node.Add(IndexBufferName, RawIndexBuffer.ExportYAML());

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
			if (!IsBindPoseFirst(container.ExportVersion))
			{
				node.Add(BindPoseName, BindPose.ExportYAML(container));
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
						node.Add(UVName, UV0.ExportYAML(container));
						node.Add(UV1Name, UV1.ExportYAML(container));
						node.Add(TangentsName, Tangents.ExportYAML(container));
						node.Add(NormalsName, Normals.ExportYAML(container));
						node.Add(ColorsName, Colors.ExportYAML(container));
					}
				}
			}
			else
			{
				node.Add(UVName, UV0.ExportYAML(container));
				node.Add(UV1Name, UV1.ExportYAML(container));
				node.Add(TangentsName, Tangents.ExportYAML(container));
				node.Add(NormalsName, Normals.ExportYAML(container));
			}

			node.Add(CompressedMeshName, CompressedMesh.ExportYAML(container));

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
			node.Add(MeshUsageFlagsName, MeshUsageFlags);

			if (HasCollision(container.ExportVersion))
			{
				node.Add(BakedConvexCollisionMeshName, BakedConvexCollisionMesh.ExportYAML());
				node.Add(BakedTriangleCollisionMeshName, BakedTriangleCollisionMesh.ExportYAML());
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
				StreamingInfo streamData = new StreamingInfo();
				node.Add(StreamDataName, streamData.ExportYAML(container));
			}
			return node;
		}

		public byte[] GetChannelsData()
		{
			if (HasStreamData(SerializedFile.Version) && StreamData.IsSet())
			{
				return StreamData.GetContent(SerializedFile);
			}
			else
			{
				return VertexData.Data;
			}
		}

		private void ProcessData(UnityVersion version)
		{
			if (!string.IsNullOrEmpty(StreamData.Path))
			{
				if (VertexData.VertexCount > 0)
				{
					VertexData.Data = StreamData.GetContent(this.AssetInfo.File);
				}
			}
			if (version.IsGreaterEqual(3, 5)) //3.5 and up
			{
				ReadVertexData(version);
			}

			if (version.IsGreaterEqual(2, 6)) //2.6.0 and later
			{
				DecompressCompressedMesh(version);
			}

			ReadTriangles(version);

			if (Vertices == null)
			{
				Logger.Warning(LogCategory.Import, $"Null Vertices for {Name}");
			}
			if (UV0 == null)
			{
				Logger.Verbose(LogCategory.Import, $"Null UV0 for {Name}");
			}
			if (Normals == null)
			{
				Logger.Verbose(LogCategory.Import, $"Null Normals for {Name}");
			}
		}

		private void ReadVertexData(UnityVersion version)
		{
			VertexCount = (int)VertexData.VertexCount;

			for (var chn = 0; chn < VertexData.m_Channels.Length; chn++)
			{
				var m_Channel = VertexData.m_Channels[chn];
				if (m_Channel.GetDataDimension() > 0)
				{
					var m_Stream = VertexData.m_Streams[m_Channel.Stream];
					var channelMask = new BitArray(BitConverter.GetBytes(m_Stream.ChannelMask));
					if (channelMask.Get(chn))
					{
						if (version.IsLess(2018) && chn == 2 && m_Channel.Format == 2) //kShaderChannelColor && kChannelFormatColor
						{
							m_Channel.SetDataDimension(4);
						}

						var vertexFormat = MeshHelper.ToVertexFormat(m_Channel.Format, version);
						var componentByteSize = (int)MeshHelper.GetFormatSize(vertexFormat);
						var componentBytes = new byte[VertexCount * m_Channel.GetDataDimension() * componentByteSize];
						for (int v = 0; v < VertexCount; v++)
						{
							var vertexOffset = (int)m_Stream.Offset + m_Channel.Offset + (int)m_Stream.Stride * v;
							for (int d = 0; d < m_Channel.GetDataDimension(); d++)
							{
								var componentOffset = vertexOffset + componentByteSize * d;
								Buffer.BlockCopy(VertexData.Data, componentOffset, componentBytes, componentByteSize * (v * m_Channel.GetDataDimension() + d), componentByteSize);
							}
						}

						if (this.EndianType == EndianType.BigEndian && componentByteSize > 1) //swap bytes
						{
							for (var i = 0; i < componentBytes.Length / componentByteSize; i++)
							{
								var buff = new byte[componentByteSize];
								Buffer.BlockCopy(componentBytes, i * componentByteSize, buff, 0, componentByteSize);
								buff = buff.Reverse().ToArray();
								Buffer.BlockCopy(buff, 0, componentBytes, i * componentByteSize, componentByteSize);
							}
						}

						int[] componentsIntArray = null;
						float[] componentsFloatArray = null;
						if (MeshHelper.IsIntFormat(vertexFormat))
							componentsIntArray = MeshHelper.BytesToIntArray(componentBytes, vertexFormat);
						else
							componentsFloatArray = MeshHelper.BytesToFloatArray(componentBytes, vertexFormat);

						if (version.IsGreaterEqual(2018))
						{
							switch (chn)
							{
								case 0: //kShaderChannelVertex
									Vertices = MeshHelper.FloatArrayToVector3(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 1: //kShaderChannelNormal
									Normals = MeshHelper.FloatArrayToVector3(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 2: //kShaderChannelTangent
									Tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 3: //kShaderChannelColor
									Colors = MeshHelper.FloatArrayToColorRGBA32(componentsFloatArray);
									break;
								case 4: //kShaderChannelTexCoord0
									UV0 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 5: //kShaderChannelTexCoord1
									UV1 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 6: //kShaderChannelTexCoord2
									UV2 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 7: //kShaderChannelTexCoord3
									UV3 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 8: //kShaderChannelTexCoord4
									UV4 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 9: //kShaderChannelTexCoord5
									UV5 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 10: //kShaderChannelTexCoord6
									UV6 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 11: //kShaderChannelTexCoord7
									UV7 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								//2018.2 and up
								case 12: //kShaderChannelBlendWeight
									if (Skin == null)
									{
										InitMSkin();
									}
									for (int i = 0; i < VertexCount; i++)
									{
										for (int j = 0; j < m_Channel.GetDataDimension(); j++)
										{
											Skin[i].Weights[j] = componentsFloatArray[i * m_Channel.GetDataDimension() + j];
										}
									}
									break;
								case 13: //kShaderChannelBlendIndices
									if (Skin == null)
									{
										InitMSkin();
									}
									for (int i = 0; i < VertexCount; i++)
									{
										for (int j = 0; j < m_Channel.GetDataDimension(); j++)
										{
											Skin[i].BoneIndices[j] = componentsIntArray[i * m_Channel.GetDataDimension() + j];
										}
									}
									break;
							}
						}
						else
						{
							switch (chn)
							{
								case 0: //kShaderChannelVertex
									Vertices = MeshHelper.FloatArrayToVector3(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 1: //kShaderChannelNormal
									Normals = MeshHelper.FloatArrayToVector3(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 2: //kShaderChannelColor
									Colors = MeshHelper.FloatArrayToColorRGBA32(componentsFloatArray);
									break;
								case 3: //kShaderChannelTexCoord0
									UV0 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 4: //kShaderChannelTexCoord1
									UV1 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 5:
									if (version.IsGreaterEqual(5)) //kShaderChannelTexCoord2
									{
										UV2 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									}
									else //kShaderChannelTangent
									{
										Tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, m_Channel.GetDataDimension());
									}
									break;
								case 6: //kShaderChannelTexCoord3
									UV3 = MeshHelper.FloatArrayToVector2(componentsFloatArray, m_Channel.GetDataDimension());
									break;
								case 7: //kShaderChannelTangent
									Tangents = MeshHelper.FloatArrayToVector4(componentsFloatArray, m_Channel.GetDataDimension());
									break;
							}
						}
					}
				}
			}
		}

		private void DecompressCompressedMesh(UnityVersion version)
		{
			//Vertex
			if (CompressedMesh.Vertices.NumItems > 0)
			{
				VertexCount = (int)CompressedMesh.Vertices.NumItems / 3;
				var vertices = CompressedMesh.Vertices.UnpackFloats(3, 3 * 4);
				Vertices = MeshHelper.FloatArrayToVector3(vertices);
			}
			//UV
			if (CompressedMesh.UV.NumItems > 0)
			{
				var m_UVInfo = CompressedMesh.UVInfo;
				if (m_UVInfo != 0)
				{
					const int kInfoBitsPerUV = 4;
					const int kUVDimensionMask = 3;
					const int kUVChannelExists = 4;
					const int kMaxTexCoordShaderChannels = 8;

					int uvSrcOffset = 0;
					for (int uv = 0; uv < kMaxTexCoordShaderChannels; uv++)
					{
						var texCoordBits = m_UVInfo >> (uv * kInfoBitsPerUV);
						texCoordBits &= (1u << kInfoBitsPerUV) - 1u;
						if ((texCoordBits & kUVChannelExists) != 0)
						{
							var uvDim = 1 + (int)(texCoordBits & kUVDimensionMask);
							var m_UV = CompressedMesh.UV.UnpackFloats(uvDim, uvDim * 4, uvSrcOffset, VertexCount);
							SetUV(uv, m_UV);
							uvSrcOffset += uvDim * VertexCount;
						}
					}
				}
				else
				{
					UV0 = MeshHelper.FloatArrayToVector2(CompressedMesh.UV.UnpackFloats(2, 2 * 4, 0, VertexCount));
					if (CompressedMesh.UV.NumItems >= VertexCount * 4)
					{
						UV1 = MeshHelper.FloatArrayToVector2(CompressedMesh.UV.UnpackFloats(2, 2 * 4, VertexCount * 2, VertexCount));
					}
				}
			}
			//BindPose
			if (version.IsLess(5))
			{
				if (CompressedMesh.BindPoses.NumItems > 0)
				{
					BindPose = new Matrix4x4f[CompressedMesh.BindPoses.NumItems / 16];
					var m_BindPoses_Unpacked = CompressedMesh.BindPoses.UnpackFloats(16, 4 * 16);
					var buffer = new float[16];
					for (int i = 0; i < BindPose.Length; i++)
					{
						Array.Copy(m_BindPoses_Unpacked, i * 16, buffer, 0, 16);
						BindPose[i] = new Matrix4x4f(buffer);
					}
				}
			}
			//Normal
			if (CompressedMesh.Normals.NumItems > 0)
			{
				var normalData = CompressedMesh.Normals.UnpackFloats(2, 4 * 2);
				var signs = CompressedMesh.NormalSigns.UnpackInts();
				Normals = new Vector3f[CompressedMesh.Normals.NumItems / 2];
				for (int i = 0; i < CompressedMesh.Normals.NumItems / 2; ++i)
				{
					var x = normalData[i * 2 + 0];
					var y = normalData[i * 2 + 1];
					var zsqr = 1 - x * x - y * y;
					float z;
					if (zsqr >= 0f)
						z = (float)System.Math.Sqrt(zsqr);
					else
					{
						z = 0;
						var normal = new Vector3f(x, y, z);
						normal.Normalize();
						x = normal.X;
						y = normal.Y;
						z = normal.Z;
					}
					if (signs[i] == 0)
						z = -z;
					Normals[i] = new Vector3f(x, y, z);
				}
			}
			//Tangent
			if (CompressedMesh.Tangents.NumItems > 0)
			{
				var tangentData = CompressedMesh.Tangents.UnpackFloats(2, 4 * 2);
				var signs = CompressedMesh.TangentSigns.UnpackInts();
				Tangents = new Vector4f[CompressedMesh.Tangents.NumItems / 2];
				for (int i = 0; i < CompressedMesh.Tangents.NumItems / 2; ++i)
				{
					var x = tangentData[i * 2 + 0];
					var y = tangentData[i * 2 + 1];
					var zsqr = 1 - x * x - y * y;
					float z;
					if (zsqr >= 0f)
						z = (float)System.Math.Sqrt(zsqr);
					else
					{
						z = 0;
						var vector3f = new Vector3f(x, y, z);
						vector3f.Normalize();
						x = vector3f.X;
						y = vector3f.Y;
						z = vector3f.Z;
					}
					if (signs[i * 2 + 0] == 0)
						z = -z;
					var w = signs[i * 2 + 1] > 0 ? 1.0f : -1.0f;
					Tangents[i] = new Vector4f(x, y, z, w);
				}
			}
			//FloatColor
			if (version.IsGreaterEqual(5))
			{
				if (CompressedMesh.FloatColors.NumItems > 0)
				{
					Colors = MeshHelper.FloatArrayToColorRGBA32(CompressedMesh.FloatColors.UnpackFloats(1, 4));
				}
			}
			//Skin
			if (CompressedMesh.Weights.NumItems > 0)
			{
				var weights = CompressedMesh.Weights.UnpackInts();
				var boneIndices = CompressedMesh.BoneIndices.UnpackInts();

				InitMSkin();

				int bonePos = 0;
				int boneIndexPos = 0;
				int j = 0;
				int sum = 0;

				for (int i = 0; i < CompressedMesh.Weights.NumItems; i++)
				{
					//read bone index and weight.
					Skin[bonePos].Weights[j] = weights[i] / 31.0f;
					Skin[bonePos].BoneIndices[j] = boneIndices[boneIndexPos++];
					j++;
					sum += weights[i];

					//the weights add up to one. fill the rest for this vertex with zero, and continue with next one.
					if (sum >= 31)
					{
						for (; j < 4; j++)
						{
							Skin[bonePos].Weights[j] = 0;
							Skin[bonePos].BoneIndices[j] = 0;
						}
						bonePos++;
						j = 0;
						sum = 0;
					}
					//we read three weights, but they don't add up to one. calculate the fourth one, and read
					//missing bone index. continue with next vertex.
					else if (j == 3)
					{
						Skin[bonePos].Weights[j] = (31 - sum) / 31.0f;
						Skin[bonePos].BoneIndices[j] = boneIndices[boneIndexPos++];
						bonePos++;
						j = 0;
						sum = 0;
					}
				}
			}
			//IndexBuffer
			if (CompressedMesh.Triangles.NumItems > 0)
			{
				ProcessedIndexBuffer = Array.ConvertAll(CompressedMesh.Triangles.UnpackInts(), x => (uint)x);
			}
			//Color
			if (CompressedMesh.Colors.NumItems > 0)
			{
				CompressedMesh.Colors.NumItems *= 4;
				CompressedMesh.Colors.BitSize /= 4;
				var tempColors = CompressedMesh.Colors.UnpackInts();
				Colors = new ColorRGBA32[CompressedMesh.Colors.NumItems / 4];
				for (int v = 0; v < CompressedMesh.Colors.NumItems / 4; v++)
				{
					Colors[v] = new ColorRGBA32((byte)tempColors[4 * v], (byte)tempColors[4 * v + 1], (byte)tempColors[4 * v + 2], (byte)tempColors[4 * v + 3]);
				}
				CompressedMesh.Colors.NumItems /= 4;
				CompressedMesh.Colors.BitSize *= 4;
			}
		}

		private void ReadTriangles(UnityVersion version)
		{
			foreach (var iter in SubMeshes)
			{
				var m_SubMesh = iter;
				var m_Triangles = new List<uint>();
				Triangles.Add(m_Triangles);
				var firstIndex = m_SubMesh.FirstByte / 2;
				if (Use16BitIndices == 0)
				{
					firstIndex /= 2;
				}
				var indexCount = m_SubMesh.IndexCount;
				var topology = m_SubMesh.Topology;
				if (topology == MeshTopology.Triangles)
				{
					for (int i = 0; i < indexCount; i += 3)
					{
						Indices.Add(ProcessedIndexBuffer[firstIndex + i]);
						Indices.Add(ProcessedIndexBuffer[firstIndex + i + 1]);
						Indices.Add(ProcessedIndexBuffer[firstIndex + i + 2]);
						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + i]);
						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + i + 1]);
						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + i + 2]);
					}
				}
				else if (version.IsLess(4) || topology == MeshTopology.TriangleStrip)
				{
					// de-stripify :
					uint triIndex = 0;
					for (int i = 0; i < indexCount - 2; i++)
					{
						var a = ProcessedIndexBuffer[firstIndex + i];
						var b = ProcessedIndexBuffer[firstIndex + i + 1];
						var c = ProcessedIndexBuffer[firstIndex + i + 2];

						// skip degenerates
						if (a == b || a == c || b == c)
							continue;

						// do the winding flip-flop of strips :
						if ((i & 1) == 1)
						{
							Indices.Add(b);
							Indices.Add(a);
							m_Triangles.Add(b);
							m_Triangles.Add(a);
						}
						else
						{
							Indices.Add(a);
							Indices.Add(b);
							m_Triangles.Add(a);
							m_Triangles.Add(b);
						}
						Indices.Add(c);
						m_Triangles.Add(c);
						triIndex += 3;
					}
					//fix indexCount
					m_SubMesh.IndexCount = triIndex;
				}
				else if (topology == MeshTopology.Quads)
				{
					for (int q = 0; q < indexCount; q += 4)
					{
						Indices.Add(ProcessedIndexBuffer[firstIndex + q]);
						Indices.Add(ProcessedIndexBuffer[firstIndex + q + 1]);
						Indices.Add(ProcessedIndexBuffer[firstIndex + q + 2]);
						Indices.Add(ProcessedIndexBuffer[firstIndex + q]);
						Indices.Add(ProcessedIndexBuffer[firstIndex + q + 2]);
						Indices.Add(ProcessedIndexBuffer[firstIndex + q + 3]);

						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + q]);
						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + q + 1]);
						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + q + 2]);
						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + q]);
						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + q + 2]);
						m_Triangles.Add(ProcessedIndexBuffer[firstIndex + q + 3]);
					}
					//fix indexCount
					m_SubMesh.IndexCount = indexCount / 2 * 3;
				}
				else
				{
					//throw new NotSupportedException("Failed getting triangles. Submesh topology is lines or points.");
				}
			}
		}

		private void InitMSkin()
		{
			Skin = new BoneWeights4[VertexCount];
			for (int i = 0; i < VertexCount; i++)
			{
				Skin[i] = new BoneWeights4();
			}
		}

		private void SetUV(int uv, float[] m_UV) => SetUV(uv, MeshHelper.FloatArrayToVector2(m_UV));
		private void SetUV(int uv, Vector2f[] m_UV)
		{
			switch (uv)
			{
				case 0:
					UV0 = m_UV;
					break;
				case 1:
					UV1 = m_UV;
					break;
				case 2:
					UV2 = m_UV;
					break;
				case 3:
					UV3 = m_UV;
					break;
				case 4:
					UV4 = m_UV;
					break;
				case 5:
					UV5 = m_UV;
					break;
				case 6:
					UV6 = m_UV;
					break;
				case 7:
					UV7 = m_UV;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public Vector2f[] GetUV(int uv)
		{
			return uv switch
			{
				0 => UV0,
				1 => UV1,
				2 => UV2,
				3 => UV3,
				4 => UV4,
				5 => UV5,
				6 => UV6,
				7 => UV7,
				_ => throw new ArgumentOutOfRangeException(),
			};
		}

		public bool MeshOptimized
		{
			get => MeshOptimizationFlags == MeshOptimizationFlags.Everything;
			set => MeshOptimizationFlags = value ? MeshOptimizationFlags.Everything : MeshOptimizationFlags.PolygonOrder;
		}
		public MeshOptimizationFlags MeshOptimizationFlags { get; set; } = MeshOptimizationFlags.Everything;

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
	}
}
