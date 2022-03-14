using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser;
using System;
using System.IO;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.IO
{
	public sealed class SerializedReader : EndianReader
	{
		public SerializedReader(Stream stream, EndianType endianess, FormatVersion generation) : base(stream, endianess)
		{
			Generation = generation;
		}

		public T ReadSerialized<T>() where T : ISerializedReadable, new()
		{
			T t = new T();
			t.Read(this);
			return t;
		}

		public T[] ReadSerializedArray<T>() where T : ISerializedReadable, new()
		{
			int count = ReadInt32();
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				T instance = new T();
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
	}
}
