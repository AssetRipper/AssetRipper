using System;

namespace UtinyRipper.SerializedFiles
{
	public sealed class SerializedFileStream : EndianStream
	{
		public SerializedFileStream(EndianStream stream, FileGeneration generation) :
			base(stream.BaseStream, stream.AlignPosition, stream.EndianType)
		{
			Generation = generation;
		}

		public new T[] ReadArray<T>()
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
		
		public T[] ReadArray<T>(Func<T> instantiator)
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
