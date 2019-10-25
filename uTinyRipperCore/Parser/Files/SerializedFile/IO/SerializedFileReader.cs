using System;
using System.IO;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedFileReader : EndianReader
	{
		public SerializedFileReader(Stream stream, EndianType endianess, FileGeneration generation) :
			base(stream, endianess)
		{
			Generation = generation;
		}

		public T ReadSerialized<T>()
			where T : ISerializedFileReadable, new()
		{
			T t = new T();
			t.Read(this);
			return t;
		}

		public T[] ReadSerializedArray<T>()
			where T : ISerializedFileReadable, new()
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
		
		public T[] ReadSerializedArray<T>(Func<T> instantiator)
			where T : ISerializedFileReadable
		{
			int count = ReadInt32();
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				T instance = instantiator();
				instance.Read(this);
				array[i] = instance;
			}
			return array;
		}

		public FileGeneration Generation { get; }
	}
}
