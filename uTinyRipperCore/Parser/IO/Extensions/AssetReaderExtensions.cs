using System;
using System.Collections.Generic;

namespace uTinyRipper
{
	public static class AssetReaderExtensions
	{
		public static Tuple<string, T> ReadTupleStringT<T>(this AssetReader reader)
			where T : IAssetReadable, new()
		{
			string item1 = reader.ReadString();
			T item2 = new T();
			item2.Read(reader);
			return new Tuple<string, T>(item1, item2);
		}

		public static Tuple<string, T>[] ReadTupleStringTArray<T>(this AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			Tuple<string, T>[] array = new Tuple<string, T>[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadTupleStringT<T>(reader);
			}
			return array;
		}

		public static Tuple<T1, T2> ReadTupleEnum32T<T1, T2>(this AssetReader reader, Func<int, T1> converter)
			where T2 : IAssetReadable, new()
		{
			T1 item1 = converter(reader.ReadInt32());
			T2 item2 = new T2();
			item2.Read(reader);
			return new Tuple<T1, T2>(item1, item2);
		}

		public static Tuple<T1, T2>[] ReadTupleEnum32TArray<T1, T2>(this AssetReader reader, Func<int, T1> converter)
			where T2 : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			Tuple<T1, T2>[] array = new Tuple<T1, T2>[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = ReadTupleEnum32T<T1, T2>(reader, converter);
			}
			return array;
		}

		public static Tuple<T, long> ReadTupleTLong<T>(this AssetReader reader)
			where T : IAssetReadable, new()
		{
			T t = new T();
			t.Read(reader);
			long value = reader.ReadInt64();
			return new Tuple<T, long>(t, value);
		}

		public static KeyValuePair<string, T>[] ReadKVPStringTArray<T>(this AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			KeyValuePair<string, T>[] array = new KeyValuePair<string, T>[count];
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadString();
				T value = new T();
				value.Read(reader);
				KeyValuePair<string, T> kvp = new KeyValuePair<string, T>(key, value);
				array[i] = kvp;
			}
			return array;
		}

		public static KeyValuePair<T1, T2>[] ReadKVPTTArray<T1, T2>(this AssetReader reader)
			where T1: IAssetReadable, new()
			where T2 : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			KeyValuePair<T1, T2>[] array = new KeyValuePair<T1, T2>[count];
			for (int i = 0; i < count; i++)
			{
				T1 key = new T1();
				key.Read(reader);
				T2 value = new T2();
				value.Read(reader);
				KeyValuePair<T1, T2> kvp = new KeyValuePair<T1, T2>(key, value);
				array[i] = kvp;
			}
			return array;
		}
	}
}
