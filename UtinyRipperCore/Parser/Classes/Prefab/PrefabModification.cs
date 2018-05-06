using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Prefabs
{
	public struct PrefabModification : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			TransformParent.Read(stream);
			m_modifications = stream.ReadArray<PropertyModification>();
			m_removedComponents = stream.ReadArray<PPtr<Object>>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_TransformParent", TransformParent.ExportYAML(container));
			node.Add("m_Modifications", m_modifications == null ? YAMLSequenceNode.Empty : Modifications.ExportYAML(container));
			node.Add("m_RemovedComponents", m_removedComponents == null ? YAMLSequenceNode.Empty : RemovedComponents.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<PropertyModification> Modifications => m_modifications;
		public IReadOnlyList<PPtr<Object>> RemovedComponents => m_removedComponents;

		public PPtr<Transform> TransformParent;

		private PropertyModification[] m_modifications;
		private PPtr<Object>[] m_removedComponents;
	}
}
