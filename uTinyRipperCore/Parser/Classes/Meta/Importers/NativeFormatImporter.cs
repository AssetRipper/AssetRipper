using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class NativeFormatImporter : AssetImporter
	{
		public NativeFormatImporter(AssetLayout layout) :
			base(layout)
		{
		}

		public NativeFormatImporter(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasMainObjectFileID(Version version) => version.IsGreaterEqual(5, 6);

		public override bool IncludesImporter(Version version)
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
