using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;
using Version = uTinyRipper.Version;

namespace DXShaderExporter
{
	public class DXShaderObjectExporter
	{
		private const uint DXBCFourCC = 0x43425844;
		public static bool IsEncoded(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		public static bool IsSerialized(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		private static bool IsOffset(GPUPlatform graphicApi)
		{
			return graphicApi != GPUPlatform.d3d9;
		}

		private static bool IsOffset5(Version version)
		{
			return version.IsEqual(5, 3);
		}

		internal static DXProgramType GetDXProgramType(ShaderGpuProgramType prgramType)
		{
			switch (prgramType)
			{
				case ShaderGpuProgramType.DX11PixelSM40:
				case ShaderGpuProgramType.DX11PixelSM50:
					return DXProgramType.PixelShader;
				case ShaderGpuProgramType.DX11VertexSM40:
				case ShaderGpuProgramType.DX11VertexSM50:
					return DXProgramType.VertexShader;
				case ShaderGpuProgramType.DX11GeometrySM40:
				case ShaderGpuProgramType.DX11GeometrySM50:
					return DXProgramType.GeometryShader;
				case ShaderGpuProgramType.DX11HullSM50:
					return DXProgramType.HullShader;
				case ShaderGpuProgramType.DX11DomainSM50:
					return DXProgramType.DomainShader;
				default:
					throw new Exception($"Unexpected program type {prgramType}");
			}
		}
		public static int GetMajorVersion(ShaderGpuProgramType prgramType)
		{
			switch (prgramType)
			{
				case ShaderGpuProgramType.DX11PixelSM40:
				case ShaderGpuProgramType.DX11VertexSM40:
				case ShaderGpuProgramType.DX11GeometrySM40:
					return 4;
				case ShaderGpuProgramType.DX11PixelSM50:
				case ShaderGpuProgramType.DX11VertexSM50:
				case ShaderGpuProgramType.DX11GeometrySM50:
				case ShaderGpuProgramType.DX11HullSM50:
				case ShaderGpuProgramType.DX11DomainSM50:
					return 5;
				default:
					throw new Exception($"Unexpected program type {prgramType}");
			}
		}
		static byte[] GetResourceChunk(ShaderSubProgram shaderSubprogram)
		{
			var memoryStream = new MemoryStream();
			using (var writer = new EndianWriter(memoryStream))
			{
				var resourceChunk = new ResourceChunk(shaderSubprogram);
				resourceChunk.Write(writer);
				var data = memoryStream.ToArray();
				var size = resourceChunk.Size;

				//if (data.Length != resourceChunk.Size) throw new Exception("Expected size does not match actual size");
				return data;
			}
		}
		public static byte[] GetObjectData(Version m_version, GPUPlatform m_graphicApi, ShaderSubProgram shaderSubProgram)
		{
			var shaderData = shaderSubProgram.ProgramData.ToArray();

			int dataOffset = 0;
			if (IsOffset(m_graphicApi))
			{
				dataOffset = IsOffset5(m_version) ? 5 : 6;
				uint fourCC = BitConverter.ToUInt32(shaderData, dataOffset);
				if (fourCC != DXBCFourCC)
				{
					throw new Exception("Magic number doesn't match");
				}
			}
			int length = shaderData.Length - dataOffset;

			var memoryStream = new MemoryStream(shaderData, dataOffset, length);
			var outStream = new MemoryStream();
			using (var reader = new EndianReader(memoryStream))
			using (var writer = new EndianWriter(outStream))
			{
				var magicBytes = reader.ReadBytes(4);
				var checksum = reader.ReadBytes(16);
				var unknown0 = reader.ReadUInt32();
				var totalSize = reader.ReadUInt32();
				var chunkCount = reader.ReadUInt32();
				var chunkOffsets = new List<uint>();
				for (int i = 0; i < chunkCount; i++)
				{
					chunkOffsets.Add(reader.ReadUInt32());
				}
				var offset = (uint)memoryStream.Position + 4;
				var resourceChunkData = GetResourceChunk(shaderSubProgram);
				//Adjust for new chunk
				totalSize += (uint)resourceChunkData.Length;
				for (int i = 0; i < chunkCount; i++)
				{
					chunkOffsets[i] += (uint)resourceChunkData.Length + 4;
				}
				chunkOffsets.Insert(0, offset);
				chunkCount += 1;
				totalSize += (uint)resourceChunkData.Length;

				writer.Write(magicBytes);
				writer.Write(checksum);
				writer.Write(unknown0);
				writer.Write(totalSize);
				writer.Write(chunkCount);
				foreach (var chunkOffset in chunkOffsets)
				{
					writer.Write(chunkOffset);
				}
				writer.Write(resourceChunkData);
				var rest = reader.ReadBytes((int)memoryStream.Length - (int)memoryStream.Position);
				writer.Write(rest);
				return outStream.ToArray();
			}
		}
	}
}
