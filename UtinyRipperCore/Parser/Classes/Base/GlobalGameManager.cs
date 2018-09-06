using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public abstract class GlobalGameManager : GameManager
	{
		protected GlobalGameManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			return node;
		}

		public override string ExportName => Path.Combine("ProjectSettings", ClassID.ToString());
	}
}
