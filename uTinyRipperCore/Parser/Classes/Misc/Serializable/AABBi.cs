using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public struct AABBi : IAsset
	{
		public AABBi(Vector3i center, Vector3i extent)
		{
			Center = center;
			Extent = extent;
		}

		public void Read(AssetReader reader)
		{
			Center.Read(reader);
			Extent.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Center.Write(writer);
			Extent.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			AABBiLayout layout = container.ExportLayout.Serialized.AABBi;
			node.Add(layout.CenterName, Center.ExportYAML(container));
			node.Add(layout.ExtentName, Extent.ExportYAML(container));
			return node;
		}

		public Vector3i Center;
		public Vector3i Extent;
	}
}
