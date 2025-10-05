namespace AssetRipper.IO.Files.Streams.Smart;

public sealed partial class SmartStream : Stream
{
	private SmartStream()
	{
		RefCounter = new();
	}

	private SmartStream(Stream baseStream)
	{
		Stream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
		RefCounter = new();
		RefCounter++;
	}

	private SmartStream(SmartStream copy)
	{
		Assign(copy);
	}

	public static SmartStream OpenRead(string path, FileSystem fileSystem)
	{
		return new SmartStream(fileSystem.File.OpenRead(path));
	}

	public static SmartStream OpenReadMulti(string path, FileSystem fileSystem)
	{
		return new SmartStream(MultiFileStream.OpenRead(path, fileSystem));
	}

	public static SmartStream CreateTemp()
	{
		string tempFile = LocalFileSystem.Instance.File.CreateTemporary();
		return new SmartStream(new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose));
	}

	public static SmartStream CreateMemory()
	{
		return new SmartStream(new MemoryStream());
	}

	public static SmartStream CreateMemory(byte[] buffer)
	{
		return new SmartStream(new MemoryStream(buffer));
	}

	public static SmartStream CreateMemory(byte[] buffer, int offset, int size, bool writable = true)
	{
		return new SmartStream(new MemoryStream(buffer, offset, size, writable));
	}

	/// <summary>
	/// Create a <see cref="SmartStream"/> with no backing stream.
	/// </summary>
	/// <returns>A new <see cref="SmartStream"/> for which <see cref="IsNull"/> is true.</returns>
	public static SmartStream CreateNull() => new();

	/// <summary>
	/// Copy the reference from another <see cref="SmartStream"/>.
	/// </summary>
	/// <param name="source">The <see cref="SmartStream"/> to copy a reference from.</param>
	[MemberNotNull(nameof(RefCounter))]
	public void Assign(SmartStream source)
	{
		FreeReference();

		Stream = source.Stream;
		RefCounter = source.RefCounter;
		if (!IsNull)
		{
			RefCounter++;
		}
	}

	/// <summary>
	/// Move the reference from another <see cref="SmartStream"/> to <see langword="this"/>.
	/// </summary>
	/// <remarks>
	/// The reference for <paramref name="source"/> is freed.
	/// </remarks>
	/// <param name="source">The <see cref="SmartStream"/> from which to move the reference.</param>
	public void Move(SmartStream source)
	{
		Assign(source);
		source.FreeReference();
	}

	/// <summary>
	/// Create a new reference to the backing stream.
	/// </summary>
	/// <returns>A new <see cref="SmartStream"/> that references the same stream as <see langword="this"/>.</returns>
	public SmartStream CreateReference()
	{
		return new SmartStream(this);
	}

	[MemberNotNull(nameof(Stream))]
	public SmartStream CreatePartial(long offset, long size)
	{
		ThrowIfNull();

		// Create a partial stream if the base stream is compatible with RandomAccessStream.
		RandomAccessStream? partialStream = Stream switch
		{
			// root case
			FileStream fileStream => new RandomAccessStream(fileStream, offset, size),

			// branch case
			RandomAccessStream parentPartial => new RandomAccessStream(
				parentPartial.Parent,
				parentPartial.BaseOffset + offset,
				size),

			// incompatible
			_ => null
		};

		if (partialStream is not null)
		{
			return new SmartStream(this) { Stream = partialStream };
		}

		// Copy otherwise.
		byte[] buffer = new byte[(int)size];
		long initialPosition = Stream.Position;
		Stream.Position = offset;
		Stream.ReadExactly(buffer);
		Stream.Position = initialPosition;
		return SmartStream.CreateMemory(buffer);
	}

	public override void Flush()
	{
		Stream?.Flush();
	}

	[MemberNotNull(nameof(Stream))]
	public override int Read(byte[] buffer, int offset, int count)
	{
		ThrowIfNull();
		return Stream.Read(buffer, offset, count);
	}

	[MemberNotNull(nameof(Stream))]
	public override int Read(Span<byte> buffer)
	{
		ThrowIfNull();
		return Stream.Read(buffer);
	}

	[MemberNotNull(nameof(Stream))]
	public override int ReadByte()
	{
		ThrowIfNull();
		return Stream.ReadByte();
	}

	[MemberNotNull(nameof(Stream))]
	public override long Seek(long offset, SeekOrigin origin)
	{
		ThrowIfNull();
		return Stream.Seek(offset, origin);
	}

	[MemberNotNull(nameof(Stream))]
	public override void SetLength(long value)
	{
		ThrowIfNull();
		Stream.SetLength(value);
	}

	[MemberNotNull(nameof(Stream))]
	public override void Write(byte[] buffer, int offset, int count)
	{
		ThrowIfNull();
		Stream.Write(buffer, offset, count);
	}

	/// <summary>
	/// Free the reference to the backing stream and become null.
	/// </summary>
	public void FreeReference()
	{
		if (!IsNull)
		{
			RefCounter--;
			if (RefCounter.IsZero)
			{
				Stream.Dispose();
			}
			Stream = null;
		}
	}

	protected override void Dispose(bool disposing)
	{
		FreeReference();
		base.Dispose(disposing);
	}

	[MemberNotNullWhen(true, nameof(Stream))]
	public override bool CanRead => Stream?.CanRead ?? false;

	[MemberNotNullWhen(true, nameof(Stream))]
	public override bool CanSeek => Stream?.CanSeek ?? false;

	[MemberNotNullWhen(true, nameof(Stream))]
	public override bool CanWrite => Stream?.CanWrite ?? false;

	public override long Position
	{
		get => Stream?.Position ?? 0;
		[MemberNotNull(nameof(Stream))]
		set
		{
			ThrowIfNull();
			Stream.Position = value;
		}
	}

	public override long Length => Stream?.Length ?? 0;

	/// <summary>
	/// The type of stream backing this <see cref="SmartStream"/>.
	/// </summary>
	public SmartStreamType StreamType => Stream switch
	{
		null => SmartStreamType.Null,
		MemoryStream => SmartStreamType.Memory,
		FileStream or MultiFileStream => SmartStreamType.File,
		_ => throw new InvalidOperationException(),
	};

	/// <summary>
	/// Write the contents to a byte array, regardless of the <see cref="Position"/> property.
	/// </summary>
	/// <returns>A new byte array.</returns>
	[MemberNotNull(nameof(Stream))]
	public byte[] ToArray()
	{
		ThrowIfNull();
		return Stream switch
		{
			MemoryStream memoryStream => memoryStream.ToArray(),
			_ => StreamToByteArray(Stream),
		};

		static byte[] StreamToByteArray(Stream stream)
		{
			long initialPosition = stream.Position;
			stream.Position = 0;
			byte[] data = new byte[stream.Length];
			stream.CopyTo(new MemoryStream(data));
			stream.Position = initialPosition;
			return data;
		}
	}

	/// <summary>
	/// Throw if <see cref="Stream"/> is null.
	/// </summary>
	/// <exception cref="NullReferenceException"><see cref="Stream"/> is null.</exception>
	[MemberNotNull(nameof(Stream))]
	private void ThrowIfNull()
	{
		if (IsNull)
		{
			throw new NullReferenceException(nameof(Stream));
		}
	}

	/// <summary>
	/// If true, this has no backing stream.
	/// </summary>
	[MemberNotNullWhen(false, nameof(Stream))]
	public bool IsNull => Stream == null;

	/// <summary>
	/// The number of references to the backing stream.
	/// </summary>
	public int RefCount => RefCounter.RefCount;

	/// <summary>
	/// The shared reference counter for the backing stream.
	/// </summary>
	private SmartRefCount RefCounter { get; set; }

	/// <summary>
	/// The backing stream. It is shared with the other <see cref="SmartStream"/>s that reference it.
	/// </summary>
	private Stream? Stream { get; set; }
}
