using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class DefaultAsset : NamedObject
	{
		public DefaultAsset(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Message = reader.ReadString();
			IsWarning = reader.ReadBoolean();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Message", Message);
			node.Add("m_IsWarning", IsWarning);
			return node;
		}

		public string Message { get; private set; }
		public bool IsWarning { get; private set; }
	}
}
