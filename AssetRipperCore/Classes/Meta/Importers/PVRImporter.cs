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
	/// 5.6.0 - has been replaced by IHVImageFormatImporter
	/// 3.0.0 - first introduction
	/// </summary>
	public sealed class PVRImporter : AssetImporter
	{
		public PVRImporter(LayoutInfo layout) : base(layout) { }

		public PVRImporter(AssetInfo assetInfo) : base(assetInfo) { }

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

		public override ClassIDType ClassID => ClassIDType.PVRImporter;

		protected override bool IncludesIDToName => false;
	}
}
