using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct AABBi : IAsset
	{
		public AABBi(Vector3i center, Vector3i extent)
		{
			Center = center;
			Extent = extent;
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddNode(TypeTreeUtils.BoundsIntName, name);
			context.BeginChildren();
			Vector3i.GenerateTypeTree(context, CenterName);
			Vector3i.GenerateTypeTree(context, ExtentName);
			context.EndChildren();
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
			node.Add(CenterName, Center.ExportYAML(container));
			node.Add(ExtentName, Extent.ExportYAML(container));
			return node;
		}

		public const string CenterName = "m_Center";
		public const string ExtentName = "m_Extent";

		public Vector3i Center;
		public Vector3i Extent;
	}
}
