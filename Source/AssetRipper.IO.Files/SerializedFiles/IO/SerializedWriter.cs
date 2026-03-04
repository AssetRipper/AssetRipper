using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.SerializedFiles.IO;

public sealed class SerializedWriter : EndianWriter
{
	public SerializedWriter(Stream stream, EndianType endianess, FormatVersion generation, UnityVersion version) : base(stream, endianess)
	{
		Generation = generation;
		Version = version;
	}

	public void WriteSerialized<T>(T value) where T : ISerializedWritable
	{
		value.Write(this);
	}

	public void WriteSerializedArray<T>(T[] array) where T : ISerializedWritable
	{
		FillInnerBuffer(array.Length);
		Write(m_buffer, 0, sizeof(int));

		for (int i = 0; i < array.Length; i++)
		{
			array[i].Write(this);
		}
	}

	public void WriteObjectInfoArray(ObjectInfo[] array)
	{
		FillInnerBuffer(array.Length);
		Write(m_buffer, 0, sizeof(int));

		long byteStart = 0;
		foreach (ObjectInfo objectInfo in array)
		{
			objectInfo.Write(this, byteStart);
			byteStart += objectInfo.ObjectData.Length;

			// each object data must be aligned to 8 bytes
			long remainder = byteStart & 0b111;
			long padding = (8 - remainder) & 0b111;
			byteStart += padding;
		}
	}

	public void WriteSerializedTypeArray<T>(T[] array, bool hasTypeTree) where T : SerializedTypeBase
	{
		FillInnerBuffer(array.Length);
		Write(m_buffer, 0, sizeof(int));

		for (int i = 0; i < array.Length; i++)
		{
			array[i].Write(this, hasTypeTree);
		}
	}

	public FormatVersion Generation { get; }

	public UnityVersion Version { get; }
}
