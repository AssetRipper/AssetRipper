using System;
using System.IO;
using System.Text;

namespace UtinyRipper
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

		public AssetReader(AssetReader reader, long alignPosition):
			base(reader, alignPosition)
		{
			Version = reader.Version;
			Platform = reader.Platform;
			Flags = reader.Flags;
		}

		public new T[] ReadArray<T>()
			where T : IAssetReadable, new()
		{
			int count = ReadInt32();
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				T instance = new T();
				instance.Read(this);
				array[i] = instance;
			}
			return array;
		}

		public T[][] ReadArrayDouble<T>()
			where T : IAssetReadable, new()
		{
			int count = ReadInt32();
			T[][] array = new T[count][];
			for (int i = 0; i < count; i++)
			{
				T[] innerArray = ReadArray<T>();
				array[i] = innerArray;
			}
			return array;
		}

		public T[] ReadArray<T>(Func<T> instantiator)
			where T : IAssetReadable
		{
			int count = ReadInt32();
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				T instance = instantiator();
				instance.Read(this);
				array[i] = instance;
			}
			return array;
		}

		public override string ReadStringAligned(int length)
		{
			byte[] buffer = ReadStringBuffer(length);
			string result = Encoding.UTF8.GetString(buffer, 0, length);
			if (Version.IsGreaterEqual(2, 1))
			{
				AlignStream(AlignType.Align4);
			}
			return result;
		}

		public Version Version { get; }
		public Platform Platform { get; }
		public TransferInstructionFlags Flags { get; }
	}
}
