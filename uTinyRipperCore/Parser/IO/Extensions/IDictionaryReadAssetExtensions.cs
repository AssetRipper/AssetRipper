using System;
using System.Collections.Generic;

namespace uTinyRipper
{
	public static class IDictionaryReadAssetExtensions
	{
		public static void Read(this IDictionary<int, int> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<int, uint> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<int, string> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<uint, string> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<long, string> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, byte> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, short> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, ushort> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, int> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, uint> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, long> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, ulong> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, float> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<string, string> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read(this IDictionary<Tuple<char, char>, float> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}
		
		public static void Read(this IDictionary<Tuple<byte, byte>, float> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void ReadSafe(this IDictionary<Tuple<byte, byte>, float> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.ReadSafe(_this, reader);
		}

		public static void Read(this IDictionary<Tuple<ushort, ushort>, float> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void ReadSafe(this IDictionary<Tuple<ushort, ushort>, float> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.ReadSafe(_this, reader);
		}

		public static void Read(this IDictionary<Tuple<int, long>, string> _this, AssetReader reader)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader);
		}

		public static void Read<T>(this IDictionary<Tuple<T, long>, string> _this, AssetReader reader, Func<int, T> converter)
		{
			IDictionaryReadEndianExtensions.Read(_this, reader, converter);
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

		public static void Read<T>(this IDictionary<string, T> _this, AssetReader reader)
			where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			for (int i = 0; i < count; i++)
			{
				string key = reader.ReadString();
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
				string key = reader.ReadString();
				T value = instantiator();
				value.Read(reader);
				_this.Add(key, value);
			}
		}

		public static void Read<T1, T2>(this IDictionary<Tuple<T1, long>, T2> _this, AssetReader reader)
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
	}
}
