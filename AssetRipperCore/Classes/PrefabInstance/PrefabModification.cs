using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.PrefabInstance
{
	public sealed class PrefabModification : IAsset, IDependent
	{
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(TransformParentName, TransformParent.ExportYaml(container));
			node.Add(ModificationsName, Modifications.ExportYaml(container));
			node.Add(RemovedComponentsName, RemovedComponents.ExportYaml(container));
			return node;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(TransformParent, TransformParentName);
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Modifications, ModificationsName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(RemovedComponents, RemovedComponentsName))
			{
				yield return asset;
			}
		}

		public PropertyModification[] Modifications { get; set; } = Array.Empty<PropertyModification>();
		/// <summary>
		/// PPtr Object [] before 2018.3
		/// </summary>
		public PPtr<Component>[] RemovedComponents { get; set; } = Array.Empty<PPtr<Component>>();

		public PPtr<Transform> TransformParent = new();

		public const string TransformParentName = "m_TransformParent";
		public const string ModificationsName = "m_Modifications";
		public const string RemovedComponentsName = "m_RemovedComponents";
	}
}
