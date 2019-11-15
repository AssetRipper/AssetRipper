using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// 5.6.0 - has been replaced by IHVImageFormatImporter
	/// 3.0.0 - first introduction
	/// </summary>
	public sealed class PVRImporter : AssetImporter
	{
		public PVRImporter(AssetLayout layout) :
			base(layout)
		{
		}

		public PVRImporter(AssetInfo assetInfo) :
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

		public override ClassIDType ClassID => ClassIDType.PVRImporter;

		protected override bool IncludesIDToName => false;
	}
}
