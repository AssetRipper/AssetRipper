using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Utils;
using AssetRipper.IO.Endian;
using K4os.Compression.LZ4;
using System;
using System.IO;
using System.Linq;

namespace ShaderTextRestorer.ShaderBlob
{
	public sealed class ShaderSubProgramBlob
	{
		public void Read(LayoutInfo layout, byte[] compressedBlob, uint[] offsets, uint[] compressedLengths, uint[] decompressedLengths)
		{
			for (int i = 0; i < offsets.Length; i++)
			{
				uint offset = offsets[i];
				uint compressedLength = compressedLengths[i];
				uint decompressedLength = decompressedLengths[i];

				ReadBlob(layout, compressedBlob, offset, compressedLength, decompressedLength, i);
			}
		}

		private void ReadBlob(LayoutInfo layout, byte[] compressedBlob, uint offset, uint compressedLength, uint decompressedLength, int segment)
		{
			byte[] decompressedBuffer = new byte[decompressedLength];
			LZ4Codec.Decode(compressedBlob, (int)offset, (int)compressedLength, decompressedBuffer, 0, (int)decompressedLength);

			using MemoryStream blobMem = new MemoryStream(decompressedBuffer);
			using AssetReader blobReader = new AssetReader(blobMem, EndianType.LittleEndian, layout);
			if (segment == 0)
			{
				Entries = blobReader.ReadAssetArray<ShaderSubProgramEntry>();
				SubPrograms = ArrayUtils.CreateAndInitializeArray<ShaderSubProgram>(Entries.Length);
			}
			ReadSegment(blobReader, segment);
		}

		private void ReadSegment(AssetReader reader, int segment)
		{
			for (int i = 0; i < Entries.Length; i++)
			{
				ShaderSubProgramEntry entry = Entries[i];
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

		private void WriteBlob(LayoutInfo layout, MemoryStream memStream, out uint compressedLength, out uint decompressedLength, int segment)
		{
			using MemoryStream blobMem = new MemoryStream();
			using (AssetWriter blobWriter = new AssetWriter(blobMem, EndianType.LittleEndian, layout))
			{
				if (segment == 0)
				{
					blobWriter.WriteAssetArray(Entries);
				}

				WriteSegment(blobWriter, segment);
			}
			decompressedLength = (uint)blobMem.Length;

			byte[] source = blobMem.ToArray();

			byte[] target = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
			int encodedLength = LZ4Codec.Encode(source, 0, source.Length, target, 0, target.Length);

			if (encodedLength < 0)
			{
				throw new Exception("Unable to compress sub program blob");
			}
			else
			{
				compressedLength = (uint)encodedLength;
				memStream.Write(target, 0, encodedLength);
			}
		}

		private void WriteSegment(AssetWriter writer, int segment)
		{
			for (int i = 0; i < Entries.Length; i++)
			{
				ShaderSubProgramEntry entry = Entries[i];
				if (entry.Segment == segment)
				{
					writer.BaseStream.Position = entry.Offset;
					SubPrograms[i].Write(writer);
				}
			}
		}

		public ShaderSubProgramEntry[] Entries { get; set; } = Array.Empty<ShaderSubProgramEntry>();
		public ShaderSubProgram[] SubPrograms { get; set; } = Array.Empty<ShaderSubProgram>();

		public const string GpuProgramIndexName = "GpuProgramIndex";
	}
}
