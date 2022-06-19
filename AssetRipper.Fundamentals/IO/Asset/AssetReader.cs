using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Files;
using AssetRipper.IO.Endian;
using System.IO;
using System.Text;


namespace AssetRipper.Core.IO.Asset
{
	public sealed class AssetReader : EndianReader
	{
		public AssetReader(Stream stream, EndianType endian, LayoutInfo info) : this(new AdjustableStream(stream), endian, info) { }
		private AssetReader(AdjustableStream stream, EndianType endian, LayoutInfo info) : base(stream, endian, info.IsAlignArrays)
		{
			Info = info;
			IsAlignString = info.IsAlign;
			AdjustableStream = stream;
		}

		public override string ReadString()
		{
			int length = ReadInt32();
			if (length == 0)
			{
				return string.Empty;
			}

			byte[] buffer = ReadStringBuffer(length);
			string result = Encoding.UTF8.GetString(buffer, 0, length);
			if (IsAlignString)
			{
				AlignStream();
			}
			return result;
		}

		public T ReadAsset<T>() where T : IAssetReadable, new()
		{
			T t = new T();
			t.Read(this);
			return t;
		}

		public T[] ReadAssetArray<T>() where T : IAssetReadable, new() => ReadAssetArray<T>(true);
		public T[] ReadAssetArray<T>(bool allowAlignment) where T : IAssetReadable, new()
		{
			int count = ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			T[] array = count == 0 ? Array.Empty<T>() : new T[count];
			for (int i = 0; i < count; i++)
			{
				T instance = new T();
				instance.Read(this);
				array[i] = instance;
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public T[][] ReadAssetArrayArray<T>() where T : IAssetReadable, new() => ReadAssetArrayArray<T>(true);
		public T[][] ReadAssetArrayArray<T>(bool allowAlignment) where T : IAssetReadable, new()
		{
			int count = ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			T[][] array = count == 0 ? Array.Empty<T[]>() : new T[count][];
			for (int i = 0; i < count; i++)
			{
				T[] innerArray = ReadAssetArray<T>();
				array[i] = innerArray;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public AssetList<T> ReadAssetList<T>() where T : IAssetReadable, new() => ReadAssetList<T>(true);
		public AssetList<T> ReadAssetList<T>(bool allowAlignment) where T : IAssetReadable, new()
		{
			int count = ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			AssetList<T> list = new AssetList<T>();
			for (int i = 0; i < count; i++)
			{
				T instance = new T();
				instance.Read(this);
				list.Add(instance);
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
			return list;
		}

		public AssetList<AssetList<T>> ReadAssetListList<T>() where T : IAssetReadable, new() => ReadAssetListList<T>(true);
		public AssetList<AssetList<T>> ReadAssetListList<T>(bool allowAlignment) where T : IAssetReadable, new()
		{
			int count = ReadInt32();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), $"Cannot be negative: {count}");
			}

			AssetList<AssetList<T>> list = new AssetList<AssetList<T>>();
			for (int i = 0; i < count; i++)
			{
				AssetList<T> innerList = ReadAssetList<T>();
				list.Add(innerList);
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return list;
		}

		public override string ToString()
		{
			return $"{nameof(AssetReader)} ({Platform} {Version})";
		}

		public LayoutInfo Info { get; }
		public UnityVersion Version => Info.Version;
		public BuildTarget Platform => Info.Platform;
		public TransferInstructionFlags Flags => Info.Flags;
		public AdjustableStream AdjustableStream { get; }

		private bool IsAlignString { get; }
	}
}
