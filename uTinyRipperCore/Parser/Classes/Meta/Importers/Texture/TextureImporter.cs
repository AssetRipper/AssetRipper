using System.Collections.Generic;
using uTinyRipper.Classes.Sprites;
using uTinyRipper.Classes.TextureImporters;
using uTinyRipper.Classes.Textures;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class TextureImporter : AssetImporter
	{
		public TextureImporter(AssetLayout layout) :
			base(layout)
		{
			EnableMipMap = 1;
			SRGBTexture = 1;
			AlphaTestReferenceValue = 0.5f;
			MipMapFadeDistanceStart = 1;
			MipMapFadeDistanceEnd = 3;
			HeightScale = 0.25f;
			GenerateCubemap = TextureImporterGenerateCubemap.AutoCubemap;
			TextureFormat = TextureFormat.Alpha8;
			MaxTextureSize = 2048;
			TextureSettings = new GLTextureSettings(layout);
			NPOTScale = TextureImporterNPOTScale.ToNearest;
			CompressionQuality = 50;

			SpriteMode = SpriteImportMode.Single;
			SpriteExtrude = 1;
			SpriteMeshType = SpriteMeshType.Tight;
			SpritePivot = new Vector2f(0.5f, 0.5f);
			SpritePixelsToUnits = 100.0f;
			SpriteGenerateFallbackPhysicsShape = 1;
			AlphaUsage = TextureImporterAlphaSource.FromInput;
			AlphaIsTransparency = 0;
			SpriteTessellationDetail = -1;
			TextureShape = TextureImporterShape.Texture2D;
			PlatformSettings = new TextureImporterPlatformSettings[] { new TextureImporterPlatformSettings(layout) };
			SpriteSheet = new SpriteSheetMetaData(layout);
			SpritePackingTag = string.Empty;
			Output = new TextureImportOutput(layout);
		}

		public TextureImporter(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// ApplyGammaDecoding default value has been changed from 1 to 0
			if (version.IsGreaterEqual(2019, 3, 6))
			{
				return 11;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(2019, 1, 0, VersionType.Final))
			{
				return 10;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(2018, 3, 4))
			{
				// NOTE: unknown conversion
				if (version.IsEqual(2019, 1, 0, VersionType.Beta, 1))
				{
					return 8;
				}
				return 9;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(2018, 2, 1))
			{
				return 7;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(2018, 2))
			{
				return 6;
			}
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(2018))
			{
				return 5;
			}
			// BuildTargetSettings has been renamed to PlatformSettings
			// TextureType enum has been changed
			if (version.IsGreaterEqual(5, 5))
			{
				return 4;
				// alpha/beta?
				// return 3;
			}
			// NOTE: unknown version
			// return 2;

			return 1;
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasSRGBTexture(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasLinearTexture(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool HasCorrectGamma(Version version) => version.IsLess(5, 5);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasMipMapsPreserveCoverage(Version version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasExternalNormalMap(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool HasIsReadable(Version version) => version.IsGreaterEqual(2, 5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasStreamingMipmaps(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasCubemapConvolution(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool HasCubemapConvolutionSteps(Version version) => version.IsLess(5, 5) && HasCubemapConvolution(version);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasSeamlessCubemap(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasLightmap(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 5.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool HasRGBM(Version version) => version.IsLess(5, 5) && version.IsGreaterEqual(5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasCompressionQuality(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.2.0 to 5.3.7 or 5.4.0 to 5.4.4 exclusive
		/// </summary>
		public static bool HasAllowsAlphaSplitting(Version version)
		{
			if (version.IsGreaterEqual(5, 4, 4))
			{
				return false;
			}
			if (version.IsGreaterEqual(5, 4))
			{
				return true;
			}
			if (version.IsGreaterEqual(5, 2) && version.IsLess(5, 3, 8))
			{
				return true;
			}
			return false;
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasSprite(Version version) => version.IsGreaterEqual(4, 3);		
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasSpriteBorder(Version version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 2017.4 to 2018.1 exclusive or 2018.1.0b8 and greater
		/// </summary>
		public static bool HasSpriteGenerateFallbackPhysicsShape(Version version)
		{
			if (version.IsGreaterEqual(2018, 1, 0, VersionType.Beta, 8))
			{
				return true;
			}
			if (version.IsGreaterEqual(2017, 4) && version.IsLess(2018))
			{
				return true;
			}
			return false;
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasAlphaUsage(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasAlphaIsTransparency(Version version) => version.IsGreater(4, 2);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool HasSpriteTessellationDetail(Version version) => version.IsGreaterEqual(5, 4);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasTextureType(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool HasRecommendedTextureFormat(Version version) => version.IsLess(4);
		/// <summary>
		/// 3.0.0 to 4.0.0 exclusive
		/// </summary>
		public static bool HasSourceTextureInformation(Version version) => version.IsLess(4) && version.IsGreaterEqual(3);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasTextureShape(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasSingleChannelComponent(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasMaxTextureSizeSet(Version version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 2019.3.6 and greater
		/// </summary>
		public static bool HasApplyGammaDecoding(Version version) => version.IsGreaterEqual(2019, 3, 6);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasPlatformSettings(Version version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		public static bool HasOutput(Version version) => version.IsGreaterEqual(4);
		/// <summary>
		/// 2018.2.1 and greater
		/// </summary>
		public static bool HasPSDRemoveMatte(Version version) => version.IsGreaterEqual(2018, 2, 1);

		/// <summary>
		/// Less than 3
		/// </summary>
		private static bool IsBoolFlags(Version version) => version.IsLess(3);
		/// <summary>
		/// 2017.4 to 2018.1 exclusive or 2018.1.0b8 and greater
		/// </summary>
		private static bool SpritePixelsToUnitsFirst(Version version) => HasSpriteGenerateFallbackPhysicsShape(version);
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		private static bool IsReadableFirst(Version version) => version.IsLess(3, 5);
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		private static bool IsAlignGrayScaleToAlpha(Version version) => version.IsLess(4);
		/// <summary>
		/// 3.0.0 to 4.0.0 exclusive
		/// </summary>
		private static bool IsAlignTextureFormat(Version version) => version.IsLess(4) && version.IsGreaterEqual(3);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		private static bool RecommendedTextureFormatFirst(Version version) => version.IsLess(3);


		public override bool IncludesImporter(Version version)
		{
			return true;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			bool isBoolFlags = IsBoolFlags(reader.Version);
			MipMapMode = (TextureImporterMipFilter)reader.ReadInt32();
			if (isBoolFlags)
			{
				EnableMipMapBool = reader.ReadBoolean();
				CorrectGammaBool = reader.ReadBoolean();
				FadeOutBool = reader.ReadBoolean();
				BorderMipMapBool = reader.ReadBoolean();
			}
			else
			{
				EnableMipMap = reader.ReadInt32();
				if (HasSRGBTexture(reader.Version))
				{
					SRGBTexture = reader.ReadInt32();
				}
				if (HasLinearTexture(reader.Version))
				{
					LinearTexture = reader.ReadInt32();
				}
				if (HasCorrectGamma(reader.Version))
				{
					CorrectGamma = reader.ReadInt32();
				}
				FadeOut = reader.ReadInt32();
				BorderMipMap = reader.ReadInt32();
			}

			if (HasMipMapsPreserveCoverage(reader.Version))
			{
				MipMapsPreserveCoverage = reader.ReadInt32();
				AlphaTestReferenceValue = reader.ReadSingle();
			}

			MipMapFadeDistanceStart = reader.ReadInt32();
			MipMapFadeDistanceEnd = reader.ReadInt32();
			if (isBoolFlags)
			{
				ConvertToNormalMapBool = reader.ReadBoolean();
				if (HasIsReadable(reader.Version))
				{
					IsReadableBool = reader.ReadBoolean();
				}
			}
			else
			{
				ConvertToNormalMap = reader.ReadInt32();
				ExternalNormalMap = reader.ReadInt32();
				if (IsReadableFirst(reader.Version))
				{
					IsReadable = reader.ReadInt32();
					reader.AlignStream();
				}
			}

			HeightScale = reader.ReadSingle();
			NormalMapFilter = (TextureImporterNormalFilter)reader.ReadInt32();
			if (!IsReadableFirst(reader.Version))
			{
				IsReadable = reader.ReadInt32();
			}
			if (HasStreamingMipmaps(reader.Version))
			{
				StreamingMipmaps = reader.ReadInt32();
				StreamingMipmapsPriority = reader.ReadInt32();
			}
			if (isBoolFlags)
			{
				GrayScaleToAlphaBool = reader.ReadBoolean();
			}
			else
			{
				GrayScaleToAlpha = reader.ReadInt32();
			}
			if (IsAlignGrayScaleToAlpha(reader.Version))
			{
				reader.AlignStream();
			}

			GenerateCubemap = (TextureImporterGenerateCubemap)reader.ReadInt32();
			if (HasCubemapConvolution(reader.Version))
			{
				CubemapConvolution = reader.ReadInt32();
			}
			if (HasCubemapConvolutionSteps(reader.Version))
			{
				CubemapConvolutionSteps = reader.ReadInt32();
				CubemapConvolutionExponent = reader.ReadSingle();
			}
			if (HasSeamlessCubemap(reader.Version))
			{
				SeamlessCubemap = reader.ReadInt32();
			}

			TextureFormat = (TextureFormat)reader.ReadInt32();
			if (IsAlignTextureFormat(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasRecommendedTextureFormat(reader.Version) && RecommendedTextureFormatFirst(reader.Version))
			{
				RecommendedTextureFormat = reader.ReadInt32();
				reader.AlignStream();
			}

			MaxTextureSize = reader.ReadInt32();
			TextureSettings.Read(reader);
			NPOTScale = (TextureImporterNPOTScale)reader.ReadInt32();
			if (HasLightmap(reader.Version))
			{
				Lightmap = reader.ReadInt32();
			}
			if (HasRGBM(reader.Version))
			{
				RGBM = reader.ReadInt32();
			}
			if (HasCompressionQuality(reader.Version))
			{
				CompressionQuality = reader.ReadInt32();
			}
			if (HasAllowsAlphaSplitting(reader.Version))
			{
				AllowsAlphaSplitting = reader.ReadInt32();
				reader.AlignStream();
			}
			if (HasSprite(reader.Version))
			{
				SpriteMode = (SpriteImportMode)reader.ReadInt32();
				SpriteExtrude = reader.ReadUInt32();
				SpriteMeshType = (SpriteMeshType)reader.ReadInt32();
				Alignment = (SpriteAlignment)reader.ReadInt32();
				SpritePivot.Read(reader);
			}
			if (HasSprite(reader.Version) && SpritePixelsToUnitsFirst(reader.Version))
			{
				SpritePixelsToUnits = reader.ReadSingle();
			}
			if (HasSpriteBorder(reader.Version))
			{
				SpriteBorder.Read(reader);
			}
			if (HasSprite(reader.Version) && !SpritePixelsToUnitsFirst(reader.Version))
			{
				SpritePixelsToUnits = reader.ReadSingle();
			}
			if (HasSpriteGenerateFallbackPhysicsShape(reader.Version))
			{
				SpriteGenerateFallbackPhysicsShape = reader.ReadInt32();
			}
			if (HasAlphaUsage(reader.Version))
			{
				AlphaUsage = (TextureImporterAlphaSource)reader.ReadInt32();
			}
			if (HasAlphaIsTransparency(reader.Version))
			{
				AlphaIsTransparency = reader.ReadInt32();
			}
			if (HasSpriteTessellationDetail(reader.Version))
			{
				SpriteTessellationDetail = reader.ReadSingle();
			}
			if (HasTextureType(reader.Version))
			{
				TextureType = (TextureImporterType)reader.ReadInt32();
			}
			if (HasRecommendedTextureFormat(reader.Version) && !RecommendedTextureFormatFirst(reader.Version))
			{
				RecommendedTextureFormat = reader.ReadInt32();
			}
			if (HasSourceTextureInformation(reader.Version))
			{
				SourceTextureInformation = reader.ReadAsset<SourceTextureInformation>();
				reader.AlignStream();
			}
			if (HasTextureShape(reader.Version))
			{
				TextureShape = (TextureImporterShape)reader.ReadInt32();
			}
			if (HasSingleChannelComponent(reader.Version))
			{
				SingleChannelComponent = reader.ReadInt32();
			}
			if (HasMaxTextureSizeSet(reader.Version))
			{
				MaxTextureSizeSet = reader.ReadInt32();
				CompressionQualitySet = reader.ReadInt32();
				TextureFormatSet = reader.ReadInt32();
			}
			if (HasApplyGammaDecoding(reader.Version))
			{
				ApplyGammaDecoding = reader.ReadInt32();
			}
			reader.AlignStream();

			if (HasPlatformSettings(reader.Version))
			{
				PlatformSettings = reader.ReadAssetArray<TextureImporterPlatformSettings>();
			}
			if (HasSprite(reader.Version))
			{
				SpriteSheet.Read(reader);
				SpritePackingTag = reader.ReadString();
			}
			if (HasOutput(reader.Version))
			{
				Output.Read(reader);
			}
			if (HasPSDRemoveMatte(reader.Version))
			{
				PSDRemoveMatte = reader.ReadBoolean();
				PSDShowRemoveMatteOption = reader.ReadBoolean();
			}
			reader.AlignStream();

			PostRead(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			bool isBoolFlags = IsBoolFlags(writer.Version);
			writer.Write((int)MipMapMode);
			if (isBoolFlags)
			{
				writer.Write(EnableMipMapBool);
				writer.Write(CorrectGammaBool);
				writer.Write(FadeOutBool);
				writer.Write(BorderMipMapBool);
			}
			else
			{
				writer.Write(EnableMipMap);
				if (HasSRGBTexture(writer.Version))
				{
					writer.Write(SRGBTexture);
				}
				if (HasLinearTexture(writer.Version))
				{
					writer.Write(LinearTexture);
				}
				if (HasCorrectGamma(writer.Version))
				{
					writer.Write(CorrectGamma);
				}
				writer.Write(FadeOut);
				writer.Write(BorderMipMap);
			}

			if (HasMipMapsPreserveCoverage(writer.Version))
			{
				writer.Write(MipMapsPreserveCoverage);
				writer.Write(AlphaTestReferenceValue);
			}

			writer.Write(MipMapFadeDistanceStart);
			writer.Write(MipMapFadeDistanceEnd);
			if (isBoolFlags)
			{
				writer.Write(ConvertToNormalMapBool);
				if (HasIsReadable(writer.Version))
				{
					writer.Write(IsReadableBool);
				}
			}
			else
			{
				writer.Write(ConvertToNormalMap);
				writer.Write(ExternalNormalMap);
				if (IsReadableFirst(writer.Version))
				{
					writer.Write(IsReadable);
					writer.AlignStream();
				}
			}

			writer.Write(HeightScale);
			writer.Write((int)NormalMapFilter);
			if (!IsReadableFirst(writer.Version))
			{
				writer.Write(IsReadable);
			}
			if (HasStreamingMipmaps(writer.Version))
			{
				writer.Write(StreamingMipmaps);
				writer.Write(StreamingMipmapsPriority);
			}
			if (isBoolFlags)
			{
				writer.Write(GrayScaleToAlphaBool);
			}
			else
			{
				writer.Write(GrayScaleToAlpha);
			}
			if (IsAlignGrayScaleToAlpha(writer.Version))
			{
				writer.AlignStream();
			}

			writer.Write((int)GenerateCubemap);
			if (HasCubemapConvolution(writer.Version))
			{
				writer.Write(CubemapConvolution);
			}
			if (HasCubemapConvolutionSteps(writer.Version))
			{
				writer.Write(CubemapConvolutionSteps);
				writer.Write(CubemapConvolutionExponent);
			}
			if (HasSeamlessCubemap(writer.Version))
			{
				writer.Write(SeamlessCubemap);
			}

			writer.Write((int)TextureFormat);
			if (IsAlignTextureFormat(writer.Version))
			{
				writer.AlignStream();
			}

			if (HasRecommendedTextureFormat(writer.Version) && RecommendedTextureFormatFirst(writer.Version))
			{
				writer.Write(RecommendedTextureFormat);
				writer.AlignStream();
			}

			writer.Write(MaxTextureSize);
			TextureSettings.Write(writer);
			writer.Write((int)NPOTScale);
			if (HasLightmap(writer.Version))
			{
				writer.Write(Lightmap);
			}
			if (HasRGBM(writer.Version))
			{
				writer.Write(RGBM);
			}
			if (HasCompressionQuality(writer.Version))
			{
				writer.Write(CompressionQuality);
			}
			if (HasAllowsAlphaSplitting(writer.Version))
			{
				writer.Write(AllowsAlphaSplitting);
				writer.AlignStream();
			}
			if (HasSprite(writer.Version))
			{
				writer.Write((int)SpriteMode);
				writer.Write(SpriteExtrude);
				writer.Write((int)SpriteMeshType);
				writer.Write((int)Alignment);
				SpritePivot.Write(writer);
			}
			if (HasSprite(writer.Version) && SpritePixelsToUnitsFirst(writer.Version))
			{
				writer.Write(SpritePixelsToUnits);
			}
			if (HasSpriteBorder(writer.Version))
			{
				SpriteBorder.Write(writer);
			}
			if (HasSprite(writer.Version) && !SpritePixelsToUnitsFirst(writer.Version))
			{
				writer.Write(SpritePixelsToUnits);
			}
			if (HasSpriteGenerateFallbackPhysicsShape(writer.Version))
			{
				writer.Write(SpriteGenerateFallbackPhysicsShape);
			}
			if (HasAlphaUsage(writer.Version))
			{
				writer.Write((int)AlphaUsage);
			}
			if (HasAlphaIsTransparency(writer.Version))
			{
				writer.Write(AlphaIsTransparency);
			}
			if (HasSpriteTessellationDetail(writer.Version))
			{
				writer.Write(SpriteTessellationDetail);
			}
			if (HasTextureType(writer.Version))
			{
				writer.Write((int)TextureType);
			}
			if (HasRecommendedTextureFormat(writer.Version) && !RecommendedTextureFormatFirst(writer.Version))
			{
				writer.Write(RecommendedTextureFormat);
			}
			if (HasSourceTextureInformation(writer.Version))
			{
				SourceTextureInformation.Write(writer);
				writer.AlignStream();
			}
			if (HasTextureShape(writer.Version))
			{
				writer.Write((int)TextureShape);
			}
			if (HasSingleChannelComponent(writer.Version))
			{
				writer.Write(SingleChannelComponent);
			}
			if (HasMaxTextureSizeSet(writer.Version))
			{
				writer.Write(MaxTextureSizeSet);
				writer.Write(CompressionQualitySet);
				writer.Write(TextureFormatSet);
			}
			if (HasApplyGammaDecoding(writer.Version))
			{
				writer.Write(ApplyGammaDecoding);
			}
			writer.AlignStream();

			if (HasPlatformSettings(writer.Version))
			{
				PlatformSettings.Write(writer);
			}
			if (HasSprite(writer.Version))
			{
				SpriteSheet.Write(writer);
				writer.Write(SpritePackingTag);
			}
			if (HasOutput(writer.Version))
			{
				Output.Write(writer);
			}
			if (HasPSDRemoveMatte(writer.Version))
			{
				writer.Write(PSDRemoveMatte);
				writer.Write(PSDShowRemoveMatteOption);
			}
			writer.AlignStream();

			PostWrite(writer);
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(SpriteSheet, SpriteSheetName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));

			YAMLMappingNode mipmap = new YAMLMappingNode();
			mipmap.Add(MipMapModeName, (int)MipMapMode);
			mipmap.Add(EnableMipMapName, EnableMipMap);
			if (HasSRGBTexture(container.ExportVersion))
			{
				mipmap.Add(SRGBTextureName, SRGBTexture);
			}
			if (HasLinearTexture(container.ExportVersion))
			{
				mipmap.Add(LinearTextureName, LinearTexture);
			}
			if (HasCorrectGamma(container.ExportVersion))
			{
				mipmap.Add(CorrectGammaName, CorrectGamma);
			}
			mipmap.Add(FadeOutName, FadeOut);
			mipmap.Add(BorderMipMapName, BorderMipMap);
			if (HasMipMapsPreserveCoverage(container.ExportVersion))
			{
				mipmap.Add(MipMapsPreserveCoverageName, MipMapsPreserveCoverage);
				mipmap.Add(AlphaTestReferenceValueName, AlphaTestReferenceValue);
			}
			mipmap.Add(MipMapFadeDistanceStartName, MipMapFadeDistanceStart);
			mipmap.Add(MipMapFadeDistanceEndName, MipMapFadeDistanceEnd);
			node.Add(MipmapsName, mipmap);

			YAMLMappingNode bumpmap = new YAMLMappingNode();
			bumpmap.Add(ConvertToNormalMapName, ConvertToNormalMap);
			if (HasExternalNormalMap(container.ExportVersion))
			{
				bumpmap.Add(ExternalNormalMapName, ExternalNormalMap);
			}
			bumpmap.Add(HeightScaleName, HeightScale);
			bumpmap.Add(NormalMapFilterName, (int)NormalMapFilter);
			node.Add(BumpmapName, bumpmap);

			if (HasIsReadable(container.ExportVersion))
			{
				node.Add(IsReadableName, IsReadable);
			}
			if (HasStreamingMipmaps(container.ExportVersion))
			{
				node.Add(StreamingMipmapsName, StreamingMipmaps);
				node.Add(StreamingMipmapsPriorityName, StreamingMipmapsPriority);
			}

			node.Add(GrayScaleToAlphaName, GrayScaleToAlpha);
			node.Add(GenerateCubemapName, (int)GenerateCubemap);
			if (HasCubemapConvolution(container.ExportVersion))
			{
				node.Add(CubemapConvolutionName, CubemapConvolution);
			}
			if (HasCubemapConvolutionSteps(container.ExportVersion))
			{
				node.Add(CubemapConvolutionStepsName, CubemapConvolutionSteps);
				node.Add(CubemapConvolutionExponentName, CubemapConvolutionExponent);
			}
			if (HasSeamlessCubemap(container.ExportVersion))
			{
				node.Add(SeamlessCubemapName, SeamlessCubemap);
			}

			node.Add(TextureFormatName, (int)TextureFormat);
			if (HasRecommendedTextureFormat(container.ExportVersion) && RecommendedTextureFormatFirst(container.ExportVersion))
			{
				node.Add(RecommendedTextureFormatName, RecommendedTextureFormat);
			}

			node.Add(MaxTextureSizeName, MaxTextureSize);
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(NPOTScaleName, (int)NPOTScale);
			if (HasLightmap(container.ExportVersion))
			{
				node.Add(LightmapName, Lightmap);
			}
			if (HasRGBM(container.ExportVersion))
			{
				node.Add(RGBMName, RGBM);
			}
			if (HasCompressionQuality(container.ExportVersion))
			{
				node.Add(CompressionQualityName, CompressionQuality);
			}
			if (HasAllowsAlphaSplitting(container.ExportVersion))
			{
				node.Add(AllowsAlphaSplittingName, AllowsAlphaSplitting);
			}
			if (HasSprite(container.ExportVersion))
			{
				node.Add(SpriteModeName, (int)SpriteMode);
				node.Add(SpriteExtrudeName, SpriteExtrude);
				node.Add(SpriteMeshTypeName, (int)SpriteMeshType);
				node.Add(AlignmentName, (int)Alignment);
				node.Add(SpritePivotName, SpritePivot.ExportYAML(container));
			}
			if (HasSprite(container.ExportVersion) && SpritePixelsToUnitsFirst(container.ExportVersion))
			{
				node.Add(SpritePixelsToUnitsName, SpritePixelsToUnits);
			}
			if (HasSpriteBorder(container.ExportVersion))
			{
				node.Add(SpriteBorderName, SpriteBorder.ExportYAML(container));
			}
			if (HasSprite(container.ExportVersion) && !SpritePixelsToUnitsFirst(container.ExportVersion))
			{
				node.Add(SpritePixelsToUnitsName, SpritePixelsToUnits);
			}
			if (HasSpriteGenerateFallbackPhysicsShape(container.ExportVersion))
			{
				node.Add(SpriteGenerateFallbackPhysicsShapeName, SpriteGenerateFallbackPhysicsShape);
			}
			if (HasAlphaUsage(container.ExportVersion))
			{
				node.Add(AlphaUsageName, (int)AlphaUsage);
			}
			if (HasAlphaIsTransparency(container.ExportVersion))
			{
				node.Add(AlphaIsTransparencyName, AlphaIsTransparency);
			}
			if (HasSpriteTessellationDetail(container.ExportVersion))
			{
				node.Add(SpriteTessellationDetailName, SpriteTessellationDetail);
			}
			if (HasTextureType(container.ExportVersion))
			{
				node.Add(TextureTypeName, (int)TextureType);
			}
			if (HasRecommendedTextureFormat(container.ExportVersion) && !RecommendedTextureFormatFirst(container.ExportVersion))
			{
				node.Add(RecommendedTextureFormatName, RecommendedTextureFormat);
			}
			if (HasSourceTextureInformation(container.ExportVersion))
			{
				node.Add(SourceTextureInformationName, SourceTextureInformation.ExportYAML(container));
			}
			if (HasTextureShape(container.ExportVersion))
			{
				node.Add(TextureShapeName, (int)TextureShape);
			}
			if (HasSingleChannelComponent(container.ExportVersion))
			{
				node.Add(SingleChannelComponentName, SingleChannelComponent);
			}
			if (HasMaxTextureSizeSet(container.ExportVersion))
			{
				node.Add(MaxTextureSizeSetName, MaxTextureSizeSet);
				node.Add(CompressionQualitySetName, CompressionQualitySet);
				node.Add(TextureFormatSetName, TextureFormatSet);
			}
			if (HasApplyGammaDecoding(container.ExportVersion))
			{
				node.Add(ApplyGammaDecodingName, GetApplyGammaDecoding(container.Version));
			}
			if (HasPlatformSettings(container.ExportVersion))
			{
				node.Add(GetPlatformSettingsName(container.ExportVersion), PlatformSettings.ExportYAML(container));
			}
			if (HasSprite(container.ExportVersion))
			{
				node.Add(SpriteSheetName, SpriteSheet.ExportYAML(container));
				node.Add(SpritePackingTagName, SpritePackingTag);
			}
			/*if (HasOutput(container.ExportVersion))
			{
				node.Add(OutputName, Output.ExportYAML(container));
			}*/
			if (HasPSDRemoveMatte(container.ExportVersion))
			{
				node.Add(PSDRemoveMatteName, PSDRemoveMatte);
				node.Add(PSDShowRemoveMatteOptionName, PSDShowRemoveMatteOption);
			}
			PostExportYAML(container, node);
			return node;
		}

		private int GetApplyGammaDecoding(Version version)
		{
			return ToSerializedVersion(version) < 11 ? 1 : ApplyGammaDecoding;
		}

		private string GetPlatformSettingsName(Version version)
		{
			return ToSerializedVersion(version) >= 4 ? PlatformSettingsName : BuildTargetSettingsName;
		}

		public override ClassIDType ClassID => ClassIDType.TextureImporter;

		public TextureImporterMipFilter MipMapMode { get; set; }
		public int EnableMipMap { get; set; }
		public int SRGBTexture { get; set; }
		public int LinearTexture { get; set; }
		public int CorrectGamma { get; set; }
		public int FadeOut { get; set; }
		public int BorderMipMap { get; set; }
		public int MipMapsPreserveCoverage { get; set; }
		public float AlphaTestReferenceValue { get; set; }
		public int MipMapFadeDistanceStart { get; set; }
		public int MipMapFadeDistanceEnd { get; set; }
		public int ConvertToNormalMap { get; set; }
		public int ExternalNormalMap { get; set; }
		public float HeightScale { get; set; }
		public TextureImporterNormalFilter NormalMapFilter { get; set; }
		public int IsReadable { get; set; }
		public int StreamingMipmaps { get; set; }
		public int StreamingMipmapsPriority { get; set; }
		public int GrayScaleToAlpha { get; set; }
		public TextureImporterGenerateCubemap GenerateCubemap { get; set; }
		public int CubemapConvolution { get; set; }
		public int CubemapConvolutionSteps { get; set; }
		public float CubemapConvolutionExponent { get; set; }
		public int SeamlessCubemap { get; set; }
		public TextureFormat TextureFormat { get; set; }
		public int MaxTextureSize { get; set; }
		public TextureImporterNPOTScale NPOTScale { get; set; }
		public int Lightmap { get; set; }
		public int RGBM { get; set; }
		public int CompressionQuality { get; set; }
		public SpriteImportMode SpriteMode { get; set; }
		public uint SpriteExtrude { get; set; }
		public SpriteMeshType SpriteMeshType { get; set; }
		public SpriteAlignment Alignment { get; set; }
		public float SpritePixelsToUnits { get; set; }
		public int SpriteGenerateFallbackPhysicsShape { get; set; }
		public TextureImporterAlphaSource AlphaUsage { get; set; }
		public int AlphaIsTransparency { get; set; }
		public float SpriteTessellationDetail { get; set; }
		public TextureImporterType TextureType { get; set; }
		public int RecommendedTextureFormat { get; set; }
		public SourceTextureInformation SourceTextureInformation
		{
			get => Output.SourceTextureInformation;
			set => Output.SourceTextureInformation = value;
		}
		public TextureImporterShape TextureShape { get; set; }
		public int SingleChannelComponent { get; set; }
		public int MaxTextureSizeSet { get; set; }
		public int CompressionQualitySet { get; set; }
		public int AllowsAlphaSplitting { get; set; }
		public int TextureFormatSet { get; set; }
		public int ApplyGammaDecoding { get; set; }
		public TextureImporterPlatformSettings[] PlatformSettings { get; set; }
		public string SpritePackingTag { get; set; }
		public bool PSDRemoveMatte { get; set; }
		public bool PSDShowRemoveMatteOption { get; set; }

		protected override bool IncludesIDToName => true;

		private bool EnableMipMapBool
		{
			get => EnableMipMap == 0 ? false : true;
			set => EnableMipMap = value ? 1 : 0;
		}
		private bool CorrectGammaBool
		{
			get => CorrectGamma == 0 ? false : true;
			set => CorrectGamma = value ? 1 : 0;
		}
		private bool FadeOutBool
		{
			get => FadeOut == 0 ? false : true;
			set => FadeOut = value ? 1 : 0;
		}
		private bool BorderMipMapBool
		{
			get => BorderMipMap == 0 ? false : true;
			set => BorderMipMap = value ? 1 : 0;
		}
		private bool ConvertToNormalMapBool
		{
			get => ConvertToNormalMap == 0 ? false : true;
			set => ConvertToNormalMap = value ? 1 : 0;
		}
		private bool IsReadableBool
		{
			get => IsReadable == 0 ? false : true;
			set => IsReadable = value ? 1 : 0;
		}
		private bool GrayScaleToAlphaBool
		{
			get => GrayScaleToAlpha == 0 ? false : true;
			set => GrayScaleToAlpha = value ? 1 : 0;
		}

		public const string MipmapsName = "mipmaps";
		public const string MipMapModeName = "m_MipMapMode";
		public const string EnableMipMapName = "m_EnableMipMap";
		public const string SRGBTextureName = "m_sRGBTexture";
		public const string LinearTextureName = "m_LinearTexture";
		public const string CorrectGammaName = "correctGamma";
		public const string MCorrectGammaName = "m_CorrectGamma";
		public const string FadeOutName = "m_FadeOut";
		public const string BorderMipMapName = "m_BorderMipMap";
		public const string MipMapsPreserveCoverageName = "m_MipMapsPreserveCoverage";
		public const string AlphaTestReferenceValueName = "m_AlphaTestReferenceValue";
		public const string MipMapFadeDistanceStartName = "m_MipMapFadeDistanceStart";
		public const string MipMapFadeDistanceEndName = "m_MipMapFadeDistanceEnd";
		public const string BumpmapName = "bumpmap";
		public const string ConvertToNormalMapName = "m_ConvertToNormalMap";
		public const string ExternalNormalMapName = "m_ExternalNormalMap";
		public const string HeightScaleName = "m_HeightScale";
		public const string NormalMapFilterName = "m_NormalMapFilter";
		public const string IsReadableName = "m_IsReadable";
		public const string StreamingMipmapsName = "m_StreamingMipmaps";
		public const string StreamingMipmapsPriorityName = "m_StreamingMipmapsPriority";
		public const string GrayScaleToAlphaName = "m_GrayScaleToAlpha";
		public const string GenerateCubemapName = "m_GenerateCubemap";
		public const string CubemapConvolutionName = "m_CubemapConvolution";
		public const string CubemapConvolutionStepsName = "m_CubemapConvolutionSteps";
		public const string CubemapConvolutionExponentName = "m_CubemapConvolutionExponent";
		public const string SeamlessCubemapName = "m_SeamlessCubemap";
		public const string TextureFormatName = "m_TextureFormat";
		public const string MaxTextureSizeName = "m_MaxTextureSize";
		public const string TextureSettingsName = "m_TextureSettings";
		public const string NPOTScaleName = "m_NPOTScale";
		public const string LightmapName = "m_Lightmap";
		public const string RGBMName = "m_RGBM";
		public const string CompressionQualityName = "m_CompressionQuality";
		public const string AllowsAlphaSplittingName = "m_AllowsAlphaSplitting";
		public const string SpriteModeName = "m_SpriteMode";
		public const string SpriteExtrudeName = "m_SpriteExtrude";
		public const string SpriteMeshTypeName = "m_SpriteMeshType";
		public const string AlignmentName = "m_Alignment";
		public const string SpritePivotName = "m_SpritePivot";
		public const string SpritePixelsToUnitsName = "m_SpritePixelsToUnits";
		public const string SpriteBorderName = "m_SpriteBorder";
		public const string SpriteGenerateFallbackPhysicsShapeName = "m_SpriteGenerateFallbackPhysicsShape";
		public const string AlphaUsageName = "m_AlphaUsage";
		public const string AlphaIsTransparencyName = "m_AlphaIsTransparency";
		public const string SpriteTessellationDetailName = "m_SpriteTessellationDetail";
		public const string TextureTypeName = "m_TextureType";
		public const string RecommendedTextureFormatName = "m_RecommendedTextureFormat";
		public const string SourceTextureInformationName = "m_SourceTextureInformation";
		public const string TextureShapeName = "m_TextureShape";
		public const string SingleChannelComponentName = "m_SingleChannelComponent";
		public const string MaxTextureSizeSetName = "m_MaxTextureSizeSet";
		public const string CompressionQualitySetName = "m_CompressionQualitySet";
		public const string TextureFormatSetName = "m_TextureFormatSet";
		public const string ApplyGammaDecodingName = "m_ApplyGammaDecoding";
		public const string BuildTargetSettingsName = "m_BuildTargetSettings";
		public const string PlatformSettingsName = "m_PlatformSettings";
		public const string SpriteSheetName = "m_SpriteSheet";
		public const string SpritePackingTagName = "m_SpritePackingTag";
		public const string OutputName = "m_Output";
		public const string PSDRemoveMatteName = "m_PSDRemoveMatte";
		public const string PSDShowRemoveMatteOptionName = "m_PSDShowRemoveMatteOption";

		public GLTextureSettings TextureSettings;
		public Vector2f SpritePivot;
		public Vector4f SpriteBorder;
		public SpriteSheetMetaData SpriteSheet;
		public TextureImportOutput Output;
	}
}
