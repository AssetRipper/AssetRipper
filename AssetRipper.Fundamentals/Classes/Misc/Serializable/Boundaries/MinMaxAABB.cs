using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public sealed class MinMaxAABB : IAsset, IMinMaxAABB
	{
		public const string MinName = "m_Min";
		public const string MaxName = "m_Max";
		private Vector3f m_Min = new();
		private Vector3f m_Max = new();
		public IVector3f Min => m_Min;
		public IVector3f Max => m_Max;

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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(MinName, m_Min.ExportYaml(container));
			node.Add(MaxName, m_Max.ExportYaml(container));
			return node;
		}
	}
}
