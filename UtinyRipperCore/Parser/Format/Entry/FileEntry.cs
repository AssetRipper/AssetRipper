using System;
using System.IO;

namespace UtinyRipper
{
	internal abstract class FileEntry
	{
		protected FileEntry(Stream stream, string name, long offset, long size)
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
		}

		public ResourcesFile ReadResourcesFile(string filePath)
		{
			ResourcesFile resesFile = new ResourcesFile(filePath, Name, m_stream, m_offset);
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

				return false;
			}
		}

		public string Name { get; }
		
		private const string ManifestExtention = ".manifest";
		private const string ResourceExtension = ".resource";
		private const string ResExtension = ".resS";

		protected readonly Stream m_stream;
		protected readonly long m_offset;
		protected readonly long m_size;
	}
}
