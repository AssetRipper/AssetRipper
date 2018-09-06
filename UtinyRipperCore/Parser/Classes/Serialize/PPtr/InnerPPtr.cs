using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public abstract class InnerPPtr<T> : IPPtr<T>
		where T: Object
	{
		public void Read(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add("fileID", GetPathID(container));
			return node;
		}

		public T FindAsset(ISerializedFile file)
		{
			throw new NotSupportedException();
		}

		public T GetAsset(ISerializedFile file)
		{
			throw new NotSupportedException();
		}

		protected abstract long GetPathID(IExportContainer container);

		public bool IsNull => false;
	}
}