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

			Inputs = reader.ReadAssetArray<ClusterInput>();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(InputsName, Inputs.ExportYAML(container));
			return node;
		}

		public ClusterInput[] Inputs { get; set; }

		public const string InputsName = "m_Inputs";
	}
}
