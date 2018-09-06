using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public abstract class Texture : NamedObject
	{
		protected Texture(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadFallbackFormat(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsExportImageContentsHash(Version version)
		{
			return Config.IsExportTopmostSerializedVersion || version.IsGreaterEqual(5);
		}
		private static bool IsExportFallbackFormat(Version version)
		{
			return Config.IsExportTopmostSerializedVersion || IsReadFallbackFormat(version);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if(IsReadFallbackFormat(reader.Version))
			{
				ForcedFallbackFormat = reader.ReadInt32();
				DownscaleFallback = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (IsExportImageContentsHash(container.Version))
			{
				node.Add("m_ImageContentsHash", ImageContentsHash.ExportYAML(container));
			}
			if (IsExportFallbackFormat(container.Version))
			{
				node.Add("m_ForcedFallbackFormat", ForcedFallbackFormat);
				node.Add("m_DownscaleFallback", DownscaleFallback);
			}
			return node;
		}
		
		public int ForcedFallbackFormat { get; private set; }
		public bool DownscaleFallback { get; private set; }

		protected virtual Hash128 ImageContentsHash => default;
	}
}
