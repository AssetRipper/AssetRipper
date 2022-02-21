using AssetRipper.Core.Extensions;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace AssetRipper.Core.IO.Endian
{
	public class EndianWriter : BinaryWriter
	{
		public EndianWriter(Stream stream, EndianType endianess) : this(stream, endianess, false) { }

		protected EndianWriter(Stream stream, EndianType endianess, bool alignArray) : base(stream, Encoding.UTF8, true)
		{
			EndianType = endianess;
			IsAlignArray = alignArray;
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
			uint value32 = BitConverter.SingleToUInt32Bits(value);
			FillInnerBuffer(value32);
			Write(m_buffer, 0, sizeof(float));
		}

		public override void Write(double value)
		{
			ulong value64 = BitConverter.DoubleToUInt64Bits(value);
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
			FillInnerBuffer(count);
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
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(char[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(char[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			Write(buffer, index, count);
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(byte[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(byte[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));
			Write(buffer, index, count);
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(short[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(short[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = count * sizeof(short);
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
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(ushort[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(ushort[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = count * sizeof(ushort);
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
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(int[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(int[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = count * sizeof(int);
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
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(uint[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(uint[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = count * sizeof(uint);
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
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(long[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(long[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = count * sizeof(long);
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
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(ulong[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(ulong[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = count * sizeof(ulong);
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
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(float[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(float[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = count * sizeof(float);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(float), index++)
				{
					FillInnerBuffer(BitConverter.SingleToUInt32Bits(buffer[index]), i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteArray(double[] buffer)
		{
			WriteArray(buffer, 0, buffer.Length);
		}

		public void WriteArray(double[] buffer, int index, int count)
		{
			FillInnerBuffer(count);
			Write(m_buffer, 0, sizeof(int));

			int byteIndex = 0;
			int byteCount = count * sizeof(double);
			int last = index + count;
			while (index < last)
			{
				int left = byteCount - byteIndex;
				int toWrite = left < BufferSize ? left : BufferSize;
				for (int i = 0; i < toWrite; i += sizeof(double), index++)
				{
					FillInnerBuffer(BitConverter.DoubleToUInt64Bits(buffer[index]), i);
				}
				Write(m_buffer, 0, toWrite);
				byteIndex += toWrite;
			}
			if (IsAlignArray)
			{
				AlignStream();
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
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteEndian<T>(T value) where T : IEndianWritable
		{
			value.Write(this);
		}

		public void WriteEndianArray<T>(T[] buffer) where T : IEndianWritable
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.Length; i++)
			{
				T t = buffer[i];
				t.Write(this);
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void WriteEndianArray<T>(T[][] buffer) where T : IEndianWritable
		{
			FillInnerBuffer(buffer.Length);
			Write(m_buffer, 0, sizeof(int));

			for (int i = 0; i < buffer.GetLength(0); i++)
			{
				WriteEndianArray(buffer[i]);
			}
			if (IsAlignArray)
			{
				AlignStream();
			}
		}

		public void AlignStream()
		{
			BaseStream.Position = (BaseStream.Position + 3) & ~3;
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

		public EndianType EndianType { get; }

		protected bool IsAlignArray { get; }

		protected const int BufferSize = 4096;

		protected readonly byte[] m_buffer = new byte[BufferSize];
	}
}
