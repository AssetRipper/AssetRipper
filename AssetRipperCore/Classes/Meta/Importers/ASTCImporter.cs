using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	/// <summary>
	/// First introduced in 5.0.0f1 (unknown version)
	/// In 5.6.0 has been replaced by IHVImageFormatImporter
	/// </summary>
	public sealed class ASTCImporter : AssetImporter
	{
		public ASTCImporter(LayoutInfo layout) : base(layout) { }

		public ASTCImporter(AssetInfo assetInfo) : base(assetInfo) { }

		public override bool IncludesImporter(UnityVersion version)
		{
			return true;
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

		public override ClassIDType ClassID => ClassIDType.ASTCImporter;

		protected override bool IncludesIDToName => false;
	}
}
