using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public static class EndianStreamExtensions
	{
		public static T[] ReadEnum32Array<T>(this EndianReader reader, Func<int, T> converter)
		{
			int count = reader.ReadInt32();
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				int value = reader.ReadInt32();
				array[i] = converter(value);
			}
			return array;
		}

		public static KeyValuePair<int, uint>[] ReadInt32KVPUInt32Array(this EndianReader reader)
		{
			int count = reader.ReadInt32();
			KeyValuePair<int, uint>[] array = new KeyValuePair<int, uint>[count];
			for (int i = 0; i < count; i++)
			{
				int key = reader.ReadInt32();
				uint value = reader.ReadUInt32();
				KeyValuePair<int, uint> kvp = new KeyValuePair<int, uint>(key, value);
				array[i] = kvp;
			}
			return array;
		}

		public static Tuple<char, char> ReadTupleCharChar(this EndianReader reader)
		{
			char value1 = Convert.ToChar(reader.ReadByte());
			char value2 = Convert.ToChar(reader.ReadByte());
			return new Tuple<char, char>(value1, value2);
		}

		public static Tuple<char, float> ReadTupleCharFloat(this EndianReader reader)
		{
			char value1 = Convert.ToChar(reader.ReadByte());
			float value2 = reader.ReadSingle();
			return new Tuple<char, float>(value1, value2);
		}

		public static Tuple<char, float>[] ReadTupleCharFloatArray(this EndianReader reader)
		{
			int count = reader.ReadInt32();
			Tuple<char, float>[] array = new Tuple<char, float>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<char, float> tuple = ReadTupleCharFloat(reader);
				array[i] = tuple;
			}
			return array;
		}
	}
}
