using System;
using System.IO;

namespace uTinyRipper
{
	public class ReverseStream : Stream
	{
		public ReverseStream(Stream stream):
			this(stream, true)
		{
		}

		public ReverseStream(Stream stream, bool leaveOpen) :
			this(stream, 0, stream.Length, leaveOpen)
		{
		}

		public ReverseStream(Stream stream, long offset, long size):
			this(stream, offset, size, true)
		{
		}

		public ReverseStream(Stream stream, long offset, long size, bool leaveOpen)
		{
			if(stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (offset > stream.Length)
			{
				throw new ArgumentException($"Start position {offset} grater than stream lengh {stream.Length}", nameof(offset));
			}
			if (size > stream.Length)
			{
				throw new ArgumentException($"Length {size} grater than stream lengh {stream.Length}", nameof(stream));
			}
			if(!stream.CanSeek)
			{
				throw new ArgumentException("Stream can't seek", nameof(stream));
			}

			m_stream = stream;
			m_begin = offset;
			m_end = offset + size;
			Position = m_stream.Position - m_begin;
			Length = size;
			m_leaveOpen = leaveOpen;
		}

		~ReverseStream()
		{
			Dispose(false);
		}

		public override void Flush()
		{
			m_stream.Flush();
		}
		
		public override void WriteByte(byte value)
		{
			if (Position < 0)
			{
				throw new Exception($"Position {Position} is out of range");
			}
			if (Position >= Length)
			{
				throw new Exception($"Position {Position} is out of range {Length}");
			}

			m_stream.Position = m_end - Position - 1;
			Position++;
			m_stream.WriteByte(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Position < 0)
			{
				throw new Exception($"Position {Position} is out of range");
			}
			if (Position >= Length)
			{
				throw new Exception($"Position {Position} is out of range {Length}");
			}

			while (count > 0)
			{
				int toWrite = Math.Min(DefaultBufferSize, count);
				Buffer.BlockCopy(buffer, offset, m_buffer, 0, toWrite);
				Array.Reverse(m_buffer, 0, toWrite);

				m_stream.Position = m_end - Position - toWrite;
				m_stream.Write(m_buffer, 0, toWrite);

				offset += toWrite;
				Position += toWrite;
				count -= toWrite;
			}
		}

		public override int ReadByte()
		{
			if (Position < 0)
			{
				throw new Exception($"Position {Position} is out of range");
			}
			if (Position >= Length)
			{
				throw new Exception($"Position {Position} is out of range {Length}");
			}

			m_stream.Position = m_end - Position - 1;
			Position++;
			return m_stream.ReadByte();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if(Position < 0)
			{
				throw new Exception($"Position {Position} is out of range");
			}
			if (Position >= Length)
			{
				throw new Exception($"Position {Position} is out of range {Length}");
			}

			int totalRead = 0;
			while (count > 0)
			{
				int toRead = Math.Min(DefaultBufferSize, count);
				m_stream.Position = m_end - Position - toRead;
				int read = m_stream.Read(m_buffer, 0, toRead);

				Array.Reverse(m_buffer, 0, read);
				Buffer.BlockCopy(m_buffer, 0, buffer, offset, read);
				
				offset += read;
				Position += read;
				count -= read;
				totalRead += read;

				if (read != toRead)
				{
					break;
				}
			}
			return totalRead;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch(origin)
			{
				case SeekOrigin.Begin:
					Position = offset;
					break;

				case SeekOrigin.Current:
					Position += offset;
					break;

				case SeekOrigin.End:
					Position = m_end - offset - 1; 
					break;
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if(!m_leaveOpen)
			{
				m_stream.Dispose();
			}
		}

		public override long Position
		{
			get => m_position;
			set
			{
				if(value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}
				m_position = value;
			}
		}
		public override long Length { get; }
		public override bool CanWrite => m_stream.CanWrite;
		public override bool CanTimeout => m_stream.CanTimeout;
		public override bool CanSeek => true;
		public override bool CanRead => m_stream.CanRead;
		public override int ReadTimeout
		{
			get => m_stream.ReadTimeout;
			set => m_stream.ReadTimeout = value;
		}
		public override int WriteTimeout
		{
			get => m_stream.WriteTimeout;
			set => m_stream.WriteTimeout = value;
		}

		private const int DefaultBufferSize = 4096;

		private readonly byte[] m_buffer = new byte[DefaultBufferSize];

		private readonly Stream m_stream;
		private readonly long m_begin;
		private readonly long m_end;
		private readonly bool m_leaveOpen;

		private long m_position;
	}
}
