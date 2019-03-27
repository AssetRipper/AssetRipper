using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace uTinyRipper
{
	public class EndianReader : BinaryReader
	{
		public EndianReader(Stream stream)
			: this(stream, EndianType.LittleEndian, 0)
		{
		}

		public EndianReader(Stream stream, EndianType endianess) :
			this(stream, endianess, 0)
		{
		}

		public EndianReader(Stream stream, EndianType endianess, long alignPosition) :
		   base(stream, Encoding.UTF8, true)
		{
			EndianType = endianess;
			AlignPosition = alignPosition;
		}

		protected EndianReader(EndianReader reader) :
			this(reader, reader.AlignPosition)
		{
		}

		private EndianReader(EndianReader reader, long alignPosition) :
			this(reader.BaseStream, reader.EndianType, alignPosition)
		{
		}

		~EndianReader()
		{
			Dispose(false);
		}

		public override short ReadInt16()
		{
			FillInnerBuffer(sizeof(short));
			return unchecked((short)BufferToUInt16());
		}

		public override ushort ReadUInt16()
		{
			FillInnerBuffer(sizeof(ushort));
			return BufferToUInt16();
		}

		public override int ReadInt32()
		{
			FillInnerBuffer(sizeof(int));
			return BufferToInt32();
		}

		public override uint ReadUInt32()
		{
			FillInnerBuffer(sizeof(uint));
			return BufferToUInt32();
		}

		public override long ReadInt64()
		{
			FillInnerBuffer(sizeof(long));
			return unchecked((long)BufferToUInt64());
		}

		public override ulong ReadUInt64()
		{
			FillInnerBuffer(sizeof(ulong));
			return BufferToUInt64();
		}

		public override float ReadSingle()
		{
			FillInnerBuffer(sizeof(float));
			return BitConverterExtensions.ToSingle(BufferToUInt32());
		}

		public override double ReadDouble()
		{
			FillInnerBuffer(sizeof(double));
			return BitConverterExtensions.ToDouble(BufferToUInt64());
		}

		public override string ReadString()
		{
			FillInnerBuffer(sizeof(int));
			int length = BufferToInt32();
			byte[] buffer = ReadStringBuffer(length);
			return Encoding.UTF8.GetString(buffer, 0, length);
		}

		public string ReadString(int length)
		{
			byte[] buffer = ReadStringBuffer(length);
			return Encoding.UTF8.GetString(buffer, 0, length);
		}

		/// <summary>
		/// Read C like UTF8 format zero terminated string
		/// </summary>
		/// <returns>Read string</returns>
		public string ReadStringZeroTerm()
		{
			if (ReadStringZeroTerm(m_buffer.Length, out string result))
			{
				return result;
			}
			throw new Exception("Can't find end of string");
		}

		/// <summary>
		/// Read C like UTF8 format zero terminated string
		/// </summary>
		/// <param name="maxLength">Max allowed character count to read</param>
		/// <param name="result">Read string if found</param>
		/// <returns>Whether zero term has been found</returns>
		public bool ReadStringZeroTerm(int maxLength, out string result)
		{
			maxLength = Math.Min(maxLength, m_buffer.Length);
			for (int i = 0; i < maxLength; i++)
			{
				byte bt = ReadByte();
				if (bt == 0)
				{
					result = Encoding.UTF8.GetString(m_buffer, 0, i);
					return true;
				}
				m_buffer[i] = bt;
			}

			result = null;
			return false;
		}

		public int Read(bool[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count;
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i++, index++)
				{
					buffer[index] = m_buffer[i] > 0;
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public int Read(short[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count * sizeof(short);
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i += sizeof(short), index++)
				{
					buffer[index] = unchecked((short)BufferToUInt16(i));
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public int Read(ushort[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count * sizeof(ushort);
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i += sizeof(ushort), index++)
				{
					buffer[index] = BufferToUInt16(i);
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public int Read(int[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count * sizeof(int);
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i += sizeof(int), index++)
				{
					buffer[index] = unchecked((int)BufferToUInt32(i));
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public int Read(uint[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count * sizeof(uint);
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i += sizeof(uint), index++)
				{
					buffer[index] = BufferToUInt32(i);
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public int Read(long[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count * sizeof(long);
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i += sizeof(long), index++)
				{
					buffer[index] = unchecked((long)BufferToUInt64(i));
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public int Read(ulong[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count * sizeof(ulong);
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i += sizeof(ulong), index++)
				{
					buffer[index] = BufferToUInt64(i);
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public int Read(float[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count * sizeof(float);
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i += sizeof(float), index++)
				{
					buffer[index] = BitConverterExtensions.ToSingle(BufferToUInt32(i));
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public int Read(double[] buffer, int index, int count)
		{
			int byteIndex = 0;
			int byteCount = count * sizeof(double);
			int first = index;
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toRead = left < BufferSize ? left : BufferSize;
				int read = Read(m_buffer, 0, toRead);
				for (int i = 0; i < read; i += sizeof(double), index++)
				{
					buffer[index] = BitConverterExtensions.ToDouble(BufferToUInt64(i));
				}
				byteIndex += read;
				if (read < toRead)
				{
					return index - first;
				}
			}
			return count;
		}

		public bool[] ReadBooleanArray()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			bool[] array = new bool[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public char[] ReadCharArray()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			char[] array = new char[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public byte[] ReadByteArray()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			byte[] array = new byte[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public short[] ReadInt16Array()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			short[] array = new short[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public ushort[] ReadUInt16Array()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			ushort[] array = new ushort[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public int[] ReadInt32Array()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			int[] array = new int[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public uint[] ReadUInt32Array()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			uint[] array = new uint[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public long[] ReadInt64Array()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			long[] array = new long[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public ulong[] ReadUInt64Array()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			ulong[] array = new ulong[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public float[] ReadSingleArray()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			float[] array = new float[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public double[] ReadDoubleArray()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			int index = 0;
			double[] array = new double[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			return array;
		}

		public string[] ReadStringArray()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			string[] array = new string[count];
			for (int i = 0; i < count; i++)
			{
				string value = ReadString();
				array[i] = value;
			}
			return array;
		}

		public T ReadEnadian<T>()
			where T : IEndianReadable, new()
		{
			T t = new T();
			t.Read(this);
			return t;
		}

		public T[] ReadEndianArray<T>()
			where T : IEndianReadable, new()
		{
			FillInnerBuffer(4);
			int count = BufferToInt32();
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				T t = new T();
				t.Read(this);
				array[i] = t;
			}
			return array;
		}

		public T[][] ReadEndianArrayArray<T>()
			where T : IEndianReadable, new()
		{
			FillInnerBuffer(sizeof(int));
			int count = BufferToInt32();

			T[][] array = new T[count][];
			for (int i = 0; i < count; i++)
			{
				T[] innerArray = ReadEndianArray<T>();
				array[i] = innerArray;
			}
			return array;
		}

		public void AlignStream(AlignType alignType)
		{
			long align = (long)alignType;
			BaseStream.Position = AlignPosition + ((BaseStream.Position - AlignPosition + align) & ~align);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected ushort BufferToUInt16(int offset = 0)
		{
			return EndianType == EndianType.LittleEndian ?
				unchecked((ushort)((m_buffer[offset + 0] << 0) | (m_buffer[offset + 1] << 8))) :
				unchecked((ushort)((m_buffer[offset + 1] << 0) | (m_buffer[offset + 0] << 8)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected int BufferToInt32(int offset = 0)
		{
			return EndianType == EndianType.LittleEndian ?
				(m_buffer[offset + 0] << 0) | (m_buffer[offset + 1] << 8) | (m_buffer[offset + 2] << 16) | (m_buffer[offset + 3] << 24) :
				(m_buffer[offset + 3] << 0) | (m_buffer[offset + 2] << 8) | (m_buffer[offset + 1] << 16) | (m_buffer[offset + 0] << 24);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected uint BufferToUInt32(int offset = 0)
		{
			return EndianType == EndianType.LittleEndian ?
				unchecked((uint)((m_buffer[offset + 0] << 0) | (m_buffer[offset + 1] << 8) | (m_buffer[offset + 2] << 16) | (m_buffer[offset + 3] << 24))) :
				unchecked((uint)((m_buffer[offset + 3] << 0) | (m_buffer[offset + 2] << 8) | (m_buffer[offset + 1] << 16) | (m_buffer[offset + 0] << 24)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected ulong BufferToUInt64(int offset = 0)
		{
			if (EndianType == EndianType.LittleEndian)
			{
				uint value1 = unchecked((uint)((m_buffer[offset + 0] << 0) | (m_buffer[offset + 1] << 8) | (m_buffer[offset + 2] << 16) | (m_buffer[offset + 3] << 24)));
				uint value2 = unchecked((uint)((m_buffer[offset + 4] << 0) | (m_buffer[offset + 5] << 8) | (m_buffer[offset + 6] << 16) | (m_buffer[offset + 7] << 24)));
				return ((ulong)value1 << 0) | ((ulong)value2 << 32);
			}
			else
			{
				uint value1 = unchecked((uint)((m_buffer[offset + 7] << 0) | (m_buffer[offset + 6] << 8) | (m_buffer[offset + 5] << 16) | (m_buffer[offset + 4] << 24)));
				uint value2 = unchecked((uint)((m_buffer[offset + 3] << 0) | (m_buffer[offset + 2] << 8) | (m_buffer[offset + 1] << 16) | (m_buffer[offset + 0] << 24)));
				return ((ulong)value1 << 0) | ((ulong)value2 << 32);
			}
		}

		protected byte[] ReadStringBuffer(int size)
		{
			if (m_buffer.Length >= size)
			{
				FillInnerBuffer(size);
				return m_buffer;
			}
			else
			{
				byte[] buffer = new byte[size];
				int offset = 0;
				int count = size;
				while (count > 0)
				{
					int read = Read(buffer, offset, count);
					if (read == 0)
					{
						throw new Exception($"End of stream. Read {offset}, expected {size} bytes");
					}
					offset += read;
					count -= read;
				}
				return buffer;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FillInnerBuffer(int size)
		{
			int offset = 0;
			int count = size;
			while (count > 0)
			{
				int read = Read(m_buffer, offset, count);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {offset}, expected {size} bytes");
				}
				offset += read;
				count -= read;
			}
		}

		public EndianType EndianType { get; }

		protected long AlignPosition { get; }

		protected const int BufferSize = 4096;

		private readonly byte[] m_buffer = new byte[BufferSize];
	}
}
