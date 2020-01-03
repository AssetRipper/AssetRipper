using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// In 5.6.0 has been replaced by IHVImageFormatImporter
	/// </summary>
	public sealed class DDSImporter : AssetImporter
	{
		public DDSImporter(AssetLayout layout) :
			base(layout)
		{
		}

		public DDSImporter(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasIsReadable(Version version) => version.IsGreaterEqual(5, 5);

		public override bool IncludesImporter(Version version)
		{
			return version.IsGreaterEqual(4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasIsReadable(reader.Version))
			{
				IsReadable = reader.ReadBoolean();
			}
			reader.AlignStream();

			PostRead(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasIsReadable(writer.Version))
			{
				writer.Write(IsReadable);
			}
			writer.AlignStream();

			PostWrite(writer);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasIsReadable(container.ExportVersion))
			{
				node.Add(IsReadableName, IsReadable);
			}
			PostExportYAML(container, node);
			return node;
		}

		public override ClassIDType ClassID => ClassIDType.DDSImporter;

		public bool IsReadable { get; set; }

		protected override bool IncludesIDToName => false;

		public const string IsReadableName = "m_IsReadable";
	}
}
