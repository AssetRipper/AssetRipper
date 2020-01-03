using System;
using System.IO;

namespace uTinyRipper
{
	public class SmartStream : Stream
	{
		private class SmartRefCount
		{
			public static SmartRefCount operator ++(SmartRefCount _this)
			{
				_this.RefCount++;
				return _this;
			}
			public static SmartRefCount operator --(SmartRefCount _this)
			{
				_this.RefCount--;
				return _this;
			}

			public void Increase()
			{
				RefCount++;
			}

			public void Decrease()
			{
				RefCount--;
			}

			public override string ToString()
			{
				return m_refCount.ToString();
			}

			public bool IsZero => RefCount == 0;

			private int RefCount
			{
				get => m_refCount;
				set
				{
					if (value < 0)
					{
						throw new ArgumentOutOfRangeException();
					}
					m_refCount = value;
				}
			}

			private int m_refCount = 0;
		}

		private SmartStream()
		{
			m_isDisposed = true;
		}

		private SmartStream(Stream baseStream, SmartStreamType type)
		{
			if (baseStream == null)
			{
				throw new ArgumentNullException(nameof(baseStream));
			}
			m_stream = baseStream;
			m_streamType = type;
			m_refCount = new SmartRefCount();
			m_refCount++;
		}

		private SmartStream(SmartStream copy)
		{
			Assign(copy);
		}

		~SmartStream()
		{
			Dispose(false);
		}

		public static SmartStream CreateNull()
		{
			return new SmartStream();
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

		public static SmartStream CreateMemory(byte[] buffer, int offset, int size)
		{
			return new SmartStream(new MemoryStream(buffer, offset, size), SmartStreamType.Memory);
		}

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
			if (m_isDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			m_stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (m_isDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			return m_stream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (m_isDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			return m_stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			if (m_isDisposed)
			{
				throw new ObjectDisposedException(null);
			}
			m_stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (m_isDisposed)
			{
				throw new ObjectDisposedException(null);
			}
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

		public override bool CanRead
		{
			get
			{
				if (m_isDisposed)
				{
					throw new ObjectDisposedException(null);
				}
				return m_stream.CanRead;
			}
		}
		public override bool CanSeek
		{
			get
			{
				if (m_isDisposed)
				{
					throw new ObjectDisposedException(null);
				}
				return m_stream.CanSeek;
			}
		}
		public override bool CanWrite
		{
			get
			{
				if (m_isDisposed)
				{
					throw new ObjectDisposedException(null);
				}
				return m_stream.CanWrite;
			}
		}

		public override long Position
		{
			get
			{
				if (m_isDisposed)
				{
					throw new ObjectDisposedException(null);
				}
				return m_stream.Position;
			}
			set
			{
				if (m_isDisposed)
				{
					throw new ObjectDisposedException(null);
				}
				m_stream.Position = value;
			}
		}

		public override long Length
		{
			get
			{
				if (m_isDisposed)
				{
					throw new ObjectDisposedException(null);
				}
				return m_stream.Length;
			}
		}

		public SmartStreamType StreamType
		{
			get
			{
				if (m_isDisposed)
				{
					throw new ObjectDisposedException(null);
				}
				return m_streamType;
			}
		}

		public bool IsNull => m_stream == null;

		private SmartRefCount m_refCount;
		private Stream m_stream;
		private SmartStreamType m_streamType;
		private bool m_isDisposed;
	}
}
