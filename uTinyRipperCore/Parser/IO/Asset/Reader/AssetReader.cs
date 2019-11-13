using System;
using System.IO;
using System.Text;

namespace uTinyRipper
{
	public sealed class AssetReader : EndianReader
	{
		public AssetReader(Stream stream, EndianType endian, Version version, Platform platform, TransferInstructionFlags flags) :
			base(stream, endian, AlignArrays(version))
		{
			Version = version;
			Platform = platform;
			Flags = flags;
			IsAlignString = AlignStrings(version);
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		public static bool AlignStrings(Version version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool AlignArrays(Version version) => version.IsGreaterEqual(2017);

		public override char ReadChar()
		{
			FillInnerBuffer(sizeof(char));
			return (char)BufferToUInt16();
		}

		public override string ReadString()
		{
			FillInnerBuffer(sizeof(int));
			int length = BufferToInt32();
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

		public override int Read(char[] buffer, int index, int count)
		{
			count += index;
			while (index < count)
			{
				int toRead = Math.Min((count - index) * sizeof(char), BufferSize);
				FillInnerBuffer(toRead);
				for (int i = 0; i < toRead; i += sizeof(char), index++)
				{
					buffer[index] = (char)BufferToUInt16(i);
				}
			}
			return count;
		}

		public T ReadAsset<T>()
			where T : IAssetReadable, new()
		{
			T t = new T();
			t.Read(this);
			return t;
		}

		public T[] ReadAssetArray<T>()
			where T : IAssetReadable, new()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();
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

		public T[][] ReadAssetArrayArray<T>()
			where T : IAssetReadable, new()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();
			T[][] array = count == 0 ? Array.Empty<T[]>() : new T[count][];
			for (int i = 0; i < count; i++)
			{
				T[] innerArray = ReadAssetArray<T>();
				array[i] = innerArray;
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public override string ToString()
		{
			return $"{nameof(AssetReader)} ({Platform} {Version})";
		}

		public Platform Platform { get; }
		public TransferInstructionFlags Flags { get; }

		private bool IsAlignString { get; }

		public Version Version;
	}
}
