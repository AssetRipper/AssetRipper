using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Game.Assembly;

namespace uTinyRipper.Classes
{
	public struct AABBi : ISerializableStructure
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
			node.Add(CenterName, Center.ExportYAML(container));
			node.Add(ExtentName, Extent.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			yield break;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new AABBi();
		}

		public const string CenterName = "m_Center";
		public const string ExtentName = "m_Extent";

		public Vector3i Center;
		public Vector3i Extent;
	}
}
