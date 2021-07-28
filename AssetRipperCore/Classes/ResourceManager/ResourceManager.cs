using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Classes.ResourceManager
{
	public sealed class ResourceManager : GlobalGameManager
	{
		public ResourceManager(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 3.5.0 and greater and Release
		/// </summary>
		public static bool HasDependentAssets(Version version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && flags.IsRelease();

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Container = reader.ReadKVPStringTArray<PPtr<Object.Object>>();
			if (HasDependentAssets(reader.Version, reader.Flags))
			{
				DependentAssets = reader.ReadAssetArray<ResourceManagerDependency>();
			}
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object.Object> asset in context.FetchDependencies(Container.Select(t => t.Value), ContainerName))
			{
				yield return asset;
			}
			if (HasDependentAssets(context.Version, context.Flags))
			{
				foreach (PPtr<Object.Object> asset in context.FetchDependencies(DependentAssets, DependentAssetsName))
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

		public KeyValuePair<string, PPtr<Object.Object>>[] Container { get; set; }
		public ResourceManagerDependency[] DependentAssets { get; set; }

		public const string ContainerName = "m_Container";
		public const string DependentAssetsName = "m_DependentAssets";
	}
}
