using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Prefabs
{
	public struct PropertyModification : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Target.Read(reader);
			PropertyPath = reader.ReadString();
			Value = reader.ReadString();
			ObjectReference.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("target", Target.ExportYAML(container));
			node.Add("propertyPath", PropertyPath);
			node.Add("value", Value);
			node.Add("objectReference", ObjectReference.ExportYAML(container));
			return node;
		}

		public string PropertyPath { get; private set; }
		public string Value { get; private set; }

		public PPtr<Object> Target;
		public PPtr<Object> ObjectReference;
	}
}
