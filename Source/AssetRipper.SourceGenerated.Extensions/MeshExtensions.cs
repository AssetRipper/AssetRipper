using AssetRipper.Assets.Generics;
using AssetRipper.IO.Endian;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.CompressedMesh;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShape;
using AssetRipper.SourceGenerated.Subclasses.SubMesh;
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AssetRipper.SourceGenerated.Extensions;

public static partial class MeshExtensions
{
	[GeneratedRegex("^Combined Mesh \\(root scene\\)( [0-9]+)?$", RegexOptions.Compiled)]
	private static partial Regex CombinedMeshRegex();

	extension(IMesh mesh)
	{
		public bool IsCombinedMesh() => CombinedMeshRegex().IsMatch(mesh.Name);

		public bool IsSet()
		{
			return mesh.CompressedMesh.IsSet || mesh.VertexData.IsSet(mesh.StreamData);
		}

		public bool CheckAssetIntegrity()
		{
			if (mesh.Has_StreamData() && mesh.VertexData.IsSet(mesh.StreamData))
			{
				return mesh.StreamData.CheckIntegrity(mesh.Collection);
			}
			return true;
		}

		public bool HasAnyVertices()
		{
			return mesh.CompressedMesh.Vertices.NumItems > 0 || mesh.VertexData.VertexCount > 0;
		}

		public void ReadData(
			out Vector3[]? vertices,
			out Vector3[]? normals,
			out Vector4[]? tangents,
			out ColorFloat[]? colors,
			out Vector2[]? uv0,
			out Vector2[]? uv1,
			out Vector2[]? uv2,
			out Vector2[]? uv3,
			out Vector2[]? uv4,
			out Vector2[]? uv5,
			out Vector2[]? uv6,
			out Vector2[]? uv7,
			out BoneWeight4[]? skin,
			out Matrix4x4[]? bindPose,
			out uint[] processedIndexBuffer)
		{
			vertices = default;
			normals = default;
			tangents = default;
			colors = default;
			skin = default;
			uv0 = default;
			uv1 = default;
			uv2 = default;
			uv3 = default;
			uv4 = default;
			uv5 = default;
			uv6 = default;
			uv7 = default;

			VertexDataBlob.Create(mesh).ReadData(
				out vertices,
				out normals,
				out tangents,
				out colors,
				out uv0,
				out uv1,
				out uv2,
				out uv3,
				out uv4,
				out uv5,
				out uv6,
				out uv7,
				out skin);

			mesh.CompressedMesh.Decompress(out Vector3[]? compressed_vertices,
				out Vector3[]? compressed_normals,
				out Vector4[]? compressed_tangents,
				out ColorFloat[]? compressed_colors,
				out Vector2[]? compressed_uv0,
				out Vector2[]? compressed_uv1,
				out Vector2[]? compressed_uv2,
				out Vector2[]? compressed_uv3,
				out Vector2[]? compressed_uv4,
				out Vector2[]? compressed_uv5,
				out Vector2[]? compressed_uv6,
				out Vector2[]? compressed_uv7,
				out BoneWeight4[]? compressed_skin,
				out Matrix4x4[]? compressed_bindPose,
				out uint[]? compressed_processedIndexBuffer);

			vertices ??= compressed_vertices;
			normals ??= compressed_normals;
			tangents ??= compressed_tangents;
			colors ??= compressed_colors;
			skin ??= compressed_skin ?? mesh.Skin?.Select(b => b.ToCommonClass()).ToArray();
			uv0 ??= compressed_uv0;
			uv1 ??= compressed_uv1;
			uv2 ??= compressed_uv2;
			uv3 ??= compressed_uv3;
			uv4 ??= compressed_uv4;
			uv5 ??= compressed_uv5;
			uv6 ??= compressed_uv6;
			uv7 ??= compressed_uv7;
			bindPose = compressed_bindPose ?? mesh.BindPose.Select(Matrix4x4fExtensions.CastToStruct).ToArray();
			processedIndexBuffer = compressed_processedIndexBuffer ?? mesh.GetProcessedIndexBuffer();
		}

		/// <summary>
		/// Fill with compressed mesh data
		/// </summary>
		/// <remarks>
		/// This method assumes the mesh does not already contain data.
		/// </remarks>
		/// <param name="meshData"></param>
		public void FillWithCompressedMeshData(MeshData meshData)
		{
			mesh.SetIndexFormat(meshData.IndexFormat);

			ICompressedMesh compressedMesh = mesh.CompressedMesh;
			compressedMesh.SetVertices(meshData.Vertices);
			compressedMesh.SetNormals(meshData.Normals);
			compressedMesh.SetTangents(meshData.Tangents);
			compressedMesh.SetFloatColors(meshData.Colors);
			compressedMesh.SetWeights(meshData.Skin);
			compressedMesh.SetUV(
				meshData.UV0,
				meshData.UV1,
				meshData.UV2,
				meshData.UV3,
				meshData.UV4,
				meshData.UV5,
				meshData.UV6,
				meshData.UV7);
			if (compressedMesh.Has_BindPoses())
			{
				compressedMesh.SetBindPoses(meshData.BindPose);
			}
			else if (meshData.BindPose is not null)
			{
				foreach (Matrix4x4 matrix in meshData.BindPose)
				{
					mesh.BindPose.AddNew().CopyValues(matrix);
				}
			}
			compressedMesh.SetTriangles(meshData.ProcessedIndexBuffer);

			mesh.KeepIndices = true;//Not sure about this. Seems to be for animated meshes
			mesh.KeepVertices = true;//Not sure about this. Seems to be for animated meshes
			mesh.MeshMetrics_0_ = meshData.CalculateMeshMetric(0);
			mesh.MeshMetrics_1_ = meshData.CalculateMeshMetric(1);
			mesh.MeshUsageFlags = (int)SourceGenerated.NativeEnums.Global.MeshUsageFlags.MeshUsageFlagNone;
			mesh.CookingOptions = (int)SourceGenerated.NativeEnums.Global.MeshColliderCookingOptions.DefaultCookingFlags;
			//I copied 30 from a vanilla compressed mesh (with MeshCompression.Low), and it aligned with this enum.
			mesh.SetMeshOptimizationFlags(MeshOptimizationFlags.Everything);
			mesh.MeshCompressionE = ModelImporterMeshCompression.Low;

			AccessListBase<ISubMesh> subMeshList = mesh.SubMeshes;
			foreach (SubMeshData subMesh in meshData.SubMeshes)
			{
				subMesh.CopyTo(subMeshList.AddNew(), mesh.GetIndexFormat());
			}

			mesh.LocalAABB.CalculateFromVertexArray(meshData.Vertices);
		}

		public byte[] GetChannelsData()
		{
			if (mesh.Has_StreamData() && mesh.StreamData.IsSet())
			{
				return mesh.StreamData.GetContent(mesh.Collection);
			}
			else
			{
				return mesh.VertexData.Data;
			}
		}

		public string? FindBlendShapeNameByCRC(uint crc)
		{
			if (mesh.Has_Shapes())
			{
				return mesh.Shapes.FindShapeNameByCRC(crc);
			}
			else if (mesh.Has_ShapesList())
			{
				foreach (MeshBlendShape_4_1 blendShape in mesh.ShapesList)
				{
					if (blendShape.IsCRCMatch(crc))
					{
						return blendShape.Name.String;
					}
				}
			}
			return null;
		}

		public bool Is16BitIndices()
		{
			return mesh.GetIndexFormat() == IndexFormat.UInt16;
		}

		public IndexFormat GetIndexFormat()
		{
			if (mesh.Has_IndexFormat())
			{
				return mesh.IndexFormatE;
			}
			else
			{
				return IndexFormat.UInt16;//Versions between 3.5 and 2017.3 used 16 bit exclusively
			}
		}

		public void SetIndexFormat(IndexFormat indexFormat)
		{
			if (mesh.Has_IndexFormat())
			{
				mesh.IndexFormatE = indexFormat;
			}
			else if (indexFormat != IndexFormat.UInt16)
			{
				//Versions between 3.5 and 2017.3 used 16 bit exclusively
				throw new NotSupportedException($"Only 16 bit vertex indices are supported on {mesh.Collection.Version}.");
			}
		}

		public MeshOptimizationFlags GetMeshOptimizationFlags()
		{
			if (mesh.Has_MeshOptimizationFlags())
			{
				return (MeshOptimizationFlags)mesh.MeshOptimizationFlags;
			}
			else if (mesh.Has_MeshOptimized())
			{
				return mesh.MeshOptimized ? MeshOptimizationFlags.Everything : MeshOptimizationFlags.PolygonOrder;
			}
			else
			{
				return MeshOptimizationFlags.PolygonOrder;
			}
		}

		public void SetMeshOptimizationFlags(MeshOptimizationFlags value)
		{
			if (mesh.Has_MeshOptimizationFlags())
			{
				mesh.MeshOptimizationFlags = (int)value;
			}
			else if (mesh.Has_MeshOptimized())
			{
				mesh.MeshOptimized = value == MeshOptimizationFlags.Everything;
			}
		}

		public byte[] GetVertexDataBytes()
		{
			return mesh.VertexData.Data.Length switch
			{
				0 => mesh.StreamData?.GetContent(mesh.Collection) ?? [],
				_ => mesh.VertexData.Data,
			};
		}

		public uint[] GetProcessedIndexBuffer()
		{
			uint[] result;
			if (mesh.Is16BitIndices())
			{
				int indexCount = mesh.IndexBuffer.Length / sizeof(ushort);
				ushort[] rentedBuffer = ArrayPool<ushort>.Shared.Rent(indexCount);
				if (!BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.LittleEndian)
				{
					ReadOnlySpan<byte> indexBuffer = mesh.IndexBuffer;
					for (int i = 0; i < indexCount; i++)
					{
						rentedBuffer[i] = BinaryPrimitives.ReadUInt16LittleEndian(indexBuffer.Slice(i * sizeof(ushort)));
					}
				}
				else if (BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.BigEndian)
				{
					ReadOnlySpan<byte> indexBuffer = mesh.IndexBuffer;
					for (int i = 0; i < indexCount; i++)
					{
						rentedBuffer[i] = BinaryPrimitives.ReadUInt16BigEndian(indexBuffer.Slice(i * sizeof(ushort)));
					}
				}
				else
				{
					Buffer.BlockCopy(mesh.IndexBuffer, 0, rentedBuffer, 0, mesh.IndexBuffer.Length);
				}
				result = new uint[indexCount];
				UShortToUInt(rentedBuffer, result, indexCount);
				ArrayPool<ushort>.Shared.Return(rentedBuffer);
			}
			else
			{
				int indexCount = mesh.IndexBuffer.Length / sizeof(uint);
				result = new uint[indexCount];
				if (!BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.LittleEndian)
				{
					ReadOnlySpan<byte> indexBuffer = mesh.IndexBuffer;
					for (int i = 0; i < indexCount; i++)
					{
						result[i] = BinaryPrimitives.ReadUInt32LittleEndian(indexBuffer.Slice(i * sizeof(uint)));
					}
				}
				else if (BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.BigEndian)
				{
					ReadOnlySpan<byte> indexBuffer = mesh.IndexBuffer;
					for (int i = 0; i < indexCount; i++)
					{
						result[i] = BinaryPrimitives.ReadUInt32BigEndian(indexBuffer.Slice(i * sizeof(uint)));
					}
				}
				else
				{
					Buffer.BlockCopy(mesh.IndexBuffer, 0, result, 0, mesh.IndexBuffer.Length);
				}
			}
			return result;
		}

		public void SetProcessedIndexBuffer(uint[] indices)
		{
			if (mesh.Is16BitIndices())
			{
				mesh.IndexBuffer = new byte[indices.Length * sizeof(ushort)];
				ushort[] rentedBuffer = ArrayPool<ushort>.Shared.Rent(indices.Length);
				UIntToUShort(indices, rentedBuffer, indices.Length);

				if (!BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.LittleEndian)
				{
					Span<byte> indexBuffer = mesh.IndexBuffer;
					for (int i = 0; i < indices.Length; i++)
					{
						BinaryPrimitives.WriteUInt16LittleEndian(indexBuffer.Slice(i * sizeof(ushort)), (ushort)indices[i]);
					}
				}
				else if (BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.BigEndian)
				{
					Span<byte> indexBuffer = mesh.IndexBuffer;
					for (int i = 0; i < indices.Length; i++)
					{
						BinaryPrimitives.WriteUInt16BigEndian(indexBuffer.Slice(i * sizeof(ushort)), (ushort)indices[i]);
					}
				}
				else
				{
					Buffer.BlockCopy(rentedBuffer, 0, mesh.IndexBuffer, 0, mesh.IndexBuffer.Length);
				}

				ArrayPool<ushort>.Shared.Return(rentedBuffer);
			}
			else
			{
				mesh.IndexBuffer = new byte[indices.Length * sizeof(uint)];

				if (!BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.LittleEndian)
				{
					Span<byte> indexBuffer = mesh.IndexBuffer;
					for (int i = 0; i < indices.Length; i++)
					{
						BinaryPrimitives.WriteUInt32LittleEndian(indexBuffer.Slice(i * sizeof(uint)), indices[i]);
					}
				}
				else if (BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.BigEndian)
				{
					Span<byte> indexBuffer = mesh.IndexBuffer;
					for (int i = 0; i < indices.Length; i++)
					{
						BinaryPrimitives.WriteUInt32BigEndian(indexBuffer.Slice(i * sizeof(uint)), indices[i]);
					}
				}
				else
				{
					Buffer.BlockCopy(indices, 0, mesh.IndexBuffer, 0, mesh.IndexBuffer.Length);
				}
			}
		}
	}

	private static void UShortToUInt(ushort[] sourceArray, uint[] destinationArray, int indexCount)
	{
		if (sourceArray.Length < indexCount || destinationArray.Length < indexCount)
		{
			throw new ArgumentOutOfRangeException(nameof(indexCount));
		}

		for (int i = 0; i < indexCount; i++)
		{
			destinationArray[i] = sourceArray[i];
		}
	}

	private static void UIntToUShort(uint[] sourceArray, ushort[] destinationArray, int indexCount)
	{
		if (sourceArray.Length < indexCount || destinationArray.Length < indexCount)
		{
			throw new ArgumentOutOfRangeException(nameof(indexCount));
		}

		for (int i = 0; i < indexCount; i++)
		{
			destinationArray[i] = (ushort)sourceArray[i];
		}
	}
}
