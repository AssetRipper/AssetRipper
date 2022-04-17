using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.PrefabInstance
{
	public sealed class PropertyModification : IAsset, IDependent
	{
		public PropertyModification() { }
		public PropertyModification(LayoutInfo info)
		{
			PropertyPath = string.Empty;
			Value = string.Empty;
			Target = new();
			ObjectReference = new();
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(TargetName, Target.ExportYaml(container));
			node.Add(PropertyPathName, PropertyPath);
			node.Add(ValueName, Value);
			node.Add(ObjectReferenceName, ObjectReference.ExportYaml(container));
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Target, TargetName);
			yield return context.FetchDependency(ObjectReference, ObjectReferenceName);
		}

		public string PropertyPath { get; set; }
		public string Value { get; set; }

		public PPtr<Object.Object> Target = new();
		public PPtr<Object.Object> ObjectReference = new();

		public const string TargetName = "target";
		public const string PropertyPathName = "propertyPath";
		public const string ValueName = "value";
		public const string ObjectReferenceName = "objectReference";
	}
}
