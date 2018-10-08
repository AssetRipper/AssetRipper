using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes
{
	public abstract class Behaviour : Component
	{
		protected Behaviour(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			IsEnabled = reader.ReadByte() == 0 ? false : true;
			reader.AlignStream(AlignType.Align4);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Enabled", IsEnabled);
			return node;
		}

		public bool IsEnabled { get; private set; }
	}
}
