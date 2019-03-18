using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace uTinyRipper
{
	public class EndianWriter : BinaryWriter
	{
		public EndianWriter(Stream stream)
			: this(stream, EndianType.LittleEndian, 0)
		{
		}

		public EndianWriter(Stream stream, EndianType endianess) :
			this(stream, endianess, 0)
		{
		}

		public EndianWriter(Stream stream, EndianType endianess, long alignPosition) :
		   base(stream, Encoding.UTF8, true)
		{
			EndianType = endianess;
			AlignPosition = alignPosition;
		}

		~EndianWriter()
		{
			Dispose(false);
		}
		
		public override void Write(short value)
		{
			FillInnerBuffer(unchecked((ushort)value));
			Write(m_buffer, 0, sizeof(short));
		}

		public override void Write(ushort value)
		{
			FillInnerBuffer(value);
			Write(m_buffer, 0, sizeof(ushort));
		}

		public override void Write(int value)
		{
			FillInnerBuffer(unchecked((uint)value));
			Write(m_buffer, 0, sizeof(int));
		}

		public override void Write(uint value)
		{
			FillInnerBuffer(value);
			Write(m_buffer, 0, sizeof(uint));
		}

		public override void Write(long value)
		{
			FillInnerBuffer(unchecked((ulong)value));
			Write(m_buffer, 0, sizeof(long));
		}

		public override void Write(ulong value)
		{
			FillInnerBuffer(value);
			Write(m_buffer, 0, sizeof(ulong));
		}

		public override void Write(float value)
		{
			uint value32 = BitConverterExtensions.ToUInt32(value);
			FillInnerBuffer(value32);
			Write(m_buffer, 0, sizeof(float));
		}

		public override void Write(double value)
		{
			ulong value64 = BitConverterExtensions.ToUInt64(value);
			FillInnerBuffer(value64);
			Write(m_buffer, 0, sizeof(double));
		}

		public override void Write(string value)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(value);
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));
			Write(buffer, 0, buffer.Length);
		}

		public void WriteStringZeroTerm(string value)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(value);
			m_buffer[0] = 0;
			Write(buffer, 0, buffer.Length);
			Write(m_buffer, 0, sizeof(byte));
		}

		public void WriteArray(bool[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(bool[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int last = index + count;
			while (index < last)
			{
				int left = last - index;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i++, index++)
				{
					m_buffer[i] = buffer[index] ? (byte)1 : (byte)0;
				}
				Write(m_buffer, 0, toWrite);
			}
		}

		public void WriteArray(char[] buffer)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			Write(buffer, 0, buffer.Length);
		}

		public void WriteArray(byte[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(byte[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));
			Write(buffer, index, count);
		}

		public void WriteArray(short[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(short[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(short);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(short), index++)
				{
					FillInnerBuffer(unchecked((ushort)buffer[index]), i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteArray(ushort[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(ushort[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(ushort);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(ushort), index++)
				{
					FillInnerBuffer(buffer[index], i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteArray(int[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(int[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(int);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(int), index++)
				{
					FillInnerBuffer(buffer[index], i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteArray(uint[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(uint[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(uint);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(uint), index++)
				{
					FillInnerBuffer(buffer[index], i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteArray(long[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(long[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(long);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(long), index++)
				{
					FillInnerBuffer(unchecked((ulong)buffer[index]), i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteArray(ulong[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(ulong[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(ulong);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(ulong), index++)
				{
					FillInnerBuffer(buffer[index], i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteArray(float[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(float[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(float);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(float), index++)
				{
					FillInnerBuffer(BitConverterExtensions.ToUInt32(buffer[index]), i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteArray(double[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(double[] buffer, int index, int count)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = buffer.Length * sizeof(double);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(double), index++)
				{
					FillInnerBuffer(BitConverterExtensions.ToUInt64(buffer[index]), i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
		}

		public void WriteArray(string[] buffer)
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.Length; i++)
			{
				string str = buffer[i];
				Write(str);
			}
		}

		public void WriteEndian<T>(T value)
			where T : IEndianWritable, new()
		{
			value.Write(this);
		}

		public void WriteEndianArray<T>(T[] buffer)
			where T : IEndianWritable, new()
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for(int i = 0; i < buffer.Length; i++)
			{
				T t = buffer[i];
				t.Write(this);
			}
		}

		public void WriteEndianArray<T>(T[][] buffer)
			where T : IEndianWritable, new()
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.GetLength(0); i++)
			{
				for (int j = 0; j < buffer.GetLength(1); j++)
				{
					T t = buffer[i][j];
					t.Write(this);
				}
			}
		}

		public void AlignStream(AlignType alignType)
		{
			long align = (long)alignType;
			BaseStream.Position = AlignPosition + ((BaseStream.Position - AlignPosition + align) & ~align);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FillInnerBuffer(ushort value, int offset = 0)
		{
			if (EndianType == EndianType.LittleEndian)
			{
				m_buffer[offset + 0] = unchecked((byte)(value >> 0));
				m_buffer[offset + 1] = unchecked((byte)(value >> 8));
			}
			else
			{
				m_buffer[offset + 1] = unchecked((byte)(value >> 0));
				m_buffer[offset + 0] = unchecked((byte)(value >> 8));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FillInnerBuffer(int value, int offset = 0)
		{
			if (EndianType == EndianType.LittleEndian)
			{
				m_buffer[offset + 0] = unchecked((byte)(value >> 0));
				m_buffer[offset + 1] = unchecked((byte)(value >> 8));
				m_buffer[offset + 2] = unchecked((byte)(value >> 16));
				m_buffer[offset + 3] = unchecked((byte)(value >> 24));
			}
			else
			{
				m_buffer[offset + 3] = unchecked((byte)(value >> 0));
				m_buffer[offset + 2] = unchecked((byte)(value >> 8));
				m_buffer[offset + 1] = unchecked((byte)(value >> 16));
				m_buffer[offset + 0] = unchecked((byte)(value >> 24));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FillInnerBuffer(uint value, int offset = 0)
		{
			if (EndianType == EndianType.LittleEndian)
			{
				m_buffer[offset + 0] = unchecked((byte)(value >> 0));
				m_buffer[offset + 1] = unchecked((byte)(value >> 8));
				m_buffer[offset + 2] = unchecked((byte)(value >> 16));
				m_buffer[offset + 3] = unchecked((byte)(value >> 24));
			}
			else
			{
				m_buffer[offset + 3] = unchecked((byte)(value >> 0));
				m_buffer[offset + 2] = unchecked((byte)(value >> 8));
				m_buffer[offset + 1] = unchecked((byte)(value >> 16));
				m_buffer[offset + 0] = unchecked((byte)(value >> 24));
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void FillInnerBuffer(ulong value, int offset = 0)
		{
			if (EndianType == EndianType.LittleEndian)
			{
				m_buffer[offset + 0] = unchecked((byte)(value >> 0));
				m_buffer[offset + 1] = unchecked((byte)(value >> 8));
				m_buffer[offset + 2] = unchecked((byte)(value >> 16));
				m_buffer[offset + 3] = unchecked((byte)(value >> 24));
				m_buffer[offset + 4] = unchecked((byte)(value >> 32));
				m_buffer[offset + 5] = unchecked((byte)(value >> 40));
				m_buffer[offset + 6] = unchecked((byte)(value >> 48));
				m_buffer[offset + 7] = unchecked((byte)(value >> 56));
			}
			else
			{
				m_buffer[offset + 7] = unchecked((byte)(value >> 0));
				m_buffer[offset + 6] = unchecked((byte)(value >> 8));
				m_buffer[offset + 5] = unchecked((byte)(value >> 16));
				m_buffer[offset + 4] = unchecked((byte)(value >> 24));
				m_buffer[offset + 3] = unchecked((byte)(value >> 32));
				m_buffer[offset + 2] = unchecked((byte)(value >> 40));
				m_buffer[offset + 1] = unchecked((byte)(value >> 48));
				m_buffer[offset + 0] = unchecked((byte)(value >> 56));
			}
		}

		protected EndianType EndianType { get; }
		protected long AlignPosition { get; private set; }

		protected const int BufferSize = 4096;

		protected readonly byte[] m_buffer = new byte[BufferSize];
	}
}
