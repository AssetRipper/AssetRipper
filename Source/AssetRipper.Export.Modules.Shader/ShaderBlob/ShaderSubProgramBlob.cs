using AssetRipper.Assets.Collections;
using AssetRipper.Assets.Generics;

namespace AssetRipper.Export.Modules.Shaders.ShaderBlob;

public sealed class ShaderSubProgramBlob
{
	public void Read(AssetCollection shaderCollection, byte[] compressedBlob, uint offset, uint compressedLength, uint decompressedLength)
	{
		m_shaderCollection = shaderCollection;
		ReadBlob(compressedBlob, offset, compressedLength, decompressedLength, 0);
	}

	public void Read(AssetCollection shaderCollection, byte[] compressedBlob, AssetList<uint> offsets, AssetList<uint> compressedLengths, AssetList<uint> decompressedLengths)
	{
		m_shaderCollection = shaderCollection;
		for (int i = 0; i < offsets.Count; i++)
		{
			ReadBlob(compressedBlob, offsets[i], compressedLengths[i], decompressedLengths[i], i);
		}
	}

	private void ReadBlob(byte[] compressedBlob, uint offset, uint compressedLength, uint decompressedLength, int segment)
	{
		while (m_decompressedBlobSegments.Count < segment + 1) { m_decompressedBlobSegments.Add([]); }
		m_decompressedBlobSegments[segment] = DecompressedBlob.DecompressBlob(compressedBlob, offset, compressedLength, decompressedLength);

		if (segment == 0)
		{
			using MemoryStream blobMem = new MemoryStream(m_decompressedBlobSegments[segment]);
			using AssetReader blobReader = new AssetReader(blobMem, m_shaderCollection);
			Entries = ReadAssetArray(blobReader);
			m_cachedSubPrograms.Clear();
		}
	}

	private static ShaderSubProgramEntry[] ReadAssetArray(AssetReader reader)
	{
		int count = reader.ReadInt32();

		ShaderSubProgramEntry[] array = CreateAndInitializeArray<ShaderSubProgramEntry>(count);
		for (int i = 0; i < count; i++)
		{
			array[i].Read(reader);
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
		ArgumentOutOfRangeException.ThrowIfNegative(length);

		if (length == 0)
		{
			return [];
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

		using MemoryStream blobMem = new MemoryStream(m_decompressedBlobSegments[Entries[blobIndex].Segment]);
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

		using MemoryStream blobMem = new MemoryStream(m_decompressedBlobSegments[Entries[blobIndex].Segment]);
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

	public ShaderSubProgramEntry[] Entries { get; set; } = [];

	private AssetCollection m_shaderCollection;
	private List<byte[]> m_decompressedBlobSegments = [];
	private readonly Dictionary<(uint, uint), ShaderSubProgram> m_cachedSubPrograms = new();

	public const string GpuProgramIndexName = "GpuProgramIndex";
}
