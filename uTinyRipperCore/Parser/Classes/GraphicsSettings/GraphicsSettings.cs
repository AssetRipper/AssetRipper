using System;
using System.Collections.Generic;
using uTinyRipper.Classes.Cameras;
using uTinyRipper.Classes.GraphicsSettingss;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// RenderManager previously
	/// </summary>
	public sealed class GraphicsSettings : GlobalGameManager
	{
		public GraphicsSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private GraphicsSettings(AssetInfo assetInfo, bool _):
			base(assetInfo)
		{
			AlwaysIncludedShaders = Array.Empty<PPtr<Shader>>();
		}

		public static GraphicsSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new GraphicsSettings(assetInfo, true));
		}

		public static int ToSerializedVersion(Version version)
		{
			// AllowEnlightenSupportForUpgradedProject default value has been changed from True to custom?
			if (version.IsGreaterEqual(2019, 3))
			{
				return 13;
			}
			// changed TierSettings to platform specific
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 7))
			{
				return 12;
			}
			// changed default LightsUseLinearIntensity value
			// NOTE: unknown version (maybe some alpha)
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta))
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
			if (version.IsGreater(5, 0, 0, VersionType.Beta, 1))
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
		public static bool HasDeferred(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasDeferredReflections(Version version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasScreenSpaceShadows(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLegacyDeferred(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasDepthNormals(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasAlwaysIncludedShaders(Version version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasPreloadedShaders(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasSpritesDefaultMaterial(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasCustomRenderPipeline(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 5);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasTransparencySortMode(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// Release or less than 5.6.0
		/// </summary>
		public static bool HasStaticTierGraphicsSettings(Version version, TransferInstructionFlags flags) => flags.IsRelease() || version.IsLess(5, 6);
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasEditorSettings(TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasDefaultRenderingPath(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasTierSettings(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasLightmapStripping(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasFogStripping(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasInstancingStripping(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasLightmapKeepPlain(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.0.0b2 to 5.6.0b1
		/// </summary>
		public static bool HasLightmapKeepDirSeparate(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2) && version.IsLessEqual(5, 6, 0, VersionType.Beta, 1);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasLightmapKeepDynamicPlain(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.2.0 to 5.6.0b1
		/// </summary>
		public static bool HasLightmapKeepDynamicDirSeparate(Version version) => version.IsGreaterEqual(5, 2) && version.IsLessEqual(5, 6, 0, VersionType.Beta, 1);
		/// <summary>
		/// 5.6.0b2 and greater
		/// </summary>
		public static bool HasLightmapKeepShadowMask(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool HasFogKeepLinear(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasAlbedoSwatchInfos(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 2017.1.0.b2 and greater
		/// </summary>
		public static bool HasShaderDefinesPerShaderCompiler(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasLightsUseLinearIntensity(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 2018.4.6 to 2019.1 exclusive or 2019.2.7 and greater
		/// </summary>
		public static bool HasLogWhenShaderIsCompiled(Version version)
		{
			if (version.IsGreaterEqual(2019))
			{
				return version.IsGreaterEqual(2019, 2, 7);
			}
			return version.IsGreaterEqual(2018, 4, 6);
		}
		/// <summary>
		/// 2019.3 and greater
		/// </summary>
		public static bool HasAllowEnlightenSupportForUpgradedProject(Version version) => version.IsGreaterEqual(2019, 3);

		/// <summary>
		/// 5.3.0 to 5.5.0 exclusive
		/// </summary>
		private static bool HasPlatformSettings(Version version) => version.IsGreaterEqual(5, 3) && version.IsLess(5, 5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool HasPlatformSettingsTiers(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		private static bool HasLightmapKeepDynamic(Version version) => version.IsLess(5, 2);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		private static bool IsFogStrippingFirst(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.0.0b2 to 5.3.0 exclusive
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2) && version.IsLess(5, 3);

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

			if (HasAlwaysIncludedShaders(reader.Version))
			{
				AlwaysIncludedShaders = reader.ReadAssetArray<PPtr<Shader>>();
			}

			if (HasPreloadedShaders(reader.Version))
			{
				PreloadedShaders = reader.ReadAssetArray<PPtr<ShaderVariantCollection>>();
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

#if UNIVERSAL
			if (HasEditorSettings(reader.Flags))
			{
				if (HasDefaultRenderingPath(reader.Version))
				{
					DefaultRenderingPath = (RenderingPath)reader.ReadInt32();
					DefaultMobileRenderingPath = (RenderingPath)reader.ReadInt32();
				}
				if (HasTierSettings(reader.Version))
				{
					TierSettings = reader.ReadAssetArray<TierSettings>();
				}

				if (HasLightmapStripping(reader.Version))
				{
					LightmapStripping = (LightmapStrippingMode)reader.ReadInt32();
				}
				if (HasFogStripping(reader.Version))
				{
					if (IsFogStrippingFirst(reader.Version))
					{
						FogStripping = (LightmapStrippingMode)reader.ReadInt32();
					}
				}
				if (HasInstancingStripping(reader.Version))
				{
					InstancingStripping = (InstancingStrippingVariant)reader.ReadInt32();
				}

				if (HasLightmapKeepPlain(reader.Version))
				{
					LightmapKeepPlain = reader.ReadBoolean();
					LightmapKeepDirCombined = reader.ReadBoolean();
				}
				if (HasLightmapKeepDirSeparate(reader.Version))
				{
					LightmapKeepDirSeparate = reader.ReadBoolean();
				}

				if (HasLightmapKeepDynamicPlain(reader.Version))
				{
					if (HasLightmapKeepDynamic(reader.Version))
					{
						bool lightmapKeepDynamic = reader.ReadBoolean();
						reader.AlignStream();

						LightmapKeepDynamicPlain = lightmapKeepDynamic;
						LightmapKeepDynamicDirCombined = lightmapKeepDynamic;
						LightmapKeepDynamicDirSeparate = lightmapKeepDynamic;
					}
					else
					{
						LightmapKeepDynamicPlain = reader.ReadBoolean();
						LightmapKeepDynamicDirCombined = reader.ReadBoolean();
					}
				}
				if (HasLightmapKeepDynamicDirSeparate(reader.Version))
				{
					LightmapKeepDynamicDirSeparate = reader.ReadBoolean();
				}
				if (IsAlign(reader.Version))
				{
					reader.AlignStream();
				}

				if (HasLightmapKeepShadowMask(reader.Version))
				{
					LightmapKeepShadowMask = reader.ReadBoolean();
					LightmapKeepSubtractive = reader.ReadBoolean();
				}
				if (HasFogStripping(reader.Version))
				{
					if (!IsFogStrippingFirst(reader.Version))
					{
						FogStripping = (LightmapStrippingMode)reader.ReadInt32();
					}
				}
				if (HasFogKeepLinear(reader.Version))
				{
					FogKeepLinear = reader.ReadBoolean();
					FogKeepExp = reader.ReadBoolean();
					FogKeepExp2 = reader.ReadBoolean();
					reader.AlignStream();
				}

				if (HasAlbedoSwatchInfos(reader.Version))
				{
					AlbedoSwatchInfos = reader.ReadAssetArray<AlbedoSwatchInfo>();
				}
			}
			else
#endif
			{
				if (HasShaderDefinesPerShaderCompiler(reader.Version))
				{
					ShaderDefinesPerShaderCompiler = reader.ReadAssetArray<PlatformShaderDefines>();
				}
			}

			if (HasLightsUseLinearIntensity(reader.Version))
			{
				LightsUseLinearIntensity = reader.ReadBoolean();
				LightsUseColorTemperature = reader.ReadBoolean();
			}
			if (HasLogWhenShaderIsCompiled(reader.Version))
			{
				LogWhenShaderIsCompiled = reader.ReadBoolean();
			}
			if (HasAllowEnlightenSupportForUpgradedProject(reader.Version))
			{
				AllowEnlightenSupportForUpgradedProject = reader.ReadBoolean();
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			foreach (PPtr<Object> asset in context.FetchDependencies(Deferred, DeferredName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(DeferredReflections, DeferredReflectionsName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(ScreenSpaceShadows, ScreenSpaceShadowsName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(LegacyDeferred, LegacyDeferredName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(DepthNormals, DepthNormalsName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(MotionVectors, MotionVectorsName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(LightHalo, LightHaloName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(LensFlare, LensFlareName))
			{
				yield return asset;
			}

			if (HasAlwaysIncludedShaders(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(AlwaysIncludedShaders, AlwaysIncludedShadersName))
				{
					yield return asset;
				}
			}
			if (HasPreloadedShaders(context.Version))
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(PreloadedShaders, PreloadedShadersName))
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
			node.Add(DefaultRenderingPathName, (int)GetDefaultRenderingPath(container.Version, container.Flags));
			node.Add(DefaultMobileRenderingPathName, (int)GetDefaultMobileRenderingPath(container.Version, container.Flags));
			node.Add(TierSettingsName, GetTierSettings(container.Version, container.Platform, container.Flags).ExportYAML(container));
			node.Add(LightmapStrippingName, (int)GetLightmapStripping(container.Flags));
			node.Add(FogStrippingName, (int)GetFogStripping(container.Flags));
			node.Add(InstancingStrippingName, (int)GetInstancingStripping(container.Flags));
			node.Add(LightmapKeepPlainName, GetLightmapKeepPlain(container.Version, container.Flags));
			node.Add(LightmapKeepDirCombinedName, GetLightmapKeepDirCombined(container.Version, container.Flags));
			node.Add(LightmapKeepDynamicPlainName, GetLightmapKeepDynamicPlain(container.Version, container.Flags));
			node.Add(LightmapKeepDynamicDirCombinedName, GetLightmapKeepDynamicDirCombined(container.Version, container.Flags));
			node.Add(LightmapKeepShadowMaskName, GetLightmapKeepShadowMask(container.Version, container.Flags));
			node.Add(LightmapKeepSubtractiveName, GetLightmapKeepSubtractive(container.Version, container.Flags));
			node.Add(FogKeepLinearName, GetFogKeepLinear(container.Version, container.Flags));
			node.Add(FogKeepExpName, GetFogKeepExp(container.Version, container.Flags));
			node.Add(FogKeepExp2Name, GetFogKeepExp2(container.Version, container.Flags));
			node.Add(AlbedoSwatchInfosName, GetAlbedoSwatchInfos(container.Version, container.Flags).ExportYAML(container));
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
			if(ToSerializedVersion(container.Version) >= 3)
			{
				return AlwaysIncludedShaders.ExportYAML(container);
			}

			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			HashSet<string> shaderNames = new HashSet<string>();
			if (HasAlwaysIncludedShaders(container.Version))
			{
				foreach (PPtr<Shader> shaderPtr in AlwaysIncludedShaders)
				{
					node.Add(shaderPtr.ExportYAML(container));
					Shader shader = shaderPtr.FindAsset(container);
					if(shader != null)
					{
						shaderNames.Add(shader.ValidName);
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
		private IReadOnlyList<PPtr<ShaderVariantCollection>> GetPreloadedShaders(Version version)
		{
			return HasPreloadedShaders(version) ? PreloadedShaders : System.Array.Empty<PPtr<ShaderVariantCollection>>();
		}
		private PPtr<Material> GetSpritesDefaultMaterial(Version version)
		{
			if (HasSpritesDefaultMaterial(version))
			{
				return SpritesDefaultMaterial;
			}
			Material material = (Material)File.FindAsset(ClassIDType.Material, "Sprites-Default");
			return material == null ? default : File.CreatePPtr(material);
		}
		private Vector3f GetTransparencySortAxis(Version version)
		{
			return HasTransparencySortMode(version) ? TransparencySortAxis : new Vector3f(0.0f, 0.0f, 1.0f);
		}
		private RenderingPath GetDefaultRenderingPath(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasDefaultRenderingPath(version))
			{
				return DefaultRenderingPath;
			}
#endif
			return RenderingPath.Forward;
		}
		private RenderingPath GetDefaultMobileRenderingPath(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasDefaultRenderingPath(version))
			{
				return DefaultMobileRenderingPath;
			}
#endif
			return RenderingPath.Forward;
		}
		private IReadOnlyList<TierSettings> GetTierSettings(Version version, Platform platform, TransferInstructionFlags flags)
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
		private LightmapStrippingMode GetLightmapStripping(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags))
			{
				return LightmapStripping;
			}
#endif
			return LightmapStrippingMode.Automatic;
		}
		private LightmapStrippingMode GetFogStripping(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags))
			{
				return FogStripping;
			}
#endif
			return LightmapStrippingMode.Automatic;
		}
		private InstancingStrippingVariant GetInstancingStripping(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags))
			{
				return InstancingStripping;
			}
#endif
			return InstancingStrippingVariant.StripUnused;
		}
		private bool GetLightmapKeepPlain(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasLightmapKeepPlain(version))
			{
				return LightmapKeepPlain;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepDirCombined(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasLightmapKeepPlain(version))
			{
				return LightmapKeepDirCombined;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepDynamicPlain(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasLightmapKeepDynamicPlain(version))
			{
				return LightmapKeepDynamicPlain;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepDynamicDirCombined(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasLightmapKeepDynamicPlain(version))
			{
				return LightmapKeepDynamicDirCombined;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepShadowMask(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasLightmapKeepShadowMask(version))
			{
				return LightmapKeepShadowMask;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepSubtractive(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasLightmapKeepShadowMask(version))
			{
				return LightmapKeepSubtractive;
			}
#endif
			return true;
		}
		private bool GetFogKeepLinear(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasFogKeepLinear(version))
			{
				return FogKeepLinear;
			}
#endif
			return true;
		}
		private bool GetFogKeepExp(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasFogKeepLinear(version))
			{
				return FogKeepExp;
			}
#endif
			return true;
		}
		private bool GetFogKeepExp2(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasFogKeepLinear(version))
			{
				return FogKeepExp2;
			}
#endif
			return true;
		}
		private IReadOnlyList<AlbedoSwatchInfo> GetAlbedoSwatchInfos(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (HasEditorSettings(flags) && HasAlbedoSwatchInfos(version))
			{
				return AlbedoSwatchInfos;
			}
#endif
			return System.Array.Empty<AlbedoSwatchInfo>();
		}
		
		private void ExportShaderPointer(IExportContainer container, YAMLSequenceNode node, HashSet<string> shaderNames, string name)
		{
			if (!shaderNames.Contains(name))
			{
				EngineBuiltInAsset buildInAsset = EngineBuiltInAssets.GetShader(name, container.ExportVersion);
				node.Add(buildInAsset.ToExportPointer().ExportYAML(container));
			}
		}

		private bool GetAllowEnlightenSupportForUpgradedProject(Version version)
		{
			return HasAllowEnlightenSupportForUpgradedProject(version) ? AllowEnlightenSupportForUpgradedProject : true;
		}

		public PPtr<Shader>[] AlwaysIncludedShaders { get; set; }
		public PPtr<ShaderVariantCollection>[] PreloadedShaders { get; set; }
		public TransparencySortMode TransparencySortMode { get; set; }
#if UNIVERSAL
		public RenderingPath DefaultRenderingPath { get; set; }
		public RenderingPath DefaultMobileRenderingPath { get; set; }
		public LightmapStrippingMode LightmapStripping { get; set; }
		public LightmapStrippingMode FogStripping { get; set; }
		public InstancingStrippingVariant InstancingStripping { get; set; }
		public bool LightmapKeepPlain { get; set; }
		public bool LightmapKeepDirCombined { get; set; }
		public bool LightmapKeepDirSeparate { get; set; }
		public bool LightmapKeepDynamicPlain { get; set; }
		public bool LightmapKeepDynamicDirCombined { get; set; }
		public bool LightmapKeepDynamicDirSeparate { get; set; }
		public bool LightmapKeepShadowMask { get; set; }
		public bool LightmapKeepSubtractive { get; set; }
		public bool FogKeepLinear { get; set; }
		public bool FogKeepExp { get; set; }
		public bool FogKeepExp2 { get; set; }
		public AlbedoSwatchInfo[] AlbedoSwatchInfos { get; set; }
#endif
		public PlatformShaderSettings[] PlatformSettings { get; set; }
		public TierGraphicsSettings[] TierGraphicSettings { get; set; }
		public TierSettings[] TierSettings { get; set; }
		public PlatformShaderDefines[] ShaderDefinesPerShaderCompiler { get; set; }
		public bool LightsUseLinearIntensity { get; set; }
		/// <summary>
		/// LightsUseCCT previously (before 5.6.0b10)
		/// </summary>
		public bool LightsUseColorTemperature { get; set; }
		public bool LogWhenShaderIsCompiled { get; set; }
		public bool AllowEnlightenSupportForUpgradedProject { get; set; }

		public BuiltinShaderSettings Deferred;
		public BuiltinShaderSettings DeferredReflections;
		public BuiltinShaderSettings ScreenSpaceShadows;
		public BuiltinShaderSettings LegacyDeferred;
		public BuiltinShaderSettings DepthNormals;
		public BuiltinShaderSettings MotionVectors;
		public BuiltinShaderSettings LightHalo;
		public BuiltinShaderSettings LensFlare;
		public PPtr<Material> SpritesDefaultMaterial;
		public PPtr<MonoBehaviour> CustomRenderPipeline;
		public Vector3f TransparencySortAxis;

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
