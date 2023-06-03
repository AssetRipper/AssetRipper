using AssetRipper.IO.Endian;
using AssetRipper.Numerics;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.ColorRGBA32;
using AssetRipper.SourceGenerated.Subclasses.MeshBlendShape;
using System.Buffers;
using System.Buffers.Binary;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class MeshExtensions
	{
		private static readonly Regex combinedMeshRegex = new Regex(@"^Combined Mesh \(root scene\)( [0-9]+)?$", RegexOptions.Compiled);

		/// <summary>
		/// Compressing meshes saves space in the built game, but more compression introduces more artifacts in vertex data.
		/// </summary>
		public enum MeshCompression : byte
		{
			/// <summary>
			/// No mesh compression (default).
			/// </summary>
			Off = 0,
			/// <summary>
			/// Low amount of mesh compression.
			/// </summary>
			Low = 1,
			/// <summary>
			/// Medium amount of mesh compression.
			/// </summary>
			Med = 2,
			/// <summary>
			/// High amount of mesh compression.
			/// </summary>
			High = 3,
			Count,
		}

		public static bool IsCombinedMesh(this IMesh mesh) => combinedMeshRegex.IsMatch(mesh.NameString);

		public static bool IsSet(this IMesh mesh)
		{
			return mesh.CompressedMesh_C43.IsSet() || mesh.VertexData_C43.IsSet(mesh.StreamData_C43);
		}

		public static void ConvertToEditorFormat(this IMesh mesh)
		{
			mesh.SetMeshOptimizationFlags(MeshOptimizationFlags.Everything);
		}

		public static bool CheckAssetIntegrity(this IMesh mesh)
		{
			if (mesh.Has_StreamData_C43() && mesh.VertexData_C43.IsSet(mesh.StreamData_C43))
			{
				return mesh.StreamData_C43.CheckIntegrity(mesh.Collection);
			}
			return true;
		}

		public static void ReadData(
			this IMesh mesh,
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

			mesh.VertexData_C43?.ReadData(mesh.Collection.Version, mesh.Collection.EndianType, mesh,
				out vertices,
				out normals,
				out tangents,
				out colors,
				out skin,
				out uv0,
				out uv1,
				out uv2,
				out uv3,
				out uv4,
				out uv5,
				out uv6,
				out uv7);

			mesh.CompressedMesh_C43.DecompressCompressedMesh(mesh.Collection.Version,
				out Vector3[]? compressed_vertices,
				out Vector3[]? compressed_normals,
				out Vector4[]? compressed_tangents,
				out ColorFloat[]? compressed_colors,
				out BoneWeight4[]? compressed_skin,
				out Vector2[]? compressed_uv0,
				out Vector2[]? compressed_uv1,
				out Vector2[]? compressed_uv2,
				out Vector2[]? compressed_uv3,
				out Vector2[]? compressed_uv4,
				out Vector2[]? compressed_uv5,
				out Vector2[]? compressed_uv6,
				out Vector2[]? compressed_uv7,
				out Matrix4x4[]? compressed_bindPose,
				out uint[]? compressed_processedIndexBuffer);

			vertices ??= compressed_vertices;
			normals ??= compressed_normals;
			tangents ??= compressed_tangents;
			colors ??= compressed_colors;
			skin ??= compressed_skin ?? mesh.Skin_C43?.Select(b => b.ToCommonClass()).ToArray();
			uv0 ??= compressed_uv0;
			uv1 ??= compressed_uv1;
			uv2 ??= compressed_uv2;
			uv3 ??= compressed_uv3;
			uv4 ??= compressed_uv4;
			uv5 ??= compressed_uv5;
			uv6 ??= compressed_uv6;
			uv7 ??= compressed_uv7;
			bindPose = compressed_bindPose;
			processedIndexBuffer = compressed_processedIndexBuffer ?? mesh.GetProcessedIndexBuffer();
		}

		private static ColorFloat ConvertToColorFloat(this ColorRGBA32 c)
		{
			return (ColorFloat)Color32.FromRgba(c.Rgba);
		}

		public static byte[] GetChannelsData(this IMesh mesh)
		{
			if (mesh.Has_StreamData_C43() && mesh.StreamData_C43.IsSet())
			{
				return mesh.StreamData_C43.GetContent(mesh.Collection);
			}
			else
			{
				return mesh.VertexData_C43?.Data ?? Array.Empty<byte>();
			}
		}

		public static string? FindBlendShapeNameByCRC(this IMesh mesh, uint crc)
		{
			if (mesh.Has_Shapes_C43())
			{
				return mesh.Shapes_C43.FindShapeNameByCRC(crc);
			}
			else if (mesh.Has_ShapesList_C43())
			{
				foreach (MeshBlendShape_4_1_0 blendShape in mesh.ShapesList_C43)
				{
					if (blendShape.IsCRCMatch(crc))
					{
						return blendShape.Name.String;
					}
				}
			}
			return null;
		}

		public static bool Is16BitIndices(this IMesh mesh)
		{
			return mesh.GetIndexFormat() == IndexFormat.UInt16;
		}

		public static IndexFormat GetIndexFormat(this IMesh mesh)
		{
			if (mesh.Has_IndexFormat_C43())
			{
				return mesh.IndexFormat_C43E;
			}
			else
			{
				return IndexFormat.UInt16;//Versions between 3.5 and 2017.3 used 16 bit exclusively
			}
		}

		public static void SetIndexFormat(this IMesh mesh, IndexFormat indexFormat)
		{
			if (mesh.Has_IndexFormat_C43())
			{
				mesh.IndexFormat_C43E = indexFormat;
			}
			else if (indexFormat != IndexFormat.UInt16)
			{
				//Versions between 3.5 and 2017.3 used 16 bit exclusively
				throw new NotSupportedException($"Only 16 bit vertex indices are supported on {mesh.Collection.Version}.");
			}
		}

		public static MeshOptimizationFlags GetMeshOptimizationFlags(this IMesh mesh)
		{
			if (mesh.Has_MeshOptimizationFlags_C43())
			{
				return (MeshOptimizationFlags)mesh.MeshOptimizationFlags_C43;
			}
			else if (mesh.Has_MeshOptimized_C43())
			{
				return mesh.MeshOptimized_C43 ? MeshOptimizationFlags.Everything : MeshOptimizationFlags.PolygonOrder;
			}
			else
			{
				return default;
			}
		}

		public static void SetMeshOptimizationFlags(this IMesh mesh, MeshOptimizationFlags value)
		{
			if (mesh.Has_MeshOptimizationFlags_C43())
			{
				mesh.MeshOptimizationFlags_C43 = (int)value;
			}
			else if (mesh.Has_MeshOptimized_C43())
			{
				mesh.MeshOptimized_C43 = value == MeshOptimizationFlags.Everything;
			}
		}

		public static MeshCompression GetMeshCompression(this IMesh mesh)
		{
			return (MeshCompression)mesh.MeshCompression_C43;
		}

		public static void SetMeshCompression(this IMesh mesh, MeshCompression meshCompression)
		{
			mesh.MeshCompression_C43 = (byte)meshCompression;
		}

		public static uint[] GetProcessedIndexBuffer(this IMesh mesh)
		{
			uint[] result;
			if (mesh.Is16BitIndices())
			{
				int indexCount = mesh.IndexBuffer_C43.Length / sizeof(ushort);
				ushort[] rentedBuffer = ArrayPool<ushort>.Shared.Rent(indexCount);
				if (!BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.LittleEndian)
				{
					ReadOnlySpan<byte> indexBuffer = mesh.IndexBuffer_C43;
					for (int i = 0; i < indexCount; i++)
					{
						rentedBuffer[i] = BinaryPrimitives.ReadUInt16LittleEndian(indexBuffer.Slice(i * sizeof(ushort)));
					}
				}
				else if (BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.BigEndian)
				{
					ReadOnlySpan<byte> indexBuffer = mesh.IndexBuffer_C43;
					for (int i = 0; i < indexCount; i++)
					{
						rentedBuffer[i] = BinaryPrimitives.ReadUInt16BigEndian(indexBuffer.Slice(i * sizeof(ushort)));
					}
				}
				else
				{
					Buffer.BlockCopy(mesh.IndexBuffer_C43, 0, rentedBuffer, 0, mesh.IndexBuffer_C43.Length);
				}
				result = new uint[indexCount];
				UShortToUInt(rentedBuffer, result, indexCount);
				ArrayPool<ushort>.Shared.Return(rentedBuffer);
			}
			else
			{
				int indexCount = mesh.IndexBuffer_C43.Length / sizeof(uint);
				result = new uint[indexCount];
				if (!BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.LittleEndian)
				{
					ReadOnlySpan<byte> indexBuffer = mesh.IndexBuffer_C43;
					for (int i = 0; i < indexCount; i++)
					{
						result[i] = BinaryPrimitives.ReadUInt32LittleEndian(indexBuffer.Slice(i * sizeof(uint)));
					}
				}
				else if (BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.BigEndian)
				{
					ReadOnlySpan<byte> indexBuffer = mesh.IndexBuffer_C43;
					for (int i = 0; i < indexCount; i++)
					{
						result[i] = BinaryPrimitives.ReadUInt32BigEndian(indexBuffer.Slice(i * sizeof(uint)));
					}
				}
				else
				{
					Buffer.BlockCopy(mesh.IndexBuffer_C43, 0, result, 0, mesh.IndexBuffer_C43.Length);
				}
			}
			return result;
		}

		public static void SetProcessedIndexBuffer(this IMesh mesh, uint[] indices)
		{
			if (mesh.Is16BitIndices())
			{
				mesh.IndexBuffer_C43 = new byte[indices.Length * sizeof(ushort)];
				ushort[] rentedBuffer = ArrayPool<ushort>.Shared.Rent(indices.Length);
				UIntToUShort(indices, rentedBuffer, indices.Length);

				if (!BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.LittleEndian)
				{
					Span<byte> indexBuffer = mesh.IndexBuffer_C43;
					for (int i = 0; i < indices.Length; i++)
					{
						BinaryPrimitives.WriteUInt16LittleEndian(indexBuffer.Slice(i * sizeof(ushort)), (ushort)indices[i]);
					}
				}
				else if (BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.BigEndian)
				{
					Span<byte> indexBuffer = mesh.IndexBuffer_C43;
					for (int i = 0; i < indices.Length; i++)
					{
						BinaryPrimitives.WriteUInt16BigEndian(indexBuffer.Slice(i * sizeof(ushort)), (ushort)indices[i]);
					}
				}
				else
				{
					Buffer.BlockCopy(rentedBuffer, 0, mesh.IndexBuffer_C43, 0, mesh.IndexBuffer_C43.Length);
				}

				ArrayPool<ushort>.Shared.Return(rentedBuffer);
			}
			else
			{
				mesh.IndexBuffer_C43 = new byte[indices.Length * sizeof(uint)];

				if (!BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.LittleEndian)
				{
					Span<byte> indexBuffer = mesh.IndexBuffer_C43;
					for (int i = 0; i < indices.Length; i++)
					{
						BinaryPrimitives.WriteUInt32LittleEndian(indexBuffer.Slice(i * sizeof(uint)), indices[i]);
					}
				}
				else if (BitConverter.IsLittleEndian && mesh.Collection.EndianType == EndianType.BigEndian)
				{
					Span<byte> indexBuffer = mesh.IndexBuffer_C43;
					for (int i = 0; i < indices.Length; i++)
					{
						BinaryPrimitives.WriteUInt32BigEndian(indexBuffer.Slice(i * sizeof(uint)), indices[i]);
					}
				}
				else
				{
					Buffer.BlockCopy(indices, 0, mesh.IndexBuffer_C43, 0, mesh.IndexBuffer_C43.Length);
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
}
