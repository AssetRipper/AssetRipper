using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO.Writing;
using K4os.Compression.LZ4;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob
{
	public sealed class ShaderSubProgramBlob
	{
		public void Read(AssetCollection shaderCollection, byte[] compressedBlob, uint[] offsets, uint[] compressedLengths, uint[] decompressedLengths)
		{
			m_shaderCollection = shaderCollection;
			for (int i = 0; i < offsets.Length; i++)
			{
				uint offset = offsets[i];
				uint compressedLength = compressedLengths[i];
				uint decompressedLength = decompressedLengths[i];

				ReadBlob(compressedBlob, offset, compressedLength, decompressedLength, i);
			}
		}

		public void Read(AssetCollection shaderCollection, byte[] compressedBlob, AssetList<uint> offsets, AssetList<uint> compressedLengths, AssetList<uint> decompressedLengths)
		{
			m_shaderCollection = shaderCollection;
			for (int i = 0; i < offsets.Count; i++)
			{
				uint offset = offsets[i];
				uint compressedLength = compressedLengths[i];
				uint decompressedLength = decompressedLengths[i];

				ReadBlob(compressedBlob, offset, compressedLength, decompressedLength, i);
			}
		}

		private void ReadBlob(byte[] compressedBlob, uint offset, uint compressedLength, uint decompressedLength, int segment)
		{
			m_decompressedBlob = new byte[decompressedLength];
			LZ4Codec.Decode(compressedBlob, (int)offset, (int)compressedLength, m_decompressedBlob, 0, (int)decompressedLength);

			using MemoryStream blobMem = new MemoryStream(m_decompressedBlob);
			using AssetReader blobReader = new AssetReader(blobMem, m_shaderCollection);
			if (segment == 0)
			{
				Entries = ReadAssetArray(blobReader);
				m_cachedSubPrograms.Clear();
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


		public ShaderSubProgram GetSubProgram(uint blobIndex)
		{
			if (m_cachedSubPrograms.TryGetValue((blobIndex, blobIndex), out ShaderSubProgram? subProgram))
			{
				return subProgram;
			}

			using MemoryStream blobMem = new MemoryStream(m_decompressedBlob);
			using AssetReader blobReader = new AssetReader(blobMem, m_shaderCollection);

			subProgram = new ShaderSubProgram();
			ReadSubProgram(blobReader, subProgram, blobIndex, true, true);

			m_cachedSubPrograms.TryAdd((blobIndex, blobIndex), subProgram);
			return subProgram;
		}

		public ShaderSubProgram GetSubProgram(uint blobIndex, uint paramBlobIndex)
		{
			if (m_cachedSubPrograms.TryGetValue((blobIndex, paramBlobIndex), out ShaderSubProgram? subProgram))
			{
				return subProgram;
			}

			using MemoryStream blobMem = new MemoryStream(m_decompressedBlob);
			using AssetReader blobReader = new AssetReader(blobMem, m_shaderCollection);

			subProgram = new ShaderSubProgram();
			ReadSubProgram(blobReader, subProgram, blobIndex, true, false);
			ReadSubProgram(blobReader, subProgram, paramBlobIndex, false, true);

			m_cachedSubPrograms.TryAdd((blobIndex, paramBlobIndex), subProgram);
			return subProgram;
		}

		private void ReadSubProgram(AssetReader reader, ShaderSubProgram subProgram, uint index, bool readProgramData, bool readParams)
		{
			ShaderSubProgramEntry entry = Entries[index];
			reader.BaseStream.Position = entry.Offset;

			subProgram.Read(reader, readProgramData, readParams);
			if (reader.BaseStream.Position != entry.Offset + entry.Length)
			{
				throw new Exception($"Read {reader.BaseStream.Position - entry.Offset} less than expected {entry.Length}");
			}
		}

		public ShaderSubProgramEntry[] Entries { get; set; } = Array.Empty<ShaderSubProgramEntry>();

		private AssetCollection m_shaderCollection;
		private byte[] m_decompressedBlob = Array.Empty<byte>();
		private readonly Dictionary<(uint, uint), ShaderSubProgram> m_cachedSubPrograms = new Dictionary<(uint, uint), ShaderSubProgram>();

		public const string GpuProgramIndexName = "GpuProgramIndex";
	}
}
