using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	/// <summary>
	/// First intriduced in 5.0.0
	/// </summary>
	public struct GISettings : IAsset
	{
		public GISettings(bool _)
		{
#warning TODO:
			SkyLightColor = default;
			SkyLightIntensity = default;
			BounceScale = 1.0f;
			IndirectOutputScale = 1.0f;
			AlbedoBoost = 1.0f;
			TemporalCoherenceThreshold = 1.0f;
			EnvironmentLightingMode = 0;
			EnableBakedLightmaps = true;
			EnableRealtimeLightmaps = true;
		}

		public static int ToSerializedVersion(Version version)
		{
			// NOTE: unknown version
			// DynamicEnv has been replaved by EnvironmentLightingMode?
			//if (version.IsGreaterEqual(5, 0, 0, VersionType.Beta))
			{
				return 2;
			}
			//return 1;
		}

		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasSkyLightColor(Version version) => version.IsEqual(5, 0, 0, VersionType.Beta);
		/// <summary>
		/// 5.0.0f1 to 2018.3 exclusive
		/// </summary>
		public static bool HasTemporalCoherenceThreshold(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta) && version.IsLess(2018, 3);
		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasDynamicEnv(Version version) => version.IsEqual(5, 0, 0, VersionType.Beta);

		public void Read(AssetReader reader)
		{
			if (HasSkyLightColor(reader.Version))
			{
				SkyLightColor.Read(reader);
				SkyLightIntensity = reader.ReadSingle();
			}

			BounceScale = reader.ReadSingle();
			IndirectOutputScale = reader.ReadSingle();
			AlbedoBoost = reader.ReadSingle();
			if (HasTemporalCoherenceThreshold(reader.Version))
			{
				TemporalCoherenceThreshold = reader.ReadSingle();
			}
			if (HasDynamicEnv(reader.Version))
			{
				DynamicEnv = reader.ReadBoolean();
			}
			else
			{
				EnvironmentLightingMode = (EnvironmentAmbeintMode)reader.ReadUInt32();
				EnableBakedLightmaps = reader.ReadBoolean();
				EnableRealtimeLightmaps = reader.ReadBoolean();
			}
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			if (HasSkyLightColor(writer.Version))
			{
				SkyLightColor.Write(writer);
				writer.Write(SkyLightIntensity);
			}

			writer.Write(BounceScale);
			writer.Write(IndirectOutputScale);
			writer.Write(AlbedoBoost);
			if (HasTemporalCoherenceThreshold(writer.Version))
			{
				writer.Write(TemporalCoherenceThreshold);
			}
			if (HasDynamicEnv(writer.Version))
			{
				writer.Write(DynamicEnv);
			}
			else
			{
				writer.Write((int)EnvironmentLightingMode);
				writer.Write(EnableBakedLightmaps);
				writer.Write(EnableRealtimeLightmaps);
			}
			writer.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (HasSkyLightColor(container.ExportVersion))
			{
				node.Add(SkyLightColorName, SkyLightColor.ExportYAML(container));
				node.Add(SkyLightIntensityName, SkyLightIntensity);
			}
			node.Add(BounceScaleName, BounceScale);
			node.Add(IndirectOutputScaleName, IndirectOutputScale);
			node.Add(AlbedoBoostName, AlbedoBoost);
			if (HasTemporalCoherenceThreshold(container.ExportVersion))
			{
				node.Add(TemporalCoherenceThresholdName, TemporalCoherenceThreshold);
			}
			if (HasDynamicEnv(container.ExportVersion))
			{
				node.Add(DynamicEnvName, DynamicEnv);
			}
			else
			{
				node.Add(EnvironmentLightingModeName, (int)EnvironmentLightingMode);
				node.Add(EnableBakedLightmapsName, EnableBakedLightmaps);
				node.Add(EnableRealtimeLightmapsName, EnableRealtimeLightmaps);
			}
			return node;
		}

		public float SkyLightIntensity { get; set; }
		public float BounceScale { get; set; }
		public float IndirectOutputScale { get; set; }
		public float AlbedoBoost { get; set; }
		public float TemporalCoherenceThreshold { get; set; }
		public bool DynamicEnv
		{
			get => EnvironmentLightingMode == EnvironmentAmbeintMode.Realtime;
			set => EnvironmentLightingMode = value ? EnvironmentAmbeintMode.Realtime : EnvironmentAmbeintMode.Baked;
		}
		public EnvironmentAmbeintMode EnvironmentLightingMode { get; set; }
		public bool EnableBakedLightmaps { get; set; }
		public bool EnableRealtimeLightmaps { get; set; }

		public const string SkyLightColorName = "m_SkyLightColor";
		public const string SkyLightIntensityName = "m_SkyLightIntensity";
		public const string BounceScaleName = "m_BounceScale";
		public const string IndirectOutputScaleName = "m_IndirectOutputScale";
		public const string AlbedoBoostName = "m_AlbedoBoost";
		public const string DynamicEnvName = "m_DynamicEnv";
		public const string TemporalCoherenceThresholdName = "m_TemporalCoherenceThreshold";
		public const string EnvironmentLightingModeName = "m_EnvironmentLightingMode";
		public const string EnableBakedLightmapsName = "m_EnableBakedLightmaps";
		public const string EnableRealtimeLightmapsName = "m_EnableRealtimeLightmaps";

		public ColorRGBAf SkyLightColor;
	}
}
