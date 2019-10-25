using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public abstract class Texture : NamedObject
	{
		protected Texture(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater and not Release
		/// </summary>
		public static bool IsReadImageContentsHash(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadFallbackFormat(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

#if UNIVERSAL
			if (IsReadImageContentsHash(reader.Version, reader.Flags))
			{
				ImageContentsHash.Read(reader);
			}
#endif
			if (IsReadFallbackFormat(reader.Version))
			{
				ForcedFallbackFormat = reader.ReadInt32();
				DownscaleFallback = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (IsReadImageContentsHash(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ImageContentsHashName, GetImageContentsHash(container.Version, container.Flags).ExportYAML(container));
			}
			if (IsReadFallbackFormat(container.ExportVersion))
			{
				node.Add(ForcedFallbackFormatName, ForcedFallbackFormat);
				node.Add(DownscaleFallbackName, DownscaleFallback);
			}
			return node;
		}

		private Hash128 GetImageContentsHash(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			return IsReadImageContentsHash(version, flags) ? ImageContentsHash : default;
#else
			return default;
#endif
		}

		public int ForcedFallbackFormat { get; private set; }
		public bool DownscaleFallback { get; private set; }

		public const string ImageContentsHashName = "m_ImageContentsHash";
		public const string ForcedFallbackFormatName = "m_ForcedFallbackFormat";
		public const string DownscaleFallbackName = "m_DownscaleFallback";

#if UNIVERSAL
		public Hash128 ImageContentsHash;
#endif
	}
}
