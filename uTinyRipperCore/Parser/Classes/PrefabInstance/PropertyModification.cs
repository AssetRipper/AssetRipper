using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Prefabs
{
	public struct PropertyModification : IAsset, IDependent
	{
		public PropertyModification(LayoutInfo info)
		{
			PropertyPath = string.Empty;
			Value = string.Empty;
			Target = default;
			ObjectReference = default;
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
			PropertyModificationLayout layout = container.Layout.PrefabInstance.PropertyModification;
			node.Add(layout.TargetName, Target.ExportYAML(container));
			node.Add(layout.PropertyPathName, PropertyPath);
			node.Add(layout.ValueName, Value);
			node.Add(layout.ObjectReferenceName, ObjectReference.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			PropertyModificationLayout layout = context.Layout.PrefabInstance.PropertyModification;
			yield return context.FetchDependency(Target, layout.TargetName);
			yield return context.FetchDependency(ObjectReference, layout.ObjectReferenceName);
		}

		public string PropertyPath { get; set; }
		public string Value { get; set; }

		public PPtr<Object> Target;
		public PPtr<Object> ObjectReference;
	}
}
