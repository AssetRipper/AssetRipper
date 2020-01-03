using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class Skybox : Behaviour
	{
		public Skybox(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			CustomSkybox.Read(reader);
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
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

		public PPtr<Material> CustomSkybox;
	}
}
