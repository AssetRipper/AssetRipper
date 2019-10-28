using System.Collections.Generic;
using System.Linq;
using System.IO;
using uTinyRipper.Classes.ResourceManagers;
using uTinyRipper.YAML;
using System;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class ResourceManager : GlobalGameManager
	{
		public ResourceManager(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 3.5.0 and greater and Release
		/// </summary>
		public static bool IsReadDependentAssets(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(3, 5) && flags.IsRelease();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_container = reader.ReadKVPStringTArray<PPtr<Object>>();
			if (IsReadDependentAssets(reader.Version, reader.Flags))
			{
				m_dependentAssets = reader.ReadAssetArray<ResourceManagerDependency>();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(Container.Select(t => t.Value), ContainerName))
			{
				yield return asset;
			}
			if (IsReadDependentAssets(context.Version, context.Flags))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(DependentAssets, DependentAssetsName))
				{
					yield return asset;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ContainerName, Container.ExportYAML(container));
			if (IsReadDependentAssets(container.Version, container.ExportFlags))
			{
				node.Add(DependentAssetsName, DependentAssets.ExportYAML(container));
			}
			return node;
		}

#warning TODO: create <asset, path> lookup in ExportContainer
		public bool TryGetResourcePathFromAsset(Object asset, out string resourcePath)
		{
			foreach (KeyValuePair<string, PPtr<Object>> containerEntry in m_container)
			{
				if (containerEntry.Value.IsAsset(File, asset))
				{
					string validName = asset.TryGetName();
					string resourceName = containerEntry.Key;
					if (validName.Length > 0 && validName != resourceName && resourceName.EndsWith(validName, StringComparison.OrdinalIgnoreCase))
					{
						string directoryPath = resourceName.Substring(0, resourceName.Length - validName.Length);
						resourcePath = Path.Combine(AssetsKeyword, ResourceKeyword, directoryPath, validName);
					}
					else
					{
						resourcePath = Path.Combine(AssetsKeyword, ResourceKeyword, resourceName);
					}
					return true;
				}
			}

			resourcePath = string.Empty;
			return false;
		}
		
		public IReadOnlyList<KeyValuePair<string, PPtr<Object>>> Container => m_container;
		public ILookup<string, PPtr<Object>> ContainerMap => Container.ToLookup(t => t.Key, t => t.Value);
		public IReadOnlyList<ResourceManagerDependency> DependentAssets => m_dependentAssets;

		public const string ResourceKeyword = "Resources";

		public const string ContainerName = "m_Container";
		public const string DependentAssetsName = "m_DependentAssets";

		private KeyValuePair<string, PPtr<Object>>[] m_container;
		private ResourceManagerDependency[] m_dependentAssets;
	}
}
