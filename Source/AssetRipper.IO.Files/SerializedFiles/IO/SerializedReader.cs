using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.SerializedFiles.IO;

internal sealed class SerializedReader : EndianReader
{
	public SerializedReader(Stream stream, EndianType endianess, FormatVersion generation) : base(stream, endianess)
	{
		Generation = generation;
	}

	public FileIdentifier[] ReadFileIdentifierArray()
	{
		int count = ReadInt32();
		FileIdentifier[] array = new FileIdentifier[count];
		for (int i = 0; i < count; i++)
		{
			FileIdentifier instance = new();
			instance.Read(this);
			array[i] = instance;
		}
		return array;
	}

	public LocalSerializedObjectIdentifier[] ReadLocalSerializedObjectIdentifierArray()
	{
		int count = ReadInt32();
		LocalSerializedObjectIdentifier[] array = new LocalSerializedObjectIdentifier[count];
		for (int i = 0; i < count; i++)
		{
			LocalSerializedObjectIdentifier instance = new();
			instance.Read(this);
			array[i] = instance;
		}
		return array;
	}

	public T[] ReadSerializedTypeArray<T>(bool hasTypeTree) where T : SerializedTypeBase, new()
	{
		int count = ReadInt32();
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			T instance = new();
			instance.Read(this, hasTypeTree);
			array[i] = instance;
		}
		return array;
	}

	public ObjectInfo[] ReadObjectInfoArray(bool longFileID, ReadOnlySpan<SerializedType> types, long dataOffset)
	{
		int count = ReadInt32();
		ObjectInfo[] array = new ObjectInfo[count];
		for (int i = 0; i < count; i++)
		{
			ObjectInfo instance = new();
			instance.Read(this, longFileID, types, dataOffset);
			array[i] = instance;
		}
		return array;
	}

	public FormatVersion Generation { get; }

	/// <summary>
	/// Gets set after reading the metadata version
	/// </summary>
	public UnityVersion Version { get; set; }
}
