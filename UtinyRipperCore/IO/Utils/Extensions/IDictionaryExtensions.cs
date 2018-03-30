using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public static class IDictionaryExtensions
	{
		public static void Read(this IDictionary<int, int> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = stream.ReadInt32();
				int value = stream.ReadInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<uint, string> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				uint key = stream.ReadUInt32();
				string value = stream.ReadStringAligned();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, byte> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				byte value = stream.ReadByte();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, short> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				short value = stream.ReadInt16();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, ushort> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				ushort value = stream.ReadUInt16();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, int> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				int value = stream.ReadInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, uint> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				uint value = stream.ReadUInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, long> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				long value = stream.ReadInt64();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, ulong> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				ulong value = stream.ReadUInt64();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, float> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				float value = stream.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, string> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				string value = stream.ReadStringAligned();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<Tuple<char, char>, float> _this, EndianStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<char, char> key = stream.ReadTupleCharChar();
				float value = stream.ReadSingle();
				_this.Add(key, value);
			}
		}
	}
}
