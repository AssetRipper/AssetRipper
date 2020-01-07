using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

using Version = uTinyRipper.Version;

namespace DXShaderRestorer
{
	public static class DXShaderProgramRestorer
	{
		/// <summary>
		/// Not D3D9
		/// </summary>
		private static bool IsOffset(GPUPlatform graphicApi) => graphicApi != GPUPlatform.d3d9;
		/// <summary>
		/// 5.3.0
		/// </summary>
		private static bool IsOffset5(Version version) => version.IsEqual(5, 3);

		public static int GetDataOffset(Version version, GPUPlatform graphicApi, ShaderSubProgram shaderSubProgram)
		{
			int dataOffset = 0;
			if (IsOffset(graphicApi))
			{
				dataOffset = IsOffset5(version) ? 5 : 6;
				uint fourCC = BitConverter.ToUInt32(shaderSubProgram.ProgramData, dataOffset);
				if (fourCC != DXBCFourCC)
				{
					throw new Exception("Magic number doesn't match");
				}
			}
			return dataOffset;
		}
		static readonly uint RDEF = ToFourCc("RDEF"); //0x46454452
		public static string ToFourCcString(uint fourCc)
		{
			char a = (char)(fourCc & 0xFF);
			char b = (char)((fourCc >> 8) & 0xFF);
			char c = (char)((fourCc >> 16) & 0xFF);
			char d = (char)((fourCc >> 24) & 0xFF);

			return new string(new[] { a, b, c, d });
		}
		public static uint ToFourCc(string fourCc)
		{
			if (string.IsNullOrEmpty(fourCc) || fourCc.Length != 4)
				throw new ArgumentOutOfRangeException("fourCc", "Invalid FOURCC: " + fourCc);
			var a = (byte)fourCc[0];
			var b = (byte)fourCc[1];
			var c = (byte)fourCc[2];
			var d = (byte)fourCc[3];
			return a | ((uint)(b << 8)) | ((uint)c << 16) | ((uint)d << 24);
		}
		public static byte[] RestoreProgramData(Version version, GPUPlatform graphicApi, ShaderSubProgram shaderSubProgram)
		{
			int dataOffset = GetDataOffset(version, graphicApi, shaderSubProgram);
			using (MemoryStream src = new MemoryStream(shaderSubProgram.ProgramData, dataOffset, shaderSubProgram.ProgramData.Length - dataOffset))
			{
				using (BinaryReader reader = new BinaryReader(src))
				{
					using (MemoryStream dest = new MemoryStream())
					{
						using (BinaryWriter writer = new BinaryWriter(dest))
						{
							byte[] magicBytes = reader.ReadBytes(4);
							byte[] checksum = reader.ReadBytes(16);
							uint unknown0 = reader.ReadUInt32();
							uint totalSize = reader.ReadUInt32();
							uint chunkCount = reader.ReadUInt32();
							List<uint> chunkOffsets = new List<uint>();
							for (int i = 0; i < chunkCount; i++)
							{
								chunkOffsets.Add(reader.ReadUInt32());
							}
							uint bodyOffset = (uint)src.Position;
							// Check if shader already has resource chunk
							foreach (uint chunkOffset in chunkOffsets)
							{
								src.Position = chunkOffset;
								uint fourCc = reader.ReadUInt32();
								if (fourCc == RDEF)
								{
									src.Position = 0;
									byte[] original = reader.ReadBytes((int)src.Length);
									return original;
								}
							}
							src.Position = bodyOffset;
							byte[] resourceChunkData = GetResourceChunk(shaderSubProgram);
							//Adjust for new chunk
							totalSize += (uint)resourceChunkData.Length;
							for (int i = 0; i < chunkCount; i++)
							{
								chunkOffsets[i] += (uint)resourceChunkData.Length + 4;
							}
							chunkOffsets.Insert(0, bodyOffset + 4);
							chunkCount += 1;
							totalSize += (uint)resourceChunkData.Length;

							writer.Write(magicBytes);
							writer.Write(checksum);
							writer.Write(unknown0);
							writer.Write(totalSize);
							writer.Write(chunkCount);
							foreach (uint chunkOffset in chunkOffsets)
							{
								writer.Write(chunkOffset);
							}
							writer.Write(resourceChunkData);
							byte[] rest = reader.ReadBytes((int)src.Length - (int)src.Position);
							writer.Write(rest);
							return dest.ToArray();
						}
					}
				}
			}
		}
		private static byte[] GetResourceChunk(ShaderSubProgram shaderSubprogram)
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (EndianWriter writer = new EndianWriter(memoryStream, EndianType.LittleEndian))
				{
					ResourceChunk resourceChunk = new ResourceChunk(shaderSubprogram);
					resourceChunk.Write(writer);
					//uint size = resourceChunk.Size;
					//if (memoryStream.Length != resourceChunk.Size) throw new Exception("Expected size does not match actual size");
					return memoryStream.ToArray();
				}
			}
		}

		/// <summary>
		/// 'DXBC' ascii
		/// </summary>
		private const uint DXBCFourCC = 0x43425844;
	}
}
