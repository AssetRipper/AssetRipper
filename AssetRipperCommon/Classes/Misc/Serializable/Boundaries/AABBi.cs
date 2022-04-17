using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public sealed class AABBi : IAsset, IAABBi
	{
		public AABBi() : this(new Vector3i(), new Vector3i()) { }
		public AABBi(Vector3i center, Vector3i extent)
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(CenterName, m_Center.ExportYaml(container));
			node.Add(ExtentName, m_Extent.ExportYaml(container));
			return node;
		}

		private Vector3i m_Center;
		private Vector3i m_Extent;
		public IVector3i Center => m_Center;
		public IVector3i Extent => m_Extent;

		public const string CenterName = "m_Center";
		public const string ExtentName = "m_Extent";
	}
}
