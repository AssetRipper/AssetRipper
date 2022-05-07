using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.NavMeshData
{
	public sealed class AutoOffMeshLinkData : IAssetReadable, IYamlExportable
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(StartName, Start.ExportYaml(container));
			node.Add(EndName, End.ExportYaml(container));
			node.Add(RadiusName, Radius);
			node.Add(LinkTypeName, LinkType);
			node.Add(AreaName, Area);
			node.Add(LinkDirectionName, LinkDirection);
			return node;
		}

		public float Radius { get; set; }
		public ushort LinkType { get; set; }
		public byte Area { get; set; }
		public byte LinkDirection { get; set; }

		public const string StartName = "m_Start";
		public const string EndName = "m_End";
		public const string RadiusName = "m_Radius";
		public const string LinkTypeName = "m_LinkType";
		public const string AreaName = "m_Area";
		public const string LinkDirectionName = "m_LinkDirection";

		public Vector3f Start = new();
		public Vector3f End = new();
	}
}
