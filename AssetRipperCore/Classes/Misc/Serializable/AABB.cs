using AssetRipper.Project;
using AssetRipper.Layout.Classes.Misc.Serializable;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using AssetRipper.Math;
using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Classes.Misc.Serializable
{
	public struct AABB : IAsset
	{
		public AABB(Vector3f center, Vector3f extent)
		{
			m_Center = center;
			m_Extent = extent;
		}

		public AABB(ObjectReader reader)
		{
			m_Center = reader.ReadVector3f();
			m_Extent = reader.ReadVector3f();
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
			AABBLayout layout = container.ExportLayout.Serialized.AABB;
			node.Add(layout.CenterName, m_Center.ExportYAML(container));
			node.Add(layout.ExtentName, m_Extent.ExportYAML(container));
			return node;
		}

		public override string ToString()
		{
			return $"C:{m_Center} E:{m_Extent}";
		}

		public Vector3f m_Center;
		public Vector3f m_Extent;
	}
}
