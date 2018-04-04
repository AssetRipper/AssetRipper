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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if(IsReadFallbackFormat(stream.Version))
			{
				ForcedFallbackFormat = stream.ReadInt32();
				DownscaleFallback = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			if (IsExportImageContentsHash(exporter.Version))
			{
				node.Add("m_ImageContentsHash", ImageContentsHash.ExportYAML(exporter));
			}
			if (IsExportFallbackFormat(exporter.Version))
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
