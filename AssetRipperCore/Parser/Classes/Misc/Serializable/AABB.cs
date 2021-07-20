using AssetRipper.Converters.Project;
using AssetRipper.Layout.Classes.Misc.Serializable;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.Misc.Serializable
{
	public struct AABB : IAsset
	{
		public AABB(Vector3f center, Vector3f extent)
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
			AABBLayout layout = container.ExportLayout.Serialized.AABB;
			node.Add(layout.CenterName, Center.ExportYAML(container));
			node.Add(layout.ExtentName, Extent.ExportYAML(container));
			return node;
		}

		public override string ToString()
		{
			return $"C:{Center} E:{Extent}";
		}

		public Vector3f Center;
		public Vector3f Extent;
	}
}
