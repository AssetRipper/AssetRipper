using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Layout.Classes.PrefabInstance;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.PrefabInstance
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

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			PropertyModificationLayout layout = context.Layout.PrefabInstance.PropertyModification;
			yield return context.FetchDependency(Target, layout.TargetName);
			yield return context.FetchDependency(ObjectReference, layout.ObjectReferenceName);
		}

		public string PropertyPath { get; set; }
		public string Value { get; set; }

		public PPtr<Object.Object> Target;
		public PPtr<Object.Object> ObjectReference;
	}
}
