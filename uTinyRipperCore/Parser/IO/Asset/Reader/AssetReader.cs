using System;
using System.IO;
using System.Text;

namespace uTinyRipper
{
	public sealed class AssetReader : EndianReader
	{
		public AssetReader(Stream stream, Version version, Platform platform, TransferInstructionFlags flags) :
			base(stream)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}

		public AssetReader(EndianReader reader, Version version, Platform platform, TransferInstructionFlags flags) :
			base(reader)
		{
			Version = version;
			Platform = platform;
			Flags = flags;
		}

		public override char ReadChar()
		{
			FillInnerBuffer(sizeof(char));
			return (char)BufferToUInt16();
		}

		public override string ReadString()
		{
			FillInnerBuffer(sizeof(int));
			int length = BufferToInt32();

			byte[] buffer = ReadStringBuffer(length);
			string result = Encoding.UTF8.GetString(buffer, 0, length);
			if (Version.IsGreaterEqual(2, 1))
			{
				AlignStream(AlignType.Align4);
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

			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				T instance = new T();
				instance.Read(this);
				array[i] = instance;
			}
			return array;
		}

		public T[][] ReadAssetArrayArray<T>()
			where T : IAssetReadable, new()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			T[][] array = new T[count][];
			for (int i = 0; i < count; i++)
			{
				T[] innerArray = ReadAssetArray<T>();
				array[i] = innerArray;
			}
			return array;
		}

		public override string ToString()
		{
			return $"{nameof(AssetReader)} ({Platform} {Version})";
		}

		public Platform Platform { get; }
		public TransferInstructionFlags Flags { get; }

		public Version Version;
	}
}
