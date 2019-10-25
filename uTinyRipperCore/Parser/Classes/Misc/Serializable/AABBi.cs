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
			node.Add("m_Center", Center.ExportYAML(container));
			node.Add("m_Extent", Extent.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new AABBi();
		}

		public Vector3i Center;
		public Vector3i Extent;
	}
}
