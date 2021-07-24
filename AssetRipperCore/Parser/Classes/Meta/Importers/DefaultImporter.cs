using AssetRipper.Converters.Project;
using AssetRipper.Layout;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Meta.Importers.Asset;
using AssetRipper.Parser.Files;
using AssetRipper.IO.Asset;
using AssetRipper.YAML;

namespace AssetRipper.Parser.Classes.Meta.Importers
{
	public sealed class DefaultImporter : AssetImporter
	{
		public DefaultImporter(AssetLayout layout) : base(layout) { }

		public DefaultImporter(AssetInfo assetInfo) : base(assetInfo) { }

		public override bool IncludesImporter(Version version)
		{
			return version.IsGreaterEqual(4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			PostRead(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			PostWrite(writer);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			PostExportYAML(container, node);
			return node;
		}

		public override ClassIDType ClassID => ClassIDType.DefaultImporter;

		protected override bool IncludesIDToName => false;
	}
}
