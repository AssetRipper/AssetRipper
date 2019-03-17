using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.ClusterInputManagers;
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
			node.Add("m_Inputs", Inputs.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<ClusterInput> Inputs => m_inputs;

		private ClusterInput[] m_inputs;
	}
}
