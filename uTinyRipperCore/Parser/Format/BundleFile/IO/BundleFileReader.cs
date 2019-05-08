using System;
using System.IO;

namespace uTinyRipper.BundleFiles
{
	public sealed class BundleFileReader : EndianReader
	{
		public BundleFileReader(Stream stream, EndianType endianess, BundleGeneration generation) :
			base(stream, endianess)
		{
			Generation = generation;
		}

		public T ReadBundle<T>()
			where T : IBundleFileReadable, new()
		{
			T t = new T();
			t.Read(this);
			return t;
		}

		public T[] ReadBundleArray<T>()
			where T : IBundleFileReadable, new()
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

		public T[] ReadBundleArray<T>(Func<T> instantiator)
			where T : IBundleFileReadable
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

		public BundleGeneration Generation { get; }
	}
}
