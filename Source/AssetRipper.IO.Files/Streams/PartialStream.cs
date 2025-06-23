namespace AssetRipper.IO.Files.Streams;

/// <summary>
/// A stream implementation for accessing a subset of another stream
/// </summary>
public sealed class PartialStream : Stream
{
	public PartialStream(Stream baseStream, long offset, long length) : this(baseStream, offset, length, true) { }

	public PartialStream(Stream baseStream, long offset, long length, bool leaveOpen)
	{
		m_stream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
		m_baseOffset = offset;
		Length = length;
		m_leaveOpen = leaveOpen;
	}

	~PartialStream()
	{
		Dispose(false);
	}

	/// <inheritdoc/>
	public override void Flush()
	{
		m_stream.Flush();
	}

	/// <inheritdoc/>
	public override int Read(byte[] buffer, int offset, int count)
	{
		count = (int)Math.Max(Math.Min(count, Length - Position), 0);
		int read = m_stream.Read(buffer, offset, count);
		return read;
	}

	public override int Read(Span<byte> buffer)
	{
		int count = buffer.Length;
		count = (int)Math.Max(Math.Min(count, Length - Position), 0);
		return m_stream.Read(buffer[..count]);
	}

	public override int ReadByte()
		=> m_stream.ReadByte();

	/// <inheritdoc/>
	public override long Seek(long offset, SeekOrigin origin)
	{
		if (origin == SeekOrigin.Begin)
		{
			return m_stream.Seek(m_baseOffset + offset, SeekOrigin.Begin) - m_baseOffset;
		}
		else if (origin == SeekOrigin.End)
		{
			return m_stream.Seek(m_baseOffset + Length + offset, SeekOrigin.Begin) - m_baseOffset;
		}
		else
		{
			return m_stream.Seek(offset, origin) - m_baseOffset;
		}
	}

	/// <inheritdoc/>
	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc/>
	public override void Write(byte[] buffer, int offset, int count)
	{
		if (Position + count > Length)
		{
			throw new Exception("Partial stream's position is out of range");
		}
		m_stream.Write(buffer, offset, count);
	}

	/// <inheritdoc/>
	protected override void Dispose(bool disposing)
	{
		if (m_leaveOpen)
		{
			if (!m_isDisposed)
			{
				m_isDisposed = true;
			}
		}
		else
		{
			m_stream.Dispose();
		}
		base.Dispose(disposing);
	}

	/// <inheritdoc/>
	public override bool CanRead => m_stream.CanRead;
	/// <inheritdoc/>
	public override bool CanSeek => m_stream.CanSeek;
	/// <inheritdoc/>
	public override bool CanWrite => m_stream.CanWrite;
	/// <inheritdoc/>
	public override long Length { get; }

	/// <inheritdoc/>
	public override long Position
	{
		get => m_stream.Position - m_baseOffset;
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), "Non-negative number required");
			}
			m_stream.Position = m_baseOffset + value;
		}
	}

	private readonly Stream m_stream;
	private readonly long m_baseOffset;
	private readonly bool m_leaveOpen;

	private bool m_isDisposed;
}
