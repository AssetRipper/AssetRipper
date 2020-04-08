using System;
using System.IO;

namespace uTinyRipper.SerializedFiles
{
	public sealed class SerializedReader : EndianReader
	{
		public SerializedReader(Stream stream, EndianType endianess, FormatVersion generation) :
			base(stream, endianess)
		{
			Generation = generation;
		}

		public T ReadSerialized<T>()
			where T : ISerializedReadable, new()
		{
			T t = new T();
			t.Read(this);
			return t;
		}

		public T[] ReadSerializedArray<T>()
			where T : ISerializedReadable, new()
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
			where T : ISerializedReadable
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

		public FormatVersion Generation { get; }
	}
}
