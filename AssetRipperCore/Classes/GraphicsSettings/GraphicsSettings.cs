using AssetRipper.Core.Classes.Camera;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Exporters.Engine;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes.GraphicsSettings
{
	/// <summary>
	/// RenderManager previously
	/// </summary>
	public sealed class GraphicsSettings : GlobalGameManager
	{
		public GraphicsSettings(AssetInfo assetInfo) : base(assetInfo) { }

		private GraphicsSettings(AssetInfo assetInfo, bool _) : base(assetInfo)
		{
			AlwaysIncludedShaders = Array.Empty<PPtr<Shader.Shader>>();
		}

		public static GraphicsSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new GraphicsSettings(assetInfo, true));
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			// AllowEnlightenSupportForUpgradedProject default value has been changed from True to custom?
			if (version.IsGreaterEqual(2019, 3))
			{
				return 13;
			}
			// changed TierSettings to platform specific
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 7))
			{
				return 12;
			}
			// changed default LightsUseLinearIntensity value
			// NOTE: unknown version (maybe some alpha)
			if (version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta))
			{
				return 11;
			}
			// NOTE: unknown version
			if (version.IsGreaterEqual(5, 6))
			{
				return 10;
			}
			// PlatformShaderSettings converted to TierGraphicsSettings
			if (version.IsGreaterEqual(5, 5))
			{
				return 9;
			}
			/*if (version.IsGreaterEqual(5, 5, 0, some alpha or beta))
			{
				return 8;
			}*/
			// added tier variations for platforms
			if (version.IsGreaterEqual(5, 4))
			{
				return 7;
			}
			// platform specific shaders included in AlwaysIncludedShaders
			/*if (version.IsGreaterEqual(5, 4, 0, some alpha or beta))
			{
				return 6;
			}*/
			if (version.IsGreaterEqual(5, 3))
			{
				return 5;
			}
			// LightmapKeepDynamic decomposed to LightmapKeepDynamicX (LightmapKeepDynamicPlain, ...)
			if (version.IsGreaterEqual(5, 2))
			{
				return 4;
			}
			// default shaders included in AlwaysIncludedShaders
			if (version.IsGreater(5, 0, 0, UnityVersionType.Beta, 1))
			{
				return 3;
			}
			// uGUI shaders included in AlwaysIncludedShaders
			if (version.IsGreaterEqual(4, 6))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasDeferred(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasDeferredReflections(UnityVersion version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasScreenSpaceShadows(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLegacyDeferred(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasDepthNormals(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasVideoShadersIncludeMode(UnityVersion version) => version.IsGreaterEqual(2020);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasAlwaysIncludedShaders(UnityVersion version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasPreloadedShaders(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasPreloadShadersBatchTimeLimit(UnityVersion version) => version.IsGreaterEqual(2021, 2);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasSpritesDefaultMaterial(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasCustomRenderPipeline(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasTransparencySortMode(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// Release or less than 5.6.0
		/// </summary>
		public static bool HasStaticTierGraphicsSettings(UnityVersion version, TransferInstructionFlags flags) => flags.IsRelease() || version.IsLess(5, 6);
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasEditorSettings(TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasDefaultRenderingPath(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasTierSettings(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasLightmapStripping(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasFogStripping(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasInstancingStripping(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasLightmapKeepPlain(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.0.0b2 to 5.6.0b1
		/// </summary>
		public static bool HasLightmapKeepDirSeparate(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2) && version.IsLessEqual(5, 6, 0, UnityVersionType.Beta, 1);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasLightmapKeepDynamicPlain(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.2.0 to 5.6.0b1
		/// </summary>
		public static bool HasLightmapKeepDynamicDirSeparate(UnityVersion version) => version.IsGreaterEqual(5, 2) && version.IsLessEqual(5, 6, 0, UnityVersionType.Beta, 1);
		/// <summary>
		/// 5.6.0b2 and greater
		/// </summary>
		public static bool HasLightmapKeepShadowMask(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasFogKeepLinear(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasAlbedoSwatchInfos(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 2017.1.0.b2 and greater
		/// </summary>
		public static bool HasShaderDefinesPerShaderCompiler(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasLightsUseLinearIntensity(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 2018.4.6 to 2019.1 exclusive or 2019.2.7 and greater
		/// </summary>
		public static bool HasLogWhenShaderIsCompiled(UnityVersion version)
		{
			if (version.IsGreaterEqual(2019))
			{
				return version.IsGreaterEqual(2019, 2, 7);
			}
			return version.IsGreaterEqual(2018, 4, 6);
		}
		/// <summary>
		/// 2019.3 and greater but less than 2020
		/// </summary>
		public static bool HasAllowEnlightenSupportForUpgradedProject(UnityVersion version) => version.IsGreaterEqual(2019, 3) && version.IsLess(2020);

		/// <summary>
		/// 2020.2 and greater
		/// </summary>
		public static bool HasDefaultRenderingLayerMask(UnityVersion version) => version.IsGreaterEqual(2020, 2);

		/// <summary>
		/// 5.3.0 to 5.5.0 exclusive
		/// </summary>
		private static bool HasPlatformSettings(UnityVersion version) => version.IsGreaterEqual(5, 3) && version.IsLess(5, 5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool HasPlatformSettingsTiers(UnityVersion version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		private static bool HasLightmapKeepDynamic(UnityVersion version) => version.IsLess(5, 2);
		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasSRPDefaultSettings(UnityVersion version) => version.IsGreaterEqual(2021, 2);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		private static bool IsFogStrippingFirst(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.0.0b2 to 5.3.0 exclusive
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2) && version.IsLess(5, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasDeferred(reader.Version))
			{
				Deferred.Read(reader);
			}
			if (HasDeferredReflections(reader.Version))
			{
				DeferredReflections.Read(reader);
			}
			if (HasScreenSpaceShadows(reader.Version))
			{
				ScreenSpaceShadows.Read(reader);
			}
			if (HasLegacyDeferred(reader.Version))
			{
				LegacyDeferred.Read(reader);
			}
			if (HasDepthNormals(reader.Version))
			{
				DepthNormals.Read(reader);
				MotionVectors.Read(reader);
				LightHalo.Read(reader);
				LensFlare.Read(reader);
			}

			if (HasVideoShadersIncludeMode(reader.Version))
			{
				VideoShadersIncludeMode = reader.ReadInt32();
			}

			if (HasAlwaysIncludedShaders(reader.Version))
			{
				AlwaysIncludedShaders = reader.ReadAssetArray<PPtr<Shader.Shader>>();
			}

			if (HasPreloadedShaders(reader.Version))
			{
				PreloadedShaders = reader.ReadAssetArray<PPtr<ShaderVariantCollection.ShaderVariantCollection>>();
			}

			if (HasPreloadShadersBatchTimeLimit(reader.Version))
			{
				reader.ReadInt32();
			}
			if (HasSpritesDefaultMaterial(reader.Version))
			{
				SpritesDefaultMaterial.Read(reader);
			}
			if (HasCustomRenderPipeline(reader.Version))
			{
				CustomRenderPipeline.Read(reader);
			}
			if (HasTransparencySortMode(reader.Version))
			{
				TransparencySortMode = (TransparencySortMode)reader.ReadInt32();
				TransparencySortAxis.Read(reader);
			}

			if (HasTierSettings(reader.Version))
			{
				if (HasPlatformSettings(reader.Version))
				{
					if (HasPlatformSettingsTiers(reader.Version))
					{
						PlatformSettings = new PlatformShaderSettings[3];
						PlatformSettings[0] = reader.ReadAsset<PlatformShaderSettings>();
						PlatformSettings[1] = reader.ReadAsset<PlatformShaderSettings>();
						PlatformSettings[2] = reader.ReadAsset<PlatformShaderSettings>();
					}
					else
					{
						PlatformSettings = new PlatformShaderSettings[1];
						PlatformSettings[0] = reader.ReadAsset<PlatformShaderSettings>();
					}
				}
				else
				{
					if (HasStaticTierGraphicsSettings(reader.Version, reader.Flags))
					{
						TierGraphicSettings = new TierGraphicsSettings[3];
						TierGraphicSettings[0] = reader.ReadAsset<TierGraphicsSettings>();
						TierGraphicSettings[1] = reader.ReadAsset<TierGraphicsSettings>();
						TierGraphicSettings[2] = reader.ReadAsset<TierGraphicsSettings>();
					}
				}
			}

			if (HasShaderDefinesPerShaderCompiler(reader.Version))
			{
				ShaderDefinesPerShaderCompiler = reader.ReadAssetArray<PlatformShaderDefines>();
			}

			if (HasLightsUseLinearIntensity(reader.Version))
			{
				LightsUseLinearIntensity = reader.ReadBoolean();
				LightsUseColorTemperature = reader.ReadBoolean();
			}

			if (HasDefaultRenderingLayerMask(reader.Version))
			{
				reader.AlignStream();
				DefaultRenderingLayerMask = reader.ReadInt32();
			}
			if (HasLogWhenShaderIsCompiled(reader.Version))
			{
				LogWhenShaderIsCompiled = reader.ReadBoolean();
			}
			if (HasAllowEnlightenSupportForUpgradedProject(reader.Version))
			{
				AllowEnlightenSupportForUpgradedProject = reader.ReadBoolean();
			}

			if (HasSRPDefaultSettings(reader.Version))
			{
				reader.AlignStream();
				srpDefaultSettings.Read(reader);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(Deferred, DeferredName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(DeferredReflections, DeferredReflectionsName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(ScreenSpaceShadows, ScreenSpaceShadowsName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(LegacyDeferred, LegacyDeferredName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(DepthNormals, DepthNormalsName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(MotionVectors, MotionVectorsName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(LightHalo, LightHaloName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(LensFlare, LensFlareName))
			{
				yield return asset;
			}

			if (HasAlwaysIncludedShaders(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(AlwaysIncludedShaders, AlwaysIncludedShadersName))
				{
					yield return asset;
				}
			}
			if (HasPreloadedShaders(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(PreloadedShaders, PreloadedShadersName))
				{
					yield return asset;
				}
			}
			yield return context.FetchDependency(SpritesDefaultMaterial, SpritesDefaultMaterialName);
			yield return context.FetchDependency(CustomRenderPipeline, CustomRenderPipelineName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(DeferredName, ExportDeferred(container));
			node.Add(DeferredReflectionsName, ExportDeferredReflections(container));
			node.Add(ScreenSpaceShadowsName, ExportScreenSpaceShadows(container));
			node.Add(LegacyDeferredName, ExportLegacyDeferred(container));
			node.Add(DepthNormalsName, ExportDepthNormals(container));
			node.Add(MotionVectorsName, ExportMotionVectors(container));
			node.Add(LightHaloName, ExportLightHalo(container));
			node.Add(LensFlareName, ExportLensFlare(container));
			node.Add(AlwaysIncludedShadersName, ExportAlwaysIncludedShaders(container));
			node.Add(PreloadedShadersName, GetPreloadedShaders(container.Version).ExportYAML(container));
			node.Add(SpritesDefaultMaterialName, GetSpritesDefaultMaterial(container.Version).ExportYAML(container));
			node.Add(CustomRenderPipelineName, CustomRenderPipeline.ExportYAML(container));
			node.Add(TransparencySortModeName, (int)TransparencySortMode);
			node.Add(TransparencySortAxisName, GetTransparencySortAxis(container.Version).ExportYAML(container));
			node.Add(DefaultRenderingPathName, (int)RenderingPath.Forward);
			node.Add(DefaultMobileRenderingPathName, (int)RenderingPath.Forward);
			node.Add(TierSettingsName, GetTierSettings(container.Version, container.Platform, container.Flags).ExportYAML(container));
			node.Add(LightmapStrippingName, (int)LightmapStrippingMode.Automatic);
			node.Add(FogStrippingName, (int)LightmapStrippingMode.Automatic);
			node.Add(InstancingStrippingName, (int)InstancingStrippingVariant.StripUnused);
			node.Add(LightmapKeepPlainName, true);
			node.Add(LightmapKeepDirCombinedName, true);
			node.Add(LightmapKeepDynamicPlainName, true);
			node.Add(LightmapKeepDynamicDirCombinedName, true);
			node.Add(LightmapKeepShadowMaskName, true);
			node.Add(LightmapKeepSubtractiveName, true);
			node.Add(FogKeepLinearName, true);
			node.Add(FogKeepExpName, true);
			node.Add(FogKeepExp2Name, true);
			node.Add(AlbedoSwatchInfosName, System.Array.Empty<AlbedoSwatchInfo>().ExportYAML(container));
			node.Add(LightsUseLinearIntensityName, LightsUseLinearIntensity);
			node.Add(LightsUseColorTemperatureName, LightsUseColorTemperature);
			if (HasLogWhenShaderIsCompiled(container.ExportVersion))
			{
				node.Add(LogWhenShaderIsCompiledName, LogWhenShaderIsCompiled);
			}
			if (HasAllowEnlightenSupportForUpgradedProject(container.ExportVersion))
			{
				node.Add(AllowEnlightenSupportForUpgradedProjectName, GetAllowEnlightenSupportForUpgradedProject(container.Version));
			}
			return node;
		}

		private YAMLNode ExportDeferred(IExportContainer container)
		{
			const string DefferedShading = "Hidden/Internal-DeferredShading";
			return HasDeferred(container.Version) ? Deferred.ExportYAML(container) : Deferred.ExportYAML(container, DefferedShading);
		}
		private YAMLNode ExportDeferredReflections(IExportContainer container)
		{
			const string DefferedReflection = "Hidden/Internal-DeferredReflections";
			return HasDeferredReflections(container.Version) ? DeferredReflections.ExportYAML(container) : DeferredReflections.ExportYAML(container, DefferedReflection);
		}
		private YAMLNode ExportScreenSpaceShadows(IExportContainer container)
		{
			const string ScreenShadows = "Hidden/Internal-ScreenSpaceShadows";
			return HasScreenSpaceShadows(container.Version) ? ScreenSpaceShadows.ExportYAML(container) : ScreenSpaceShadows.ExportYAML(container, ScreenShadows);
		}
		private YAMLNode ExportLegacyDeferred(IExportContainer container)
		{
			const string PrePassLighting = "Hidden/Internal-PrePassLighting";
			return HasLegacyDeferred(container.Version) ? LegacyDeferred.ExportYAML(container) : LegacyDeferred.ExportYAML(container, PrePassLighting);
		}
		private YAMLNode ExportDepthNormals(IExportContainer container)
		{
			const string DepthNormal = "Hidden/Internal-DepthNormalsTexture";
			return HasDepthNormals(container.Version) ? DepthNormals.ExportYAML(container) : DepthNormals.ExportYAML(container, DepthNormal);
		}
		private YAMLNode ExportMotionVectors(IExportContainer container)
		{
			const string Motions = "Hidden/Internal-MotionVectors";
			return HasDepthNormals(container.Version) ? MotionVectors.ExportYAML(container) : MotionVectors.ExportYAML(container, Motions);
		}
		private YAMLNode ExportLightHalo(IExportContainer container)
		{
			const string Halo = "Hidden/Internal-Halo";
			return HasDepthNormals(container.Version) ? LightHalo.ExportYAML(container) : LightHalo.ExportYAML(container, Halo);
		}
		private YAMLNode ExportLensFlare(IExportContainer container)
		{
			const string Flare = "Hidden/Internal-Flare";
			return HasDepthNormals(container.Version) ? LensFlare.ExportYAML(container) : LensFlare.ExportYAML(container, Flare);
		}
		private YAMLNode ExportAlwaysIncludedShaders(IExportContainer container)
		{
			if (ToSerializedVersion(container.Version) >= 3)
			{
				return AlwaysIncludedShaders.ExportYAML(container);
			}

			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			HashSet<string> shaderNames = new HashSet<string>();
			if (HasAlwaysIncludedShaders(container.Version))
			{
				foreach (PPtr<Shader.Shader> shaderPtr in AlwaysIncludedShaders)
				{
					node.Add(shaderPtr.ExportYAML(container));
					Shader.Shader shader = shaderPtr.FindAsset(container);
					if (shader != null)
					{
						shaderNames.Add(shader.GetValidShaderName());
					}
				}
			}

			ExportShaderPointer(container, node, shaderNames, EngineBuiltInAssets.LegacyDiffuse);
			ExportShaderPointer(container, node, shaderNames, EngineBuiltInAssets.CubeBlur);
			ExportShaderPointer(container, node, shaderNames, EngineBuiltInAssets.CubeCopy);
			ExportShaderPointer(container, node, shaderNames, EngineBuiltInAssets.CubeBlend);
			ExportShaderPointer(container, node, shaderNames, EngineBuiltInAssets.SpriteDefault);
			ExportShaderPointer(container, node, shaderNames, EngineBuiltInAssets.UIDefault);
			return node;
		}
		private IReadOnlyList<PPtr<ShaderVariantCollection.ShaderVariantCollection>> GetPreloadedShaders(UnityVersion version)
		{
			return HasPreloadedShaders(version) ? PreloadedShaders : System.Array.Empty<PPtr<ShaderVariantCollection.ShaderVariantCollection>>();
		}
		private PPtr<Material.Material> GetSpritesDefaultMaterial(UnityVersion version)
		{
			if (HasSpritesDefaultMaterial(version))
			{
				return SpritesDefaultMaterial;
			}
			Material.Material material = (Material.Material)SerializedFile.FindAsset(ClassIDType.Material, "Sprites-Default");
			return material == null ? new() : SerializedFile.CreatePPtr(material);
		}
		private Vector3f GetTransparencySortAxis(UnityVersion version)
		{
			return HasTransparencySortMode(version) ? TransparencySortAxis : new Vector3f(0.0f, 0.0f, 1.0f);
		}

		private IReadOnlyList<TierSettings> GetTierSettings(UnityVersion version, Platform platform, TransferInstructionFlags flags)
		{
			if (!HasTierSettings(version))
			{
				return System.Array.Empty<TierSettings>();
			}

			if (HasEditorSettings(flags))
			{
				return TierSettings;
			}

			if (HasPlatformSettings(version))
			{
				TierSettings[] settings = new TierSettings[PlatformSettings.Length];
				for (int i = 0; i < PlatformSettings.Length; i++)
				{
					PlatformShaderSettings psettings = PlatformSettings[i];
					settings[i] = new TierSettings(psettings, platform, (GraphicsTier)i, version, flags);
				}
				return settings;
			}
			else
			{
				TierSettings[] settings = new TierSettings[TierGraphicSettings.Length];
				for (int i = 0; i < TierGraphicSettings.Length; i++)
				{
					TierGraphicsSettings gsettings = TierGraphicSettings[i];
					settings[i] = new TierSettings(gsettings, platform, (GraphicsTier)i);
				}
				return settings;
			}
		}

		private void ExportShaderPointer(IExportContainer container, YAMLSequenceNode node, HashSet<string> shaderNames, string name)
		{
			if (!shaderNames.Contains(name))
			{
				EngineBuiltInAsset buildInAsset = EngineBuiltInAssets.GetShader(name, container.ExportVersion);
				node.Add(buildInAsset.ToExportPointer().ExportYAML(container));
			}
		}

		private bool GetAllowEnlightenSupportForUpgradedProject(UnityVersion version)
		{
			return HasAllowEnlightenSupportForUpgradedProject(version) ? AllowEnlightenSupportForUpgradedProject : true;
		}

		public PPtr<Shader.Shader>[] AlwaysIncludedShaders { get; set; }
		public PPtr<ShaderVariantCollection.ShaderVariantCollection>[] PreloadedShaders { get; set; }
		public TransparencySortMode TransparencySortMode { get; set; }
		public PlatformShaderSettings[] PlatformSettings { get; set; }
		public TierGraphicsSettings[] TierGraphicSettings { get; set; }
		public TierSettings[] TierSettings { get; set; }
		public PlatformShaderDefines[] ShaderDefinesPerShaderCompiler { get; set; }
		public bool LightsUseLinearIntensity { get; set; }
		/// <summary>
		/// LightsUseCCT previously (before 5.6.0b10)
		/// </summary>
		public bool LightsUseColorTemperature { get; set; }

		public int DefaultRenderingLayerMask { get; set; }
		public bool LogWhenShaderIsCompiled { get; set; }
		public bool AllowEnlightenSupportForUpgradedProject { get; set; }

		public int VideoShadersIncludeMode { get; set; }

		public Dictionary<string, PPtr<Object.Object>> srpDefaultSettings { get; set; } = new();

		public BuiltinShaderSettings Deferred = new();
		public BuiltinShaderSettings DeferredReflections = new();
		public BuiltinShaderSettings ScreenSpaceShadows = new();
		public BuiltinShaderSettings LegacyDeferred = new();
		public BuiltinShaderSettings DepthNormals = new();
		public BuiltinShaderSettings MotionVectors = new();
		public BuiltinShaderSettings LightHalo = new();
		public BuiltinShaderSettings LensFlare = new();
		public PPtr<Material.Material> SpritesDefaultMaterial = new();
		public PPtr<MonoBehaviour> CustomRenderPipeline = new();
		public Vector3f TransparencySortAxis = new();

		public const string DeferredName = "m_Deferred";
		public const string DeferredReflectionsName = "m_DeferredReflections";
		public const string ScreenSpaceShadowsName = "m_ScreenSpaceShadows";
		public const string LegacyDeferredName = "m_LegacyDeferred";
		public const string DepthNormalsName = "m_DepthNormals";
		public const string MotionVectorsName = "m_MotionVectors";
		public const string LightHaloName = "m_LightHalo";
		public const string LensFlareName = "m_LensFlare";
		public const string AlwaysIncludedShadersName = "m_AlwaysIncludedShaders";
		public const string PreloadedShadersName = "m_PreloadedShaders";
		public const string SpritesDefaultMaterialName = "m_SpritesDefaultMaterial";
		public const string CustomRenderPipelineName = "m_CustomRenderPipeline";
		public const string TransparencySortModeName = "m_TransparencySortMode";
		public const string TransparencySortAxisName = "m_TransparencySortAxis";
		public const string DefaultRenderingPathName = "m_DefaultRenderingPath";
		public const string DefaultMobileRenderingPathName = "m_DefaultMobileRenderingPath";
		public const string TierSettingsName = "m_TierSettings";
		public const string LightmapStrippingName = "m_LightmapStripping";
		public const string FogStrippingName = "m_FogStripping";
		public const string InstancingStrippingName = "m_InstancingStripping";
		public const string LightmapKeepPlainName = "m_LightmapKeepPlain";
		public const string LightmapKeepDirCombinedName = "m_LightmapKeepDirCombined";
		public const string LightmapKeepDynamicPlainName = "m_LightmapKeepDynamicPlain";
		public const string LightmapKeepDynamicDirCombinedName = "m_LightmapKeepDynamicDirCombined";
		public const string LightmapKeepShadowMaskName = "m_LightmapKeepShadowMask";
		public const string LightmapKeepSubtractiveName = "m_LightmapKeepSubtractive";
		public const string FogKeepLinearName = "m_FogKeepLinear";
		public const string FogKeepExpName = "m_FogKeepExp";
		public const string FogKeepExp2Name = "m_FogKeepExp2";
		public const string AlbedoSwatchInfosName = "m_AlbedoSwatchInfos";
		public const string LightsUseLinearIntensityName = "m_LightsUseLinearIntensity";
		public const string LightsUseCCTName = "m_LightsUseCCT";
		public const string LightsUseColorTemperatureName = "m_LightsUseColorTemperature";
		public const string LogWhenShaderIsCompiledName = "m_LogWhenShaderIsCompiled";
		public const string AllowEnlightenSupportForUpgradedProjectName = "m_AllowEnlightenSupportForUpgradedProject";
	}
}
