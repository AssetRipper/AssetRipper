using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.ClusterInputManager
{
	public sealed class ClusterInputManager : GlobalGameManager
	{
		public ClusterInputManager(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Inputs = reader.ReadAssetArray<ClusterInput>();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(InputsName, Inputs.ExportYaml(container));
			return node;
		}

		public ClusterInput[] Inputs { get; set; }

		public const string InputsName = "m_Inputs";
	}
}
