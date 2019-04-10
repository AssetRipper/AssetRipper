using System;
using System.IO;

namespace uTinyRipper
{
	public class PartialStream : Stream
	{
		public PartialStream(Stream baseStream, long offset, long length) :
			this(baseStream, offset, length, true)
		{
		}

		public PartialStream(Stream baseStream, long offset, long length, bool leaveOpen)
		{
			if (baseStream == null)
			{
				throw new ArgumentNullException(nameof(baseStream));
			}
			m_stream = baseStream;
			m_baseOffset = offset;
			Length = length;
			m_leaveOpen = leaveOpen;

			m_initialPosition = baseStream.Position;
			Position = 0;
		}

		~PartialStream()
		{
			Dispose(false);
		}

		public override void Flush()
		{
			m_stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			count = (int)Math.Max(Math.Min(count, Length - Position), 0);
			int read = m_stream.Read(buffer, offset, count);
			return read;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return m_stream.Seek(offset, origin) - m_baseOffset;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Position + count > Length)
			{
				throw new Exception("Partial stream's position is out of range");
			}
			m_stream.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			if (m_leaveOpen)
			{
				if (!m_isDisposed)
				{
					m_stream.Position = m_initialPosition;
					m_isDisposed = true;
				}
			}
			else
			{
				m_stream.Dispose();
			}
			base.Dispose(disposing);
		}

		public override bool CanRead => m_stream.CanRead;
		public override bool CanSeek => m_stream.CanSeek;
		public override bool CanWrite => m_stream.CanWrite;
		public override long Length { get; }

		public override long Position
		{
			get => m_stream.Position - m_baseOffset;
			set
			{
				if(value < 0)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Non-negative number required");
				}
				m_stream.Position = m_baseOffset + value;
			}
		}

		private readonly Stream m_stream;
		private readonly long m_baseOffset;
		private readonly bool m_leaveOpen;
		private readonly long m_initialPosition;

		private bool m_isDisposed;
	}
}
