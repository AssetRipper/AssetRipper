using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AssetRipper.Core.IO.Extensions
{
	public static class BinaryReaderExtensions
	{
		/// <summary>
		/// Reads the specified number of bytes from the stream, starting from a specified point in the byte array.
		/// </summary>
		/// <param name="_this">The binary reader to read from.</param>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The starting point in the buffer at which to begin reading into the buffer.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <exception cref="System.ArgumentException"></exception> 
		/// <exception cref="System.ArgumentNullException">buffer is null</exception> 
		/// <exception cref="System.ArgumentOutOfRangeException">index or count is negative</exception> 
		/// <exception cref="System.ObjectDisposedException">The stream is closed</exception> 
		/// <exception cref="System.IO.IOException">An I/O error occurred</exception> 
		public static void ReadBuffer(this BinaryReader _this, byte[] buffer, int offset, int count)
		{
			do
			{
				int read = _this.Read(buffer, offset, count);
				if (read == 0)
				{
					throw new IOException($"No data left");
				}
				offset += read;
				count -= read;
			}
			while (count > 0);
		}

		public static void AlignStream(this BinaryReader reader) => reader.BaseStream.Align();
		public static void AlignStream(this BinaryReader reader, int alignment) => reader.BaseStream.Align(alignment);

		public static string ReadAlignedString(this BinaryReader reader)
		{
			int length = reader.ReadInt32();
			if (length > 0 && length <= reader.BaseStream.Length - reader.BaseStream.Position)
			{
				byte[] stringData = reader.ReadBytes(length);
				string result = Encoding.UTF8.GetString(stringData);
				reader.AlignStream(4);
				return result;
			}
			return "";
		}

		public static string ReadStringToNull(this BinaryReader reader, int maxLength = 32767)
		{
			List<byte> bytes = new List<byte>();
			int count = 0;
			while (reader.BaseStream.Position != reader.BaseStream.Length && count < maxLength)
			{
				byte b = reader.ReadByte();
				if (b == 0)
				{
					break;
				}
				bytes.Add(b);
				count++;
			}
			return Encoding.UTF8.GetString(bytes.ToArray());
		}

		private static T[] ReadArray<T>(Func<T> del, int length)
		{
			T[] array = new T[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = del();
			}
			return array;
		}

		public static bool[] ReadBooleanArray(this BinaryReader reader)
		{
			return ReadArray(reader.ReadBoolean, reader.ReadInt32());
		}

		public static byte[] ReadUInt8Array(this BinaryReader reader)
		{
			return reader.ReadBytes(reader.ReadInt32());
		}

		public static ushort[] ReadUInt16Array(this BinaryReader reader)
		{
			return ReadArray(reader.ReadUInt16, reader.ReadInt32());
		}

		public static int[] ReadInt32Array(this BinaryReader reader)
		{
			return ReadArray(reader.ReadInt32, reader.ReadInt32());
		}

		public static int[] ReadInt32Array(this BinaryReader reader, int length)
		{
			return ReadArray(reader.ReadInt32, length);
		}

		public static uint[] ReadUInt32Array(this BinaryReader reader)
		{
			return ReadArray(reader.ReadUInt32, reader.ReadInt32());
		}

		public static uint[][] ReadUInt32ArrayArray(this BinaryReader reader)
		{
			return ReadArray(reader.ReadUInt32Array, reader.ReadInt32());
		}

		public static uint[] ReadUInt32Array(this BinaryReader reader, int length)
		{
			return ReadArray(reader.ReadUInt32, length);
		}

		public static float[] ReadSingleArray(this BinaryReader reader)
		{
			return ReadArray(reader.ReadSingle, reader.ReadInt32());
		}

		public static float[] ReadSingleArray(this BinaryReader reader, int length)
		{
			return ReadArray(reader.ReadSingle, length);
		}

		public static string[] ReadStringArray(this BinaryReader reader)
		{
			return ReadArray(reader.ReadAlignedString, reader.ReadInt32());
		}
	}
}
