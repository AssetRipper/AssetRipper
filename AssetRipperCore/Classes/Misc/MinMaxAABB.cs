using AssetRipper.Core.Project;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using AssetRipper.Core.Math;
using System.IO;
using AssetRipper.Core.IO.Extensions;

namespace AssetRipper.Core.Classes.Misc
{
	public struct MinMaxAABB : IAsset
	{
		public const string MinName = "m_Min";
		public const string MaxName = "m_Max";
		public Vector3f m_Min;
		public Vector3f m_Max;

		public MinMaxAABB(BinaryReader reader)
		{
			m_Min = reader.ReadVector3f();
			m_Max = reader.ReadVector3f();
		}

		public void Read(AssetReader reader)
		{
			m_Min.Read(reader);
			m_Max.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			m_Min.Write(writer);
			m_Max.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(MinName, m_Min.ExportYAML(container));
			node.Add(MaxName, m_Max.ExportYAML(container));
			return node;
		}
	}
}
