using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	public sealed class DefaultImporter : AssetImporter
	{
		public DefaultImporter(LayoutInfo layout) : base(layout) { }

		public DefaultImporter(AssetInfo assetInfo) : base(assetInfo) { }

		public override bool IncludesImporter(UnityVersion version)
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

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			PostExportYaml(container, node);
			return node;
		}

		public override ClassIDType ClassID => ClassIDType.DefaultImporter;

		protected override bool IncludesIDToName => false;
	}
}
