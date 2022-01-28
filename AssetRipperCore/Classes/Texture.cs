using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public abstract class Texture : NamedObject
	{
		protected Texture(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.0.0 and greater and not Release
		/// </summary>
		public static bool HasImageContentsHash(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(5);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasFallbackFormat(UnityVersion version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasAlphaChannelOptional(UnityVersion version) => version.IsGreaterEqual(2020, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasFallbackFormat(reader.Version))
			{
				ForcedFallbackFormat = reader.ReadInt32();
				DownscaleFallback = reader.ReadBoolean();
				if (HasAlphaChannelOptional(reader.Version))
				{
					IsAlphaChannelOptional = reader.ReadBoolean();
				}
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
			if (HasAlphaChannelOptional(container.ExportVersion))
			{
				node.Add(IsAlphaChannelOptionalName, IsAlphaChannelOptional);
			}
			return node;
		}

		private Hash128 GetImageContentsHash(UnityVersion version, TransferInstructionFlags flags)
		{
			return new();
		}

		public int ForcedFallbackFormat { get; set; }
		public bool DownscaleFallback { get; set; }
		public bool IsAlphaChannelOptional { get; set; }

		public const string ImageContentsHashName = "m_ImageContentsHash";
		public const string ForcedFallbackFormatName = "m_ForcedFallbackFormat";
		public const string DownscaleFallbackName = "m_DownscaleFallback";
		public const string IsAlphaChannelOptionalName = "m_IsAlphaChannelOptional";
	}
}
