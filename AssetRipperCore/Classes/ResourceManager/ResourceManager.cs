using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Classes.ResourceManager
{
	public sealed class ResourceManager : GlobalGameManager, IResourceManager
	{
		public ResourceManager(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 3.5.0 and greater and Release
		/// </summary>
		public static bool HasDependentAssets(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && flags.IsRelease();

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Container = reader.ReadKVPStringTArray<PPtr<Object.Object>>();
			if (HasDependentAssets(reader.Version, reader.Flags))
			{
				DependentAssets = reader.ReadAssetArray<ResourceManagerDependency>();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Container.Select(t => t.Value), ContainerName))
			{
				yield return asset;
			}
			if (HasDependentAssets(context.Version, context.Flags))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(DependentAssets, DependentAssetsName))
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

		public NullableKeyValuePair<Utf8StringBase, PPtr<IUnityObjectBase>>[] GetAssets()
		{
			return Container
				.Select(t => new NullableKeyValuePair<Utf8StringBase, PPtr<IUnityObjectBase>>(new Utf8StringLegacy(t.Key), t.Value.CastTo<IUnityObjectBase>()))
				.ToArray();
		}

		public KeyValuePair<string, PPtr<Object.Object>>[] Container { get; set; }
		public ResourceManagerDependency[] DependentAssets { get; set; }

		public const string ContainerName = "m_Container";
		public const string DependentAssetsName = "m_DependentAssets";
	}
}
