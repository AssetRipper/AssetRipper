using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Prefabs
{
	public struct PropertyModification : IAsset, IDependent
	{
		public PropertyModification(Version version)
		{
			PropertyPath = string.Empty;
			Value = string.Empty;
			Target = default;
			ObjectReference = default;
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddNode(nameof(PropertyModification), name);
			context.BeginChildren();
			context.AddString(PropertyPathName);
			context.AddString(ValueName);
			context.AddPPtr(nameof(Object), ObjectReferenceName);
			context.EndChildren();
		}

		public void Read(AssetReader reader)
		{
			Target.Read(reader);
			PropertyPath = reader.ReadString();
			Value = reader.ReadString();
			ObjectReference.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Target.Write(writer);
			writer.Write(PropertyPath);
			writer.Write(Value);
			ObjectReference.Write(writer);
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

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Target, TargetName);
			yield return context.FetchDependency(ObjectReference, ObjectReferenceName);
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
