using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public static class AssetStreamExtensions
	{
		public static KeyValuePair<string, T>[] ReadStringKVPArray<T>(this AssetStream stream)
			where T : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			KeyValuePair<string, T>[] array = new KeyValuePair<string, T>[count];
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				T value = new T();
				value.Read(stream);
				KeyValuePair<string, T> kvp = new KeyValuePair<string, T>(key, value);
				array[i] = kvp;
			}
			return array;
		}

		public static KeyValuePair<string, T>[] ReadStringKVPArray<T>(this AssetStream stream, Func<T> valueInstantiator)
			where T : IAssetReadable
		{
			int count = stream.ReadInt32();
			KeyValuePair<string, T>[] array = new KeyValuePair<string, T>[count];
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				T value = valueInstantiator();
				value.Read(stream);
				KeyValuePair<string, T> kvp = new KeyValuePair<string, T>(key, value);
				array[i] = kvp;
			}
			return array;
		}

		public static Tuple<char, char> ReadTupleCharChar(this AssetStream stream)
		{
			char value1 = Convert.ToChar(stream.ReadByte());
			char value2 = Convert.ToChar(stream.ReadByte());
			return new Tuple<char, char>(value1, value2);
		}

		public static Tuple<byte, byte> ReadTupleByteByte(this AssetStream stream)
		{
			byte value1 = stream.ReadByte();
			byte value2 = stream.ReadByte();
			return new Tuple<byte, byte>(value1, value2);
		}

		public static Tuple<ushort, ushort> ReadTupleUInt16UInt16(this AssetStream stream)
		{
			ushort value1 = stream.ReadUInt16();
			ushort value2 = stream.ReadUInt16();
			return new Tuple<ushort, ushort>(value1, value2);
		}

		public static Tuple<char, float> ReadTupleCharSingle(this AssetStream stream)
		{
			char value1 = Convert.ToChar(stream.ReadByte());
			float value2 = stream.ReadSingle();
			return new Tuple<char, float>(value1, value2);
		}

		public static Tuple<byte, float> ReadTupleByteSingle(this AssetStream stream)
		{
			byte value1 = stream.ReadByte();
			float value2 = stream.ReadSingle();
			return new Tuple<byte, float>(value1, value2);
		}

		public static Tuple<int, float> ReadTupleInt32Single(this AssetStream stream)
		{
			int value1 = stream.ReadInt32();
			float value2 = stream.ReadSingle();
			return new Tuple<int, float>(value1, value2);
		}

		public static Tuple<char, float>[] ReadTupleCharSingleArray(this AssetStream stream)
		{
			int count = stream.ReadInt32();
			Tuple<char, float>[] array = new Tuple<char, float>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<char, float> tuple = ReadTupleCharSingle(stream);
				array[i] = tuple;
			}
			return array;
		}

		public static Tuple<byte, float>[] ReadTupleByteSingleArray(this AssetStream stream)
		{
			int count = stream.ReadInt32();
			Tuple<byte, float>[] array = new Tuple<byte, float>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<byte, float> tuple = ReadTupleByteSingle(stream);
				array[i] = tuple;
			}
			return array;
		}

		public static Tuple<int, float>[] ReadTupleIntFloatArray(this AssetStream stream)
		{
			int count = stream.ReadInt32();
			Tuple<int, float>[] array = new Tuple<int, float>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<int, float> tuple = ReadTupleInt32Single(stream);
				array[i] = tuple;
			}
			return array;
		}

		public static Tuple<T, long> ReadTupleTLong<T>(this AssetStream stream)
			where T : IAssetReadable, new()
		{
			T t = new T();
			t.Read(stream);
			long value = stream.ReadInt64();
			return new Tuple<T, long>(t, value);
		}
		
		public static Tuple<string, T> ReadTupleStringT<T>(this AssetStream stream)
			where T : IAssetReadable, new()
		{
			string value = stream.ReadStringAligned();
			T t = new T();
			t.Read(stream);
			return new Tuple<string, T>(value, t);
		}
		
		public static Tuple<string, T>[] ReadTupleStringTArray<T>(this AssetStream stream)
			where T : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			Tuple<string, T>[] array = new Tuple<string, T>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<string, T> tuple = ReadTupleStringT<T>(stream);
				array[i] = tuple;
			}
			return array;
		}
	}
}
