using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// First introduced in 5.0.0f1 (unknown version)
	/// In 5.6.0 has been replaced by IHVImageFormatImporter
	/// </summary>
	public sealed class ASTCImporter : AssetImporter
	{
		public ASTCImporter(AssetLayout layout) :
			base(layout)
		{
		}

		public ASTCImporter(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override bool IncludesImporter(Version version)
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
