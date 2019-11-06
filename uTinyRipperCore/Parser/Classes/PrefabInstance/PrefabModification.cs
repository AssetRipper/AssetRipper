using System;
using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Prefabs
{
	public struct PrefabModification : IAsset, IDependent
	{
		public PrefabModification(Version versione)
		{
			TransformParent = default;
			Modifications = Array.Empty<PropertyModification>();
			RemovedComponents = Array.Empty<PPtr<Component>>();
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			context.AddNode(nameof(PrefabModification), name);
			context.BeginChildren();
			context.AddPPtr(nameof(Transform), TransformParentName);
			context.AddArray(ModificationsName, PropertyModification.GenerateTypeTree);
			if (IsRemovedComponentsComponentPointer(context.Version))
			{
				context.AddArray(RemovedComponentsName, PPtr<Component>.GenerateTypeTree);
			}
			else
			{
				context.AddArray(RemovedComponentsName, PPtr<Object>.GenerateTypeTree);
			}
			context.EndChildren();
		}

		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		private static bool IsRemovedComponentsComponentPointer(Version version) => version.IsGreaterEqual(2018, 3);

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
			node.Add(TransformParentName, TransformParent.ExportYAML(container));
			node.Add(ModificationsName, Modifications.ExportYAML(container));
			node.Add(RemovedComponentsName, RemovedComponents.ExportYAML(container));
			return node;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(TransformParent, TransformParentName);
			foreach (PPtr<Object> asset in context.FetchDependencies(Modifications, ModificationsName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(RemovedComponents, RemovedComponentsName))
			{
				yield return asset;
			}
		}

		public PropertyModification[] Modifications { get; set; }
		public PPtr<Component>[] RemovedComponents { get; set; }

		public const string TransformParentName = "m_TransformParent";
		public const string ModificationsName = "m_Modifications";
		public const string RemovedComponentsName = "m_RemovedComponents";

		public PPtr<Transform> TransformParent;
	}
}
