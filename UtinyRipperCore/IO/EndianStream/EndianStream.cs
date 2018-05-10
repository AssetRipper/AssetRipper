using System;
using System.IO;
using System.Text;

namespace UtinyRipper
{
	public class EndianStream : BinaryReader
	{
		public EndianStream(Stream stream)
			: this(stream, EndianType.LittleEndian)
		{
		}

		public EndianStream(Stream stream, EndianType endType):
			this(stream, 0, endType)
		{
		}

		public EndianStream(Stream stream, long alignPosition, EndianType endType) :
		   base(stream, Encoding.Default, true)
		{
			EndianType = endType;
			AlignPosition = alignPosition;
		}

		public override short ReadInt16()
		{
			if (EndianType == EndianType.BigEndian)
			{
				const int readBytes = 2;
				int read = Read(m_buffer16, 0, readBytes);
				if(read < readBytes)
				{
					throw new Exception($"Read {read} bytes intead of {readBytes}");
				}
				Array.Reverse(m_buffer16);
				return BitConverter.ToInt16(m_buffer16, 0);
			}

			return base.ReadInt16();
		}

		public override ushort ReadUInt16()
		{
			if (EndianType == EndianType.BigEndian)
			{
				const int readBytes = 2;
				int read = Read(m_buffer16, 0, readBytes);
				if (read < readBytes)
				{
					throw new Exception($"Read {read} bytes intead of {readBytes}");
				}
				Array.Reverse(m_buffer16);
				return BitConverter.ToUInt16(m_buffer16, 0);
			}

			return base.ReadUInt16();
		}

		public override int ReadInt32()
		{
			if (EndianType == EndianType.BigEndian)
			{
				const int readBytes = 4;
				int read = Read(m_buffer32, 0, readBytes);
				if (read < readBytes)
				{
					throw new Exception($"Read {read} bytes intead of {readBytes}");
				}
				Array.Reverse(m_buffer32);
				return BitConverter.ToInt32(m_buffer32, 0);
			}

			return base.ReadInt32();
		}

		public override uint ReadUInt32()
		{
			if (EndianType == EndianType.BigEndian)
			{
				const int readBytes = 4;
				int read = Read(m_buffer32, 0, readBytes);
				if (read < readBytes)
				{
					throw new Exception($"Read {read} bytes intead of {readBytes}");
				}
				Array.Reverse(m_buffer32);
				return BitConverter.ToUInt32(m_buffer32, 0);
			}

			return base.ReadUInt32();
		}

		public override long ReadInt64()
		{
			if (EndianType == EndianType.BigEndian)
			{
				const int readBytes = 8;
				int read = Read(m_buffer64, 0, readBytes);
				if (read < readBytes)
				{
					throw new Exception($"Read {read} bytes intead of {readBytes}");
				}
				Array.Reverse(m_buffer64);
				return BitConverter.ToInt64(m_buffer64, 0);
			}

			return base.ReadInt64();
		}

		public override ulong ReadUInt64()
		{
			if (EndianType == EndianType.BigEndian)
			{
				const int readBytes = 8;
				int read = Read(m_buffer64, 0, readBytes);
				if (read < readBytes)
				{
					throw new Exception($"Read {read} bytes intead of {readBytes}");
				}
				Array.Reverse(m_buffer64);
				return BitConverter.ToUInt64(m_buffer64, 0);
			}

			return base.ReadUInt64();
		}

		public override float ReadSingle()
		{
			if(EndianType == EndianType.BigEndian)
			{
				const int readBytes = 4;
				int read = Read(m_buffer32, 0, readBytes);
				if (read < readBytes)
				{
					throw new Exception($"Read {read} bytes intead of {readBytes}");
				}
				Array.Reverse(m_buffer32);
				return BitConverter.ToSingle(m_buffer32, 0);
			}

			return base.ReadSingle();
		}

		public override double ReadDouble()
		{
			if (EndianType == EndianType.BigEndian)
			{
				const int readBytes = 8;
				int read = Read(m_buffer64, 0, readBytes);
				if (read < readBytes)
				{
					throw new Exception($"Read {read} bytes intead of {readBytes}");
				}
				Array.Reverse(m_buffer64);
				return BitConverter.ToSingle(m_buffer64, 0);
			}

			return base.ReadDouble();
		}

		public override string ReadString()
		{
			// just to be sure ReadStringAligned is called
			throw new NotSupportedException();
		}

		public string ReadString(int length)
		{
			byte[] buffer = ReadStringBuffer(length);
			return Encoding.UTF8.GetString(buffer, 0, length);
		}

		public string ReadStringAligned()
		{
			int length = ReadInt32();
			return ReadStringAligned(length);
		}

		public virtual string ReadStringAligned(int length)
		{
			byte[] buffer = ReadStringBuffer(length);
			string result = Encoding.UTF8.GetString(buffer, 0, length);
			AlignStream(AlignType.Align4);
			return result;
		}
		
		/// <summary>
		/// Read C like UTF8 format zero terminated string
		/// </summary>
		/// <returns></returns>
		public string ReadStringZeroTerm()
		{
			int i;
			for(i = 0; i < m_bufferString.Length; i++)
			{
				byte character = ReadByte();
				if(character == 0)
				{
					break;
				}
				m_bufferString[i] = character;
			}
			if(i == m_bufferString.Length)
			{
				throw new Exception("Can't find end of string");
			}
			return Encoding.UTF8.GetString(m_bufferString, 0, i);
		}

		public bool[] ReadBooleanArray()
		{
			int count = ReadInt32();
			bool[] array = new bool[count];
			for (int i = 0; i < count; i++)
			{
				bool value = ReadBoolean();
				array[i] = value;
			}
			return array;
		}

		public byte[] ReadByteArray()
		{
			int count = ReadInt32();
			byte[] array = new byte[count];
			int read = Read(array, 0, count);
			if (read != count)
			{
				throw new Exception($"Read {read} but expected {count}");
			}
			return array;
		}

		public short[] ReadInt16Array()
		{
			int count = ReadInt32();
			short[] array = new short[count];
			for (int i = 0; i < count; i++)
			{
				short value = ReadInt16();
				array[i] = value;
			}
			return array;
		}

		public ushort[] ReadUInt16Array()
		{
			int count = ReadInt32();
			ushort[] array = new ushort[count];
			for (int i = 0; i < count; i++)
			{
				ushort value = ReadUInt16();
				array[i] = value;
			}
			return array;
		}

		public int[] ReadInt32Array()
		{
			int count = ReadInt32();
			int[] array = new int[count];
			for (int i = 0; i < count; i++)
			{
				int value = ReadInt32();
				array[i] = value;
			}
			return array;
		}

		public uint[] ReadUInt32Array()
		{
			int count = ReadInt32();
			uint[] array = new uint[count];
			for (int i = 0; i < count; i++)
			{
				uint value = ReadUInt32();
				array[i] = value;
			}
			return array;
		}

		public long[] ReadInt64Array()
		{
			int count = ReadInt32();
			long[] array = new long[count];
			for (int i = 0; i < count; i++)
			{
				long value = ReadInt64();
				array[i] = value;
			}
			return array;
		}

		public ulong[] ReadUInt64Array()
		{
			int count = ReadInt32();
			ulong[] array = new ulong[count];
			for (int i = 0; i < count; i++)
			{
				ulong value = ReadUInt64();
				array[i] = value;
			}
			return array;
		}

		public float[] ReadSingleArray()
		{
			int count = ReadInt32();
			float[] array = new float[count];
			for (int i = 0; i < count; i++)
			{
				float value = ReadSingle();
				array[i] = value;
			}
			return array;
		}

		public double[] ReadDoubleArray()
		{
			int count = ReadInt32();
			double[] array = new double[count];
			for (int i = 0; i < count; i++)
			{
				double value = ReadDouble();
				array[i] = value;
			}
			return array;
		}

		public string[] ReadStringArray()
		{
			int count = ReadInt32();
			string[] array = new string[count];
			for (int i = 0; i < count; i++)
			{
				string value = ReadStringAligned();
				array[i] = value;
			}
			return array;
		}

		public void Read(short[] buffer, int index, int count)
		{
			for (int i = 0, j = index; i < count; i++, j++)
			{
				buffer[j] = ReadInt16();
			}
		}

		public void Read(ushort[] buffer, int index, int count)
		{
			for (int i = 0, j = index; i < count; i++, j++)
			{
				buffer[j] = ReadUInt16();
			}
		}

		public void Read(int[] buffer, int index, int count)
		{
			for (int i = 0, j = index; i < count; i++, j++)
			{
				buffer[j] = ReadInt32();
			}
		}

		public void Read(uint[] buffer, int index, int count)
		{
			for (int i = 0, j = index; i < count; i++, j++)
			{
				buffer[j] = ReadUInt32();
			}
		}

		public void Read(long[] buffer, int index, int count)
		{
			for (int i = 0, j = index; i < count; i++, j++)
			{
				buffer[j] = ReadInt64();
			}
		}

		public void Read(ulong[] buffer, int index, int count)
		{
			for (int i = 0, j = index; i < count; i++, j++)
			{
				buffer[j] = ReadUInt64();
			}
		}

		public void Read(float[] buffer, int index, int count)
		{
			for(int i = 0, j = index; i < count; i++, j++)
			{
				buffer[j] = ReadSingle();
			}
		}

		public T[] ReadArray<T>()
			where T: IEndianReadable, new()
		{
			int count = ReadInt32();
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				T t = new T();
				t.Read(this);
				array[i] = t;
			}
			return array;
		}

		public void AlignStream(AlignType alignType)
		{
			if(BaseStream.Position < 0)
			{
				throw new Exception($"Unsupported stream position '{BaseStream.Position}'");
			}
			long align = (long)alignType;
			long cut = long.MaxValue - align;
			long position = BaseStream.Position - AlignPosition;

			position = (position + align) & cut;
			BaseStream.Position = AlignPosition + position;
		}

		protected byte[] ReadStringBuffer(int length)
		{
			if (m_bufferString.Length >= length)
			{
				int read = Read(m_bufferString, 0, length);
				if (read < length)
				{
					throw new Exception($"Read {read} bytes intead of {length}");
				}
				return m_bufferString;
			}
			else
			{
				byte[] buffer = new byte[length];
				int read = Read(buffer, 0, length);
				if (read < length)
				{
					throw new Exception($"Read {read} bytes intead of {length}");
				}
				return buffer;
			}
		}

		public EndianType EndianType { get; set; }
		public long AlignPosition { get; set; }

		private const int StringBufferSize = 8096;
		
		private readonly byte[] m_buffer16 = new byte[2];
		private readonly byte[] m_buffer32 = new byte[4];
		private readonly byte[] m_buffer64 = new byte[8];
		private readonly byte[] m_bufferString = new byte[StringBufferSize];
	}
}
