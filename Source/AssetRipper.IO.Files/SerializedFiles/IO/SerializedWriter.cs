using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.SerializedFiles.IO;

internal sealed class SerializedWriter : EndianWriter
{
	public SerializedWriter(Stream stream, EndianType endianess, FormatVersion generation, UnityVersion version) : base(stream, endianess)
	{
		Generation = generation;
		Version = version;
	}

	public void WriteFileIdentifierArray(FileIdentifier[] array)
	{
		Write(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Write(this);
		}
	}

	public void WriteLocalSerializedObjectIdentifierArray(LocalSerializedObjectIdentifier[] array)
	{
		Write(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Write(this);
		}
	}

	public void WriteObjectInfoArray(ObjectInfo[] array)
	{
		Write(array.Length);

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
		Write(array.Length);

		for (int i = 0; i < array.Length; i++)
		{
			array[i].Write(this, hasTypeTree);
		}
	}

	public FormatVersion Generation { get; }

	public UnityVersion Version { get; }
}
