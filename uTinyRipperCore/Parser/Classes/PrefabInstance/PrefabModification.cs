using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Prefabs
{
	public struct PrefabModification : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			TransformParent.Read(reader);
			Modifications = reader.ReadAssetArray<PropertyModification>();
			RemovedComponents = reader.ReadAssetArray<PPtr<Object>>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TransformParentName, TransformParent.ExportYAML(container));
			node.Add(ModificationsName, Modifications == null ? YAMLSequenceNode.Empty : Modifications.ExportYAML(container));
			node.Add(RemovedComponentsName, RemovedComponents == null ? YAMLSequenceNode.Empty : RemovedComponents.ExportYAML(container));
			return node;
		}

		public PropertyModification[] Modifications { get; set; }
		public PPtr<Object>[] RemovedComponents { get; set; }

		public const string TransformParentName = "m_TransformParent";
		public const string ModificationsName = "m_Modifications";
		public const string RemovedComponentsName = "m_RemovedComponents";

		public PPtr<Transform> TransformParent;
	}
}
