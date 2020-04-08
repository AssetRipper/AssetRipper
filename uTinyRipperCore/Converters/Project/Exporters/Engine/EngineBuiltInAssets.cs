using System;
using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes;

namespace uTinyRipper.Converters
{
	public struct EngineBuiltInAsset
	{
		public EngineBuiltInAsset(uint exportID, uint parameter, bool isF)
		{
			ExportID = exportID;
			Parameter = parameter;
			m_isF = isF;
		}

		public MetaPtr ToExportPointer()
		{
			return new MetaPtr(ExportID, GUID, AssetType.Internal);
		}

		public UnityGUID GUID => m_isF ? EngineBuiltInAssets.FGUID : EngineBuiltInAssets.EGUID;

		public bool IsValid => ExportID != 0;
		public uint ExportID { get; }
		public uint Parameter { get; }

		/// <summary>
		///  Is assets located in DefaultResources file
		/// </summary>
		private readonly bool m_isF;
	}

	internal struct EngineBuiltInAssetInfo
	{
		public EngineBuiltInAssetInfo(Version version, EngineBuiltInAsset asset)
		{
			KeyValuePair<Version, EngineBuiltInAsset> kvp = new KeyValuePair<Version, EngineBuiltInAsset>(version, asset);
			m_variations = new List<KeyValuePair<Version, EngineBuiltInAsset>>(1);
			m_variations.Add(kvp);
		}

		public void AddVariation(Version version, EngineBuiltInAsset asset)
		{
			KeyValuePair<Version, EngineBuiltInAsset> kvp = new KeyValuePair<Version, EngineBuiltInAsset>(version, asset);
			for (int i = 0; i < m_variations.Count; i++)
			{
				Version key = m_variations[i].Key;
				if (key < version)
				{
					m_variations.Insert(i, kvp);
					return;
				}
			}
			m_variations.Add(kvp);
		}

		public bool ContainsAsset(Version version)
		{
			foreach (KeyValuePair<Version, EngineBuiltInAsset> kvp in m_variations)
			{
				if (version >= kvp.Key)
				{
					return true;
				}
			}
			return false;
		}

		public EngineBuiltInAsset GetAsset(Version version)
		{
			foreach (KeyValuePair<Version, EngineBuiltInAsset> kvp in m_variations)
			{
				if (version >= kvp.Key)
				{
					return kvp.Value;
				}
			}
			throw new Exception($"There is no asset for {version} version");
		}

		public bool TryGetAsset(Version version, out EngineBuiltInAsset asset)
		{
			foreach (KeyValuePair<Version, EngineBuiltInAsset> kvp in m_variations)
			{
				if (version >= kvp.Key)
				{
					asset = kvp.Value;
					return true;
				}
			}
			asset = default;
			return false;
		}

		private readonly List<KeyValuePair<Version, EngineBuiltInAsset>> m_variations;
	}

	public static class EngineBuiltInAssets
	{
		static EngineBuiltInAssets()
		{
			///////////////////////////////////////////////////////
			// New extra
			///////////////////////////////////////////////////////

			AddShader("Autodesk Interactive", 47, true);
			AddShader("Legacy Shaders/Particles/Additive", 200, true);
			AddShader("Legacy Shaders/Particles/~Additive-Multiply", 201, true);
			AddShader("Legacy Shaders/Particles/Additive (Soft)", 202, true);
			AddShader("Legacy Shaders/Particles/Alpha Blended", 203, true);
			AddShader("Legacy Shaders/Particles/Multiply", 205, true);
			AddShader("Legacy Shaders/Particles/Multiply (Double)", 206, true);
			AddShader("Legacy Shaders/Particles/Alpha Blended Premultiply", 207, true);
			AddShader("Legacy Shaders/Particles/VertexLit Blended", 208, true);
			AddShader("Legacy Shaders/Particles/Anim Alpha Blended", 209, true);
			AddShader("Mobile/Bumped Specular (1 Directional Realtime Light)", 10706, true);
			AddShader("Hidden/VideoComposite", new Version(2019), 16000, true);
			AddShader("Hidden/VideoDecode", new Version(2019), 16001, true);
			AddShader("Hidden/VideoDecodeOSX", new Version(2019), 16002, true);
			AddShader("Hidden/VideoDecodeAndroid", new Version(2019), 16003, true);
			AddShader("Hidden/VideoDecodeML", new Version(2019), 16004, true);

			///////////////////////////////////////////////////////
			// Current default
			///////////////////////////////////////////////////////

			AddMaterial(FontMaterialName, 10100, false);
			AddMaterial("FrameDebuggerRenderTargetDisplay", 10756, true);

			AddTexture("Soft", 10001, false);
			AddTexture("Font Texture", 10103, false);
			AddTexture("UnityWatermark-small", 10400, false);
			AddTexture("EscToExit_back", 10401, false);
			AddTexture("EscToExit_Text", 10402, false);
			AddTexture("UnitySplash-cube", 10403, false);
			AddTexture("UnityWatermark-trial-big", 10406, false);
			AddTexture("UnityWatermark-trial", 10407, false);
			AddTexture("UnityWatermark-beta", 10408, false);
			AddTexture("UnityWatermark-edu", 10409, false);
			AddTexture("UnityWatermark-dev", 10410, false);
			AddTexture("WarningSign", 10411, false);
			AddTexture("UnityWatermark-proto", 10413, false);
			AddTexture("UnityWatermarkPlugin-beta", 10414, false);
			AddTexture("box", 11001, false);
			AddTexture("button active", 11002, false);
			AddTexture("button hover", 11003, false);
			AddTexture("button on hover", 11004, false);
			AddTexture("button on", 11005, false);
			AddTexture("button", 11006, false);
			AddTexture("horizontal scrollbar thumb", 11007, false);
			AddTexture("horizontal scrollbar", 11008, false);
			AddTexture("horizontalslider", 11009, false);
			AddTexture("slider thumb active", 11010, false);
			AddTexture("slider thumb", 11011, false);
			AddTexture("slidert humb hover", 11012, false);
			AddTexture("toggle active", 11013, false);
			AddTexture("toggle hover", 11014, false);
			AddTexture("toggle on hover", 11015, false);
			AddTexture("toggle on", 11016, false);
			AddTexture("toggle on active", 11017, false);
			AddTexture("toggle", 11018, false);
			AddTexture("vertical scrollbar thumb", 11019, false);
			AddTexture("vertical scrollbar", 11020, false);
			AddTexture("verticalslider", 11021, false);
			AddTexture("window on", 11022, false);
			AddTexture("window", 11023, false);
			AddTexture("textfield", 11024, false);
			AddTexture("textfield on", 11025, false);
			AddTexture("textfield hover", 11026, false);
			AddTexture("UnitySplash-HolographicTrackingLoss", 15000, false);

			AddMesh("pSphere1", 10200, false);
			AddMesh("Cube", 10202, false);
			AddMesh("pCylinder1", 10203, false);
			AddMesh("pPlane1", 10204, false);
			AddMesh("polySurface2", 10205, false);
			AddMesh("Cylinder", 10206, false);
			AddMesh("Sphere", 10207, false);
			AddMesh("Capsule", 10208, false);
			AddMesh("Plane", 10209, false);
			AddMesh("Quad", 10210, false);
			AddMesh("Icosphere", 10211, false);
			AddMesh("icosahedron", 10212, false);
			AddMesh("pyramid", 10213, false);

			AddFont("Arial", 10102, false);

			AddShader("Hidden/InternalErrorShader", 17, true);
			AddShader("Hidden/InternalClear", 68, false);
			AddShader("Hidden/Internal-Colored", 69, false);
			AddShader("GUI/Text Shader", 10101, false);
			AddShader("Hidden/FrameDebuggerRenderTargetDisplay", 10755, false);

			AddSprite("UnitySplash-cube", 10404, false);

			///////////////////////////////////////////////////////
			// Current extra
			///////////////////////////////////////////////////////

			AddMaterial("Default-Particle", 10301, true);
			AddMaterial("Default-Diffuse", 10302, true);
			AddMaterial("Default-Material", 10303, true);
			AddMaterial("Default-Skybox", 10304, true);
			AddMaterial("Default-Line", 10306, true);
			AddMaterial("Default-ParticleSystem", 10308, true);
			AddMaterial(DefaultTerrainDiffuseName, 10650, true);
			AddMaterial(DefaultTerrainSpecularName, 10651, true);
			AddMaterial(DefaultTerrainStandardName, 10652, true);
			AddMaterial("Sprites-Default", 10754, false);
			AddMaterial("Sprites-Mask", 10758, false);
			AddMaterial("SpatialMappingOcclusion", 15302, true);
			AddMaterial("SpatialMappingWireframe", 15303, true);

			AddTexture("Default-Particle", 10300, true);
			AddTexture("Default-Checker", 10305, true);
			AddTexture("Default-ParticleSystem", 10307, true);
			AddTexture("Default-Checker-Gray", 10309, true);
			AddTexture("Checkmark", 10900, true);
			AddTexture("UISprite", Version.MinVersion, 10904, 5460, true);
			AddTexture("Background", 10906, true);
			AddTexture("InputFieldBackground", 10910, true);
			AddTexture("Knob", 10912, true);
			AddTexture("DropdownArrow", 10914, true);
			AddTexture("UIMask", 10916, true);

			AddShader("Legacy Shaders/Diffuse Fast", 1, true);
			AddShader("Legacy Shaders/Bumped Diffuse", 2, true);
			AddShader("Legacy Shaders/Specular", 3, true);
			AddShader("Legacy Shaders/Bumped Specular", 4, true);
			AddShader("Legacy Shaders/Diffuse Detail", 5, true);
			AddShader("Legacy Shaders/VertexLit", 6, true);
			AddShader(LegacyDiffuse, 7, true);
			AddShader("Legacy Shaders/Parallax Diffuse", 8, true);
			AddShader("Legacy Shaders/Parallax Specular", 9, true);
			AddShader("Legacy Shaders/Self-Illumin/Diffuse", 10, true);
			AddShader("Legacy Shaders/Self-Illumin/Bumped Diffuse", 11, true);
			AddShader("Legacy Shaders/Self-Illumin/Specular", 12, true);
			AddShader("Legacy Shaders/Self-Illumin/Bumped Specular", 13, true);
			AddShader("Legacy Shaders/Self-Illumin/VertexLit", 14, true);
			AddShader("Legacy Shaders/Self-Illumin/Parallax Diffuse", 15, true);
			AddShader("Legacy Shaders/Self-Illumin/Parallax Specular", 16, true);
			AddShader("Hidden/Internal-StencilWrite", 19, true);
			AddShader("Legacy Shaders/Reflective/Diffuse", 20, true);
			AddShader("Legacy Shaders/Reflective/Bumped Diffuse", 21, true);
			AddShader("Legacy Shaders/Reflective/Specular", 22, true);
			AddShader("Legacy Shaders/Reflective/Bumped Specular", 23, true);
			AddShader("Legacy Shaders/Reflective/VertexLit", 24, true);
			AddShader("Legacy Shaders/Reflective/Bumped Unlit", 25, true);
			AddShader("Legacy Shaders/Reflective/Bumped VertexLit", 26, true);
			AddShader("Legacy Shaders/Reflective/Parallax Diffuse", 27, true);
			AddShader("Legacy Shaders/Reflective/Parallax Specular", 28, true);
			AddShader("Legacy Shaders/Transparent/Diffuse", 30, true);
			AddShader("Legacy Shaders/Transparent/Bumped Diffuse", 31, true);
			AddShader("Legacy Shaders/Transparent/Specular", 32, true);
			AddShader("Legacy Shaders/Transparent/Bumped Specular", 33, true);
			AddShader("Legacy Shaders/Transparent/VertexLit", 34, true);
			AddShader("Legacy Shaders/Transparent/Parallax Diffuse", 35, true);
			AddShader("Legacy Shaders/Transparent/Parallax Specular", 36, true);
			AddShader("Legacy Shaders/Lightmapped/VertexLit", 40, true);
			AddShader("Legacy Shaders/Lightmapped/Diffuse", 41, true);
			AddShader("Legacy Shaders/Lightmapped/Bumped Diffuse", 42, true);
			AddShader("Legacy Shaders/Lightmapped/Specular", 43, true);
			AddShader("Legacy Shaders/Lightmapped/Bumped Specular", 44, true);
			AddShader("Standard (Specular setup)", 45, true);
			AddShader("Standard", 46, true);
			AddShader("Standard (Roughness setup)", 47, true);
			AddShader("Legacy Shaders/Transparent/Cutout/VertexLit", 50, true);
			AddShader("Legacy Shaders/Transparent/Cutout/Diffuse", 51, true);
			AddShader("Legacy Shaders/Transparent/Cutout/Bumped Diffuse", 52, true);
			AddShader("Legacy Shaders/Transparent/Cutout/Specular", 53, true);
			AddShader("Legacy Shaders/Transparent/Cutout/Bumped Specular", 54, true);
			AddShader("Hidden/Internal-DepthNormalsTexture", 62, true);
			AddShader("Hidden/Internal-PrePassLighting", 63, true);
			AddShader("Hidden/Internal-ScreenSpaceShadows", 64, true);
			AddShader("Hidden/Internal-CombineDepthNormals", 65, true);
			AddShader("Hidden/BlitCopy", 66, true);
			AddShader("Hidden/BlitCopyDepth", 67, true);
			AddShader("Hidden/ConvertTexture", 68, true);
			AddShader("Hidden/Internal-DeferredShading", 69, true);
			AddShader("Hidden/Internal-DeferredReflections", 74, true);
			AddShader("Hidden/Internal-MotionVectors", 75, true);
			AddShader("Legacy Shaders/Decal", 100, true);
			AddShader("FX/Flare", 101, true);
			AddShader("Hidden/Internal-Flare", 102, true);
			AddShader("Skybox/Cubemap", 103, true);
			AddShader("Skybox/6 Sided", 104, true);
			AddShader("Hidden/Internal-Halo", 105, true);
			AddShader("Skybox/Procedural", 106, true);
			AddShader("Hidden/BlitCopyWithDepth", 107, true);
			AddShader("Skybox/Panoramic", 108, true);
			AddShader("Hidden/BlitToDepth", 109, true);
			AddShader("Hidden/BlitToDepth_MSAA", 110, true);
			AddShader("Particles/Additive", 200, true);
			AddShader("Particles/~Additive-Multiply", 201, true);
			AddShader("Particles/Additive (Soft)", 202, true);
			AddShader("Particles/Alpha Blended", 203, true);
			AddShader("Particles/Multiply", 205, true);
			AddShader("Particles/Multiply (Double)", 206, true);
			AddShader("Particles/Alpha Blended Premultiply", 207, true);
			AddShader("Particles/VertexLit Blended", 208, true);
			AddShader("Particles/Anim Alpha Blended", 209, true);
			AddShader("Particles/Standard Surface", 210, true);
			AddShader("Particles/Standard Unlit", 211, true);
			AddShader("Hidden/Internal-GUITextureClip", 9000, true);
			AddShader("Hidden/Internal-GUITextureClipText", 9001, true);
			AddShader("Hidden/Internal-GUITexture", 9002, true);
			AddShader("Hidden/Internal-GUITextureBlit", 9003, true);
			AddShader("Hidden/Internal-GUIRoundedRect", 9004, true);
			AddShader("Hidden/Internal-UIRDefault", 9005, true);
			AddShader("Hidden/Internal-UIRAtlasBlitCopy", 9006, true);
			AddShader("Hidden/Internal-GUIRoundedRectWithColorPerBorder", 9007, true);
			AddShader("Hidden/Nature/Terrain/Utilities", 10490, true);
			AddShader("Hidden/TerrainEngine/Details/Vertexlit", 10500, true);
			AddShader("Hidden/TerrainEngine/Details/WavingDoublePass", 10501, true);
			AddShader(TerrainBillboardWavingDoublePass, 10502, true);
			AddShader("Hidden/TerrainEngine/Splatmap/Diffuse-AddPass", 10503, true);
			AddShader("Hidden/TerrainEngine/Splatmap/Diffuse-Base", 10504, true);
			AddShader("Nature/Terrain/Diffuse", 10505, true);
			AddShader("Hidden/TerrainEngine/Splatmap/Diffuse-BaseGen", 10506, true);
			AddShader("Hidden/TerrainEngine/BillboardTree", 10507, true);
			AddShader("Hidden/Nature/Tree Soft Occlusion Bark Rendertex", 10508, true);
			AddShader("Nature/Tree Soft Occlusion Bark", 10509, true);
			AddShader("Hidden/Nature/Tree Soft Occlusion Leaves Rendertex", 10510, true);
			AddShader("Nature/Tree Soft Occlusion Leaves", 10511, true);
			AddShader("Legacy Shaders/Transparent/Cutout/Soft Edge Unlit", 10512, true);
			AddShader("Hidden/TerrainEngine/CameraFacingBillboardTree", 10513, true);
			AddShader("Nature/Tree Creator Bark", 10600, true);
			AddShader("Nature/Tree Creator Leaves", 10601, true);
			AddShader("Hidden/Nature/Tree Creator Bark Rendertex", 10602, true);
			AddShader("Hidden/Nature/Tree Creator Leaves Rendertex", 10603, true);
			AddShader("Hidden/Nature/Tree Creator Bark Optimized", 10604, true);
			AddShader("Hidden/Nature/Tree Creator Leaves Optimized", 10605, true);
			AddShader("Nature/Tree Creator Leaves Fast", 10606, true);
			AddShader("Hidden/Nature/Tree Creator Leaves Fast Optimized", 10607, true);
			AddShader("Hidden/Nature/Tree Creator Albedo Rendertex", 10608, true);
			AddShader("Hidden/Nature/Tree Creator Normal Rendertex", 10609, true);
			AddShader("Nature/Terrain/Specular", 10620, true);
			AddShader("Hidden/TerrainEngine/Splatmap/Specular-AddPass", 10621, true);
			AddShader("Hidden/TerrainEngine/Splatmap/Specular-Base", 10622, true);
			AddShader("Nature/Terrain/Standard", 10623, true);
			AddShader("Hidden/TerrainEngine/Splatmap/Standard-AddPass", 10624, true);
			AddShader("Hidden/TerrainEngine/Splatmap/Standard-Base", 10625, true);
			AddShader("Hidden/TerrainEngine/Splatmap/Standard-BaseGen", 10626, true);
			AddShader("Mobile/Skybox", 10700, true);
			AddShader("Mobile/VertexLit", 10701, true);
			AddShader("Mobile/Diffuse", 10703, true);
			AddShader("Mobile/Bumped Diffuse", 10704, true);
			AddShader("Mobile/Bumped Specular", 10705, true);
			AddShader("Mobile/Bumped Specular (1 Directional Light)", 10706, true);
			AddShader("Mobile/VertexLit (Only Directional Lights)", 10707, true);
			AddShader("Mobile/Unlit (Supports Lightmap)", 10708, true);
			AddShader("Mobile/Particles/Additive", 10720, true);
			AddShader("Mobile/Particles/Alpha Blended", 10721, true);
			AddShader("Mobile/Particles/VertexLit Blended", 10722, true);
			AddShader("Mobile/Particles/Multiply", 10723, true);
			AddShader("Unlit/Transparent", 10750, true);
			AddShader("Unlit/Transparent Cutout", 10751, true);
			AddShader("Unlit/Texture", 10752, true);
			AddShader(SpriteDefault, 10753, true);
			AddShader("Unlit/Color", 10755, true);
			AddShader("Sprites/Mask", 10757, true);
			AddShader("UI/Unlit/Transparent", 10760, true);
			AddShader("UI/Unlit/Detail", 10761, true);
			AddShader("UI/Unlit/Text", 10762, true);
			AddShader("UI/Unlit/Text Detail", 10763, true);
			AddShader("UI/Lit/Transparent", 10764, true);
			AddShader("UI/Lit/Bumped", 10765, true);
			AddShader("UI/Lit/Detail", 10766, true);
			AddShader("UI/Lit/Refraction", 10767, true);
			AddShader("UI/Lit/Refraction Detail", 10768, true);
			AddShader(UIDefault, 10770, true);
			AddShader("UI/Default Font", 10782, true);
			AddShader("UI/DefaultETC1", 10783, true);
			AddShader("Hidden/UI/CompositeOverdraw", 10784, true);
			AddShader("Hidden/UI/Overdraw", 10785, true);
			AddShader("Sprites/Diffuse", 10800, true);
			AddShader("Nature/SpeedTree", 14000, true);
			AddShader("Nature/SpeedTree Billboard", 14001, true);
			AddShader("Nature/SpeedTree8", 14002, true);
			AddShader("Hidden/GIDebug/TextureUV", 15100, true);
			AddShader("Hidden/GIDebug/ShowLightMask", 15101, true);
			AddShader("Hidden/GIDebug/UV1sAsPositions", 15102, true);
			AddShader("Hidden/GIDebug/VertexColors", 15103, true);
			AddShader(CubeBlur, 15104, true);
			AddShader(CubeCopy, 15105, true);
			AddShader(CubeBlend, 15106, true);
			AddShader("VR/SpatialMapping/Occlusion", 15300, true);
			AddShader("VR/SpatialMapping/Wireframe", 15301, true);
			AddShader("Hidden/VR/BlitCopyFromTexArray", 15304, true);
			AddShader("Hidden/VR/BlitTexArraySlice", 15304, true);
			AddShader("Hidden/VR/Internal-VRDistortion", 15305, true);
			AddShader("Hidden/VR/BlitTexArraySliceToDepth", 15306, true);
			AddShader("Hidden/VR/BlitTexArraySliceToDepth_MSAA", 15307, true);
			AddShader("Hidden/Internal-ODSWorldTexture", 15308, true);
			AddShader("Hidden/Internal-CubemapToEquirect", 15309, true);
			AddShader("Hidden/VR/ClippingMask", 15310, true);
			AddShader("Hidden/VR/VideoBackground", 15311, true);
			AddShader("Hidden/VR/BlitFromTex2DToTexArraySlice", 15312, true);
			AddShader("AR/TangoARRender", 15401, true);
			AddShader("Hidden/VideoDecode", 16000, true);
			AddShader("Hidden/VideoDecodeOSX", 16001, true);
			AddShader("Hidden/VideoDecodeAndroid", 16002, true);
			AddShader("Hidden/Compositing", 17000, true);
			AddShader("Hidden/TerrainEngine/PaintHeight", 18000, true);
			AddShader("Hidden/TerrainEngine/HeightBlitCopy", 18001, true);
			AddShader("Hidden/TerrainEngine/GenerateNormalmap", 18002, true);
			AddShader("Hidden/TerrainEngine/TerrainLayerUtils", 18003, true);
			AddShader("Hidden/TerrainEngine/BrushPreview", 18004, true);
			AddShader("Hidden/TerrainEngine/CrossBlendNeighbors", 18005, true);
			AddShader("Hidden/TextCore/Distance Field", 19010, true);
			AddShader("Hidden/TextCore/Distance Field SSD", 19011, true);

			AddSprite("Checkmark", 10901, true);
			AddSprite("UISprite", 10905, true);
			AddSprite("Background", 10907, true);
			AddSprite("InputFieldBackground", 10911, true);
			AddSprite("Knob", 10913, true);
			AddSprite("DropdownArrow", 10915, true);
			AddSprite("UIMask", 10917, true);

			AddLightmapParams("Default-HighResolution", 15200, true);
			AddLightmapParams("Default-LowResolution", 15201, true);
			AddLightmapParams("Default-VeryLowResolution", 15203, true);
			AddLightmapParams("Default-Medium", 15204, true);

			AddBehaviour("GameSkin", 11000, false);

			///////////////////////////////////////////////////////
			// Old default
			///////////////////////////////////////////////////////

			AddTexture("UnitySplash", 10403, false);
			AddTexture("UnitySplash2", 10404, false);
			AddTexture("UnitySplash-text", 10404, false);
			AddTexture("UnitySplash3", 10405, false);
			AddTexture("UnitySplash-free", 10405, false);
			AddTexture("UnitySplashBack", 10406, false);
			AddTexture("UnityWatermark-DebugFlashPlayer", 10412, false);
			AddTexture("UnitySplashVRLogo", 10415, false);
			AddTexture("UnitySplashVRPoweredBy", 10416, false);

			AddShader("Internal-ErrorShader", 17, true);
			AddShader("Shadow-ScreenBlur", 60, false);
			AddShader("Camera-DepthTexture", 61, false);
			AddShader("Camera-DepthNormalTexture", 62, false);
			AddShader("Internal-PrePassLighting", 63, false);
			AddShader("Internal-PrePassCollectShadows", 64, false);
			AddShader("Internal-CombineDepthNormals", 65, false);
			AddShader("Internal-BlitCopy", 66, false);
			AddShader("Shadow-ScreenBlurRotated", 67, false);
			AddShader("Internal-Clear", 68, false);
			AddShader("Internal-Colored", 69, false);
			AddShader("Internal-SplashShadowCaster", 70, false);
			AddShader("Internal-SplashShadowBlur", 71, false);
			AddShader("Internal-SplashShadowReceiver", 72, false);
			AddShader("Internal-SplashShadowReceiverSimple", 73, false);
			AddShader("Internal-Flare", 102, true);
			AddShader("Internal-Halo", 105, true);
			AddShader("Internal-GUITextureClip", 9000, true);
			AddShader("Internal-GUITextureClipText", 9001, true);
			AddShader("Internal-GUITexture", 9002, true);
			AddShader("Internal-GUITextureBlit", 9003, true);
			AddShader("Font", 10101, false);
			AddShader("Sprites-Default", 10753, true);

			///////////////////////////////////////////////////////
			// Old Extra
			///////////////////////////////////////////////////////

			AddShader("Normal-DiffuseFast", 1, true);
			AddShader("Normal-Bumped", 2, true);
			AddShader("Normal-Glossy", 3, true);
			AddShader("Normal-BumpSpec", 4, true);
			AddShader("Normal-DiffuseDetail", 5, true);
			AddShader("Normal-VertexLit", 6, true);
			AddShader("Normal-Diffuse", 7, true);
			AddShader("Normal-Parallax", 8, true);
			AddShader("Normal-ParallaxSpec", 9, true);
			AddShader("Illumin-Diffuse", 10, true);
			AddShader("Illumin-Bumped", 11, true);
			AddShader("Illumin-Glossy", 12, true);
			AddShader("Illumin-BumpSpec", 13, true);
			AddShader("Illumin-VertexLit", 14, true);
			AddShader("Illumin-Parallax", 15, true);
			AddShader("Illumin-ParallaxSpec", 16, true);
			AddShader("Reflect-Diffuse", 20, true);
			AddShader("Reflect-Bumped", 21, true);
			AddShader("Reflect-Glossy", 22, true);
			AddShader("Reflect-BumpSpec", 23, true);
			AddShader("Reflect-VertexLit", 24, true);
			AddShader("Reflect-BumpNolight", 25, true);
			AddShader("Reflect-BumpVertexLit", 26, true);
			AddShader("Reflect-Parallax", 27, true);
			AddShader("Reflect-ParallaxSpec", 28, true);
			AddShader("Alpha-Diffuse", 30, true);
			AddShader("Alpha-Bumped", 31, true);
			AddShader("Alpha-Glossy", 32, true);
			AddShader("Alpha-BumpSpec", 33, true);
			AddShader("Alpha-VertexLit", 34, true);
			AddShader("Alpha-Parallax", 35, true);
			AddShader("Alpha-ParallaxSpec", 36, true);
			AddShader("Lightmap-VertexLit", 40, true);
			AddShader("Lightmap-Diffuse", 41, true);
			AddShader("Lightmap-Bumped", 42, true);
			AddShader("Lightmap-Glossy", 43, true);
			AddShader("Lightmap-BumpSpec", 44, true);
			AddShader("StandardSpecular", 45, true);
			AddShader("Shader", new Version(5), 46, true);
			AddShader("AlphaTest-VertexLit", 50, true);
			AddShader("AlphaTest-Diffuse", 51, true);
			AddShader("AlphaTest-Bumped", 52, true);
			AddShader("AlphaTest-Glossy", 53, true);
			AddShader("AlphaTest-BumpSpec", 54, true);
			AddShader("Internal-DeferredShading", 69, false);
			AddShader("Internal-DeferredReflections", 74, false);
			AddShader("Shader", Version.MinVersion, 100, true);
			AddShader("Decal", 100, false);
			AddShader("Flare", 101, true);
			AddShader("skybox cubed", 103, true);
			AddShader("Skybox-Cubed", 103, false);
			AddShader("Skybox", 104, true);
			AddShader("Skybox-Procedural", 106, false);
			AddShader("Particle Add", 200, true);
			AddShader("Particle AddMultiply", 201, true);
			AddShader("Particle AddSmooth", 202, true);
			AddShader("Particle Alpha Blend", 203, true);
			AddShader("Particle Multiply", 205, true);
			AddShader("Particle MultiplyDouble", 206, true);
			AddShader("Particle Premultiply Blend", 207, true);
			AddShader("Particle VertexLit Blended", 208, true);
			AddShader("VertexLit", 10500, true);
			AddShader("WavingGrass", 10501, true);
			AddShader("WavingGrassBillboard", 10502, true);
			AddShader("AddPass", 10503, true);
			AddShader("FirstPass", 10505, true);
			AddShader("BillboardTree", 10507, true);
			AddShader("TreeSoftOcclusionBarkRendertex", 10508, true);
			AddShader("TreeSoftOcclusionBark", 10509, true);
			AddShader("TreeSoftOcclusionLeavesRendertex", 10510, true);
			AddShader("TreeSoftOcclusionLeaves", 10511, true);
			AddShader("AlphaTest-SoftEdgeUnlit", 10512, true);
			AddShader("TreeCreatorBark", 10600, true);
			AddShader("TreeCreatorLeaves", 10601, true);
			AddShader("TreeCreatorBarkRendertex", 10602, true);
			AddShader("TreeCreatorLeavesRendertex", 10603, true);
			AddShader("TreeCreatorBarkOptimized", 10604, true);
			AddShader("TreeCreatorLeavesOptimized", 10605, true);
			AddShader("TreeCreatorLeavesFast", 10606, true);
			AddShader("TreeCreatorLeavesFastOptimized", 10607, true);
			AddShader("TerrBumpFirstPass", 10620, true);
			AddShader("Specular-FirstPass", 10620, false);
			AddShader("TerrBumpAddPass", 10621, true);
			AddShader("Specular-AddPass", 10621, false);
			AddShader("Specular-Base", 10622, false);
			AddShader("Standard-FirstPass", 10623, false);
			AddShader("Standard-AddPass", 10624, false);
			AddShader("Standard-Base", 10625, false);
			AddShader("Mobile-Skybox", 10700, true);
			AddShader("Mobile-VertexLit", 10701, true);
			AddShader("Mobile-Diffuse", 10703, true);
			AddShader("Mobile-Bumped", 10704, true);
			AddShader("Mobile-BumpSpec", 10705, true);
			AddShader("Mobile-BumpSpec-1DirectionalLight", 10706, true);
			AddShader("Mobile-VertexLit-OnlyDirectionalLights", 10707, true);
			AddShader("Mobile-Lightmap-Unlit", 10708, true);
			AddShader("Mobile-Particle-Add", 10720, true);
			AddShader("Mobile-Particle-Alpha", 10721, true);
			AddShader("Mobile-Particle-Alpha-VertexLit", 10722, true);
			AddShader("Mobile-Particle-Multiply", 10723, true);
			AddShader("Unlit-Alpha", 10750, true);
			AddShader("Unlit-AlphaTest", 10751, true);
			AddShader("Unlit-Normal", 10752, true);
			AddShader("Unlit-Color", 10755, false);
			AddShader("UI-Lit-Refraction", 10767, false);
			AddShader("UI-Lit-RefractionDetail", 10768, false);
			AddShader("UI-Unlit-Transparent", 10760, true);
			AddShader("UI-Unlit-Detail", 10761, true);
			AddShader("UI-Unlit-Text", 10762, true);
			AddShader("UI-Unlit-TextDetail", 10763, true);
			AddShader("UI-Lit-Transparent", 10764, true);
			AddShader("UI-Lit-Bumped", 10765, true);
			AddShader("UI-Lit-Detail", 10766, true);
			AddShader("UI-Lit-Refraction(ProOnly)", 10767, true);
			AddShader("UI-Lit-RefractionDetail(ProOnly)", 10768, true);
			AddShader("UI-Default", 10770, true);
			AddShader("UI-DefaultFont", 10782, true);
			AddShader("Sprites-Diffuse", 10800, true);
			AddShader("SpeedTree", 14000, false);
			AddShader("SpeedTreeBillboard", 14001, false);
			AddShader("TextureUV", 15100, false);
			AddShader("UV1sAsPositions", 15102, false);
			AddShader("VertexColors", 15103, false);
			AddShader("CubeBlur", 15104, false);
			AddShader("CubeCopy", 15105, false);
			AddShader("CubeBlend", 15106, false);
		}

		public static bool ContainsMaterial(string name, Version version)
		{
			return ContainsAsset(m_materials, name, version);
		}
		public static bool ContainsTexture(string name, Version version)
		{
			return ContainsAsset(m_textures, name, version);
		}
		public static bool ContainsMesh(string name, Version version)
		{
			return ContainsAsset(m_meshes, name, version);
		}
		public static bool ContainsFont(string name, Version version)
		{
			return ContainsAsset(m_fonts, name, version);
		}
		public static bool ContainsShader(string name, Version version)
		{
			return ContainsAsset(m_shaders, name, version);
		}
		public static bool ContainsSprite(string name, Version version)
		{
			return ContainsAsset(m_sprites, name, version);
		}
		public static bool ContainsLightmapParams(string name, Version version)
		{
			return ContainsAsset(m_lightmapParams, name, version);
		}
		public static bool ContainsBehaviour(string name, Version version)
		{
			return ContainsAsset(m_behaviours, name, version);
		}

		public static EngineBuiltInAsset GetMaterial(string name, Version version)
		{
			return m_materials[name].GetAsset(version);
		}
		public static bool TryGetMaterial(string name, Version version, out EngineBuiltInAsset asset)
		{
			return TryGetAsset(m_materials, name, version, out asset);
		}
		public static EngineBuiltInAsset GetTexture(string name, Version version)
		{
			return m_textures[name].GetAsset(version);
		}
		public static bool TryGetTexture(string name, Version version, out EngineBuiltInAsset asset)
		{
			return TryGetAsset(m_textures, name, version, out asset);
		}
		public static EngineBuiltInAsset GetMesh(string name, Version version)
		{
			return m_meshes[name].GetAsset(version);
		}
		public static bool TryGetMesh(string name, Version version, out EngineBuiltInAsset asset)
		{
			return TryGetAsset(m_meshes, name, version, out asset);
		}
		public static EngineBuiltInAsset GetFont(string name, Version version)
		{
			return m_fonts[name].GetAsset(version);
		}
		public static bool TryGetFont(string name, Version version, out EngineBuiltInAsset asset)
		{
			return TryGetAsset(m_fonts, name, version, out asset);
		}
		public static EngineBuiltInAsset GetShader(string name, Version version)
		{
			return m_shaders[name].GetAsset(version);
		}
		public static bool TryGetShader(string name, Version version, out EngineBuiltInAsset asset)
		{
			return TryGetAsset(m_shaders, name, version, out asset);
		}
		public static EngineBuiltInAsset GetSprite(string name, Version version)
		{
			return m_sprites[name].GetAsset(version);
		}
		public static bool TryGetSprite(string name, Version version, out EngineBuiltInAsset asset)
		{
			return TryGetAsset(m_sprites, name, version, out asset);
		}
		public static EngineBuiltInAsset GetLightmapParams(string name, Version version)
		{
			return m_lightmapParams[name].GetAsset(version);
		}
		public static bool TryGetLightmapParams(string name, Version version, out EngineBuiltInAsset asset)
		{
			return TryGetAsset(m_lightmapParams, name, version, out asset);
		}
		public static EngineBuiltInAsset GetBehaviour(string name, Version version)
		{
			return m_behaviours[name].GetAsset(version);
		}
		public static bool TryGetBehaviour(string name, Version version, out EngineBuiltInAsset asset)
		{
			return TryGetAsset(m_behaviours, name, version, out asset);
		}

		private static void AddMaterial(string name, uint exportID, bool isF)
		{
			AddMaterial(name, Version.MinVersion, exportID, isF);
		}
		private static void AddMaterial(string name, Version version, uint exportID, bool isF)
		{
			AddAsset(m_materials, name, version, exportID, 0, isF);
		}

		private static void AddTexture(string name, uint exportID, bool isF)
		{
			AddTexture(name, Version.MinVersion, exportID, isF);
		}
		private static void AddTexture(string name, Version version, uint exportID, bool isF)
		{
			AddAsset(m_textures, name, version, exportID, 0, isF);
		}
		private static void AddTexture(string name, Version version, uint exportID, uint param, bool isF)
		{
			AddAsset(m_textures, name, version, exportID, param, isF);
		}

		private static void AddMesh(string name, uint exportID, bool isF)
		{
			AddMesh(name, Version.MinVersion, exportID, isF);
		}
		private static void AddMesh(string name, Version version, uint exportID, bool isF)
		{
			AddAsset(m_meshes, name, version, exportID, 0, isF);
		}

		private static void AddFont(string name, uint exportID, bool isF)
		{
			AddFont(name, Version.MinVersion, exportID, isF);
		}
		private static void AddFont(string name, Version version, uint exportID, bool isF)
		{
			AddAsset(m_fonts, name, version, exportID, 0, isF);
		}

		private static void AddShader(string name, uint exportID, bool isF)
		{
			AddShader(name, Version.MinVersion, exportID, isF);
		}
		private static void AddShader(string name, Version version, uint exportID, bool isF)
		{
			AddAsset(m_shaders, name, version, exportID, 0, isF);
		}

		private static void AddSprite(string name, uint exportID, bool isF)
		{
			AddSprite(name, Version.MinVersion, exportID, isF);
		}
		private static void AddSprite(string name, Version version, uint exportID, bool isF)
		{
			AddAsset(m_sprites, name, version, exportID, 0, isF);
		}

		private static void AddLightmapParams(string name, uint exportID, bool isF)
		{
			AddLightmapParams(name, Version.MinVersion, exportID, isF);
		}
		private static void AddLightmapParams(string name, Version version, uint exportID, bool isF)
		{
			AddAsset(m_lightmapParams, name, version, exportID, 0, isF);
		}

		private static void AddBehaviour(string name, uint exportID, bool isF)
		{
			AddBehaviour(name, Version.MinVersion, exportID, isF);
		}
		private static void AddBehaviour(string name, Version version, uint exportID, bool isF)
		{
			AddAsset(m_behaviours, name, version, exportID, 0, isF);
		}

		private static bool ContainsAsset(Dictionary<string, EngineBuiltInAssetInfo> lookup, string name, Version version)
		{
			if (lookup.TryGetValue(name, out EngineBuiltInAssetInfo info))
			{
				return info.ContainsAsset(version);
			}
			return false;
		}

		private static bool TryGetAsset(Dictionary<string, EngineBuiltInAssetInfo> lookup, string name, Version version, out EngineBuiltInAsset asset)
		{
			if (lookup.TryGetValue(name, out EngineBuiltInAssetInfo info))
			{
				return info.TryGetAsset(version, out asset);
			}
			asset = default;
			return false;
		}

		private static void AddAsset(Dictionary<string, EngineBuiltInAssetInfo> lookup, string name, Version version, uint exportID, uint param, bool isF)
		{
			EngineBuiltInAsset asset = new EngineBuiltInAsset(exportID, param, isF);
			if (lookup.TryGetValue(name, out EngineBuiltInAssetInfo assetInfo))
			{
				assetInfo.AddVariation(version, asset);
			}
			else
			{
				assetInfo = new EngineBuiltInAssetInfo(version, asset);
				lookup.Add(name, assetInfo);
			}
		}

		public const string FontMaterialName = "Font Material";
		public const string DefaultTerrainDiffuseName = "Default-Terrain-Diffuse";
		public const string DefaultTerrainSpecularName = "Default-Terrain-Specular";
		public const string DefaultTerrainStandardName = "Default-Terrain-Standard";

		public const string LegacyDiffuse = "Legacy Shaders/Diffuse";
		public const string SpriteDefault = "Sprites/Default";
		public const string UIDefault = "UI/Default";
		public const string CubeBlur = "Hidden/CubeBlur";
		public const string CubeCopy = "Hidden/CubeCopy";
		public const string CubeBlend = "Hidden/CubeBlend";
		public const string TerrainVertexLit = "Hidden/TerrainEngine/Details/Vertexlit";
		public const string TerrainWavingDoublePass = "Hidden/TerrainEngine/Details/WavingDoublePass";
		public const string TerrainBillboardWavingDoublePass = "Hidden/TerrainEngine/Details/BillboardWavingDoublePass";

		public static readonly UnityGUID DGUID = new UnityGUID(0x00000000, 0x00000000, 0x0000000D, 0x00000000);
		public static readonly UnityGUID EGUID = new UnityGUID(0x00000000, 0x00000000, 0x0000000E, 0x00000000);
		public static readonly UnityGUID FGUID = new UnityGUID(0x00000000, 0x00000000, 0x0000000F, 0x00000000);

		private static Dictionary<string, EngineBuiltInAssetInfo> m_materials = new Dictionary<string, EngineBuiltInAssetInfo>();
		private static Dictionary<string, EngineBuiltInAssetInfo> m_textures = new Dictionary<string, EngineBuiltInAssetInfo>();
		private static Dictionary<string, EngineBuiltInAssetInfo> m_meshes = new Dictionary<string, EngineBuiltInAssetInfo>();
		private static Dictionary<string, EngineBuiltInAssetInfo> m_shaders = new Dictionary<string, EngineBuiltInAssetInfo>();
		private static Dictionary<string, EngineBuiltInAssetInfo> m_fonts = new Dictionary<string, EngineBuiltInAssetInfo>();
		private static Dictionary<string, EngineBuiltInAssetInfo> m_sprites = new Dictionary<string, EngineBuiltInAssetInfo>();
		private static Dictionary<string, EngineBuiltInAssetInfo> m_lightmapParams = new Dictionary<string, EngineBuiltInAssetInfo>();
		private static Dictionary<string, EngineBuiltInAssetInfo> m_behaviours = new Dictionary<string, EngineBuiltInAssetInfo>();
	}
}
