using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace AssetRipper.Core.IO.Endian
{
	public class EndianReader : BinaryReader
	{
		private bool isBigEndian = false;
		public EndianType EndianType
		{
			get => isBigEndian ? EndianType.BigEndian : EndianType.LittleEndian;
			set => isBigEndian = value == EndianType.BigEndian;
		}

		protected bool IsAlignArray { get; }

		protected const int BufferSize = 4096;

		private readonly byte[] m_buffer = new byte[BufferSize];

		public EndianReader(Stream stream, EndianType endianess) : this(stream, endianess, false) { }

		protected EndianReader(Stream stream, EndianType endianess, bool alignArray) : base(stream, Encoding.UTF8, true)
		{
			EndianType = endianess;
			IsAlignArray = alignArray;
		}

		~EndianReader()
		{
			Dispose(false);
		}

		public override char ReadChar()
		{
			return (char)ReadUInt16();
		}

		public override short ReadInt16()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadInt16BigEndian(base.ReadBytes(2));
			else
				return base.ReadInt16();
		}

		public override ushort ReadUInt16()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadUInt16BigEndian(base.ReadBytes(2));
			else
				return base.ReadUInt16();
		}

		public override int ReadInt32()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadInt32BigEndian(base.ReadBytes(4));
			else
				return base.ReadInt32();
		}

		public override uint ReadUInt32()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadUInt32BigEndian(base.ReadBytes(4));
			else
				return base.ReadUInt32();
		}

		public override long ReadInt64()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadInt64BigEndian(base.ReadBytes(8));
			else
				return base.ReadInt64();
		}

		public override ulong ReadUInt64()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadUInt64BigEndian(base.ReadBytes(8));
			else
				return base.ReadUInt64();
		}

		public override Half ReadHalf()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadHalfBigEndian(base.ReadBytes(2));
			else
				return base.ReadHalf();
		}

		public override float ReadSingle()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadSingleBigEndian(base.ReadBytes(4));
			else
				return base.ReadSingle();
		}

		public override double ReadDouble()
		{
			if (isBigEndian)
				return BinaryPrimitives.ReadDoubleBigEndian(base.ReadBytes(8));
			else
				return base.ReadDouble();
		}

		public override decimal ReadDecimal()
		{
			if (isBigEndian)
				throw new NotSupportedException();
			else
				return base.ReadDecimal();
		}

		public override string ReadString()
		{
			int length = ReadInt32();
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
			maxLength = System.Math.Min(maxLength, m_buffer.Length);
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

		public bool[] ReadBooleanArray() => ReadBooleanArray(true);
		public bool[] ReadBooleanArray(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			bool[] array = count == 0 ? Array.Empty<bool>() : new bool[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadBoolean();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public char[] ReadCharArray() => ReadCharArray(true);
		public char[] ReadCharArray(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			char[] array = count == 0 ? Array.Empty<char>() : new char[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadChar();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public byte[] ReadByteArray() => ReadByteArray(true);
		public byte[] ReadByteArray(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			byte[] array = count == 0 ? Array.Empty<byte>() : new byte[count];
			while (index < count)
			{
				int read = Read(array, index, count - index);
				if (read == 0)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements");
				}
				index += read;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public sbyte[] ReadSByteArray() => ReadSByteArray(true);
		public sbyte[] ReadSByteArray(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			sbyte[] array = count == 0 ? Array.Empty<sbyte>() : new sbyte[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadSByte();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public short[] ReadInt16Array() => ReadInt16Array(true);
		public short[] ReadInt16Array(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			short[] array = count == 0 ? Array.Empty<short>() : new short[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadInt16();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public ushort[] ReadUInt16Array() => ReadUInt16Array(true);
		public ushort[] ReadUInt16Array(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			ushort[] array = count == 0 ? Array.Empty<ushort>() : new ushort[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadUInt16();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public int[] ReadInt32Array() => ReadInt32Array(true);
		public int[] ReadInt32Array(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			int[] array = count == 0 ? Array.Empty<int>() : new int[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadInt32();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public uint[] ReadUInt32Array() => ReadUInt32Array(true);
		public uint[] ReadUInt32Array(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			uint[] array = count == 0 ? Array.Empty<uint>() : new uint[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadUInt32();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public long[] ReadInt64Array() => ReadInt64Array(true);
		public long[] ReadInt64Array(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			long[] array = count == 0 ? Array.Empty<long>() : new long[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadInt64();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public ulong[] ReadUInt64Array() => ReadUInt64Array(true);
		public ulong[] ReadUInt64Array(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			ulong[] array = count == 0 ? Array.Empty<ulong>() : new ulong[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadUInt64();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public float[] ReadSingleArray() => ReadSingleArray(true);
		public float[] ReadSingleArray(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			float[] array = count == 0 ? Array.Empty<float>() : new float[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadSingle();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public double[] ReadDoubleArray() => ReadDoubleArray(true);
		public double[] ReadDoubleArray(bool allowAlignment)
		{
			int count = ReadInt32();
			int index = 0;
			double[] array = count == 0 ? Array.Empty<double>() : new double[count];
			while (index < count)
			{
				try
				{
					array[index] = ReadDouble();
				}
				catch (Exception ex)
				{
					throw new Exception($"End of stream. Read {index}, expected {count} elements", ex);
				}
				index++;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public string[] ReadStringArray() => ReadStringArray(true);
		public string[] ReadStringArray(bool allowAlignment)
		{
			int count = ReadInt32();
			string[] array = count == 0 ? Array.Empty<string>() : new string[count];
			for (int i = 0; i < count; i++)
			{
				string value = ReadString();
				array[i] = value;
			}
			if (allowAlignment && IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public T ReadEndian<T>() where T : IEndianReadable, new()
		{
			T t = new T();
			t.Read(this);
			return t;
		}

		public T[] ReadEndianArray<T>() where T : IEndianReadable, new()
		{
			int count = ReadInt32();
			T[] array = count == 0 ? Array.Empty<T>() : new T[count];
			for (int i = 0; i < count; i++)
			{
				T t = new T();
				t.Read(this);
				array[i] = t;
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public T[][] ReadEndianArrayArray<T>() where T : IEndianReadable, new()
		{
			int count = ReadInt32();
			T[][] array = count == 0 ? Array.Empty<T[]>() : new T[count][];
			for (int i = 0; i < count; i++)
			{
				T[] innerArray = ReadEndianArray<T>();
				array[i] = innerArray;
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
			return array;
		}

		public void AlignStream()
		{
			BaseStream.Position = (BaseStream.Position + 3) & ~3;
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
	}
}
