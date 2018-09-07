using System;
using System.IO;

namespace UtinyRipper
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

		private SmartStream(Stream baseStream)
		{
			if (baseStream == null)
			{
				throw new ArgumentNullException(nameof(baseStream));
			}
			m_stream = baseStream;
			RefCount = new SmartRefCount();
			RefCount.Increase();
		}

		private SmartStream(SmartStream copy)
		{
			if (copy == null)
			{
				throw new ArgumentNullException(nameof(copy));
			}
			m_stream = copy.m_stream;
			RefCount = copy.RefCount;
			RefCount.Increase();
		}

		~SmartStream()
		{
			Dispose(false);
		}

		public static SmartStream OpenRead(string path)
		{
			return new SmartStream(FileMultiStream.OpenRead(path));
		}

		public static SmartStream CreateTemp()
		{
			string tempFile = Path.GetTempFileName();
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
		
		public SmartStream CreateReference()
		{
			return new SmartStream(this);
		}

		public override void Flush()
		{
			if(m_isDisposed)
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
			if (!m_isDisposed)
			{
				RefCount.Decrease();
				if (RefCount.IsZero)
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

		private SmartRefCount RefCount { get; }

		private readonly Stream m_stream;
		private bool m_isDisposed;
	}
}
