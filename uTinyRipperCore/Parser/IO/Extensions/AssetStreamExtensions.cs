using System;
using System.Collections.Generic;

namespace uTinyRipper
{
	public static class AssetStreamExtensions
	{
		public static KeyValuePair<T1, T2>[] ReadTTKVPArray<T1, T2>(this AssetReader reader)
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

		public static KeyValuePair<string, T>[] ReadStringTKVPArray<T>(this AssetReader reader)
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

		public static KeyValuePair<string, T>[] ReadStringTKVPArray<T>(this AssetReader reader, Func<T> valueInstantiator)
			where T : IAssetReadable
		{
			int count = reader.ReadInt32();
			KeyValuePair<string, T>[] array = new KeyValuePair<string, T>[count];
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadString();
				T value = valueInstantiator();
				value.Read(reader);
				KeyValuePair<string, T> kvp = new KeyValuePair<string, T>(key, value);
				array[i] = kvp;
			}
			return array;
		}

		public static Tuple<char, char> ReadTupleCharChar(this AssetReader reader)
		{
			char value1 = Convert.ToChar(reader.ReadByte());
			char value2 = Convert.ToChar(reader.ReadByte());
			return new Tuple<char, char>(value1, value2);
		}

		public static Tuple<byte, byte> ReadTupleByteByte(this AssetReader reader)
		{
			byte value1 = reader.ReadByte();
			byte value2 = reader.ReadByte();
			return new Tuple<byte, byte>(value1, value2);
		}

		public static Tuple<ushort, ushort> ReadTupleUInt16UInt16(this AssetReader reader)
		{
			ushort value1 = reader.ReadUInt16();
			ushort value2 = reader.ReadUInt16();
			return new Tuple<ushort, ushort>(value1, value2);
		}

		public static Tuple<char, float> ReadTupleCharSingle(this AssetReader reader)
		{
			char value1 = Convert.ToChar(reader.ReadByte());
			float value2 = reader.ReadSingle();
			return new Tuple<char, float>(value1, value2);
		}

		public static Tuple<byte, float> ReadTupleByteSingle(this AssetReader reader)
		{
			byte value1 = reader.ReadByte();
			float value2 = reader.ReadSingle();
			return new Tuple<byte, float>(value1, value2);
		}

		public static Tuple<int, float> ReadTupleInt32Single(this AssetReader reader)
		{
			int value1 = reader.ReadInt32();
			float value2 = reader.ReadSingle();
			return new Tuple<int, float>(value1, value2);
		}

		public static Tuple<char, float>[] ReadTupleCharSingleArray(this AssetReader reader)
		{
			int count = reader.ReadInt32();
			Tuple<char, float>[] array = new Tuple<char, float>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<char, float> tuple = ReadTupleCharSingle(reader);
				array[i] = tuple;
			}
			return array;
		}

		public static Tuple<byte, float>[] ReadTupleByteSingleArray(this AssetReader reader)
		{
			int count = reader.ReadInt32();
			Tuple<byte, float>[] array = new Tuple<byte, float>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<byte, float> tuple = ReadTupleByteSingle(reader);
				array[i] = tuple;
			}
			return array;
		}

		public static Tuple<int, float>[] ReadTupleIntFloatArray(this AssetReader reader)
		{
			int count = reader.ReadInt32();
			Tuple<int, float>[] array = new Tuple<int, float>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<int, float> tuple = ReadTupleInt32Single(reader);
				array[i] = tuple;
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
		
		public static Tuple<string, T> ReadTupleStringT<T>(this AssetReader reader)
			where T : IAssetReadable, new()
		{
			string value = reader.ReadString();
			T t = new T();
			t.Read(reader);
			return new Tuple<string, T>(value, t);
		}
		
		public static Tuple<string, T>[] ReadTupleStringTArray<T>(this AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			Tuple<string, T>[] array = new Tuple<string, T>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<string, T> tuple = ReadTupleStringT<T>(reader);
				array[i] = tuple;
			}
			return array;
		}
	}
}
