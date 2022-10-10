using AssetRipper.Assets.Generics;
using AssetRipper.Assets.IO.Reading;

namespace AssetRipper.Core.IO.Extensions
{
	public static class AssetReaderExtensions
	{
		public static T[] ReadAssetArray<T>(this AssetReader reader) where T : IAssetReadable, new() => reader.ReadAssetArray<T>(true);
		public static T[] ReadAssetArray<T>(this AssetReader reader, bool allowAlignment) where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			T[] array = count == 0 ? Array.Empty<T>() : new T[count];
			for (int i = 0; i < count; i++)
			{
				T instance = new T();
				instance.Read(reader);
				array[i] = instance;
			}
			if (reader.IsAlignArray)
			{
				reader.AlignStream();
			}
			return array;
		}

		public static T[][] ReadAssetArrayArray<T>(this AssetReader reader) where T : IAssetReadable, new() => reader.ReadAssetArrayArray<T>(true);
		public static T[][] ReadAssetArrayArray<T>(this AssetReader reader, bool allowAlignment) where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			T[][] array = count == 0 ? Array.Empty<T[]>() : new T[count][];
			for (int i = 0; i < count; i++)
			{
				T[] innerArray = reader.ReadAssetArray<T>();
				array[i] = innerArray;
			}
			if (allowAlignment && reader.IsAlignArray)
			{
				reader.AlignStream();
			}
			return array;
		}

		public static AssetList<T> ReadAssetList<T>(this AssetReader reader) where T : IAssetReadable, new() => reader.ReadAssetList<T>(true);
		public static AssetList<T> ReadAssetList<T>(this AssetReader reader, bool allowAlignment) where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			AssetList<T> list = new AssetList<T>();
			for (int i = 0; i < count; i++)
			{
				T instance = new T();
				instance.Read(reader);
				list.Add(instance);
			}
			if (reader.IsAlignArray)
			{
				reader.AlignStream();
			}
			return list;
		}

		public static AssetList<AssetList<T>> ReadAssetListList<T>(this AssetReader reader) where T : IAssetReadable, new() => reader.ReadAssetListList<T>(true);
		public static AssetList<AssetList<T>> ReadAssetListList<T>(this AssetReader reader, bool allowAlignment) where T : IAssetReadable, new()
		{
			int count = reader.ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			AssetList<AssetList<T>> list = new AssetList<AssetList<T>>();
			for (int i = 0; i < count; i++)
			{
				AssetList<T> innerList = reader.ReadAssetList<T>();
				list.Add(innerList);
			}
			if (allowAlignment && reader.IsAlignArray)
			{
				reader.AlignStream();
			}
			return list;
		}
	}
}
