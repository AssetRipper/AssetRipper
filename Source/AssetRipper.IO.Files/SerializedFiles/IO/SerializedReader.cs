using AssetRipper.IO.Endian;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.SerializedFiles.IO;

public sealed class SerializedReader : EndianReader
{
	public SerializedReader(Stream stream, EndianType endianess, FormatVersion generation) : base(stream, endianess)
	{
		Generation = generation;
	}

	public T ReadSerialized<T>() where T : ISerializedReadable, new()
	{
		T t = new();
		t.Read(this);
		return t;
	}

	public T[] ReadSerializedArray<T>() where T : ISerializedReadable, new()
	{
		int count = ReadInt32();
		T[] array = new T[count];
		for (int i = 0; i < count; i++)
		{
			T instance = new();
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

	public FormatVersion Generation { get; }

	/// <summary>
	/// Gets set after reading the metadata version
	/// </summary>
	public UnityVersion Version { get; set; }
}
