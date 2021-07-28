using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Classes
{
	public sealed class Skybox : Behaviour
	{
		public Skybox(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			CustomSkybox.Read(reader);
		}

		public override IEnumerable<PPtr<Object.UnityObject>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.UnityObject> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(CustomSkybox, CustomSkyboxName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(CustomSkyboxName, CustomSkybox.ExportYAML(container));
			return node;
		}

		public const string CustomSkyboxName = "m_CustomSkybox";

		public PPtr<Material.Material> CustomSkybox;
	}
}
