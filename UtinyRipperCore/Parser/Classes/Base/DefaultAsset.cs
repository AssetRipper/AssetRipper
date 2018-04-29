using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public abstract class DefaultAsset : NamedObject
	{
		public DefaultAsset(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Message = stream.ReadStringAligned();
			IsWarning = stream.ReadBoolean();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_Message", Message);
			node.Add("m_IsWarning", IsWarning);
			return node;
		}

		public string Message { get; private set; }
		public bool IsWarning { get; private set; }
	}
}
