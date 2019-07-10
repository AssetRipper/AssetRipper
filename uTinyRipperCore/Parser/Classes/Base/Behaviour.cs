using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

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

			Enabled = reader.ReadByte() == 0 ? false : true;
			reader.AlignStream(AlignType.Align4);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EnabledName, Enabled);
			return node;
		}

		protected void ReadBase(AssetReader reader)
		{
			base.Read(reader);
		}

		public const string EnabledName = "m_Enabled";

		public bool Enabled { get; private set; }
	}
}
