using System;
using System.IO;

namespace UtinyRipper
{
	public abstract class FileEntry
	{
		protected FileEntry(Stream stream, string filePath, string name, long offset, long size, bool isStreamPermanent)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(name));
			}
			if (offset < 0)
			{
				throw new ArgumentException(nameof(offset));
			}
			if (size <= 0)
			{
				throw new ArgumentException(nameof(size));
			}

			m_stream = stream;
			Name = name;
			FilePath = filePath;
			m_offset = offset;
			m_size = size;
			m_isStreamPermanent = isStreamPermanent;
		}

		public void ReadResourcesFile(FileCollection collection)
		{
			m_stream.Position = m_offset;
			if (m_isStreamPermanent)
			{
				collection.ReadResourceFile(m_stream, FilePath, Name);
			}
			else
			{
				MemoryStream stream = new MemoryStream();
				m_stream.CopyStream(stream, m_size);
				collection.ReadResourceFile(m_stream, FilePath, Name);
			}
		}

		public void ReadSerializedFile(FileCollection collection, Action<string> dependencyCallback)
		{
			m_stream.Position = m_offset;
			collection.ReadSerializedFile(m_stream, FilePath, Name, dependencyCallback);
		}

		public void ReadBundleFile(FileCollection collection)
		{
			m_stream.Position = m_offset;
			collection.ReadBundleFile(m_stream, FilePath);
		}

		public void ReadWebFile(FileCollection collection)
		{
			m_stream.Position = m_offset;
			collection.ReadWebFile(m_stream, FilePath);
		}

		public override string ToString()
		{
			return Name;
		}

		public abstract FileEntryType EntryType { get; }
		public string Name { get; }
		public string FilePath { get; }

		protected bool IsSerializedFile
		{
			get
			{
				if (IsSkipFile)
				{
					return false;
				}
				if (IsResourceFile)
				{
					return false;
				}

				return IsSerializedFileInner;
			}
		}

		protected bool IsResourceFile
		{
			get
			{
				string ext = Path.GetExtension(Name);
				switch (ext)
				{
					case ResourceExtension:
					case ResExtension:
						return true;

					default:
						return IsResourceFileInner;
				}
			}
		}

		protected bool IsSkipFile
		{
			get
			{
				string ext = Path.GetExtension(Name);
				switch (ext)
				{
					case MdbExtension:
						return true;

					case CawExtention:
					case EcmExtention:
					case SseExtention:
					case RgbExtention:
					case VisExtention:
						return true;

					default:
						return IsSkipFileInner;
				}
			}
		}

		protected virtual bool IsSerializedFileInner => true;
		protected virtual bool IsResourceFileInner => false;
		protected virtual bool IsSkipFileInner => false;
		
		private const string ResourceExtension = ".resource";
		private const string ResExtension = ".resS";

		private const string MdbExtension = ".mdb";
		// Scene GI extensions
		private const string CawExtention = ".caw";
		private const string EcmExtention = ".ecm";
		private const string SseExtention = ".sse";
		private const string RgbExtention = ".rgb";
		private const string VisExtention = ".vis";

		protected readonly Stream m_stream;
		protected readonly long m_offset;
		protected readonly long m_size;

		private readonly bool m_isStreamPermanent;
	}
}
