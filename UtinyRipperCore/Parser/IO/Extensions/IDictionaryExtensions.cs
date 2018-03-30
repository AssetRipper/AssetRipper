using System;
using System.Collections.Generic;

namespace UtinyRipper
{
	public static class IDictionaryUtinyExtensions
	{
		public static void Read(this IDictionary<int, int> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = stream.ReadInt32();
				int value = stream.ReadInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<int, uint> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = stream.ReadInt32();
				uint value = stream.ReadUInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<uint, string> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				uint key = stream.ReadUInt32();
				string value = stream.ReadStringAligned();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, byte> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				byte value = stream.ReadByte();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, short> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				short value = stream.ReadInt16();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, ushort> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				ushort value = stream.ReadUInt16();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, int> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				int value = stream.ReadInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, uint> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				uint value = stream.ReadUInt32();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, long> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				long value = stream.ReadInt64();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, ulong> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				ulong value = stream.ReadUInt64();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, float> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				float value = stream.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<string, string> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				string value = stream.ReadStringAligned();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<Tuple<char, char>, float> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<char, char> key = stream.ReadTupleCharChar();
				float value = stream.ReadSingle();
				_this.Add(key, value);
			}
		}
		
		public static void Read(this IDictionary<Tuple<byte, byte>, float> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<byte, byte> key = stream.ReadTupleByteByte();
				float value = stream.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read(this IDictionary<Tuple<ushort, ushort>, float> _this, AssetStream stream)
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<ushort, ushort> key = stream.ReadTupleUInt16UInt16();
				float value = stream.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<string, T> _this, AssetStream stream)
			where T : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				T value = new T();
				value.Read(stream);
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<string, T> _this, AssetStream stream, Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = stream.ReadStringAligned();
				T value = instantiator();
				value.Read(stream);
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<int, T> _this, AssetStream stream)
			where T : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = stream.ReadInt32();
				T value = new T();
				value.Read(stream);
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<int, T> _this, AssetStream stream, Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				int key = stream.ReadInt32();
				T value = instantiator();
				value.Read(stream);
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<T, float> _this, AssetStream stream)
			where T : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T key = new T();
				key.Read(stream);
				float value = stream.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read<T>(this IDictionary<T, float> _this, AssetStream stream, Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T key = instantiator();
				key.Read(stream);
				float value = stream.ReadSingle();
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<T1, T2> _this, AssetStream stream)
			where T1 : IAssetReadable, new()
			where T2 : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T1 key = new T1();
				key.Read(stream);
				T2 value = new T2();
				value.Read(stream);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<T1, T2> _this, AssetStream stream, Func<T1> keyInstantiator)
			where T1 : IAssetReadable
			where T2 : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T1 key = keyInstantiator();
				key.Read(stream);
				T2 value = new T2();
				value.Read(stream);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<T1, T2> _this, AssetStream stream, Func<T2> valueInstantiator)
			where T1 : IAssetReadable, new()
			where T2 : IAssetReadable
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T1 key = new T1();
				key.Read(stream);
				T2 value = valueInstantiator();
				value.Read(stream);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<T1, T2> _this, AssetStream stream, Func<T1> keyInstantiator, Func<T2> valueInstantiator)
			where T1 : IAssetReadable
			where T2 : IAssetReadable
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				T1 key = keyInstantiator();
				key.Read(stream);
				T2 value = valueInstantiator();
				value.Read(stream);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this Dictionary<Tuple<T1, long>, T2> _this, AssetStream stream)
			where T1 : IAssetReadable, new()
			where T2 : IAssetReadable, new()
		{
			int count = stream.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				Tuple<T1, long> key = stream.ReadTupleTLong<T1>();
				T2 value = new T2();
				value.Read(stream);
				_this.Add(key, value);
			}
		}
	}
}
