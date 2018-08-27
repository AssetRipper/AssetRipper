using System;
using System.IO;
using UtinyRipper.AssetExporters;

namespace UtinyRipper
{
	internal abstract class FileEntry
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
			m_filePath = filePath;
			m_offset = offset;
			m_size = size;
			m_isStreamPermanent = isStreamPermanent;
		}

		public void ReadResourcesFile(FileCollection collection)
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

			ResourcesFile resesFile = new ResourcesFile(m_filePath, Name, stream, offset);
			collection.AddResourceFile(resesFile);
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

				if(AssemblyManager.IsAssembly(Name))
				{
					return false;
				}

				string ext = Path.GetExtension(Name);
				switch (ext)
				{
					case ResourceExtension:
					case ResExtension:
						return false;

					default:
						return true;
				}
			}
		}

		public virtual bool IsSkipFile
		{
			get
			{
				string ext = Path.GetExtension(Name);
				switch(ext)
				{
					case ManifestExtention:
						return true;

					case CawExtention:
					case EcmExtention:
					case SseExtention:
					case RgbExtention:
					case VisExtention:
						return true;

					default:
						return false;
				}
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
		protected readonly string m_filePath;
		protected readonly long m_offset;
		protected readonly long m_size;

		private readonly bool m_isStreamPermanent;
	}
}
