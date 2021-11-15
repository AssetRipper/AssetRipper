using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	public sealed class NativeFormatImporter : AssetImporter, INativeFormatImporter
	{
		public NativeFormatImporter(LayoutInfo layout) : base(layout) { }

		public NativeFormatImporter(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasMainObjectFileID(UnityVersion version) => version.IsGreaterEqual(5, 6);

		public override bool IncludesImporter(UnityVersion version)
		{
			return version.IsGreaterEqual(4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasMainObjectFileID(reader.Version))
			{
				MainObjectFileID = reader.ReadInt64();
			}
			reader.AlignStream();

			PostRead(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasMainObjectFileID(writer.Version))
			{
				writer.Write(MainObjectFileID);
			}
			writer.AlignStream();

			PostWrite(writer);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasMainObjectFileID(container.ExportVersion))
			{
				node.Add(MainObjectFileIDName, MainObjectFileID);
			}
			PostExportYAML(container, node);
			return node;
		}

		public override ClassIDType ClassID => ClassIDType.NativeFormatImporter;

		public long MainObjectFileID { get; set; }

		protected override bool IncludesIDToName => false;

		public const string MainObjectFileIDName = "m_MainObjectFileID";
	}
}
