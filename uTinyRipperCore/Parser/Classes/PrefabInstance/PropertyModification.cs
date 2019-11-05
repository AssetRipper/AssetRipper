using uTinyRipper.Converters;
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
			node.Add(TargetName, Target.ExportYAML(container));
			node.Add(PropertyPathName, PropertyPath);
			node.Add(ValueName, Value);
			node.Add(ObjectReferenceName, ObjectReference.ExportYAML(container));
			return node;
		}

		public string PropertyPath { get; set; }
		public string Value { get; set; }

		public const string TargetName = "target";
		public const string PropertyPathName = "propertyPath";
		public const string ValueName = "value";
		public const string ObjectReferenceName = "objectReference";

		public PPtr<Object> Target;
		public PPtr<Object> ObjectReference;
	}
}
