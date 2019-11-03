using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct PlatformShaderSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool HasStandardShaderQuality(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 4) && !flags.IsRelease();
		}

		public void Read(AssetReader reader)
		{
			UseScreenSpaceShadows = reader.ReadBoolean();
			reader.AlignStream();

#if UNIVERSAL
			if (HasStandardShaderQuality(reader.Version, reader.Flags))
			{
				StandardShaderQuality = (ShaderQuality)reader.ReadInt32();
				UseReflectionProbeBoxProjection = reader.ReadBoolean();
				UseReflectionProbeBlending = reader.ReadBoolean();
				reader.AlignStream();
			}
#endif
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(UseScreenSpaceShadowsName, UseScreenSpaceShadows);
			node.Add(StandardShaderQualityName, (int)GetStandardShaderQuality(container.Version, container.Flags));
			node.Add(UseReflectionProbeBoxProjectionName, GetUseReflectionProbeBoxProjection(container.Version, container.Flags));
			node.Add(UseReflectionProbeBlendingName, GetUseReflectionProbeBlending(container.Version, container.Flags));
			return node;
		}

		public ShaderQuality GetStandardShaderQuality(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasStandardShaderQuality(version, flags))
			{
				return StandardShaderQuality;
			}
#endif
			return ShaderQuality.High;
		}
		public bool GetUseReflectionProbeBoxProjection(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasStandardShaderQuality(version, flags))
			{
				return UseReflectionProbeBoxProjection;
			}
#endif
			return true;
		}
		public bool GetUseReflectionProbeBlending(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasStandardShaderQuality(version, flags))
			{
				return UseReflectionProbeBlending;
			}
#endif
			return true;
		}

		public bool UseScreenSpaceShadows { get; set; }
#if UNIVERSAL
		public ShaderQuality StandardShaderQuality { get; set; }
		public bool UseReflectionProbeBoxProjection { get; set; }
		public bool UseReflectionProbeBlending { get; set; }
#endif

		public const string UseScreenSpaceShadowsName = "useScreenSpaceShadows";
		public const string StandardShaderQualityName = "standardShaderQuality";
		public const string UseReflectionProbeBoxProjectionName = "useReflectionProbeBoxProjection";
		public const string UseReflectionProbeBlendingName = "useReflectionProbeBlending";
	}
}
