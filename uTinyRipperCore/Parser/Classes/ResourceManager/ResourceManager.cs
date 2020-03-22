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
		public static bool HasDependentAssets(Version version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && flags.IsRelease();

		public static string ResourceToExportPath(Object asset, string resourceName)
		{
			return AssetToExportPath(asset, ResourceBasePath, resourceName);
		}

		public static string AssetToExportPath(Object asset, string basePath, string assetPath)
		{
			bool replace = false;
			string validName = asset.TryGetName();
			if (validName.Length > 0)
			{
				if (validName != assetPath && assetPath.EndsWith(validName, StringComparison.OrdinalIgnoreCase))
				{
					if (validName.Length == assetPath.Length)
					{
						replace = true;
					}
					else if (assetPath[assetPath.Length - validName.Length - 1] == DirectorySeparator)
					{
						replace = true;
					}
				}
			}

			if (replace)
			{
				string directoryPath = assetPath.Substring(0, assetPath.Length - validName.Length);
				return Path.Combine(basePath, directoryPath + validName);
			}
			else
			{
				return Path.Combine(basePath, assetPath);
			}
		}

		public bool TryGetResourcePathFromAsset(Object asset, out string resourcePath)
		{
			foreach (KeyValuePair<string, PPtr<Object>> containerEntry in Container)
			{
				if (containerEntry.Value.IsAsset(File, asset))
				{
					resourcePath = ResourceToExportPath(asset, containerEntry.Key);
					return true;
				}
			}

			resourcePath = string.Empty;
			return false;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Container = reader.ReadKVPStringTArray<PPtr<Object>>();
			if (HasDependentAssets(reader.Version, reader.Flags))
			{
				DependentAssets = reader.ReadAssetArray<ResourceManagerDependency>();
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
			if (HasDependentAssets(context.Version, context.Flags))
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
			if (HasDependentAssets(container.Version, container.ExportFlags))
			{
				node.Add(DependentAssetsName, DependentAssets.ExportYAML(container));
			}
			return node;
		}
		
		public KeyValuePair<string, PPtr<Object>>[] Container { get; set; }
		public ResourceManagerDependency[] DependentAssets { get; set; }

		public const string ResourceKeyword = "Resources";

		private static readonly string ResourceBasePath = Path.Combine(AssetsKeyword, ResourceKeyword);
		private const char DirectorySeparator = '/';

		public const string ContainerName = "m_Container";
		public const string DependentAssetsName = "m_DependentAssets";
	}
}
