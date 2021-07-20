using AssetRipper.Converters.Project;
using AssetRipper.Layout.Classes.Misc.Serializable;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.Misc.Serializable
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
