using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public static class EndianStreamExtensions
	{
		public static T[] ReadEnum32Array<T>(this EndianStream stream, Func<int, T> converter)
		{
			int count = stream.ReadInt32();
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				int value = stream.ReadInt32();
				array[i] = converter(value);
			}
			return array;
		}

		public static KeyValuePair<int, uint>[] ReadInt32KVPUInt32Array(this EndianStream stream)
		{
			int count = stream.ReadInt32();
			KeyValuePair<int, uint>[] array = new KeyValuePair<int, uint>[count];
			for (int i = 0; i < count; i++)
			{
				int key = stream.ReadInt32();
				uint value = stream.ReadUInt32();
				KeyValuePair<int, uint> kvp = new KeyValuePair<int, uint>(key, value);
				array[i] = kvp;
			}
			return array;
		}

		public static Tuple<char, char> ReadTupleCharChar(this EndianStream stream)
		{
			char value1 = Convert.ToChar(stream.ReadByte());
			char value2 = Convert.ToChar(stream.ReadByte());
			return new Tuple<char, char>(value1, value2);
		}

		public static Tuple<char, float> ReadTupleCharFloat(this EndianStream stream)
		{
			char value1 = Convert.ToChar(stream.ReadByte());
			float value2 = stream.ReadSingle();
			return new Tuple<char, float>(value1, value2);
		}

		public static Tuple<char, float>[] ReadTupleCharFloatArray(this EndianStream stream)
		{
			int count = stream.ReadInt32();
			Tuple<char, float>[] array = new Tuple<char, float>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<char, float> tuple = ReadTupleCharFloat(stream);
				array[i] = tuple;
			}
			return array;
		}
	}
}
