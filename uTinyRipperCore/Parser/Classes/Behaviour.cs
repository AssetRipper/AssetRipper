using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class Behaviour : Component
	{
		protected Behaviour(AssetLayout layout) :
			base(layout)
		{
			Enabled = 1;
		}

		protected Behaviour(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			BehaviourLayout layout = reader.Layout.Behaviour;
			Enabled = reader.ReadByte();
			if (layout.IsAlignEnabled)
			{
				reader.AlignStream();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			BehaviourLayout layout = writer.Layout.Behaviour;
			writer.Write(Enabled);
			if (layout.IsAlignEnabled)
			{
				writer.AlignStream();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			BehaviourLayout layout = container.ExportLayout.Behaviour;
			node.Add(layout.EnabledName, Enabled);
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

		public bool EnabledBool
		{
			get => Enabled != 0;
			set => Enabled = value ? (byte)1 : (byte)0;
		}

		public byte Enabled { get; set; }
	}
}
