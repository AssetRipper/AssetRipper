using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc.Serializable
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
			node.Add(CenterName, m_Center.ExportYAML(container));
			node.Add(ExtentName, m_Extent.ExportYAML(container));
			return node;
		}

		public override string ToString()
		{
			return $"C:{m_Center} E:{m_Extent}";
		}

		public Vector3f m_Center;
		public Vector3f m_Extent;

		public const string CenterName = "m_Center";
		public const string ExtentName = "m_Extent";
	}
}
