using System;
using System.Collections;
using System.IO;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.Meshes;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.Shaders;
using uTinyRipper.Converters.Meshes;

namespace uTinyRipper.Converters
{
	public static class MeshConverter
	{
		public static Mesh Convert(IExportContainer container, Mesh origin)
		{
			Mesh instance = new Mesh(origin.AssetInfo);
			NamedObjectConverter.Convert(container, origin, instance);

			if (Mesh.HasBlendShapes(container.ExportVersion))
			{
				if (Mesh.HasBlendChannels(container.ExportVersion))
				{
					instance.Shapes = GetShapes(container, origin);
				}
				else
				{
					instance.BlendShapes = GetBlendShapes(container, origin);
					instance.ShapeVertices = GetShapeVertices(container, origin);
				}
			}
			if (Mesh.HasBindPose(container.ExportVersion))
			{
				instance.BindPose = GetBindPose(container, origin);
			}
			if (Mesh.HasBoneNameHashes(container.ExportVersion))
			{
				instance.BoneNameHashes = GetBoneNameHashes(container, origin);
				instance.RootBoneNameHash = GetRootBoneNameHash(container, origin);
			}
			if (Mesh.HasBonesAABB(container.ExportVersion))
			{
				instance.BonesAABB = GetBonesAABB(container, origin);
				instance.VariableBoneCountWeights = GetVariableBoneCountWeights(container, origin);
			}
			if (Mesh.HasMeshCompression(container.ExportVersion))
			{
				instance.MeshCompression = GetMeshCompression(container, origin);
			}
			if (Mesh.HasStreamCompression(container.ExportVersion))
			{
				instance.StreamCompression = GetStreamCompression(container, origin);
			}
			if (Mesh.HasIsReadable(container.ExportVersion))
			{
				instance.IsReadable = GetIsReadable(container, origin);
				instance.KeepVertices = GetKeepVertices(container, origin);
				instance.KeepIndices = GetKeepIndices(container, origin);
			}
			if (Mesh.HasSkin(container.ExportVersion))
			{
				instance.Skin = origin.Skin;
			}

			if (Mesh.HasVertexData(container.ExportVersion))
			{
				if (Mesh.IsOnlyVertexData(container.ExportVersion))
				{
					instance.VertexData = GetVertexData(container, origin);
				}
				else
				{
					if (instance.MeshCompression == MeshCompression.Off)
					{
						instance.VertexData = GetVertexData(container, origin);
					}
					else
					{
						instance.Vertices = origin.Vertices.ToArray();
						instance.UV = origin.UV.ToArray();
						instance.UV1 = origin.UV1.ToArray();
						instance.Tangents = origin.Tangents.ToArray();
						instance.Normals = origin.Normals.ToArray();
						instance.Colors = origin.Colors.ToArray();
					}
				}
			}
			else
			{
				instance.Vertices = origin.Vertices.ToArray();
				instance.UV = origin.UV.ToArray();
				if (Mesh.HasUV1(container.ExportVersion))
				{
					instance.UV1 = GetUV1(container, origin);
				}
				if (Mesh.HasTangentSpace(container.ExportVersion))
				{
					instance.TangentSpace = origin.TangentSpace.ToArray();
				}
				else
				{
					instance.Tangents = GetTangents(container, origin);
					instance.Normals = GetNormals(container, origin);
				}
				instance.Colors = origin.Colors.ToArray();
			}

			if (Mesh.HasCompressedMesh(container.ExportVersion))
			{
				instance.CompressedMesh = GetCompressedMesh(container, origin);
			}
			instance.LocalAABB = origin.LocalAABB;
			if (Mesh.HasCollisionTriangles(container.ExportVersion))
			{
				instance.CollisionTriangles = origin.CollisionTriangles.ToArray();
				instance.CollisionVertexCount = origin.CollisionVertexCount;
			}
			instance.MeshUsageFlags = origin.MeshUsageFlags;
			if (Mesh.HasCollision(container.ExportVersion))
			{
				instance.CollisionData = GetCollisionData(container, origin);
			}
			if (Mesh.HasMeshMetrics(container.ExportVersion))
			{
				instance.MeshMetrics = GetMeshMetrics(container, origin);
			}
#if UNIVERSAL
			instance.MeshOptimizationFlags = origin.MeshOptimizationFlags;
#endif
			instance.StreamData = GetStreamData(container, origin);

			if (Mesh.HasLODData(container.ExportVersion))
			{
				instance.LODData = origin.LODData.Select(t => t.Convert(container)).ToArray();
			}
			else
			{
				if (Mesh.HasUse16bitIndices(container.ExportVersion))
				{
					instance.Use16BitIndices = GetUse16bitIndices(container, origin);
				}
				else if (Mesh.HasIndexFormat(container.ExportVersion))
				{
					instance.IndexFormat = GetIndexFormat(container, origin);
				}
				instance.IndexBuffer = GetIndexBuffer(container, origin);
				// since this method uses instance, we need to call it last
				instance.SubMeshes = GetSubMeshes(container, origin, instance);
			}

			return instance;
		}

		private static uint GetUse16bitIndices(IExportContainer container, Mesh origin)
		{
			if (Mesh.HasUse16bitIndices(container.Version))
			{
				return origin.Use16BitIndices;
			}
			return 1;
		}

		private static BlendShape[] GetBlendShapes(IExportContainer container, Mesh origin)
		{
			return Mesh.HasBlendShapes(container.Version) ? origin.Shapes.Shapes : Array.Empty<BlendShape>();
		}

		private static BlendShapeVertex[] GetShapeVertices(IExportContainer container, Mesh origin)
		{
			return Mesh.HasBlendShapes(container.Version) ? origin.Shapes.Vertices : Array.Empty<BlendShapeVertex>();
		}

		private static BlendShapeData GetShapes(IExportContainer container, Mesh origin)
		{
			if (Mesh.HasBlendShapes(container.Version))
			{
				if (Mesh.HasBlendChannels(container.Version))
				{
					return origin.Shapes.Convert();
				}
				else
				{
					BlendShapeData instance = new BlendShapeData();
					instance.Vertices = origin.ShapeVertices.ToArray();
					instance.Shapes = origin.BlendShapes.ToArray();
					instance.Channels = BlendShapeConverter.GenerateBlendChannels(origin.BlendShapes);
					instance.FullWeights = BlendShapeConverter.GenerateFullWeights(origin.BlendShapes);
					return instance;
				}
			}
			else
			{
				return new BlendShapeData(container.ExportVersion);
			}
		}

		private static Matrix4x4f[] GetBindPose(IExportContainer container, Mesh origin)
		{
			return Mesh.HasBindPose(container.Version) ? origin.BindPose.ToArray() : Array.Empty<Matrix4x4f>();
		}

		private static uint[] GetBoneNameHashes(IExportContainer container, Mesh origin)
		{
			return Mesh.HasBoneNameHashes(container.Version) ? origin.BoneNameHashes.ToArray() : Array.Empty<uint>();
		}

		private static uint GetRootBoneNameHash(IExportContainer container, Mesh origin)
		{
			return Mesh.HasBoneNameHashes(container.Version) ? origin.RootBoneNameHash : 0;
		}

		private static MinMaxAABB[] GetBonesAABB(IExportContainer container, Mesh origin)
		{
			return Mesh.HasBonesAABB(container.Version) ? origin.BonesAABB.ToArray() : Array.Empty<MinMaxAABB>();
		}

		private static VariableBoneCountWeights GetVariableBoneCountWeights(IExportContainer container, Mesh origin)
		{
			return Mesh.HasBonesAABB(container.Version) ? origin.VariableBoneCountWeights : new VariableBoneCountWeights(true);
		}

		private static MeshCompression GetMeshCompression(IExportContainer container, Mesh origin)
		{
			return Mesh.HasMeshCompression(container.Version) ? origin.MeshCompression : MeshCompression.Off;
		}

		private static byte GetStreamCompression(IExportContainer container, Mesh origin)
		{
			return Mesh.HasStreamCompression(container.Version) ? origin.StreamCompression : (byte)0;
		}

		private static bool GetIsReadable(IExportContainer container, Mesh origin)
		{
			return Mesh.HasIsReadable(container.Version) ? origin.IsReadable : true;
		}

		private static bool GetKeepVertices(IExportContainer container, Mesh origin)
		{
			return Mesh.HasIsReadable(container.Version) ? origin.KeepVertices : true;
		}

		private static bool GetKeepIndices(IExportContainer container, Mesh origin)
		{
			return Mesh.HasIsReadable(container.Version) ? origin.KeepIndices : true;
		}

		private static IndexFormat GetIndexFormat(IExportContainer container, Mesh origin)
		{
			if (Mesh.HasIndexFormat(container.Version))
			{
				return origin.IndexFormat;
			}
			if (Mesh.HasUse16bitIndices(container.Version))
			{
				return origin.Use16BitIndices == 0 ? IndexFormat.UInt32 : IndexFormat.UInt16;
			}
			return IndexFormat.UInt16;
		}

		private static byte[] GetIndexBuffer(IExportContainer container, Mesh origin)
		{
			if (Mesh.HasLODData(container.Version))
			{
				return LODConverter.GenerateIndexBuffer(container, ref origin.LODData[0]);
			}
			else
			{
#warning TODO: convert MeshTopology.Deprecated indices to MeshTopology.Triangles
				if (container.Platform == container.ExportPlatform ||
					container.Platform != Platform.XBox360 && container.ExportPlatform != Platform.XBox360)
				{
					return origin.IndexBuffer.ToArray();
				}
				else
				{
					int size = GetUse16bitIndices(container, origin) == 0 ? 4 : 2;
					return origin.IndexBuffer.SwapBytes(size);
				}
			}
		}

		private static SubMesh[] GetSubMeshes(IExportContainer container, Mesh origin, Mesh instance)
		{
			if (Mesh.HasLODData(container.Version))
			{
				return LODConverter.GenerateSubMeshes(container, instance, ref origin.LODData[0]);
			}
			else
			{
				return SubMeshConverter.Convert(container, instance, origin.SubMeshes);
			}
		}

		private static VertexData GetVertexData(IExportContainer container, Mesh origin)
		{
			if (Mesh.HasVertexData(container.Version))
			{
				if (Mesh.IsOnlyVertexData(container.Version))
				{
					return VertexDataConverter.Convert(container, origin);
				}
				else
				{
					if (origin.MeshCompression == MeshCompression.Off)
					{
						return VertexDataConverter.Convert(container, origin);
					}
					else
					{
						return GenerateVertexData(container, origin);
					}
				}
			}
			else
			{
				return GenerateVertexData(container, origin);
			}
		}

		private static Vector2f[] GetUV1(IExportContainer container, Mesh origin)
		{
			return Mesh.HasUV1(container.Version) ? origin.UV1.ToArray() : Array.Empty<Vector2f>();
		}

		private static Vector4f[] GetTangents(IExportContainer container, Mesh origin)
		{
			return Mesh.HasTangentSpace(container.Version) ? TangentConverter.GenerateTangents(origin.TangentSpace) : origin.Tangents.ToArray();
		}

		private static Vector3f[] GetNormals(IExportContainer container, Mesh origin)
		{
			return Mesh.HasTangentSpace(container.Version) ? TangentConverter.GenerateNormals(origin.TangentSpace) : origin.Normals.ToArray();
		}

		private static CompressedMesh GetCompressedMesh(IExportContainer container, Mesh origin)
		{
			return Mesh.HasCompressedMesh(container.Version) ? origin.CompressedMesh.Convert(container) : new CompressedMesh(container.ExportVersion);
		}

		private static CollisionMeshData GetCollisionData(IExportContainer container, Mesh origin)
		{
			return Mesh.HasCollision(container.Version) ? origin.CollisionData.Convert(container) : new CollisionMeshData(container.ExportVersion);
		}

		private static float[] GetMeshMetrics(IExportContainer container, Mesh origin)
		{
			return Mesh.HasMeshMetrics(container.Version) ? origin.MeshMetrics.ToArray() : new float[] { 1.0f, 1.0f };
		}

		private static StreamingInfo GetStreamData(IExportContainer container, Mesh origin)
		{
			return Mesh.HasStreamData(container.Version) ? origin.StreamData : new StreamingInfo(container.ExportVersion);
		}

		private static VertexData GenerateVertexData(IExportContainer container, Mesh origin)
		{
			Vector3f[] normals = origin.Normals;
			Vector4f[] tangents = origin.Tangents;
			if (Mesh.HasTangentSpace(container.Version))
			{
				normals = TangentConverter.GenerateNormals(origin.TangentSpace);
				tangents = TangentConverter.GenerateTangents(origin.TangentSpace);
			}

			VertexData instance = new VertexData();
			bool hasVertices = origin.Vertices.Length > 0;
			bool hasNormals = normals.Length > 0;
			bool hasColors = origin.Colors.Length > 0;
			bool hasUV0 = origin.UV.Length > 0;
			bool hasUV1 = Mesh.HasUV1(container.Version) && origin.UV1.Length > 0;
			bool hasTangents = tangents.Length > 0;
			bool hasChannels = VertexData.HasChannels(container.ExportVersion);

			if (hasChannels)
			{
				int channelCount = ShaderChannelExtensions.GetChannelCount(container.ExportVersion);
				instance.Channels = new ChannelInfo[channelCount];
			}

			byte stride = 0;
			BitArray curChannels = new BitArray(32);
			if (hasVertices)
			{
				int index = ShaderChannel.Vertex.ToChannel(container.ExportVersion);
				curChannels.Set(index, true);
				if (hasChannels)
				{
					byte format = ShaderChannel.Vertex.GetVertexFormat(container.ExportVersion).ToFormat(container.ExportVersion);
					byte dimention = ShaderChannel.Vertex.GetDimention(container.ExportVersion);
					instance.Channels[index] = new ChannelInfo(0, stride, format, dimention);
				}
				stride += ShaderChannel.Vertex.GetStride(container.ExportVersion);
			}
			if (hasNormals)
			{
				int index = ShaderChannel.Normal.ToChannel(container.ExportVersion);
				curChannels.Set(index, true);
				if (hasChannels)
				{
					byte format = ShaderChannel.Normal.GetVertexFormat(container.ExportVersion).ToFormat(container.ExportVersion);
					byte dimention = ShaderChannel.Normal.GetDimention(container.ExportVersion);
					instance.Channels[index] = new ChannelInfo(0, stride, format, dimention);
				}
				stride += ShaderChannel.Normal.GetStride(container.ExportVersion);
			}
			if (hasColors)
			{
				int index = ShaderChannel.Color.ToChannel(container.ExportVersion);
				curChannels.Set(index, true);
				if (hasChannels)
				{
					byte format = ShaderChannel.Color.GetVertexFormat(container.ExportVersion).ToFormat(container.ExportVersion);
					byte dimention = ShaderChannel.Color.GetDimention(container.ExportVersion);
					instance.Channels[index] = new ChannelInfo(0, stride, format, dimention);
				}
				stride += ShaderChannel.Color.GetStride(container.ExportVersion);
			}
			if (hasUV0)
			{
				int index = ShaderChannel.UV0.ToChannel(container.ExportVersion);
				curChannels.Set(index, true);
				if (hasChannels)
				{
					byte format = ShaderChannel.UV0.GetVertexFormat(container.ExportVersion).ToFormat(container.ExportVersion);
					byte dimention = ShaderChannel.UV0.GetDimention(container.ExportVersion);
					instance.Channels[index] = new ChannelInfo(0, stride, format, dimention);
				}
				stride += ShaderChannel.UV0.GetStride(container.ExportVersion);
			}
			if (hasUV1)
			{
				int index = ShaderChannel.UV1.ToChannel(container.ExportVersion);
				curChannels.Set(index, true);
				if (hasChannels)
				{
					byte format = ShaderChannel.UV1.GetVertexFormat(container.ExportVersion).ToFormat(container.ExportVersion);
					byte dimention = ShaderChannel.UV1.GetDimention(container.ExportVersion);
					instance.Channels[index] = new ChannelInfo(0, stride, format, dimention);
				}
				stride += ShaderChannel.UV1.GetStride(container.ExportVersion);
			}
			if (hasTangents)
			{
				int index = ShaderChannel.Tangent.ToChannel(container.ExportVersion);
				curChannels.Set(index, true);
				if (hasChannels)
				{
					byte format = ShaderChannel.Tangent.GetVertexFormat(container.ExportVersion).ToFormat(container.ExportVersion);
					byte dimention = ShaderChannel.Tangent.GetDimention(container.ExportVersion);
					instance.Channels[index] = new ChannelInfo(0, stride, format, dimention);
				}
				stride += ShaderChannel.Tangent.GetStride(container.ExportVersion);
			}

			if (VertexData.HasCurrentChannels(container.ExportVersion))
			{
				instance.CurrentChannels = curChannels.ToUInt32();
			}
			instance.VertexCount = origin.Vertices.Length;
			if (VertexData.HasStreams(container.ExportVersion))
			{
				StreamInfo info = new StreamInfo(instance.CurrentChannels, 0, stride);
				if (VertexData.IsStreamStatic(container.ExportVersion))
				{
					instance.Streams = new StreamInfo[] { info, default, default, default, };
				}
				else
				{
					instance.Streams = new StreamInfo[] { info, };
				}
			}

			instance.Data = new byte[instance.VertexCount * stride];
			using (MemoryStream stream = new MemoryStream(instance.Data))
			{
				using (AssetWriter writer = new AssetWriter(stream, EndianType.LittleEndian, container.ExportLayout))
				{
					for (int i = 0; i < instance.VertexCount; i++)
					{
						if (hasVertices)
						{
							origin.Vertices[i].Write(writer);
						}
						if (hasNormals)
						{
							normals[i].Write(writer);
						}
						if (hasColors)
						{
							origin.Colors[i].Write(writer);
						}
						if (hasUV0)
						{
							origin.UV[i].Write(writer);
						}
						if (hasUV1)
						{
							origin.UV1[i].Write(writer);
						}
						if (hasTangents)
						{
							tangents[i].Write(writer);
						}
					}
				}
			}

			return instance;
		}
	}
}
