using AssetRipper.IO.Files.Streams.Smart;
using Microsoft.Win32.SafeHandles;

namespace AssetRipper.IO.Files.Streams;

/// <summary>
/// Read a slice of a file using a SafeFileHandle directly with System.IO.RandomAccess.
/// This allows a container file with multiple logical files (eg, AssetBundles)
/// to be streamed as if they are separate files, without buffering the entire file at once.
/// </summary>
internal sealed class RandomAccessStream : Stream
{
	public SafeFileHandle Handle { get; }

	public FileStream Parent { get; }

	public long BaseOffset { get; }

	/// <summary>
	/// Raw position in file.
	/// </summary>
	private long position = 0;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length { get; }

	// position corrected for logical file offset.
	public override long Position
	{
		get => position - BaseOffset;
		set => position = value + BaseOffset;
	}

	public RandomAccessStream(FileStream parent, long offset, long length)
	{
		Parent = parent;
		Handle = parent.SafeFileHandle;
		BaseOffset = offset;
		Length = length;
		position = BaseOffset;
	}

	public override void Flush()
	{
		// Read-only streams shouldn't flush.
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return Read(buffer.AsSpan(offset, count));
	}

	public override int Read(Span<byte> buffer)
	{
		long toRead = Math.Min(buffer.Length, Length - Position);
		int read = RandomAccess.Read(Handle, buffer[..(int)toRead], position);
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
		int read = RandomAccess.Read(Handle, result, position);
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
				position = (Length - offset) + BaseOffset;
				break;
		}
		return Position;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}


	~RandomAccessStream()
	{
		GC.KeepAlive(Parent);
	}
}
