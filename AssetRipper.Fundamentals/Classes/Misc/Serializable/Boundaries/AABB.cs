using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Misc.Serializable.Boundaries
{
	public sealed class AABB : IAsset, IAABB
	{
		public AABB() : this(new Vector3f(), new Vector3f()) { }
		public AABB(IVector3f center, IVector3f extent)
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(CenterName, Center.ExportYaml(container));
			node.Add(ExtentName, Extent.ExportYaml(container));
			return node;
		}

		public override string ToString()
		{
			return $"C:{Center} E:{Extent}";
		}

		public IVector3f Center { get; }
		public IVector3f Extent { get; }

		public const string CenterName = "m_Center";
		public const string ExtentName = "m_Extent";
	}
}
