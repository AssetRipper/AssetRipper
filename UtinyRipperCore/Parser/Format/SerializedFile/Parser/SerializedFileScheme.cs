using System;
using UtinyRipper.AssetExporters;

namespace UtinyRipper.SerializedFiles
{
	public sealed class SerializedFileScheme : FileScheme
	{
		public SerializedFileScheme(SmartStream stream, string filePath, string name):
			base(stream, filePath)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(filePath));
			}
			Name = name;
		}

		public SerializedFile ReadFile(IFileCollection collection, IAssemblyManager manager)
		{
			SerializedFile file = new SerializedFile(collection, manager, this);
			m_stream.Dispose();
			return file;
		}

		public string Name { get; }

		public SerializedFileHeader Header { get; private set; }
		public SerializedFileMetadata Metadata { get; private set; }
	}
}
