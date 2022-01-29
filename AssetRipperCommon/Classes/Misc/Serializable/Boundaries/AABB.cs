using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public sealed class AABB : IAsset
	{
		public AABB() : this(new Vector3f(), new Vector3f()) { }
		public AABB(Vector3f center, Vector3f extent)
		{
			m_Center = center;
			m_Extent = extent;
		}

		public void Read(AssetReader reader)
		{
			m_Center.Read(reader);
			m_Extent.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			m_Center.Write(writer);
			m_Extent.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(CenterName, m_Center.ExportYAML(container));
			node.Add(ExtentName, m_Extent.ExportYAML(container));
			return node;
		}

		public override string ToString()
		{
			return $"C:{m_Center} E:{m_Extent}";
		}

		public Vector3f m_Center = new();
		public Vector3f m_Extent = new();

		public const string CenterName = "m_Center";
		public const string ExtentName = "m_Extent";
	}
}
