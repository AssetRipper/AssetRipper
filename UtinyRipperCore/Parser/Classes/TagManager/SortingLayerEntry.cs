using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.TagManagers
{
	public struct SortingLayerEntry : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadUserID(Version version)
		{
			return version.IsLess(5);
		}

		public void Read(AssetStream stream)
		{
			Name = stream.ReadStringAligned();
			if(IsReadUserID(stream.Version))
			{
				UserID = stream.ReadUInt32();
			}
			UniqueID = stream.ReadUInt32();
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name);
			node.Add("uniqueID", UniqueID);
			node.Add("locked", false);
			return node;
		}

		public string Name { get; private set; }
		public uint UserID { get; private set; }
		public uint UniqueID { get; private set; }
	}
}
