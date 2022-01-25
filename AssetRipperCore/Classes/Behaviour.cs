using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public abstract class Behaviour : Component, IBehaviour
	{
		protected Behaviour(LayoutInfo layout) : base(layout)
		{
			Enabled = true;
		}

		protected Behaviour(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_Enabled = reader.ReadByte();
			reader.AlignStream();
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			writer.Write(m_Enabled);
			writer.AlignStream();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(EnabledName, m_Enabled);
			return node;
		}

		protected void ReadComponent(AssetReader reader)
		{
			base.Read(reader);
		}

		protected void WriteComponent(AssetWriter writer)
		{
			base.Write(writer);
		}

		protected YAMLMappingNode ExportYAMLRootComponent(IExportContainer container)
		{
			return base.ExportYAMLRoot(container);
		}

		public bool Enabled
		{
			get => m_Enabled != 0;
			set => m_Enabled = value ? (byte)1 : (byte)0;
		}

		public byte m_Enabled { get; set; }

		public const string EnabledName = "m_Enabled";
	}
}
