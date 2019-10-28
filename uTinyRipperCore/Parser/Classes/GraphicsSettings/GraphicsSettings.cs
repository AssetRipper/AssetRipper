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
			m_alwaysIncludedShaders = System.Array.Empty<PPtr<Shader>>();
		}

		public static GraphicsSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new GraphicsSettings(assetInfo, true));
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadDeferred(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool IsReadDeferredReflections(Version version)
		{
			return version.IsGreaterEqual(5, 2);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadScreenSpaceShadows(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLegacyDeferred(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadDepthNormals(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadAlwaysIncludedShaders(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadPreloadedShaders(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsReadSpritesDefaultMaterial(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadCustomRenderPipeline(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// Release or less than 5.6.0
		/// </summary>
		public static bool IsReadStaticTierGraphicsSettings(Version version, TransferInstructionFlags flags)
		{
			return flags.IsRelease() || version.IsLess(5, 6);
		}
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadEditorSettings(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadDefaultRenderingPath(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadTierSettings(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadLightmapStripping(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadFogStripping(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadInstancingStripping(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadLightmapKeepPlain(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.0.0b2 to 5.6.0 exclusive
		/// </summary>
		public static bool IsReadLightmapKeepDirSeparate(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2) && version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadLightmapKeepDynamicPlain(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.2.0 to 5.6.0 exclusive
		/// </summary>
		public static bool IsReadLightmapKeepDynamicDirSeparate(Version version)
		{
			return version.IsGreaterEqual(5, 2) && version.IsLess(5, 6);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadLightmapKeepShadowMask(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		public static bool IsReadFogKeepLinear(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadAlbedoSwatchInfos(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 2017.1.0.b2 and greater
		/// </summary>
		public static bool IsReadShaderDefinesPerShaderCompiler(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadLightsUseLinearIntensity(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
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
		/// 5.3.0 to 5.5.0 exclusive
		/// </summary>
		private static bool IsReadPlatformSettings(Version version)
		{
			return version.IsGreaterEqual(5, 3) && version.IsLess(5, 5);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsReadPlatformSettingsTiers(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		private static bool IsReadFogStrippingFirst(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		private static bool IsReadLightmapKeepDynamic(Version version)
		{
			return version.IsLess(5, 2);
		}
		/// <summary>
		/// 5.0.0b2 to 5.3.0 exclusive
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2) && version.IsLess(5, 3);
		}

		private static int ToSerializedVersion(Version version)
		{
			// changed TierSettings to platform specific
			if (version.IsGreaterEqual(5, 6))
			{
				return 12;
			}
			// changed default LightsUseLinearIntensity value
			/*if (version.IsGreaterEqual(5, 6, 0, some alpha or beta))
			{
				return 11;
			}
			if (version.IsGreaterEqual(5, 6, 0, some alpha or beta))
			{
				return 10;
			}*/
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if(IsReadDeferred(reader.Version))
			{
				Deferred.Read(reader);
			}
			if (IsReadDeferredReflections(reader.Version))
			{
				DeferredReflections.Read(reader);
			}
			if (IsReadScreenSpaceShadows(reader.Version))
			{
				ScreenSpaceShadows.Read(reader);
			}
			if (IsReadLegacyDeferred(reader.Version))
			{
				LegacyDeferred.Read(reader);
			}
			if (IsReadDepthNormals(reader.Version))
			{
				DepthNormals.Read(reader);
				MotionVectors.Read(reader);
				LightHalo.Read(reader);
				LensFlare.Read(reader);
			}

			if(IsReadAlwaysIncludedShaders(reader.Version))
			{
				m_alwaysIncludedShaders = reader.ReadAssetArray<PPtr<Shader>>();
			}

			if (IsReadPreloadedShaders(reader.Version))
			{
				m_preloadedShaders = reader.ReadAssetArray<PPtr<ShaderVariantCollection>>();
			}
			if (IsReadSpritesDefaultMaterial(reader.Version))
			{
				SpritesDefaultMaterial.Read(reader);
			}
			if (IsReadCustomRenderPipeline(reader.Version))
			{
				CustomRenderPipeline.Read(reader);
				TransparencySortMode = (TransparencySortMode)reader.ReadInt32();
				TransparencySortAxis.Read(reader);
			}

			if (IsReadTierSettings(reader.Version))
			{
				if (IsReadPlatformSettings(reader.Version))
				{
					if (IsReadPlatformSettingsTiers(reader.Version))
					{
						m_platformSettings = new PlatformShaderSettings[3];
						m_platformSettings[0] = reader.ReadAsset<PlatformShaderSettings>();
						m_platformSettings[1] = reader.ReadAsset<PlatformShaderSettings>();
						m_platformSettings[2] = reader.ReadAsset<PlatformShaderSettings>();
					}
					else
					{
						m_platformSettings = new PlatformShaderSettings[1];
						m_platformSettings[0] = reader.ReadAsset<PlatformShaderSettings>();
					}
				}
				else
				{
					if(IsReadStaticTierGraphicsSettings(reader.Version, reader.Flags))
					{
						m_tierGraphicSettings = new TierGraphicsSettings[3];
						m_tierGraphicSettings[0] = reader.ReadAsset<TierGraphicsSettings>();
						m_tierGraphicSettings[1] = reader.ReadAsset<TierGraphicsSettings>();
						m_tierGraphicSettings[2] = reader.ReadAsset<TierGraphicsSettings>();
					}
				}
			}

#if UNIVERSAL
			if (IsReadEditorSettings(reader.Flags))
			{
				if (IsReadDefaultRenderingPath(reader.Version))
				{
					DefaultRenderingPath = (RenderingPath)reader.ReadInt32();
					DefaultMobileRenderingPath = (RenderingPath)reader.ReadInt32();
				}
				if (IsReadTierSettings(reader.Version))
				{
					m_tierSettings = reader.ReadAssetArray<TierSettings>();
				}

				if (IsReadLightmapStripping(reader.Version))
				{
					LightmapStripping = (LightmapStrippingMode)reader.ReadInt32();
				}
				if (IsReadFogStripping(reader.Version))
				{
					if (IsReadFogStrippingFirst(reader.Version))
					{
						FogStripping = (LightmapStrippingMode)reader.ReadInt32();
					}
				}
				if (IsReadInstancingStripping(reader.Version))
				{
					InstancingStripping = (InstancingStrippingVariant)reader.ReadInt32();
				}

				if (IsReadLightmapKeepPlain(reader.Version))
				{
					LightmapKeepPlain = reader.ReadBoolean();
					LightmapKeepDirCombined = reader.ReadBoolean();
				}
				if (IsReadLightmapKeepDirSeparate(reader.Version))
				{
					LightmapKeepDirSeparate = reader.ReadBoolean();
				}

				if (IsReadLightmapKeepDynamicPlain(reader.Version))
				{
					if (IsReadLightmapKeepDynamic(reader.Version))
					{
						bool lightmapKeepDynamic = reader.ReadBoolean();
						reader.AlignStream(AlignType.Align4);

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
				if (IsReadLightmapKeepDynamicDirSeparate(reader.Version))
				{
					LightmapKeepDynamicDirSeparate = reader.ReadBoolean();
				}
				if (IsAlign(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}

				if (IsReadLightmapKeepShadowMask(reader.Version))
				{
					LightmapKeepShadowMask = reader.ReadBoolean();
					LightmapKeepSubtractive = reader.ReadBoolean();
				}
				if (IsReadFogStripping(reader.Version))
				{
					if (!IsReadFogStrippingFirst(reader.Version))
					{
						FogStripping = (LightmapStrippingMode)reader.ReadInt32();
					}
				}
				if (IsReadFogKeepLinear(reader.Version))
				{
					FogKeepLinear = reader.ReadBoolean();
					FogKeepExp = reader.ReadBoolean();
					FogKeepExp2 = reader.ReadBoolean();
					reader.AlignStream(AlignType.Align4);
				}

				if (IsReadAlbedoSwatchInfos(reader.Version))
				{
					m_albedoSwatchInfos = reader.ReadAssetArray<AlbedoSwatchInfo>();
				}
			}
			else
#endif
			{
				if(IsReadShaderDefinesPerShaderCompiler(reader.Version))
				{
					m_shaderDefinesPerShaderCompiler = reader.ReadAssetArray<PlatformShaderDefines>();
				}
			}

			if (IsReadLightsUseLinearIntensity(reader.Version))
			{
				LightsUseLinearIntensity = reader.ReadBoolean();
				LightsUseColorTemperature = reader.ReadBoolean();
			}
			if (HasLogWhenShaderIsCompiled(reader.Version))
			{
				LogWhenShaderIsCompiled = reader.ReadBoolean();
			}
		}

		public override IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			foreach(Object asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			foreach (Object asset in context.FetchDependencies(Deferred, DeferredName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(DeferredReflections, DeferredReflectionsName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(ScreenSpaceShadows, ScreenSpaceShadowsName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(LegacyDeferred, LegacyDeferredName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(DepthNormals, DepthNormalsName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(MotionVectors, MotionVectorsName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(LightHalo, LightHaloName))
			{
				yield return asset;
			}
			foreach (Object asset in context.FetchDependencies(LensFlare, LensFlareName))
			{
				yield return asset;
			}

			if (IsReadAlwaysIncludedShaders(context.Version))
			{
				foreach (Object asset in context.FetchDependencies(AlwaysIncludedShaders, AlwaysIncludedShadersName))
				{
					yield return asset;
				}
			}
			if (IsReadPreloadedShaders(context.Version))
			{
				foreach (Object asset in context.FetchDependencies(PreloadedShaders, PreloadedShadersName))
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
			return node;
		}

		private YAMLNode ExportDeferred(IExportContainer container)
		{
			const string DefferedShading = "Hidden/Internal-DeferredShading";
			return IsReadDeferred(container.Version) ? Deferred.ExportYAML(container) : Deferred.ExportYAML(container, DefferedShading);
		}
		private YAMLNode ExportDeferredReflections(IExportContainer container)
		{
			const string DefferedReflection = "Hidden/Internal-DeferredReflections";
			return IsReadDeferredReflections(container.Version) ? DeferredReflections.ExportYAML(container) : DeferredReflections.ExportYAML(container, DefferedReflection);
		}
		private YAMLNode ExportScreenSpaceShadows(IExportContainer container)
		{
			const string ScreenShadows = "Hidden/Internal-ScreenSpaceShadows";
			return IsReadScreenSpaceShadows(container.Version) ? ScreenSpaceShadows.ExportYAML(container) : ScreenSpaceShadows.ExportYAML(container, ScreenShadows);
		}
		private YAMLNode ExportLegacyDeferred(IExportContainer container)
		{
			const string PrePassLighting = "Hidden/Internal-PrePassLighting";
			return IsReadLegacyDeferred(container.Version) ? LegacyDeferred.ExportYAML(container) : LegacyDeferred.ExportYAML(container, PrePassLighting);
		}
		private YAMLNode ExportDepthNormals(IExportContainer container)
		{
			const string DepthNormal = "Hidden/Internal-DepthNormalsTexture";
			return IsReadDepthNormals(container.Version) ? DepthNormals.ExportYAML(container) : DepthNormals.ExportYAML(container, DepthNormal);
		}
		private YAMLNode ExportMotionVectors(IExportContainer container)
		{
			const string Motions = "Hidden/Internal-MotionVectors";
			return IsReadDepthNormals(container.Version) ? MotionVectors.ExportYAML(container) : MotionVectors.ExportYAML(container, Motions);
		}
		private YAMLNode ExportLightHalo(IExportContainer container)
		{
			const string Halo = "Hidden/Internal-Halo";
			return IsReadDepthNormals(container.Version) ? LightHalo.ExportYAML(container) : LightHalo.ExportYAML(container, Halo);
		}
		private YAMLNode ExportLensFlare(IExportContainer container)
		{
			const string Flare = "Hidden/Internal-Flare";
			return IsReadDepthNormals(container.Version) ? LensFlare.ExportYAML(container) : LensFlare.ExportYAML(container, Flare);
		}
		private YAMLNode ExportAlwaysIncludedShaders(IExportContainer container)
		{
			if(ToSerializedVersion(container.Version) >= 3)
			{
				return AlwaysIncludedShaders.ExportYAML(container);
			}

			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			HashSet<string> shaderNames = new HashSet<string>();
			if(IsReadAlwaysIncludedShaders(container.Version))
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
			return IsReadPreloadedShaders(version) ? PreloadedShaders : System.Array.Empty<PPtr<ShaderVariantCollection>>();
		}
		private PPtr<Material> GetSpritesDefaultMaterial(Version version)
		{
			if (IsReadSpritesDefaultMaterial(version))
			{
				return SpritesDefaultMaterial;
			}
			Material material = (Material)File.FindAsset(ClassIDType.Material, "Sprites-Default");
			return material == null ? default : File.CreatePPtr(material);
		}
		private Vector3f GetTransparencySortAxis(Version version)
		{
			return IsReadCustomRenderPipeline(version) ? TransparencySortAxis : new Vector3f(0.0f, 0.0f, 1.0f);
		}
		private RenderingPath GetDefaultRenderingPath(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadDefaultRenderingPath(version))
			{
				return DefaultRenderingPath;
			}
#endif
			return RenderingPath.Forward;
		}
		private RenderingPath GetDefaultMobileRenderingPath(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadDefaultRenderingPath(version))
			{
				return DefaultMobileRenderingPath;
			}
#endif
			return RenderingPath.Forward;
		}
		private IReadOnlyList<TierSettings> GetTierSettings(Version version, Platform platform, TransferInstructionFlags flags)
		{
			if (!IsReadTierSettings(version))
			{
				return System.Array.Empty<TierSettings>();
			}

			if (IsReadEditorSettings(flags))
			{
				return TierSettings;
			}

			if (IsReadPlatformSettings(version))
			{
				TierSettings[] settings = new TierSettings[PlatformSettings.Count];
				for (int i = 0; i < PlatformSettings.Count; i++)
				{
					PlatformShaderSettings psettings = PlatformSettings[i];
					settings[i] = new TierSettings(psettings, platform, (GraphicsTier)i, version, flags);
				}
				return settings;
			}
			else
			{
				TierSettings[] settings = new TierSettings[TierGraphicSettings.Count];
				for (int i = 0; i < TierGraphicSettings.Count; i++)
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
			if (IsReadEditorSettings(flags))
			{
				return LightmapStripping;
			}
#endif
			return LightmapStrippingMode.Automatic;
		}
		private LightmapStrippingMode GetFogStripping(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags))
			{
				return FogStripping;
			}
#endif
			return LightmapStrippingMode.Automatic;
		}
		private InstancingStrippingVariant GetInstancingStripping(TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags))
			{
				return InstancingStripping;
			}
#endif
			return InstancingStrippingVariant.StripUnused;
		}
		private bool GetLightmapKeepPlain(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadLightmapKeepPlain(version))
			{
				return LightmapKeepPlain;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepDirCombined(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadLightmapKeepPlain(version))
			{
				return LightmapKeepDirCombined;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepDynamicPlain(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadLightmapKeepDynamicPlain(version))
			{
				return LightmapKeepDynamicPlain;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepDynamicDirCombined(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadLightmapKeepDynamicPlain(version))
			{
				return LightmapKeepDynamicDirCombined;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepShadowMask(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadLightmapKeepShadowMask(version))
			{
				return LightmapKeepShadowMask;
			}
#endif
			return true;
		}
		private bool GetLightmapKeepSubtractive(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadLightmapKeepShadowMask(version))
			{
				return LightmapKeepSubtractive;
			}
#endif
			return true;
		}
		private bool GetFogKeepLinear(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadFogKeepLinear(version))
			{
				return FogKeepLinear;
			}
#endif
			return true;
		}
		private bool GetFogKeepExp(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadFogKeepLinear(version))
			{
				return FogKeepExp;
			}
#endif
			return true;
		}
		private bool GetFogKeepExp2(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadFogKeepLinear(version))
			{
				return FogKeepExp2;
			}
#endif
			return true;
		}
		private IReadOnlyList<AlbedoSwatchInfo> GetAlbedoSwatchInfos(Version version, TransferInstructionFlags flags)
		{
#if UNIVERSAL
			if (IsReadEditorSettings(flags) && IsReadAlbedoSwatchInfos(version))
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

		public IReadOnlyList<PPtr<Shader>> AlwaysIncludedShaders => m_alwaysIncludedShaders;
		public IReadOnlyList<PPtr<ShaderVariantCollection>> PreloadedShaders => m_preloadedShaders;
		public TransparencySortMode TransparencySortMode { get; private set; }
#if UNIVERSAL
		public RenderingPath DefaultRenderingPath { get; private set; }
		public RenderingPath DefaultMobileRenderingPath { get; private set; }
		public LightmapStrippingMode LightmapStripping { get; private set; }
		public LightmapStrippingMode FogStripping { get; private set; }
		public InstancingStrippingVariant InstancingStripping { get; private set; }
		public bool LightmapKeepPlain { get; private set; }
		public bool LightmapKeepDirCombined { get; private set; }
		public bool LightmapKeepDirSeparate { get; private set; }
		public bool LightmapKeepDynamicPlain { get; private set; }
		public bool LightmapKeepDynamicDirCombined { get; private set; }
		public bool LightmapKeepDynamicDirSeparate { get; private set; }
		public bool LightmapKeepShadowMask { get; private set; }
		public bool LightmapKeepSubtractive { get; private set; }
		public bool FogKeepLinear { get; private set; }
		public bool FogKeepExp { get; private set; }
		public bool FogKeepExp2 { get; private set; }
		public IReadOnlyList<AlbedoSwatchInfo> AlbedoSwatchInfos => m_albedoSwatchInfos;
#endif
		public IReadOnlyList<PlatformShaderSettings> PlatformSettings => m_platformSettings;
		public IReadOnlyList<TierGraphicsSettings> TierGraphicSettings => m_tierGraphicSettings;
		public IReadOnlyList<TierSettings> TierSettings => m_tierSettings;
		public IReadOnlyList<PlatformShaderDefines> ShaderDefinesPerShaderCompiler => m_shaderDefinesPerShaderCompiler;
		public bool LightsUseLinearIntensity { get; private set; }
		public bool LightsUseColorTemperature { get; private set; }
		public bool LogWhenShaderIsCompiled { get; private set; }

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
		public const string LightsUseColorTemperatureName = "m_LightsUseColorTemperature";
		public const string LogWhenShaderIsCompiledName = "m_LogWhenShaderIsCompiled";


		private PPtr<Shader>[] m_alwaysIncludedShaders;
		private PPtr<ShaderVariantCollection>[] m_preloadedShaders;
		private PlatformShaderSettings[] m_platformSettings;
		private TierGraphicsSettings[] m_tierGraphicSettings;
		private TierSettings[] m_tierSettings;
		private AlbedoSwatchInfo[] m_albedoSwatchInfos;
		private PlatformShaderDefines[] m_shaderDefinesPerShaderCompiler;
	}
}
