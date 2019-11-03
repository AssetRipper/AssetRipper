using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
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
		public static bool HasImageContentsHash(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasFallbackFormat(Version version) => version.IsGreaterEqual(2017, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

#if UNIVERSAL
			if (HasImageContentsHash(reader.Version, reader.Flags))
			{
				ImageContentsHash.Read(reader);
			}
#endif
			if (HasFallbackFormat(reader.Version))
			{
				ForcedFallbackFormat = reader.ReadInt32();
				DownscaleFallback = reader.ReadBoolean();
				reader.AlignStream();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasImageContentsHash(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ImageContentsHashName, GetImageContentsHash(container.Version, container.Flags).ExportYAML(container));
			}
			if (HasFallbackFormat(container.ExportVersion))
			{
				node.Add(ForcedFallbackFormatName, ForcedFallbackFormat);
				node.Add(DownscaleFallbackName, DownscaleFallback);
			}
			return node;
		}

		private Hash128 GetImageContentsHash(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			return HasImageContentsHash(version, flags) ? ImageContentsHash : default;
#else
			return default;
#endif
		}

		public int ForcedFallbackFormat { get; set; }
		public bool DownscaleFallback { get; set; }

		public const string ImageContentsHashName = "m_ImageContentsHash";
		public const string ForcedFallbackFormatName = "m_ForcedFallbackFormat";
		public const string DownscaleFallbackName = "m_DownscaleFallback";

#if UNIVERSAL
		public Hash128 ImageContentsHash;
#endif
	}
}
