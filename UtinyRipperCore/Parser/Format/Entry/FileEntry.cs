using System;
using System.IO;

namespace UtinyRipper
{
	internal abstract class FileEntry
	{
		protected FileEntry(Stream stream, string name, long offset, long size, bool isStreamPermanent)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			if (string.IsNullOrEmpty(name))
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
			m_offset = offset;
			m_size = size;
			m_isStreamPermanent = isStreamPermanent;
		}

		public ResourcesFile ReadResourcesFile(string filePath)
		{
			Stream stream = m_stream;
			long offset = m_offset;
			if (!m_isStreamPermanent)
			{
				stream = new MemoryStream();
				offset = 0;
				m_stream.Position = m_offset;
				m_stream.CopyStream(stream, m_size);
			}

			ResourcesFile resesFile = new ResourcesFile(filePath, Name, stream, offset);
			return resesFile;
		}

		public override string ToString()
		{
			return Name;
		}

		public bool IsSerializedFile
		{
			get
			{
				if(IsSkipFile)
				{
					return false;
				}

				string ext = Path.GetExtension(Name);
				if (ext == ResourceExtension)
				{
					return false;
				}
				if (ext == ResExtension)
				{
					return false;
				}
				return true;
			}
		}

		public virtual bool IsSkipFile
		{
			get
			{
				string ext = Path.GetExtension(Name);
				if (ext == ManifestExtention)
				{
					return true;
				}
				if(ext == CawExtention || ext == EcmExtention || ext == SseExtention ||
					ext == RgbExtention || ext == VisExtention)
				{
					return true;
				}

				return false;
			}
		}

		public string Name { get; }
		
		private const string ManifestExtention = ".manifest";
		private const string ResourceExtension = ".resource";
		private const string ResExtension = ".resS";
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
