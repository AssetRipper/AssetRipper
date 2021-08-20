using AssetRipper.Core.Project;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.GraphicsSettings
{
	public struct PlatformShaderSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater and Not Release
		/// </summary>
		public static bool HasStandardShaderQuality(UnityVersion version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 4) && !flags.IsRelease();
		}

		public void Read(AssetReader reader)
		{
			UseScreenSpaceShadows = reader.ReadBoolean();
			reader.AlignStream();

			if (HasStandardShaderQuality(reader.Version, reader.Flags))
			{
				StandardShaderQuality = (ShaderQuality)reader.ReadInt32();
				UseReflectionProbeBoxProjection = reader.ReadBoolean();
				UseReflectionProbeBlending = reader.ReadBoolean();
				reader.AlignStream();
			}
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

		public ShaderQuality GetStandardShaderQuality(UnityVersion version, TransferInstructionFlags flags)
		{
			if (HasStandardShaderQuality(version, flags))
			{
				return StandardShaderQuality;
			}
			return ShaderQuality.High;
		}
		public bool GetUseReflectionProbeBoxProjection(UnityVersion version, TransferInstructionFlags flags)
		{
			if (HasStandardShaderQuality(version, flags))
			{
				return UseReflectionProbeBoxProjection;
			}
			return true;
		}
		public bool GetUseReflectionProbeBlending(UnityVersion version, TransferInstructionFlags flags)
		{
			if (HasStandardShaderQuality(version, flags))
			{
				return UseReflectionProbeBlending;
			}
			return true;
		}

		public bool UseScreenSpaceShadows { get; set; }
		/// <summary> Editor Only </summary>
		public ShaderQuality StandardShaderQuality { get; set; }
		/// <summary> Editor Only </summary>
		public bool UseReflectionProbeBoxProjection { get; set; }
		/// <summary> Editor Only </summary>
		public bool UseReflectionProbeBlending { get; set; }

		public const string UseScreenSpaceShadowsName = "useScreenSpaceShadows";
		public const string StandardShaderQualityName = "standardShaderQuality";
		public const string UseReflectionProbeBoxProjectionName = "useReflectionProbeBoxProjection";
		public const string UseReflectionProbeBlendingName = "useReflectionProbeBlending";
	}
}
