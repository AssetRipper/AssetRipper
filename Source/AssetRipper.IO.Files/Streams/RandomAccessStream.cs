using Microsoft.Win32.SafeHandles;

namespace AssetRipper.IO.Files.Streams;

/// <summary>
/// Read a slice of a file using a SafeFileHandle directly with System.IO.RandomAccess.
/// This allows a container file with multiple logical files (eg, AssetBundles)
/// to be streamed as if they are separate files, without buffering the entire file at once.
/// </summary>
internal class RandomAccessStream : Stream
{
	private readonly SafeFileHandle handle;
	private readonly long baseOffset;

	/// <summary>
	/// Raw position in file.
	/// </summary>
	private long position = 0;

	public override bool CanRead { get; }

	public override bool CanSeek => true;

	public override bool CanWrite { get; }

	public override long Length { get; }

	// position corrected for logical file offset.
	public override long Position
	{
		get => position - baseOffset;
		set => position = value + baseOffset;
	}

	public RandomAccessStream(Stream stream, long offset, long length)
	{
		switch (stream)
		{
			case FileStream fileStream:
				handle = fileStream.SafeFileHandle;
				break;
			case RandomAccessStream other:
				handle = other.handle;
				baseOffset = other.baseOffset + offset;
				break;
			default:
				throw new ArgumentException("For performance reasons, this needs direct access to the FileStream.SafeFilehandle.");
		}
		baseOffset = offset;
		Length = length;
		CanRead = stream.CanRead;
		CanWrite = stream.CanWrite;
		position = baseOffset;
	}

	public override void Flush()
	{
		RandomAccess.FlushToDisk(handle);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return Read(buffer.AsSpan(offset, count));
	}

	public override int Read(Span<byte> buffer)
	{
		long toRead = Math.Min(buffer.Length, Length - Position);
		int read = RandomAccess.Read(handle, buffer[..(int)toRead], position);
		position += read;
		return read;
	}

	public override int ReadByte()
	{
		if (Position >= Length)
		{
			return -1;
		}

		Span<byte> result = stackalloc byte[1];
		int read = RandomAccess.Read(handle, result, position);
		if (read > 0)
		{
			position += read;
			return result[0];
		}

		return -1;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
			case SeekOrigin.Current:
				Position += offset;
				break;
			case SeekOrigin.Begin:
				Position = offset;
				break;
			case SeekOrigin.End:
				position = (Length - offset) + baseOffset;
				break;
		}
		return Position;
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}
}
