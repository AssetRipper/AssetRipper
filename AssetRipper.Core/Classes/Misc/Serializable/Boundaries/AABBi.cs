using AssetRipper.Assets.Export;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.SourceGenerated.Subclasses.Vector3Int;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public sealed class AABBi : IAsset, IAABBi
	{
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
			YamlMappingNode node = new();
			node.Add(CenterName, m_Center.ExportYaml(container));
			node.Add(ExtentName, m_Extent.ExportYaml(container));
			return node;
		}

		private readonly Vector3Int m_Center = new();
		private readonly Vector3Int m_Extent = new();
		public IVector3Int Center => m_Center;
		public IVector3Int Extent => m_Extent;

		public const string CenterName = "m_Center";
		public const string ExtentName = "m_Extent";
	}
}
