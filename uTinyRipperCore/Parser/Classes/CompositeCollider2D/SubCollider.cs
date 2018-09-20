using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.CompositeCollider2Ds
{
	public struct SubCollider : IAssetReadable, IYAMLExportable, IDependent
	{
		public void Read(AssetReader reader)
		{
			Collider.Read(reader);
			m_colliderPaths = reader.ReadArray<IntPoint>();
			reader.AlignStream(AlignType.Align4);
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Collider.FetchDependency(file, isLog, () => nameof(SubCollider), "m_Collider");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Collider", Collider.ExportYAML(container));
			node.Add("m_ColliderPaths", ColliderPaths.ExportYAML(container));
			return node;
		}

		public PPtr<Collider2D> Collider;
		public IReadOnlyList<IntPoint> ColliderPaths => m_colliderPaths;

		private IntPoint[] m_colliderPaths;
	}
}
