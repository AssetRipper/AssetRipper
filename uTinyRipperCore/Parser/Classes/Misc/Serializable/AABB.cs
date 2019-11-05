using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct AABB : IAsset
	{
		public AABB(Vector3f center, Vector3f extent)
		{
			Center = center;
			Extent = extent;
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddNode(TypeTreeUtils.BoundsName, name);
			context.BeginChildren();
			Vector3f.GenerateTypeTree(context, CenterName);
			Vector3f.GenerateTypeTree(context, ExtentName);
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

		public override string ToString()
		{
			return $"C:{Center} E:{Extent}";
		}

		public const string CenterName = "m_Center";
		public const string ExtentName = "m_Extent";

		public Vector3f Center;
		public Vector3f Extent;
	}
}
