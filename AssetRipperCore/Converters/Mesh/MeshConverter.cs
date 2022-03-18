using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.Classes.Shader.Enums.ShaderChannel;
using AssetRipper.Core.Classes.Shader.Enums.VertexFormat;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetRipper.Core.Converters.Mesh
{
	public static class MeshConverter
	{
		public static Classes.Mesh.Mesh Convert(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			Classes.Mesh.Mesh instance = new Classes.Mesh.Mesh(origin.AssetInfo);
			NamedObjectConverter.Convert(container, origin, instance);

			if (Classes.Mesh.Mesh.HasBlendShapes(container.ExportVersion))
			{
				if (Classes.Mesh.Mesh.HasBlendChannels(container.ExportVersion))
				{
					instance.Shapes = GetShapes(container, origin);
				}
				else
				{
					instance.BlendShapes = GetBlendShapes(container, origin);
					instance.ShapeVertices = GetShapeVertices(container, origin);
				}
			}
			if (Classes.Mesh.Mesh.HasBindPose(container.ExportVersion))
			{
				instance.BindPose = GetBindPose(container, origin);
			}
			if (Classes.Mesh.Mesh.HasBoneNameHashes(container.ExportVersion))
			{
				instance.BoneNameHashes = GetBoneNameHashes(container, origin);
				instance.RootBoneNameHash = GetRootBoneNameHash(container, origin);
			}
			if (Classes.Mesh.Mesh.HasBonesAABB(container.ExportVersion))
			{
				instance.BonesAABB = GetBonesAABB(container, origin);
			}
			instance.VariableBoneCountWeights.CopyValues(origin.VariableBoneCountWeights ?? new VariableBoneCountWeights());
			if (Classes.Mesh.Mesh.HasMeshCompression(container.ExportVersion))
			{
				instance.MeshCompression = GetMeshCompression(container, origin);
			}
			if (Classes.Mesh.Mesh.HasStreamCompression(container.ExportVersion))
			{
				instance.StreamCompression = GetStreamCompression(container, origin);
			}
			if (Classes.Mesh.Mesh.HasIsReadable(container.ExportVersion))
			{
				instance.IsReadable = GetIsReadable(container, origin);
				instance.KeepVertices = GetKeepVertices(container, origin);
				instance.KeepIndices = GetKeepIndices(container, origin);
			}
			if (Classes.Mesh.Mesh.HasSkin(container.ExportVersion))
			{
				instance.Skin = origin.Skin;
			}

			instance.Vertices = origin.Vertices?.ToArray();
			instance.UV0 = origin.UV0?.ToArray();
			instance.UV1 = origin.UV1?.ToArray();
			instance.UV2 = origin.UV2?.ToArray();
			instance.UV3 = origin.UV3?.ToArray();
			instance.UV4 = origin.UV4?.ToArray();
			instance.UV5 = origin.UV5?.ToArray();
			instance.UV6 = origin.UV6?.ToArray();
			instance.UV7 = origin.UV7?.ToArray();
			instance.Tangents = origin.Tangents?.ToArray();
			instance.Normals = origin.Normals?.ToArray();
			instance.Colors = origin.Colors?.ToArray();
			instance.Indices = new List<uint>(origin.Indices.ToArray());
			instance.Triangles = new List<List<uint>>(origin.Triangles.ConvertAll(x => x.ToArray()).ConvertAll(y => new List<uint>(y)).ToArray());

			if (Classes.Mesh.Mesh.HasVertexData(container.ExportVersion))
			{
				if (Classes.Mesh.Mesh.IsOnlyVertexData(container.ExportVersion))
				{
					instance.VertexData = GetVertexData(container, origin);
				}
				else if (instance.MeshCompression == MeshCompression.Off)
				{
					instance.VertexData = GetVertexData(container, origin);
				}
			}
			else
			{
				instance.UV1 = origin.UV1.ToArray();
				instance.Tangents = origin.Tangents.ToArray();
				instance.Normals = origin.Normals.ToArray();
			}

			instance.CompressedMesh.CopyValues(origin.CompressedMesh);
			instance.LocalAABB.CopyValuesFrom(origin.LocalAABB);
			if (Classes.Mesh.Mesh.HasCollisionTriangles(container.ExportVersion))
			{
				instance.CollisionTriangles = origin.CollisionTriangles.ToArray();
				instance.CollisionVertexCount = origin.CollisionVertexCount;
			}
			instance.MeshUsageFlags = origin.MeshUsageFlags;
			if (Classes.Mesh.Mesh.HasCollision(container.ExportVersion))
			{
				instance.BakedConvexCollisionMesh = origin.BakedConvexCollisionMesh?.ToArray() ?? Array.Empty<byte>();
				instance.BakedTriangleCollisionMesh = origin.BakedTriangleCollisionMesh?.ToArray() ?? Array.Empty<byte>();
			}
			if (Classes.Mesh.Mesh.HasMeshMetrics(container.ExportVersion))
			{
				instance.MeshMetrics = GetMeshMetrics(container, origin);
			}

			instance.StreamData.CopyValues(origin.StreamData ?? new StreamingInfo());

			if (Classes.Mesh.Mesh.HasUse16bitIndices(container.ExportVersion))
			{
				instance.Use16BitIndices = GetUse16bitIndices(container, origin);
			}
			else if (Classes.Mesh.Mesh.HasIndexFormat(container.ExportVersion))
			{
				instance.IndexFormat = GetIndexFormat(container, origin);
			}
			instance.RawIndexBuffer = GetIndexBuffer(container, origin);
			// since this method uses instance, we need to call it last
			instance.SubMeshes = SubMeshConverter.Convert(container, instance, origin.SubMeshes);

			return instance;
		}

		private static uint GetUse16bitIndices(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			if (Classes.Mesh.Mesh.HasUse16bitIndices(container.Version))
			{
				return origin.Use16BitIndices;
			}
			return 1;
		}

		private static IMeshBlendShape[] GetBlendShapes(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasBlendShapes(container.Version) ? origin.Shapes.Shapes : Array.Empty<MeshBlendShape>();
		}

		private static IBlendShapeVertex[] GetShapeVertices(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasBlendShapes(container.Version) ? origin.Shapes.Vertices : Array.Empty<BlendShapeVertex>();
		}

		private static BlendShapeData GetShapes(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			if (Classes.Mesh.Mesh.HasBlendShapes(container.Version))
			{
				if (Classes.Mesh.Mesh.HasBlendChannels(container.Version))
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
				return new BlendShapeData();
			}
		}

		private static Matrix4x4f[] GetBindPose(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasBindPose(container.Version) ? origin.BindPose.ToArray() : Array.Empty<Matrix4x4f>();
		}

		private static uint[] GetBoneNameHashes(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasBoneNameHashes(container.Version) ? origin.BoneNameHashes.ToArray() : Array.Empty<uint>();
		}

		private static uint GetRootBoneNameHash(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasBoneNameHashes(container.Version) ? origin.RootBoneNameHash : 0;
		}

		private static MinMaxAABB[] GetBonesAABB(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasBonesAABB(container.Version) ? origin.BonesAABB.ToArray() : Array.Empty<MinMaxAABB>();
		}

		private static MeshCompression GetMeshCompression(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasMeshCompression(container.Version) ? origin.MeshCompression : MeshCompression.Off;
		}

		private static byte GetStreamCompression(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasStreamCompression(container.Version) ? origin.StreamCompression : (byte)0;
		}

		private static bool GetIsReadable(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasIsReadable(container.Version) ? origin.IsReadable : true;
		}

		private static bool GetKeepVertices(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasIsReadable(container.Version) ? origin.KeepVertices : true;
		}

		private static bool GetKeepIndices(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasIsReadable(container.Version) ? origin.KeepIndices : true;
		}

		private static IndexFormat GetIndexFormat(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			if (Classes.Mesh.Mesh.HasIndexFormat(container.Version))
			{
				return origin.IndexFormat;
			}
			if (Classes.Mesh.Mesh.HasUse16bitIndices(container.Version))
			{
				return origin.Use16BitIndices == 0 ? IndexFormat.UInt32 : IndexFormat.UInt16;
			}
			return IndexFormat.UInt16;
		}

		private static byte[] GetIndexBuffer(IExportContainer container, Classes.Mesh.Mesh origin)
		{
#warning TODO: convert MeshTopology.Deprecated indices to MeshTopology.Triangles
			if (container.Platform == container.ExportPlatform ||
				container.Platform != Platform.XBox360 && container.ExportPlatform != Platform.XBox360)
			{
				return origin.RawIndexBuffer.ToArray();
			}
			else
			{
				int size = GetUse16bitIndices(container, origin) == 0 ? 4 : 2;
				return origin.RawIndexBuffer.SwapBytes(size);
			}
		}

		private static VertexData GetVertexData(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			if (Classes.Mesh.Mesh.HasVertexData(container.Version))
			{
				if (Classes.Mesh.Mesh.IsOnlyVertexData(container.Version))
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

		private static float[] GetMeshMetrics(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			return Classes.Mesh.Mesh.HasMeshMetrics(container.Version) ? origin.MeshMetrics.ToArray() : new float[] { 1.0f, 1.0f };
		}

		private static VertexData GenerateVertexData(IExportContainer container, Classes.Mesh.Mesh origin)
		{
			VertexData instance = new VertexData();
			bool hasVertices = origin.Vertices.Length > 0;
			bool hasNormals = origin.Normals.Length > 0;
			bool hasColors = origin.Colors.Length > 0;
			bool hasUV0 = origin.UV0.Length > 0;
			bool hasUV1 = origin.UV1.Length > 0;
			bool hasTangents = origin.Tangents.Length > 0;
			bool hasChannels = VertexData.HasChannels(container.ExportVersion);

			if (hasChannels)
			{
				int channelCount = ShaderChannelExtensions.GetChannelCount(container.ExportVersion);
				instance.m_Channels = ArrayUtils.CreateAndInitializeArray<ChannelInfo>(channelCount);
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
					instance.m_Channels[index] = new ChannelInfo(0, stride, format, dimention);
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
					instance.m_Channels[index] = new ChannelInfo(0, stride, format, dimention);
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
					instance.m_Channels[index] = new ChannelInfo(0, stride, format, dimention);
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
					instance.m_Channels[index] = new ChannelInfo(0, stride, format, dimention);
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
					instance.m_Channels[index] = new ChannelInfo(0, stride, format, dimention);
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
					instance.m_Channels[index] = new ChannelInfo(0, stride, format, dimention);
				}
				stride += ShaderChannel.Tangent.GetStride(container.ExportVersion);
			}

			if (VertexData.HasCurrentChannels(container.ExportVersion))
			{
				instance.CurrentChannels = curChannels.ToUInt32();
			}
			instance.VertexCount = (uint)origin.Vertices.Length;
			if (VertexData.HasStreams(container.ExportVersion))
			{
				StreamInfo info = new StreamInfo(instance.CurrentChannels, 0, stride);
				if (VertexData.IsStreamStatic(container.ExportVersion))
				{
					instance.m_Streams = new StreamInfo[] { info, new(), new(), new(), };
				}
				else
				{
					instance.m_Streams = new StreamInfo[] { info, };
				}
			}

			instance.Data = new byte[instance.VertexCount * stride];
			using (MemoryStream stream = new MemoryStream(instance.Data))
			{
				using AssetWriter writer = new AssetWriter(stream, EndianType.LittleEndian, container.ExportLayout);
				for (int i = 0; i < instance.VertexCount; i++)
				{
					if (hasVertices)
					{
						origin.Vertices[i].Write(writer);
					}
					if (hasNormals)
					{
						origin.Normals[i].Write(writer);
					}
					if (hasColors)
					{
						origin.Colors[i].Write(writer);
					}
					if (hasUV0)
					{
						origin.UV0[i].Write(writer);
					}
					if (hasUV1)
					{
						origin.UV1[i].Write(writer);
					}
					if (hasTangents)
					{
						origin.Tangents[i].Write(writer);
					}
				}
			}

			return instance;
		}
	}
}
