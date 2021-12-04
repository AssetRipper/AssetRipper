using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	public sealed class DefaultImporter : AssetImporter, IDefaultImporter
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
