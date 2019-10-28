using System.Collections.Generic;
using uTinyRipper.Classes.ParticleSystemForceFields;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class ParticleSystemForceField : Behaviour
	{
		public ParticleSystemForceField(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Parameters.Read(reader);
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(Parameters, ParametersName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ParametersName, Parameters.ExportYAML(container));
			return node;
		}

		public const string ParametersName = "m_Parameters";

		public ParticleSystemForceFieldParameters Parameters;
	}
}
