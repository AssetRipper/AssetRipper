using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Lights;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class Light : Behaviour
	{
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public static bool IsReadAttenuate(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadIntensity(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool IsReadInnerSpotAngle(Version version)
		{
			return version.IsGreaterEqual(2019);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadCookieSize(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadShadows(Version version)
		{
			return version.IsGreaterEqual(2);
		}
		/// <summary>
		/// 3.0.0 to 5.4.0 excludsive
		/// </summary>
		public static bool IsReadActuallyLightmapped(Version version)
		{
			return version.IsGreaterEqual(3) && version.IsLess(5, 4);
		}
		/// <summary>
		/// 5.4.0 to 5.6.0 exclusive
		/// </summary>
		public static bool IsReadBakedIndex(Version version)
		{
			return version.IsGreaterEqual(5, 4) && version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.6.0 and greater and Release
		/// </summary>
		public static bool IsReadBakingOutput(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(5, 6) && flags.IsRelease();
		}
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool IsReadCullingMask(Version version)
		{
			return version.IsGreaterEqual(1, 5);
		}
		/// <summary>
		/// 2019.1.0b3 and greater
		/// </summary>
		public static bool IsReadRenderingLayerMask(Version version)
		{
			return version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 3);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadLightmapping(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadLightShadowCasterMode(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadAreaSize(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadBounceIntensity(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 2017.1.0b1 to 2017.1.0p5 exclusive
		/// </summary>
		public static bool IsReadFalloffTable(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0) && version.IsLess(2017, 1, 0, VersionType.Patch, 5);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadColorTemperature(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.6.0b10 and greater
		/// </summary>
		public static bool IsReadUseColorTemperature(Version version)
		{
			return version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 10);
		}
		/// <summary>
		/// 2019.1.0b4 and greater
		/// </summary>
		public static bool IsReadBoundingSphereOverride(Version version)
		{
			return version.IsGreaterEqual(2019, 1, 0, VersionType.Beta, 4);
		}
		public static bool IsReadShadowRadius(Version version, TransferInstructionFlags flags)
		{
			// unknown version
			return !flags.IsRelease();
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}

		private static int GetSerializedVersion(Version version)
		{
			// Range value has been recalculated
			if (version.IsGreaterEqual(2019))
			{
				return 9;
			}
			// ColorTemperature default value and enabled state has been changed
			if (version.IsGreaterEqual(5, 6))
			{
				return 8;
			}
			if (version.IsGreaterEqual(5, 4))
			{
				return 7;
			}
			if (version.IsGreaterEqual(5, 0, 0, VersionType.Final, 4))
			{
				return 6;
			}
			if (version.IsEqual(5, 0, 0, VersionType.Beta, 1))
			{
				return 4;
			}
			if (version.IsGreaterEqual(5))
			{
				throw new NotSupportedException($"Version {version} isn't supported");
			}
			if (version.IsGreaterEqual(3))
			{
				return 3;
			}
			if (version.IsGreaterEqual(2))
			{
				return 2;
			}
			return 1;
		}

		public Light(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Type = (LightType)reader.ReadInt32();
			Color.Read(reader);
			if (IsReadAttenuate(reader.Version))
			{
				Attenuate = reader.ReadBoolean();
				if (IsAlign(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}
			}
			if (IsReadIntensity(reader.Version))
			{
				Intensity = reader.ReadSingle();
			}
			Range = reader.ReadSingle();
			SpotAngle = reader.ReadSingle();
			if (IsReadInnerSpotAngle(reader.Version))
			{
				InnerSpotAngle = reader.ReadSingle();
			}
			if (IsReadCookieSize(reader.Version))
			{
				CookieSize = reader.ReadSingle();
			}
			if (IsReadShadows(reader.Version))
			{
				Shadows.Read(reader);
			}
			Cookie.Read(reader);
			DrawHalo = reader.ReadBoolean();
			if (IsReadActuallyLightmapped(reader.Version))
			{
				ActuallyLightmapped = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadBakedIndex(reader.Version))
			{
				BakedIndex = reader.ReadInt32();
			}
			if (IsReadBakingOutput(reader.Version, reader.Flags))
			{
				BakingOutput.Read(reader);
			}
			Flare.Read(reader);
			RenderMode = (LightRenderMode)reader.ReadInt32();
			if (IsReadCullingMask(reader.Version))
			{
				CullingMask.Read(reader);
			}
			if (IsReadRenderingLayerMask(reader.Version))
			{
				RenderingLayerMask = reader.ReadInt32();
			}
			if (IsReadLightmapping(reader.Version))
			{
				Lightmapping = (LightmappingMode)reader.ReadInt32();
			}
			if (IsReadLightShadowCasterMode(reader.Version))
			{
				LightShadowCasterMode = (LightShadowCasterMode)reader.ReadInt32();
			}
			if (IsReadAreaSize(reader.Version))
			{
				AreaSize.Read(reader);
			}
			if (IsReadBounceIntensity(reader.Version))
			{
				BounceIntensity = reader.ReadSingle();
			}
			if (IsReadFalloffTable(reader.Version))
			{
				FalloffTable.Read(reader);
			}
			if (IsReadColorTemperature(reader.Version))
			{
				ColorTemperature = reader.ReadSingle();
			}
			if (IsReadUseColorTemperature(reader.Version))
			{
				UseColorTemperature = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadBoundingSphereOverride(reader.Version))
			{
				BoundingSphereOverride.Read(reader);
				UseBoundingSphereOverride = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
#if UNIVERSAL
			if (IsReadShadowRadius(reader.Version, reader.Flags))
			{
				ShadowRadius = reader.ReadSingle();
				ShadowAngle = reader.ReadSingle();
			}
#endif
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return Cookie.FetchDependency(file, isLog, ToLogString, CookieName);
			yield return Flare.FetchDependency(file, isLog, ToLogString, FlareName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TypeName, (int)Type);
			node.Add(ColorName, Color.ExportYAML(container));
			node.Add(IntensityName, Intensity);
			node.Add(RangeName, Range);
			node.Add(SpotAngleName, SpotAngle);
			if (IsReadInnerSpotAngle(container.ExportVersion))
			{
				node.Add(InnerSpotAngleName, InnerSpotAngle);
			}
			node.Add(CookieSizeName, CookieSize);
			node.Add(ShadowsName, Shadows.ExportYAML(container));
			node.Add(CookieName, Cookie.ExportYAML(container));
			node.Add(DrawHaloName, DrawHalo);
			if (IsReadBakingOutput(container.ExportVersion, container.ExportFlags))
			{
				node.Add(BakingOutputName, BakingOutput.ExportYAML(container));
			}
			node.Add(FlareName, Flare.ExportYAML(container));
			node.Add(RenderModeName, (int)RenderMode);
			node.Add(CullingMaskName, CullingMask.ExportYAML(container));
			if (IsReadRenderingLayerMask(container.ExportVersion))
			{
				node.Add(RenderingLayerMaskName, RenderingLayerMask);
			}
			node.Add(LightmappingName, (int)Lightmapping);
			node.Add(AreaSizeName, AreaSize.ExportYAML(container));
			node.Add(BounceIntensityName, BounceIntensity);
			node.Add(ColorTemperatureName, ColorTemperature);
			node.Add(UseColorTemperatureName, UseColorTemperature);
			if (IsReadBoundingSphereOverride(container.ExportVersion))
			{
				node.Add(BoundingSphereOverrideName, BoundingSphereOverride.ExportYAML(container));
				node.Add(UseBoundingSphereOverrideName, UseBoundingSphereOverride);
			}
			node.Add(ShadowRadiusName, GetShadowRadius(container.Version, container.Flags));
			node.Add(ShadowAngleName, GetShadowAngle(container.Version, container.Flags));
			return node;
		}

		private float GetShadowRadius(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadShadowRadius(version, flags))
			{
				return ShadowRadius;
			}
#endif
			return 0.0f;
		}
		private float GetShadowAngle(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadShadowRadius(version, flags))
			{
				return ShadowAngle;
			}
#endif
			return 0.0f;
		}

		public LightType Type { get; private set; }
		public bool Attenuate { get; private set; }
		public float Intensity { get; private set; }
		public float Range { get; private set; }
		public float SpotAngle { get; private set; }
		public float InnerSpotAngle { get; private set; }
		public float CookieSize { get; private set; }
		public bool DrawHalo { get; private set; }
		public bool ActuallyLightmapped { get; private set; }
		public int BakedIndex { get; private set; }
		public LightRenderMode RenderMode { get; private set; }
		public int RenderingLayerMask { get; private set; }
		public LightmappingMode Lightmapping { get; private set; }
		public LightShadowCasterMode LightShadowCasterMode { get; private set; }
		/// <summary>
		/// IndirectIntensity in 5.0.0 beta
		/// </summary>
		public float BounceIntensity { get; private set; }
		/// <summary>
		/// CCT in 5.6.0 beta
		/// </summary>
		public float ColorTemperature { get; private set; }
		public bool UseColorTemperature { get; private set; }
		public bool UseBoundingSphereOverride { get; private set; }
#if UNIVERSAL
		public float ShadowRadius { get; private set; }
		public float ShadowAngle { get; private set; }
#endif

		public const string TypeName = "m_Type";
		public const string ColorName = "m_Color";
		public const string IntensityName = "m_Intensity";
		public const string RangeName = "m_Range";
		public const string SpotAngleName = "m_SpotAngle";
		public const string InnerSpotAngleName = "m_InnerSpotAngle";
		public const string CookieSizeName = "m_CookieSize";
		public const string ShadowsName = "m_Shadows";
		public const string CookieName = "m_Cookie";
		public const string DrawHaloName = "m_DrawHalo";
		public const string BakingOutputName = "m_BakingOutput";
		public const string FlareName = "m_Flare";
		public const string RenderModeName = "m_RenderMode";
		public const string CullingMaskName = "m_CullingMask";
		public const string RenderingLayerMaskName = "m_RenderingLayerMask";
		public const string LightmappingName = "m_Lightmapping";
		public const string AreaSizeName = "m_AreaSize";
		public const string BounceIntensityName = "m_BounceIntensity";
		public const string ColorTemperatureName = "m_ColorTemperature";
		public const string UseColorTemperatureName = "m_UseColorTemperature";
		public const string BoundingSphereOverrideName = "m_BoundingSphereOverride";
		public const string UseBoundingSphereOverrideName = "m_UseBoundingSphereOverride";
		public const string ShadowRadiusName = "m_ShadowRadius";
		public const string ShadowAngleName = "m_ShadowAngle";

		public ColorRGBAf Color;
		public ShadowSettings Shadows;
		public PPtr<Texture> Cookie;
		public LightBakingOutput BakingOutput;
		public PPtr<Flare> Flare;
		public BitField CullingMask;
		public Vector2f AreaSize;
		public FalloffTable FalloffTable;
		public Vector4f BoundingSphereOverride;
	}
}
