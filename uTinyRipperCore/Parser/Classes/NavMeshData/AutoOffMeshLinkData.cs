using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshDatas
{
	public struct AutoOffMeshLinkData : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Start.Read(reader);
			End.Read(reader);
			Radius = reader.ReadSingle();
			LinkType = reader.ReadUInt16();
			Area = reader.ReadByte();
			LinkDirection = reader.ReadByte();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(StartName, Start.ExportYAML(container));
			node.Add(EndName, End.ExportYAML(container));
			node.Add(RadiusName, Radius);
			node.Add(LinkTypeName, LinkType);
			node.Add(AreaName, Area);
			node.Add(LinkDirectionName, LinkDirection);
			return node;
		}

		public float Radius { get; private set; }
		public ushort LinkType { get; private set; }
		public byte Area { get; private set; }
		public byte LinkDirection { get; private set; }

		public const string StartName = "m_Start";
		public const string EndName = "m_End";
		public const string RadiusName = "m_Radius";
		public const string LinkTypeName = "m_LinkType";
		public const string AreaName = "m_Area";
		public const string LinkDirectionName = "m_LinkDirection";

		public Vector3f Start;
		public Vector3f End;
	}
}
