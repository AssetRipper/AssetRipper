using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Lz4;
using System;
using System.IO;
using System.Linq;

namespace AssetRipper.Core.Classes.Shader.Blob
{
	public struct ShaderSubProgramBlob
	{
		public void Read(LayoutInfo layout, MemoryStream memStream, uint[] offsets, uint[] compressedLengths, uint[] decompressedLengths)
		{
			for (int i = 0; i < offsets.Length; i++)
			{
				uint offset = offsets[i];
				uint compressedLength = compressedLengths[i];
				uint decompressedLength = decompressedLengths[i];

				memStream.Position = offset;
				ReadBlob(layout, memStream, compressedLength, decompressedLength, i);
			}
		}

		public void Write(LayoutInfo layout, MemoryStream memStream, out uint[] offsets, out uint[] compressedLengths, out uint[] decompressedLengths)
		{
			int segmentCount = Entries.Length == 0 ? 0 : Entries.Max(t => t.Segment) + 1;
			offsets = new uint[segmentCount];
			compressedLengths = new uint[segmentCount];
			decompressedLengths = new uint[segmentCount];
			for (int i = 0; i < segmentCount; i++)
			{
				uint offset = (uint)memStream.Position;
				WriteBlob(layout, memStream, out uint compressedLength, out uint decompressedLength, i);

				offsets[i] = offset;
				compressedLengths[i] = compressedLength;
				decompressedLengths[i] = decompressedLength;
			}
		}

		private void ReadBlob(LayoutInfo layout, MemoryStream memStream, uint compressedLength, uint decompressedLength, int segment)
		{
			byte[] decompressedBuffer = new byte[decompressedLength];
			using (Lz4DecodeStream lz4Stream = new Lz4DecodeStream(memStream, compressedLength))
			{
				lz4Stream.ReadBuffer(decompressedBuffer, 0, decompressedBuffer.Length);
			}

			using (MemoryStream blobMem = new MemoryStream(decompressedBuffer))
			{
				using (AssetReader blobReader = new AssetReader(blobMem, EndianType.LittleEndian, layout))
				{
					if (segment == 0)
					{
						Entries = blobReader.ReadAssetArray<ShaderSubProgramEntry>();
						SubPrograms = new ShaderSubProgram[Entries.Length];
					}
					ReadSegment(blobReader, segment);
				}
			}
		}

		private void WriteBlob(LayoutInfo layout, MemoryStream memStream, out uint compressedLength, out uint decompressedLength, int segment)
		{
			using (MemoryStream blobMem = new MemoryStream())
			{
				using (AssetWriter blobWriter = new AssetWriter(blobMem, EndianType.LittleEndian, layout))
				{
					if (segment == 0)
					{
						blobWriter.WriteAssetArray(Entries);
					}
					WriteSegment(blobWriter, segment);
				}
				decompressedLength = (uint)blobMem.Length;

				blobMem.Position = 0;
#warning TODO:
				compressedLength = 0;
				/*using (Lz4EncodeStream lz4Stream = new Lz4EncodeStream(blobMem, blobMem.Length))
				{
					lz4Stream.Write(memStream);
					compressedLength = lz4Stream.Length;
				}*/
			}
		}

		private void ReadSegment(AssetReader reader, int segment)
		{
			for (int i = 0; i < Entries.Length; i++)
			{
				ref ShaderSubProgramEntry entry = ref Entries[i];
				if (entry.Segment == segment)
				{
					reader.BaseStream.Position = entry.Offset;
					SubPrograms[i].Read(reader);
					if (reader.BaseStream.Position != entry.Offset + entry.Length)
					{
						throw new Exception($"Read {reader.BaseStream.Position - entry.Offset} less than expected {entry.Length}");
					}
				}
			}
		}

		private void WriteSegment(AssetWriter writer, int segment)
		{
			for (int i = 0; i < Entries.Length; i++)
			{
				ref ShaderSubProgramEntry entry = ref Entries[i];
				if (entry.Segment == segment)
				{
					writer.BaseStream.Position = entry.Offset;
					SubPrograms[i].Write(writer);
				}
			}
		}

		public ShaderSubProgramEntry[] Entries { get; set; }
		public ShaderSubProgram[] SubPrograms { get; set; }

		public const string GpuProgramIndexName = "GpuProgramIndex";
	}
}
