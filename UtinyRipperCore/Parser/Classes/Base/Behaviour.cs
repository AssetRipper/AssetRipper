using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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

			IsEnabled = reader.ReadByte();
			reader.AlignStream(AlignType.Align4);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Enabled", IsEnabled);
			return node;
		}

		public byte IsEnabled { get; private set; }
	}
}
