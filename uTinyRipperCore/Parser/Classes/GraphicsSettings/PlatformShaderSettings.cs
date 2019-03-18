using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.GraphicsSettingss
{
	public struct PlatformShaderSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool IsReadStandardShaderQuality(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 4) && !flags.IsRelease();
		}

		public void Read(AssetReader reader)
		{
			UseScreenSpaceShadows = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);

#if UNIVERSAL
			if(IsReadStandardShaderQuality(reader.Version, reader.Flags))
			{
				StandardShaderQuality = (ShaderQuality)reader.ReadInt32();
				UseReflectionProbeBoxProjection = reader.ReadBoolean();
				UseReflectionProbeBlending = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
#endif
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("useScreenSpaceShadows", UseScreenSpaceShadows);
			node.Add("standardShaderQuality", (int)GetStandardShaderQuality(container.Version, container.Flags));
			node.Add("useReflectionProbeBoxProjection", GetUseReflectionProbeBoxProjection(container.Version, container.Flags));
			node.Add("useReflectionProbeBlending", GetUseReflectionProbeBlending(container.Version, container.Flags));
			return node;
		}

		public ShaderQuality GetStandardShaderQuality(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadStandardShaderQuality(version, flags))
			{
				return StandardShaderQuality;
			}
#endif
			return ShaderQuality.High;
		}
		public bool GetUseReflectionProbeBoxProjection(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadStandardShaderQuality(version, flags))
			{
				return UseReflectionProbeBoxProjection;
			}
#endif
			return true;
		}
		public bool GetUseReflectionProbeBlending(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadStandardShaderQuality(version, flags))
			{
				return UseReflectionProbeBlending;
			}
#endif
			return true;
		}

		public bool UseScreenSpaceShadows { get; private set; }
#if UNIVERSAL
		public ShaderQuality StandardShaderQuality { get; private set; }
		public bool UseReflectionProbeBoxProjection { get; private set; }
		public bool UseReflectionProbeBlending { get; private set; }
#endif
	}
}
