using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Layout.Classes.PrefabInstance;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.PrefabInstance
{
	public struct PrefabModification : IAsset, IDependent
	{
		public PrefabModification(AssetLayout layout)
		{
			TransformParent = default;
			Modifications = Array.Empty<PropertyModification>();
			RemovedComponents = Array.Empty<PPtr<Component>>();
		}

		public void Read(AssetReader reader)
		{
			TransformParent.Read(reader);
			Modifications = reader.ReadAssetArray<PropertyModification>();
			RemovedComponents = reader.ReadAssetArray<PPtr<Component>>();
		}

		public void Write(AssetWriter writer)
		{
			TransformParent.Write(writer);
			Modifications.Write(writer);
			RemovedComponents.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			PrefabModificationLayout layout = container.Layout.PrefabInstance.PrefabModification;
			node.Add(layout.TransformParentName, TransformParent.ExportYAML(container));
			node.Add(layout.ModificationsName, Modifications.ExportYAML(container));
			node.Add(layout.RemovedComponentsName, RemovedComponents.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			PrefabModificationLayout layout = context.Layout.PrefabInstance.PrefabModification;
			yield return context.FetchDependency(TransformParent, layout.TransformParentName);
			foreach (PPtr<Object.Object> asset in context.FetchDependencies(Modifications, layout.ModificationsName))
			{
				yield return asset;
			}
			foreach (PPtr<Object.Object> asset in context.FetchDependencies(RemovedComponents, layout.RemovedComponentsName))
			{
				yield return asset;
			}
		}

		public PropertyModification[] Modifications { get; set; }
		public PPtr<Component>[] RemovedComponents { get; set; }

		public PPtr<Transform> TransformParent;
	}
}
