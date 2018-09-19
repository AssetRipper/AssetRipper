using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.ClusterInputManagers;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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

			m_inputs = reader.ReadArray<ClusterInput>();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Inputs", Inputs.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<ClusterInput> Inputs => m_inputs;

		private ClusterInput[] m_inputs;
	}
}
