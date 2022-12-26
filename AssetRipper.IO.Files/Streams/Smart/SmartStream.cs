using AssetRipper.IO.Files.Streams.MultiFile;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AssetRipper.IO.Files.Streams.Smart
{
	public sealed partial class SmartStream : Stream
	{
		public static new SmartStream Null { get; } = new();

		private SmartStream()
		{
			m_isDisposed = true;
			m_refCount = new();
		}

		private SmartStream(Stream baseStream, SmartStreamType type)
		{
			m_stream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
			m_streamType = type;
			m_refCount = new();
			m_refCount++;
		}

		private SmartStream(SmartStream copy)
		{
			Assign(copy);
		}

		public static SmartStream OpenRead(string path)
		{
			return new SmartStream(MultiFileStream.OpenRead(path), SmartStreamType.File);
		}

		public static SmartStream CreateTemp()
		{
			string tempFile = Path.GetTempFileName();
			return new SmartStream(new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose), SmartStreamType.File);
		}

		public static SmartStream CreateMemory()
		{
			return new SmartStream(new MemoryStream(), SmartStreamType.Memory);
		}

		public static SmartStream CreateMemory(byte[] buffer)
		{
			return new SmartStream(new MemoryStream(buffer), SmartStreamType.Memory);
		}

		public static SmartStream CreateMemory(byte[] buffer, int offset, int size, bool writable = true)
		{
			return new SmartStream(new MemoryStream(buffer, offset, size, writable), SmartStreamType.Memory);
		}

		[MemberNotNull(nameof(m_refCount))]
		public void Assign(SmartStream source)
		{
			Dispose();

			m_stream = source.m_stream;
			m_streamType = source.m_streamType;
			m_refCount = source.m_refCount;
			m_isDisposed = source.m_isDisposed;
			if (m_isDisposed)
			{
				if (!IsNull)
				{
					throw new ObjectDisposedException(nameof(source));
				}
			}
			else
			{
				m_refCount++;
			}
		}

		public void Move(SmartStream source)
		{
			Assign(source);
			source.Dispose();
		}

		public SmartStream CreateReference()
		{
			return new SmartStream(this);
		}

		public override void Flush()
		{
			ThrowIfDisposed();
			m_stream?.Flush();
		}

		[MemberNotNull(nameof(m_stream))]
		public override int Read(byte[] buffer, int offset, int count)
		{
			ThrowIfDisposed();
			ThrowIfNull();
			return m_stream.Read(buffer, offset, count);
		}

		[MemberNotNull(nameof(m_stream))]
		public override int Read(Span<byte> buffer)
		{
			ThrowIfDisposed();
			ThrowIfNull();
			return m_stream.Read(buffer);
		}

		[MemberNotNull(nameof(m_stream))]
		public override int ReadByte()
		{
			ThrowIfDisposed();
			ThrowIfNull();
			return m_stream.ReadByte();
		}

		[MemberNotNull(nameof(m_stream))]
		public override long Seek(long offset, SeekOrigin origin)
		{
			ThrowIfDisposed();
			ThrowIfNull();
			return m_stream.Seek(offset, origin);
		}

		[MemberNotNull(nameof(m_stream))]
		public override void SetLength(long value)
		{
			ThrowIfDisposed();
			ThrowIfNull();
			m_stream.SetLength(value);
		}

		[MemberNotNull(nameof(m_stream))]
		public override void Write(byte[] buffer, int offset, int count)
		{
			ThrowIfDisposed();
			ThrowIfNull();
			m_stream.Write(buffer, offset, count);
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsNull && !m_isDisposed)
			{
				m_refCount--;
				if (m_refCount.IsZero)
				{
					m_stream.Dispose();
				}
				m_isDisposed = true;
			}
			base.Dispose(disposing);
		}

		[MemberNotNullWhen(true, nameof(m_stream))]
		public override bool CanRead
		{
			get
			{
				ThrowIfDisposed();
				return m_stream?.CanRead ?? false;
			}
		}

		[MemberNotNullWhen(true, nameof(m_stream))]
		public override bool CanSeek
		{
			get
			{
				ThrowIfDisposed();
				return m_stream?.CanSeek ?? false;
			}
		}

		[MemberNotNullWhen(true, nameof(m_stream))]
		public override bool CanWrite
		{
			get
			{
				ThrowIfDisposed();
				return m_stream?.CanWrite ?? false;
			}
		}

		public override long Position
		{
			get
			{
				ThrowIfDisposed();
				return m_stream?.Position ?? 0;
			}
			[MemberNotNull(nameof(m_stream))]
			set
			{
				ThrowIfDisposed();
				ThrowIfNull();
				m_stream.Position = value;
			}
		}

		public override long Length
		{
			get
			{
				ThrowIfDisposed();
				return m_stream?.Length ?? 0;
			}
		}

		public SmartStreamType StreamType
		{
			get
			{
				ThrowIfDisposed();
				return m_streamType;
			}
		}

		[MemberNotNull(nameof(m_stream))]
		private void ThrowIfNull()
		{
			if (IsNull)
			{
				throw new NullReferenceException(nameof(m_stream));
			}
		}

		private void ThrowIfDisposed()
		{
			if (m_isDisposed)
			{
				throw new ObjectDisposedException(null);
			}
		}

		[MemberNotNullWhen(false, nameof(m_stream))]
		public bool IsNull => m_stream == null;

		private SmartRefCount m_refCount;
		private Stream? m_stream;
		private SmartStreamType m_streamType;
		private bool m_isDisposed;
	}
}
