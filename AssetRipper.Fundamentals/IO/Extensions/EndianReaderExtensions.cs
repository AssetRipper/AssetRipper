using AssetRipper.IO.Endian;
using System.Collections.Generic;

namespace AssetRipper.Core.IO.Extensions
{
	public static class EndianReaderExtensions
	{
		public static uint[][] ReadUInt32ArrayArray(this EndianReader _this)
		{
			int count = _this.ReadInt32();
			uint[][] array = new uint[count][];
			for (int i = 0; i < count; i++)
			{
				array[i] = _this.ReadUInt32Array();
			}
			return array;
		}

		public static string[][] ReadStringArrayArray(this EndianReader _this)
		{
			int count = _this.ReadInt32();
			string[][] array = new string[count][];
			for (int i = 0; i < count; i++)
			{
				array[i] = _this.ReadStringArray();
			}
			return array;
		}

		public static Tuple<bool, string> ReadTupleBoolString(this EndianReader reader)
		{
			bool value1 = reader.ReadBoolean();
			string value2 = reader.ReadString();
			return new Tuple<bool, string>(value1, value2);
		}

		public static Tuple<bool, string>[] ReadTupleBoolStringArray(this EndianReader reader)
		{
			int count = reader.ReadInt32();
			Tuple<bool, string>[] array = new Tuple<bool, string>[count];
			for (int i = 0; i < count; i++)
			{
				Tuple<bool, string> tuple = ReadTupleBoolString(reader);
				array[i] = tuple;
			}
			return array;
		}

		public static Tuple<char, char> ReadTupleCharChar(this EndianReader reader)
		{
			char value1 = Convert.ToChar(reader.ReadByte());
			char value2 = Convert.ToChar(reader.ReadByte());
			return new Tuple<char, char>(value1, value2);
		}

		public static Tuple<char, float> ReadTupleCharSingle(this EndianReader reader)
		{
			char value1 = Convert.ToChar(reader.ReadByte());
			float value2 = reader.ReadSingle();
			return new Tuple<char, float>(value1, value2);
		}

		public static Tuple<char, float>[] ReadTupleCharSingleArray(this EndianReader reader)
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

		public static Tuple<byte, byte> ReadTupleByteByte(this EndianReader reader)
		{
			byte value1 = reader.ReadByte();
			byte value2 = reader.ReadByte();
			return new Tuple<byte, byte>(value1, value2);
		}

		public static Tuple<byte, float> ReadTupleByteSingle(this EndianReader reader)
		{
			byte value1 = reader.ReadByte();
			float value2 = reader.ReadSingle();
			return new Tuple<byte, float>(value1, value2);
		}

		public static Tuple<byte, float>[] ReadTupleByteSingleArray(this EndianReader reader)
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

		public static Tuple<ushort, ushort> ReadTupleUInt16UInt16(this EndianReader reader)
		{
			ushort value1 = reader.ReadUInt16();
			ushort value2 = reader.ReadUInt16();
			return new Tuple<ushort, ushort>(value1, value2);
		}

		public static Tuple<int, long> ReadTupleInt32Int64(this EndianReader reader)
		{
			int value1 = reader.ReadInt32();
			long value2 = reader.ReadInt64();
			return new Tuple<int, long>(value1, value2);
		}

		public static Tuple<int, float> ReadTupleInt32Single(this EndianReader reader)
		{
			int value1 = reader.ReadInt32();
			float value2 = reader.ReadSingle();
			return new Tuple<int, float>(value1, value2);
		}

		public static Tuple<int, float>[] ReadTupleIntSingleArray(this EndianReader reader)
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

		public static Tuple<T, long> ReadTupleTLong<T>(this EndianReader reader, Func<int, T> converter)
		{
			T value1 = converter(reader.ReadInt32());
			long value2 = reader.ReadInt64();
			return new Tuple<T, long>(value1, value2);
		}

		public static KeyValuePair<int, uint>[] ReadKVPInt32UInt32Array(this EndianReader reader)
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

		public static T[] ReadArray<T>(this EndianReader reader, Func<int, T> converter)
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
	}
}
