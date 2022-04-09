using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes.Light
{
	public sealed class Light : Behaviour
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			// unknown conversion
			if (version.IsGreaterEqual(2019, 3))
			{
				return 10;
			}
			// InnerSpotAngle value has become configurable
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
			if (version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final, 4))
			{
				return 6;
			}
			if (version.IsEqual(5, 0, 0, UnityVersionType.Beta, 1))
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

		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasShape(UnityVersion version) => version.IsGreaterEqual(2019, 3);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasAttenuate(UnityVersion version) => version.IsLess(3);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasIntensity(UnityVersion version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 2019.1 and greater
		/// </summary>
		public static bool HasInnerSpotAngle(UnityVersion version) => version.IsGreaterEqual(2019);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasCookieSize(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasShadows(UnityVersion version) => version.IsGreaterEqual(2);
		/// <summary>
		/// 3.0.0 to 5.4.0 excludsive
		/// </summary>
		public static bool HasActuallyLightmapped(UnityVersion version) => version.IsGreaterEqual(3) && version.IsLess(5, 4);
		/// <summary>
		/// 5.4.0 to 5.6.0 exclusive
		/// </summary>
		public static bool HasBakedIndex(UnityVersion version) => version.IsGreaterEqual(5, 4) && version.IsLess(5, 6);
		/// <summary>
		/// 5.6.0 and greater and Release
		/// </summary>
		public static bool HasBakingOutput(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(5, 6) && flags.IsRelease();
		/// <summary>
		/// 1.5.0 and greater
		/// </summary>
		public static bool HasCullingMask(UnityVersion version) => version.IsGreaterEqual(1, 5);
		/// <summary>
		/// 2019.1.0b3 and greater
		/// </summary>
		public static bool HasRenderingLayerMask(UnityVersion version) => version.IsGreaterEqual(2019, 1, 0, UnityVersionType.Beta, 3);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasLightmapping(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasLightShadowCasterMode(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasAreaSize(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasBounceIntensity(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 2017.1.0b1 to 2017.1.0p4
		/// </summary>
		public static bool HasFalloffTable(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0) && version.IsLessEqual(2017, 1, 0, UnityVersionType.Patch, 4);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasColorTemperature(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.6.0b10 and greater
		/// </summary>
		public static bool HasUseColorTemperature(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 10);
		/// <summary>
		/// 2019.1.0b4 and greater
		/// </summary>
		public static bool HasBoundingSphereOverride(UnityVersion version) => version.IsGreaterEqual(2019, 1, 0, UnityVersionType.Beta, 4);
		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasUseViewFrustumForShadowCasterCull(UnityVersion version) => version.IsGreaterEqual(2020, 2);
		/// <summary>
		/// Not Release (NOTE: unknown version)
		/// </summary>
		public static bool HasShadowRadius(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease();

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2, 1);

		public Light(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Type = (LightType)reader.ReadInt32();
			if (HasShape(reader.Version))
			{
				Shape = (LightShape)reader.ReadInt32();
			}
			Color.Read(reader);
			if (HasAttenuate(reader.Version))
			{
				Attenuate = reader.ReadBoolean();
				if (IsAlign(reader.Version))
				{
					reader.AlignStream();
				}
			}
			if (HasIntensity(reader.Version))
			{
				Intensity = reader.ReadSingle();
			}
			Range = reader.ReadSingle();
			SpotAngle = reader.ReadSingle();
			if (HasInnerSpotAngle(reader.Version))
			{
				InnerSpotAngle = reader.ReadSingle();
			}
			if (HasCookieSize(reader.Version))
			{
				CookieSize = reader.ReadSingle();
			}
			if (HasShadows(reader.Version))
			{
				Shadows.Read(reader);
			}
			Cookie.Read(reader);
			DrawHalo = reader.ReadBoolean();
			if (HasActuallyLightmapped(reader.Version))
			{
				ActuallyLightmapped = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasBakedIndex(reader.Version))
			{
				BakedIndex = reader.ReadInt32();
			}
			if (HasBakingOutput(reader.Version, reader.Flags))
			{
				BakingOutput.Read(reader);
			}
			Flare.Read(reader);
			RenderMode = (LightRenderMode)reader.ReadInt32();
			if (HasCullingMask(reader.Version))
			{
				CullingMask.Read(reader);
			}
			if (HasRenderingLayerMask(reader.Version))
			{
				RenderingLayerMask = reader.ReadInt32();
			}
			if (HasLightmapping(reader.Version))
			{
				Lightmapping = (LightmappingMode)reader.ReadInt32();
			}
			if (HasLightShadowCasterMode(reader.Version))
			{
				LightShadowCasterMode = (LightShadowCasterMode)reader.ReadInt32();
			}
			if (HasAreaSize(reader.Version))
			{
				AreaSize.Read(reader);
			}
			if (HasBounceIntensity(reader.Version))
			{
				BounceIntensity = reader.ReadSingle();
			}
			if (HasFalloffTable(reader.Version))
			{
				FalloffTable.Read(reader);
			}
			if (HasColorTemperature(reader.Version))
			{
				ColorTemperature = reader.ReadSingle();
			}
			if (HasUseColorTemperature(reader.Version))
			{
				UseColorTemperature = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasBoundingSphereOverride(reader.Version))
			{
				BoundingSphereOverride.Read(reader);
				UseBoundingSphereOverride = reader.ReadBoolean();

				if (HasUseViewFrustumForShadowCasterCull(reader.Version))
				{
					//2020.2
					UseViewFrustumForShadowCasterCull = reader.ReadBoolean();
				}

				reader.AlignStream();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Cookie, CookieName);
			yield return context.FetchDependency(Flare, FlareName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TypeName, (int)Type);
			if (HasShape(container.ExportVersion))
			{
				node.Add(ShapeName, (int)Shape);
			}
			node.Add(ColorName, Color.ExportYAML(container));
			node.Add(IntensityName, Intensity);
			node.Add(RangeName, Range);
			node.Add(SpotAngleName, SpotAngle);
			if (HasInnerSpotAngle(container.ExportVersion))
			{
				node.Add(InnerSpotAngleName, InnerSpotAngle);
			}
			node.Add(CookieSizeName, CookieSize);
			node.Add(ShadowsName, Shadows.ExportYAML(container));
			node.Add(CookieName, Cookie.ExportYAML(container));
			node.Add(DrawHaloName, DrawHalo);
			if (HasBakingOutput(container.ExportVersion, container.ExportFlags))
			{
				node.Add(BakingOutputName, BakingOutput.ExportYAML(container));
			}
			node.Add(FlareName, Flare.ExportYAML(container));
			node.Add(RenderModeName, (int)RenderMode);
			node.Add(CullingMaskName, CullingMask.ExportYAML(container));
			if (HasRenderingLayerMask(container.ExportVersion))
			{
				node.Add(RenderingLayerMaskName, RenderingLayerMask);
			}
			node.Add(LightmappingName, (int)Lightmapping);
			node.Add(AreaSizeName, AreaSize.ExportYAML(container));
			node.Add(BounceIntensityName, BounceIntensity);
			node.Add(ColorTemperatureName, ColorTemperature);
			node.Add(UseColorTemperatureName, UseColorTemperature);
			if (HasBoundingSphereOverride(container.ExportVersion))
			{
				node.Add(BoundingSphereOverrideName, BoundingSphereOverride.ExportYAML(container));
				node.Add(UseBoundingSphereOverrideName, UseBoundingSphereOverride);
			}
			node.Add(ShadowRadiusName, GetShadowRadius(container.Version, container.Flags));
			node.Add(ShadowAngleName, GetShadowAngle(container.Version, container.Flags));
			return node;
		}

		private float GetShadowRadius(UnityVersion version, TransferInstructionFlags flags)
		{
			return 0.0f;
		}
		private float GetShadowAngle(UnityVersion version, TransferInstructionFlags flags)
		{
			return 0.0f;
		}

		public LightType Type { get; set; }
		public LightShape Shape { get; set; }
		public bool Attenuate { get; set; }
		public float Intensity { get; set; }
		public float Range { get; set; }
		public float SpotAngle { get; set; }
		public float InnerSpotAngle { get; set; }
		public float CookieSize { get; set; }
		public bool DrawHalo { get; set; }
		public bool ActuallyLightmapped { get; set; }
		public int BakedIndex { get; set; }
		public LightRenderMode RenderMode { get; set; }
		public int RenderingLayerMask { get; set; }
		public LightmappingMode Lightmapping { get; set; }
		public LightShadowCasterMode LightShadowCasterMode { get; set; }
		/// <summary>
		/// IndirectIntensity in 5.0.0 beta
		/// </summary>
		public float BounceIntensity { get; set; }
		/// <summary>
		/// CCT previously (before 5.6.0b10)
		/// </summary>
		public float ColorTemperature { get; set; }
		public bool UseColorTemperature { get; set; }
		public bool UseBoundingSphereOverride { get; set; }
		public bool UseViewFrustumForShadowCasterCull { get; set; }

		public const string TypeName = "m_Type";
		public const string ShapeName = "m_Shape";
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

		public ColorRGBAf Color = new();
		public ShadowSettings Shadows = new();
		public PPtr<Texture> Cookie = new();
		public LightBakingOutput BakingOutput = new();
		public PPtr<Flare.Flare> Flare = new();
		public BitField CullingMask = new();
		public Vector2f AreaSize = new();
		public FalloffTable FalloffTable = new();
		public Vector4f BoundingSphereOverride = new();
	}
}
