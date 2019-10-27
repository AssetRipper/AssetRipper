using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.Classes.ClusterInputManagers;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class ClusterInputManager : GlobalGameManager
	{
		public ClusterInputManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_inputs = reader.ReadAssetArray<ClusterInput>();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(InputsName, Inputs.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<ClusterInput> Inputs => m_inputs;

		public const string InputsName = "m_Inputs";

		private ClusterInput[] m_inputs;
	}
}
