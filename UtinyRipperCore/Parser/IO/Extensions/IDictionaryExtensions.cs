using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public static class IDictionaryUtinyExtensions
	{
		public static void Read(this IDictionary<int, int> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = reader.ReadInt32();
				int value = reader.ReadInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<int, uint> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = reader.ReadInt32();
				uint value = reader.ReadUInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<uint, string> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				uint key = reader.ReadUInt32();
				string value = reader.ReadStringAligned();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, byte> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				byte value = reader.ReadByte();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, short> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				short value = reader.ReadInt16();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, ushort> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				ushort value = reader.ReadUInt16();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, int> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				int value = reader.ReadInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, uint> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				uint value = reader.ReadUInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, long> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				long value = reader.ReadInt64();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, ulong> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				ulong value = reader.ReadUInt64();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, float> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				float value = reader.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, string> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				string value = reader.ReadStringAligned();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<Tuple<char, char>, float> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<char, char> key = reader.ReadTupleCharChar();
				float value = reader.ReadSingle();
				_this.Add(key, value);
			}
		}
		
		public static void Read(this IDictionary<Tuple<byte, byte>, float> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<byte, byte> key = reader.ReadTupleByteByte();
				float value = reader.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<Tuple<ushort, ushort>, float> _this, AssetReader reader)
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<ushort, ushort> key = reader.ReadTupleUInt16UInt16();
				float value = reader.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<string, T> _this, AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				T value = new T();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<string, T> _this, AssetReader reader, Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadStringAligned();
				T value = instantiator();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<int, T> _this, AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = reader.ReadInt32();
				T value = new T();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<int, T> _this, AssetReader reader, Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = reader.ReadInt32();
				T value = instantiator();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<T, int> _this, AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T key = new T();
				key.Read(reader);
				int value = reader.ReadInt32();
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<T, float> _this, AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T key = new T();
				key.Read(reader);
				float value = reader.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<T, float> _this, AssetReader reader, Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T key = instantiator();
				key.Read(reader);
				float value = reader.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<T1, T2> _this, AssetReader reader)
			where T1 : IAssetReadable, new()
			where T2 : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T1 key = new T1();
				key.Read(reader);
				T2 value = new T2();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<T1, T2> _this, AssetReader reader, Func<T1> keyInstantiator)
			where T1 : IAssetReadable
			where T2 : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T1 key = keyInstantiator();
				key.Read(reader);
				T2 value = new T2();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<T1, T2> _this, AssetReader reader, Func<T2> valueInstantiator)
			where T1 : IAssetReadable, new()
			where T2 : IAssetReadable
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T1 key = new T1();
				key.Read(reader);
				T2 value = valueInstantiator();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<T1, T2> _this, AssetReader reader, Func<T1> keyInstantiator, Func<T2> valueInstantiator)
			where T1 : IAssetReadable
			where T2 : IAssetReadable
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T1 key = keyInstantiator();
				key.Read(reader);
				T2 value = valueInstantiator();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this Dictionary<Tuple<T1, long>, T2> _this, AssetReader reader)
			where T1 : IAssetReadable, new()
			where T2 : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<T1, long> key = reader.ReadTupleTLong<T1>();
				T2 value = new T2();
				value.Read(reader);
				_this.Add(key, value);
			}
		}
	}
}
