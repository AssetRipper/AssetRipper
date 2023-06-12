using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using K4os.Compression.LZ4;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob
{
	public sealed class ShaderSubProgramBlob
	{
		public void Read(AssetCollection shaderCollection, byte[] compressedBlob, uint[] offsets, uint[] compressedLengths, uint[] decompressedLengths)
		{
			for (int i = 0; i < offsets.Length; i++)
			{
				uint offset = offsets[i];
				uint compressedLength = compressedLengths[i];
				uint decompressedLength = decompressedLengths[i];

				ReadBlob(shaderCollection, compressedBlob, offset, compressedLength, decompressedLength, i);
			}
		}

		public void Read(AssetCollection shaderCollection, byte[] compressedBlob, AssetList<uint> offsets, AssetList<uint> compressedLengths, AssetList<uint> decompressedLengths)
		{
			for (int i = 0; i < offsets.Count; i++)
			{
				uint offset = offsets[i];
				uint compressedLength = compressedLengths[i];
				uint decompressedLength = decompressedLengths[i];

				ReadBlob(shaderCollection, compressedBlob, offset, compressedLength, decompressedLength, i);
			}
		}

		private void ReadBlob(AssetCollection shaderCollection, byte[] compressedBlob, uint offset, uint compressedLength, uint decompressedLength, int segment)
		{
			byte[] decompressedBuffer = new byte[decompressedLength];
			LZ4Codec.Decode(compressedBlob, (int)offset, (int)compressedLength, decompressedBuffer, 0, (int)decompressedLength);

			using MemoryStream blobMem = new MemoryStream(decompressedBuffer);
			using AssetReader blobReader = new AssetReader(blobMem, shaderCollection);
			if (segment == 0)
			{
				Entries = ReadAssetArray(blobReader);
				SubPrograms = CreateAndInitializeArray<ShaderSubProgram>(Entries.Length);
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

		public void Write(AssetCollection shaderCollection, MemoryStream memStream, out uint[] offsets, out uint[] compressedLengths, out uint[] decompressedLengths)
		{
			int segmentCount = Entries.Length == 0 ? 0 : Entries.Max(t => t.Segment) + 1;
			offsets = new uint[segmentCount];
			compressedLengths = new uint[segmentCount];
			decompressedLengths = new uint[segmentCount];
			for (int i = 0; i < segmentCount; i++)
			{
				uint offset = (uint)memStream.Position;
				WriteBlob(shaderCollection, memStream, out uint compressedLength, out uint decompressedLength, i);

				offsets[i] = offset;
				compressedLengths[i] = compressedLength;
				decompressedLengths[i] = decompressedLength;
			}
		}

		private void WriteBlob(AssetCollection shaderCollection, MemoryStream memStream, out uint compressedLength, out uint decompressedLength, int segment)
		{
			using MemoryStream blobMem = new MemoryStream();
			using (AssetWriter blobWriter = new AssetWriter(blobMem, shaderCollection))
			{
				if (segment == 0)
				{
					WriteAssetArray(blobWriter, Entries);
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

		private static ShaderSubProgramEntry[] ReadAssetArray(AssetReader reader)
		{
			int count = reader.ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			ShaderSubProgramEntry[] array = count == 0 ? Array.Empty<ShaderSubProgramEntry>() : new ShaderSubProgramEntry[count];
			for (int i = 0; i < count; i++)
			{
				ShaderSubProgramEntry instance = new();
				instance.Read(reader);
				array[i] = instance;
			}
			if (reader.IsAlignArray)
			{
				reader.AlignStream();
			}
			return array;
		}

		private static void WriteAssetArray(AssetWriter writer, ShaderSubProgramEntry[] buffer)
		{
			writer.Write(buffer.Length);

			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i].Write(writer);
			}

			if (writer.IsAlignArray)
			{
				writer.AlignStream();
			}
		}

		/// <summary>
		/// Creates an array with non-null elements
		/// </summary>
		/// <typeparam name="T">The type of the array elements</typeparam>
		/// <param name="length">The length of the array</param>
		/// <returns>A new array of the specified length and type</returns>
		/// <exception cref="ArgumentOutOfRangeException">Length less than zero</exception>
		private static T[] CreateAndInitializeArray<T>(int length) where T : new()
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			if (length == 0)
			{
				return Array.Empty<T>();
			}

			T[] array = new T[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = new();
			}
			return array;
		}

		public ShaderSubProgramEntry[] Entries { get; set; } = Array.Empty<ShaderSubProgramEntry>();
		public ShaderSubProgram[] SubPrograms { get; set; } = Array.Empty<ShaderSubProgram>();

		public const string GpuProgramIndexName = "GpuProgramIndex";
	}
}
