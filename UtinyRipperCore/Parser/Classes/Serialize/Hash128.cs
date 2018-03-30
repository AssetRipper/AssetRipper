using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public struct Hash128 : IAssetReadable, ISerializedFileReadable, IYAMLExportable
	{
		public void Read(EndianStream stream)
		{
			Data0 = stream.ReadUInt32();
			Data1 = stream.ReadUInt32();
			Data2 = stream.ReadUInt32();
			Data3 = stream.ReadUInt32();
		}
		
		public void Read(AssetStream stream)
		{
			Read((EndianStream)stream);
		}

		public void Read(SerializedFileStream stream)
		{
			Read((EndianStream)stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new System.NotImplementedException();
		}

		public override int GetHashCode()
		{
			int hash = 17;
			unchecked
			{
				hash = hash * 31 + Data0.GetHashCode();
				hash = hash * 31 + Data1.GetHashCode();
				hash = hash * 31 + Data2.GetHashCode();
				hash = hash * 31 + Data3.GetHashCode();
			}
			return hash;
		}

		public uint Data0 { get; private set; }
		public uint Data1 { get; private set; }
		public uint Data2 { get; private set; }
		public uint Data3 { get; private set; }
	}
}
