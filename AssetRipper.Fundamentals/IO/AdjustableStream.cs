using System.IO;

namespace AssetRipper.Core.IO
{
	/// <summary>
	/// A wrapper stream with moveable boundaries
	/// </summary>
	public class AdjustableStream : Stream
	{
		private readonly Stream m_stream;
		private readonly bool m_leaveOpen;
		private long m_minPosition = 0;
		private long m_maxPosition = long.MaxValue;
		private bool m_isDisposed;

		public AdjustableStream(Stream baseStream) : this(baseStream, true) { }

		public AdjustableStream(Stream baseStream, bool leaveOpen)
		{
			m_stream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
			m_leaveOpen = leaveOpen;
		}

		/// <inheritdoc/>
		public override bool CanRead => m_stream.CanRead;
		/// <inheritdoc/>
		public override bool CanWrite => m_stream.CanWrite;
		/// <inheritdoc/>
		public override bool CanSeek => false;


		/// <inheritdoc/>
		public override long Position
		{
			get => m_stream.Position;
			set
			{
				if (value < m_minPosition || m_maxPosition < value)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "New position must be between the minimum and maximum positions, inclusive");
				}

				m_stream.Position = value;
			}
		}
		/// <inheritdoc/>
		public override long Length => System.Math.Min(m_maxPosition, m_stream.Length);
		public long ActualLength => Length - m_minPosition;
		public long MinPosition => m_minPosition;
		public long MaxPosition => m_maxPosition;

		public void ResetPositionBoundaries()
		{
			m_minPosition = 0;
			m_maxPosition = long.MaxValue;
		}

		public void SetPositionBoundaries(long minimumPosition, long maximumPosition, long newPosition)
		{
			if (minimumPosition < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minimumPosition), "Cannot be negative");
			}

			if (maximumPosition < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(maximumPosition), "Cannot be negative");
			}

			if (newPosition < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(newPosition), "Cannot be negative");
			}

			if (maximumPosition < minimumPosition)
			{
				throw new ArgumentOutOfRangeException(nameof(maximumPosition), "Maximum cannot be less than the minimum");
			}

			if (newPosition < minimumPosition || maximumPosition < newPosition)
			{
				throw new ArgumentOutOfRangeException(nameof(newPosition), "New position must be between the minimum and maximum positions, inclusive");
			}

			m_minPosition = minimumPosition;
			m_maxPosition = maximumPosition;
			if (Position != newPosition)
			{
				Position = newPosition;
			}
		}

		/// <inheritdoc/>
		public override void Flush() => m_stream.Flush();

		/// <inheritdoc/>
		public override int Read(byte[] buffer, int offset, int count)
		{
			count = (int)System.Math.Max(System.Math.Min(count, m_maxPosition - Position), 0);
			return m_stream.Read(buffer, offset, count);
		}

		/// <inheritdoc/>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		/// <inheritdoc/>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Position + count > m_maxPosition)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "Cannot write past the max position");
			}
			m_stream.Write(buffer, offset, count);
		}

		~AdjustableStream()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if (!m_isDisposed)
			{
				if (!m_leaveOpen)
				{
					m_stream?.Dispose();
				}
				m_isDisposed = true;
			}
			base.Dispose(disposing);
		}
	}
}
