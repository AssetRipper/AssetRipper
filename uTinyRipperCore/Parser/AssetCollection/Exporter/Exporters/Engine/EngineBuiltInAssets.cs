using System.Collections.Generic;
using uTinyRipper.Classes;

namespace uTinyRipper.AssetExporters
{
	public struct EngineBuiltInAsset
	{
		public EngineBuiltInAsset(uint exportID, bool isDefault)
		{
			ExportID = exportID;
			m_isDefault = isDefault;
		}

		public bool IsValid => ExportID != 0;
		public EngineGUID GUID => m_isDefault ? EngineBuiltInAssets.FGUID : EngineBuiltInAssets.EGUID;

		public uint ExportID { get; }

		/// <summary>
		///  Is assets located in DefaultResources file
		/// </summary>
		private readonly bool m_isDefault;
	}

	public static class EngineBuiltInAssets
	{
		static EngineBuiltInAssets()
		{
			///////////////////////////////////////////////////////
			// Icons
			///////////////////////////////////////////////////////

			m_textures.Add("EscToExit_back", default);
			m_textures.Add("EscToExit_Text", default);
			m_textures.Add("UnitySplash-cube", default);
			m_textures.Add("UnitySplash-HolographicTrackingLoss", default);
			m_textures.Add("UnityWatermark-beta", default);
			m_textures.Add("UnityWatermark-dev", default);
			m_textures.Add("UnityWatermark-edu", default);
			m_textures.Add("UnityWatermarkPlugin-beta", default);
			m_textures.Add("UnityWatermark-proto", default);
			m_textures.Add("UnityWatermark-small", default);
			m_textures.Add("UnityWatermark-trial", default);
			m_textures.Add("WarningSign", default);

			m_sprites.Add("UnitySplash-cube", default);

			///////////////////////////////////////////////////////
			// Current default
			///////////////////////////////////////////////////////

			m_materials.Add("Font Material", new EngineBuiltInAsset(10100, true));
			m_materials.Add("FrameDebuggerRenderTargetDisplay", new EngineBuiltInAsset(10756, true));

			m_textures.Add("Soft", new EngineBuiltInAsset(10001, false));
			m_textures.Add("Font Texture", new EngineBuiltInAsset(10103, false));
			m_textures.Add("box", new EngineBuiltInAsset(11001, true));
			m_textures.Add("button active", new EngineBuiltInAsset(11002, true));
			m_textures.Add("button hover", new EngineBuiltInAsset(11003, true));
			m_textures.Add("button on hover", new EngineBuiltInAsset(11004, true));
			m_textures.Add("button on", new EngineBuiltInAsset(11005, true));
			m_textures.Add("button", new EngineBuiltInAsset(11006, true));
			m_textures.Add("horizontal scrollbar thumb", new EngineBuiltInAsset(11007, true));
			m_textures.Add("horizontal scrollbar", new EngineBuiltInAsset(11008, true));
			m_textures.Add("horizontalslider", new EngineBuiltInAsset(11009, true));
			m_textures.Add("slider thumb active", new EngineBuiltInAsset(11010, true));
			m_textures.Add("slider thumb", new EngineBuiltInAsset(11011, true));
			m_textures.Add("slidert humb hover", new EngineBuiltInAsset(11012, true));
			m_textures.Add("toggle active", new EngineBuiltInAsset(11013, true));
			m_textures.Add("toggle hover", new EngineBuiltInAsset(11014, true));
			m_textures.Add("toggle on hover", new EngineBuiltInAsset(11015, true));
			m_textures.Add("toggle on", new EngineBuiltInAsset(11016, true));
			m_textures.Add("toggle on active", new EngineBuiltInAsset(11017, true));
			m_textures.Add("toggle", new EngineBuiltInAsset(11018, true));
			m_textures.Add("vertical scrollbar thumb", new EngineBuiltInAsset(11019, true));
			m_textures.Add("vertical scrollbar", new EngineBuiltInAsset(11020, true));
			m_textures.Add("verticalslider", new EngineBuiltInAsset(11021, true));
			m_textures.Add("window on", new EngineBuiltInAsset(11022, true));
			m_textures.Add("window", new EngineBuiltInAsset(11023, true));
			m_textures.Add("textfield", new EngineBuiltInAsset(11024, true));
			m_textures.Add("textfield on", new EngineBuiltInAsset(11025, true));
			m_textures.Add("textfield hover", new EngineBuiltInAsset(11026, true));

			m_meshes.Add("pSphere1", new EngineBuiltInAsset(10200, false));
			m_meshes.Add("Cube", new EngineBuiltInAsset(10202, false));
			m_meshes.Add("pCylinder1", new EngineBuiltInAsset(10203, false));
			m_meshes.Add("pPlane1", new EngineBuiltInAsset(10204, false));
			m_meshes.Add("polySurface2", new EngineBuiltInAsset(10205, false));
			m_meshes.Add("Cylinder", new EngineBuiltInAsset(10206, false));
			m_meshes.Add("Sphere", new EngineBuiltInAsset(10207, false));
			m_meshes.Add("Capsule", new EngineBuiltInAsset(10208, false));
			m_meshes.Add("Plane", new EngineBuiltInAsset(10209, false));
			m_meshes.Add("Quad", new EngineBuiltInAsset(10210, false));
			m_meshes.Add("Icosphere", new EngineBuiltInAsset(10211, false));
			m_meshes.Add("icosahedron", new EngineBuiltInAsset(10212, false));
			m_meshes.Add("pyramid", new EngineBuiltInAsset(10213, false));

			m_fonts.Add("Arial", new EngineBuiltInAsset(10102, false));

			m_shaders.Add("Hidden/InternalErrorShader", new EngineBuiltInAsset(17, true));
			m_shaders.Add("Hidden/InternalClear", new EngineBuiltInAsset(68, false));
			m_shaders.Add("Hidden/Internal-Colored", new EngineBuiltInAsset(69, false));
			m_shaders.Add("GUI/Text Shader", new EngineBuiltInAsset(10101, false));
			m_shaders.Add("Hidden/FrameDebuggerRenderTargetDisplay", new EngineBuiltInAsset(10755, false));

			///////////////////////////////////////////////////////
			// Current extra
			///////////////////////////////////////////////////////

			m_materials.Add("Default-Particle", new EngineBuiltInAsset(10301, true));
			m_materials.Add("Default-Diffuse", new EngineBuiltInAsset(10302, true));
			m_materials.Add("Default-Material", new EngineBuiltInAsset(10303, true));
			m_materials.Add("Default-Skybox", new EngineBuiltInAsset(10304, true));
			m_materials.Add("Default-Line", new EngineBuiltInAsset(10306, true));
			m_materials.Add("Sprites-Default", new EngineBuiltInAsset(10754, false));
			m_materials.Add("Sprites-Mask", new EngineBuiltInAsset(10758, false));
			m_materials.Add("SpatialMappingOcclusion", new EngineBuiltInAsset(15302, true));
			m_materials.Add("SpatialMappingWireframe", new EngineBuiltInAsset(15303, true));

			m_textures.Add("Default-Particle", new EngineBuiltInAsset(10300, true));
			m_textures.Add("Default-Checker", new EngineBuiltInAsset(10305, true));
			m_textures.Add("Checkmark", new EngineBuiltInAsset(10900, true));
			m_textures.Add("UISprite", new EngineBuiltInAsset(10904, true));
			m_textures.Add("Background", new EngineBuiltInAsset(10906, true));
			m_textures.Add("InputFieldBackground", new EngineBuiltInAsset(10910, true));
			m_textures.Add("Knob", new EngineBuiltInAsset(10912, true));
			m_textures.Add("DropdownArrow", new EngineBuiltInAsset(10914, true));
			m_textures.Add("UIMask", new EngineBuiltInAsset(10916, true));

			m_shaders.Add("Legacy Shaders/Diffuse Fast", new EngineBuiltInAsset(1, true));
			m_shaders.Add("Legacy Shaders/Bumped Diffuse", new EngineBuiltInAsset(2, true));
			m_shaders.Add("Legacy Shaders/Specular", new EngineBuiltInAsset(3, true));
			m_shaders.Add("Legacy Shaders/Bumped Specular", new EngineBuiltInAsset(4, true));
			m_shaders.Add("Legacy Shaders/Diffuse Detail", new EngineBuiltInAsset(5, true));
			m_shaders.Add("Legacy Shaders/VertexLit", new EngineBuiltInAsset(6, true));
			m_shaders.Add("Legacy Shaders/Diffuse", new EngineBuiltInAsset(7, true));
			m_shaders.Add("Legacy Shaders/Parallax Diffuse", new EngineBuiltInAsset(8, true));
			m_shaders.Add("Legacy Shaders/Parallax Specular", new EngineBuiltInAsset(9, true));
			m_shaders.Add("Legacy Shaders/Self-Illumin/Diffuse", new EngineBuiltInAsset(10, true));
			m_shaders.Add("Legacy Shaders/Self-Illumin/Bumped Diffuse", new EngineBuiltInAsset(11, true));
			m_shaders.Add("Legacy Shaders/Self-Illumin/Specular", new EngineBuiltInAsset(12, true));
			m_shaders.Add("Legacy Shaders/Self-Illumin/Bumped Specular", new EngineBuiltInAsset(13, true));
			m_shaders.Add("Legacy Shaders/Self-Illumin/VertexLit", new EngineBuiltInAsset(14, true));
			m_shaders.Add("Legacy Shaders/Self-Illumin/Parallax Diffuse", new EngineBuiltInAsset(15, true));
			m_shaders.Add("Legacy Shaders/Self-Illumin/Parallax Specular", new EngineBuiltInAsset(16, true));
			m_shaders.Add("Hidden/Internal-StencilWrite", new EngineBuiltInAsset(19, true));
			m_shaders.Add("Legacy Shaders/Reflective/Diffuse", new EngineBuiltInAsset(20, true));
			m_shaders.Add("Legacy Shaders/Reflective/Bumped Diffuse", new EngineBuiltInAsset(21, true));
			m_shaders.Add("Legacy Shaders/Reflective/Specular", new EngineBuiltInAsset(22, true));
			m_shaders.Add("Legacy Shaders/Reflective/Bumped Specular", new EngineBuiltInAsset(23, true));
			m_shaders.Add("Legacy Shaders/Reflective/VertexLit", new EngineBuiltInAsset(24, true));
			m_shaders.Add("Legacy Shaders/Reflective/Bumped Unlit", new EngineBuiltInAsset(25, true));
			m_shaders.Add("Legacy Shaders/Reflective/Bumped VertexLit", new EngineBuiltInAsset(26, true));
			m_shaders.Add("Legacy Shaders/Reflective/Parallax Diffuse", new EngineBuiltInAsset(27, true));
			m_shaders.Add("Legacy Shaders/Reflective/Parallax Specular", new EngineBuiltInAsset(28, true));
			m_shaders.Add("Legacy Shaders/Transparent/Diffuse", new EngineBuiltInAsset(30, true));
			m_shaders.Add("Legacy Shaders/Transparent/Bumped Diffuse", new EngineBuiltInAsset(31, true));
			m_shaders.Add("Legacy Shaders/Transparent/Specular", new EngineBuiltInAsset(32, true));
			m_shaders.Add("Legacy Shaders/Transparent/Bumped Specular", new EngineBuiltInAsset(33, true));
			m_shaders.Add("Legacy Shaders/Transparent/VertexLit", new EngineBuiltInAsset(34, true));
			m_shaders.Add("Legacy Shaders/Transparent/Parallax Diffuse", new EngineBuiltInAsset(35, true));
			m_shaders.Add("Legacy Shaders/Transparent/Parallax Specular", new EngineBuiltInAsset(36, true));
			m_shaders.Add("Legacy Shaders/Lightmapped/VertexLit", new EngineBuiltInAsset(40, true));
			m_shaders.Add("Legacy Shaders/Lightmapped/Diffuse", new EngineBuiltInAsset(41, true));
			m_shaders.Add("Legacy Shaders/Lightmapped/Bumped Diffuse", new EngineBuiltInAsset(42, true));
			m_shaders.Add("Legacy Shaders/Lightmapped/Specular", new EngineBuiltInAsset(43, true));
			m_shaders.Add("Legacy Shaders/Lightmapped/Bumped Specular", new EngineBuiltInAsset(44, true));
			m_shaders.Add("Standard (Specular setup)", new EngineBuiltInAsset(45, true));
			m_shaders.Add("Standard", new EngineBuiltInAsset(46, true));
			m_shaders.Add("Standard (Roughness setup)", new EngineBuiltInAsset(47, true));
			m_shaders.Add("Legacy Shaders/Transparent/Cutout/VertexLit", new EngineBuiltInAsset(50, true));
			m_shaders.Add("Legacy Shaders/Transparent/Cutout/Diffuse", new EngineBuiltInAsset(51, true));
			m_shaders.Add("Legacy Shaders/Transparent/Cutout/Bumped Diffuse", new EngineBuiltInAsset(52, true));
			m_shaders.Add("Legacy Shaders/Transparent/Cutout/Specular", new EngineBuiltInAsset(53, true));
			m_shaders.Add("Legacy Shaders/Transparent/Cutout/Bumped Specular", new EngineBuiltInAsset(54, true));
			m_shaders.Add("Hidden/Internal-DepthNormalsTexture", new EngineBuiltInAsset(62, true));
			m_shaders.Add("Hidden/Internal-PrePassLighting", new EngineBuiltInAsset(63, true));
			m_shaders.Add("Hidden/Internal-ScreenSpaceShadows", new EngineBuiltInAsset(64, true));
			m_shaders.Add("Hidden/Internal-CombineDepthNormals", new EngineBuiltInAsset(65, true));
			m_shaders.Add("Hidden/BlitCopy", new EngineBuiltInAsset(66, true));
			m_shaders.Add("Hidden/BlitCopyDepth", new EngineBuiltInAsset(67, true));
			m_shaders.Add("Hidden/ConvertTexture", new EngineBuiltInAsset(68, true));
			m_shaders.Add("Hidden/Internal-DeferredShading", new EngineBuiltInAsset(69, true));
			m_shaders.Add("Hidden/Internal-DeferredReflections", new EngineBuiltInAsset(74, true));
			m_shaders.Add("Hidden/Internal-MotionVectors", new EngineBuiltInAsset(75, true));
			m_shaders.Add("Legacy Shaders/Decal", new EngineBuiltInAsset(100, true));
			m_shaders.Add("FX/Flare", new EngineBuiltInAsset(101, true));
			m_shaders.Add("Hidden/Internal-Flare", new EngineBuiltInAsset(102, true));
			m_shaders.Add("Skybox/Cubemap", new EngineBuiltInAsset(103, true));
			m_shaders.Add("Skybox/6 Sided", new EngineBuiltInAsset(104, true));
			m_shaders.Add("Hidden/Internal-Halo", new EngineBuiltInAsset(105, true));
			m_shaders.Add("Skybox/Procedural", new EngineBuiltInAsset(106, true));
			m_shaders.Add("Hidden/BlitCopyWithDepth", new EngineBuiltInAsset(107, true));
			m_shaders.Add("Skybox/Panoramic", new EngineBuiltInAsset(108, true));
			m_shaders.Add("Hidden/BlitToDepth", new EngineBuiltInAsset(109, true));
			m_shaders.Add("Hidden/BlitToDepth_MSAA", new EngineBuiltInAsset(110, true));
			m_shaders.Add("Particles/Additive", new EngineBuiltInAsset(200, true));
			m_shaders.Add("Particles/~Additive-Multiply", new EngineBuiltInAsset(201, true));
			m_shaders.Add("Particles/Additive (Soft)", new EngineBuiltInAsset(202, true));
			m_shaders.Add("Particles/Alpha Blended", new EngineBuiltInAsset(203, true));
			m_shaders.Add("Particles/Multiply", new EngineBuiltInAsset(205, true));
			m_shaders.Add("Particles/Multiply (Double)", new EngineBuiltInAsset(206, true));
			m_shaders.Add("Particles/Alpha Blended Premultiply", new EngineBuiltInAsset(207, true));
			m_shaders.Add("Particles/VertexLit Blended", new EngineBuiltInAsset(208, true));
			m_shaders.Add("Particles/Anim Alpha Blended", new EngineBuiltInAsset(209, true));
			m_shaders.Add("Particles/Standard Surface", new EngineBuiltInAsset(210, true));
			m_shaders.Add("Particles/Standard Unlit", new EngineBuiltInAsset(211, true));
			m_shaders.Add("Hidden/Internal-GUITextureClip", new EngineBuiltInAsset(9000, true));
			m_shaders.Add("Hidden/Internal-GUITextureClipText", new EngineBuiltInAsset(9001, true));
			m_shaders.Add("Hidden/Internal-GUITexture", new EngineBuiltInAsset(9002, true));
			m_shaders.Add("Hidden/Internal-GUITextureBlit", new EngineBuiltInAsset(9003, true));
			m_shaders.Add("Hidden/Internal-GUIRoundedRect", new EngineBuiltInAsset(9004, true));
			m_shaders.Add("Hidden/TerrainEngine/Details/Vertexlit", new EngineBuiltInAsset(10500, true));
			m_shaders.Add("Hidden/TerrainEngine/Details/WavingDoublePass", new EngineBuiltInAsset(10501, true));
			m_shaders.Add("Hidden/TerrainEngine/Details/BillboardWavingDoublePass", new EngineBuiltInAsset(10502, true));
			m_shaders.Add("Hidden/TerrainEngine/Splatmap/Diffuse-AddPass", new EngineBuiltInAsset(10503, true));
			m_shaders.Add("Nature/Terrain/Diffuse", new EngineBuiltInAsset(10505, true));
			m_shaders.Add("Hidden/TerrainEngine/BillboardTree", new EngineBuiltInAsset(10507, true));
			m_shaders.Add("Hidden/Nature/Tree Soft Occlusion Bark Rendertex", new EngineBuiltInAsset(10508, true));
			m_shaders.Add("Nature/Tree Soft Occlusion Bark", new EngineBuiltInAsset(10509, true));
			m_shaders.Add("Hidden/Nature/Tree Soft Occlusion Leaves Rendertex", new EngineBuiltInAsset(10510, true));
			m_shaders.Add("Nature/Tree Soft Occlusion Leaves", new EngineBuiltInAsset(10511, true));
			m_shaders.Add("Legacy Shaders/Transparent/Cutout/Soft Edge Unlit", new EngineBuiltInAsset(10512, true));
			m_shaders.Add("Hidden/TerrainEngine/CameraFacingBillboardTree", new EngineBuiltInAsset(10513, true));
			m_shaders.Add("Nature/Tree Creator Bark", new EngineBuiltInAsset(10600, true));
			m_shaders.Add("Nature/Tree Creator Leaves", new EngineBuiltInAsset(10601, true));
			m_shaders.Add("Hidden/Nature/Tree Creator Bark Rendertex", new EngineBuiltInAsset(10602, true));
			m_shaders.Add("Hidden/Nature/Tree Creator Leaves Rendertex", new EngineBuiltInAsset(10603, true));
			m_shaders.Add("Hidden/Nature/Tree Creator Bark Optimized", new EngineBuiltInAsset(10604, true));
			m_shaders.Add("Hidden/Nature/Tree Creator Leaves Optimized", new EngineBuiltInAsset(10605, true));
			m_shaders.Add("Nature/Tree Creator Leaves Fast", new EngineBuiltInAsset(10606, true));
			m_shaders.Add("Hidden/Nature/Tree Creator Leaves Fast Optimized", new EngineBuiltInAsset(10607, true));
			m_shaders.Add("Nature/Terrain/Specular", new EngineBuiltInAsset(10620, true));
			m_shaders.Add("Hidden/TerrainEngine/Splatmap/Specular-AddPass", new EngineBuiltInAsset(10621, true));
			m_shaders.Add("Hidden/TerrainEngine/Splatmap/Specular-Base", new EngineBuiltInAsset(10622, true));
			m_shaders.Add("Nature/Terrain/Standard", new EngineBuiltInAsset(10623, true));
			m_shaders.Add("Hidden/TerrainEngine/Splatmap/Standard-AddPass", new EngineBuiltInAsset(10624, true));
			m_shaders.Add("Hidden/TerrainEngine/Splatmap/Standard-Base", new EngineBuiltInAsset(10625, true));
			m_shaders.Add("Mobile/Skybox", new EngineBuiltInAsset(10700, true));
			m_shaders.Add("Mobile/VertexLit", new EngineBuiltInAsset(10701, true));
			m_shaders.Add("Mobile/Diffuse", new EngineBuiltInAsset(10703, true));
			m_shaders.Add("Mobile/Bumped Diffuse", new EngineBuiltInAsset(10704, true));
			m_shaders.Add("Mobile/Bumped Specular", new EngineBuiltInAsset(10705, true));
			m_shaders.Add("Mobile/Bumped Specular (1 Directional Light)", new EngineBuiltInAsset(10706, true));
			m_shaders.Add("Mobile/VertexLit (Only Directional Lights)", new EngineBuiltInAsset(10707, true));
			m_shaders.Add("Mobile/Unlit (Supports Lightmap)", new EngineBuiltInAsset(10708, true));
			m_shaders.Add("Mobile/Particles/Additive", new EngineBuiltInAsset(10720, true));
			m_shaders.Add("Mobile/Particles/Alpha Blended", new EngineBuiltInAsset(10721, true));
			m_shaders.Add("Mobile/Particles/VertexLit Blended", new EngineBuiltInAsset(10722, true));
			m_shaders.Add("Mobile/Particles/Multiply", new EngineBuiltInAsset(10723, true));
			m_shaders.Add("Unlit/Transparent", new EngineBuiltInAsset(10750, true));
			m_shaders.Add("Unlit/Transparent Cutout", new EngineBuiltInAsset(10751, true));
			m_shaders.Add("Unlit/Texture", new EngineBuiltInAsset(10752, true));
			m_shaders.Add("Sprites/Default", new EngineBuiltInAsset(10753, true));
			m_shaders.Add("Unlit/Color", new EngineBuiltInAsset(10755, true));
			m_shaders.Add("Sprites/Mask", new EngineBuiltInAsset(10757, true));
			m_shaders.Add("UI/Unlit/Transparent", new EngineBuiltInAsset(10760, true));
			m_shaders.Add("UI/Unlit/Detail", new EngineBuiltInAsset(10761, true));
			m_shaders.Add("UI/Unlit/Text", new EngineBuiltInAsset(10762, true));
			m_shaders.Add("UI/Unlit/Text Detail", new EngineBuiltInAsset(10763, true));
			m_shaders.Add("UI/Lit/Transparent", new EngineBuiltInAsset(10764, true));
			m_shaders.Add("UI/Lit/Bumped", new EngineBuiltInAsset(10765, true));
			m_shaders.Add("UI/Lit/Detail", new EngineBuiltInAsset(10766, true));
			m_shaders.Add("UI/Lit/Refraction", new EngineBuiltInAsset(10767, true));
			m_shaders.Add("UI/Lit/Refraction Detail", new EngineBuiltInAsset(10768, true));
			m_shaders.Add("UI/Default", new EngineBuiltInAsset(10770, true));
			m_shaders.Add("UI/Default Font", new EngineBuiltInAsset(10782, true));
			m_shaders.Add("UI/DefaultETC1", new EngineBuiltInAsset(10783, true));
			m_shaders.Add("Hidden/UI/CompositeOverdraw", new EngineBuiltInAsset(10784, true));
			m_shaders.Add("Hidden/UI/Overdraw", new EngineBuiltInAsset(10785, true));
			m_shaders.Add("Sprites/Diffuse", new EngineBuiltInAsset(10800, true));
			m_shaders.Add("Nature/SpeedTree", new EngineBuiltInAsset(14000, true));
			m_shaders.Add("Nature/SpeedTree Billboard", new EngineBuiltInAsset(14001, true));
			m_shaders.Add("Hidden/GIDebug/TextureUV", new EngineBuiltInAsset(15100, true));
			m_shaders.Add("Hidden/GIDebug/ShowLightMask", new EngineBuiltInAsset(15101, true));
			m_shaders.Add("Hidden/GIDebug/UV1sAsPositions", new EngineBuiltInAsset(15102, true));
			m_shaders.Add("Hidden/GIDebug/VertexColors", new EngineBuiltInAsset(15103, true));
			m_shaders.Add("Hidden/CubeBlur", new EngineBuiltInAsset(15104, true));
			m_shaders.Add("Hidden/CubeCopy", new EngineBuiltInAsset(15105, true));
			m_shaders.Add("Hidden/CubeBlend", new EngineBuiltInAsset(15106, true));
			m_shaders.Add("VR/SpatialMapping/Occlusion", new EngineBuiltInAsset(15300, true));
			m_shaders.Add("VR/SpatialMapping/Wireframe", new EngineBuiltInAsset(15301, true));
			m_shaders.Add("Hidden/VR/BlitTexArraySlice", new EngineBuiltInAsset(15304, true));
			m_shaders.Add("Hidden/VR/Internal-VRDistortion", new EngineBuiltInAsset(15305, true));
			m_shaders.Add("Hidden/VR/BlitTexArraySliceToDepth", new EngineBuiltInAsset(15306, true));
			m_shaders.Add("Hidden/VR/BlitTexArraySliceToDepth_MSAA", new EngineBuiltInAsset(15307, true));
			m_shaders.Add("Hidden/VR/ClippingMask", new EngineBuiltInAsset(15310, true));
			m_shaders.Add("Hidden/VR/VideoBackground", new EngineBuiltInAsset(15311, true));
			m_shaders.Add("AR/TangoARRender", new EngineBuiltInAsset(15401, true));
			m_shaders.Add("Hidden/VideoDecode", new EngineBuiltInAsset(16000, true));
			m_shaders.Add("Hidden/VideoDecodeOSX", new EngineBuiltInAsset(16001, true));
			m_shaders.Add("Hidden/VideoDecodeAndroid", new EngineBuiltInAsset(16002, true));
			m_shaders.Add("Hidden/Compositing", new EngineBuiltInAsset(17000, true));

			m_sprites.Add("Checkmark", new EngineBuiltInAsset(10901, true));
			m_sprites.Add("UISprite", new EngineBuiltInAsset(10905, true));
			m_sprites.Add("Background", new EngineBuiltInAsset(10907, true));
			m_sprites.Add("InputFieldBackground", new EngineBuiltInAsset(10911, true));
			m_sprites.Add("Knob", new EngineBuiltInAsset(10913, true));
			m_sprites.Add("DropdownArrow", new EngineBuiltInAsset(10915, true));
			m_sprites.Add("UIMask", new EngineBuiltInAsset(10917, true));
			
			m_lightmapParams.Add("Default-HighResolution", new EngineBuiltInAsset(15200, true));
			m_lightmapParams.Add("Default-LowResolution", new EngineBuiltInAsset(15201, true));
			m_lightmapParams.Add("Default-VeryLowResolution", new EngineBuiltInAsset(15203, true));
			m_lightmapParams.Add("Default-Medium", new EngineBuiltInAsset(15204, true));

			///////////////////////////////////////////////////////
			// Old default
			///////////////////////////////////////////////////////
			
			m_shaders.Add("Internal-ErrorShader", new EngineBuiltInAsset(17, true));
			m_shaders.Add("Shadow-ScreenBlur", new EngineBuiltInAsset(60, false));
			m_shaders.Add("Camera-DepthTexture", new EngineBuiltInAsset(61, false));
			m_shaders.Add("Camera-DepthNormalTexture", new EngineBuiltInAsset(62, false));
			m_shaders.Add("Internal-PrePassLighting", new EngineBuiltInAsset(63, false));
			m_shaders.Add("Internal-PrePassCollectShadows", new EngineBuiltInAsset(64, false));
			m_shaders.Add("Internal-CombineDepthNormals", new EngineBuiltInAsset(65, false));
			m_shaders.Add("Internal-BlitCopy", new EngineBuiltInAsset(66, false));
			m_shaders.Add("Shadow-ScreenBlurRotated", new EngineBuiltInAsset(67, false));
			m_shaders.Add("Internal-Clear", new EngineBuiltInAsset(68, false));
			m_shaders.Add("Internal-Flare", new EngineBuiltInAsset(102, true));
			m_shaders.Add("Internal-Halo", new EngineBuiltInAsset(105, true));
			m_shaders.Add("Internal-GUITextureClip", new EngineBuiltInAsset(9000, true));
			m_shaders.Add("Internal-GUITextureClipText", new EngineBuiltInAsset(9001, true));
			m_shaders.Add("Internal-GUITexture", new EngineBuiltInAsset(9002, true));
			m_shaders.Add("Internal-GUITextureBlit", new EngineBuiltInAsset(9003, true));
			m_shaders.Add("Font", new EngineBuiltInAsset(10101, false));
			m_shaders.Add("Sprites-Default", new EngineBuiltInAsset(10753, true));

			///////////////////////////////////////////////////////
			// Old Extra
			///////////////////////////////////////////////////////
			
			m_shaders.Add("Normal-DiffuseFast", new EngineBuiltInAsset(1, true));
			m_shaders.Add("Normal-Bumped", new EngineBuiltInAsset(2, true));
			m_shaders.Add("Normal-Glossy", new EngineBuiltInAsset(3, true));
			m_shaders.Add("Normal-BumpSpec", new EngineBuiltInAsset(4, true));
			m_shaders.Add("Normal-DiffuseDetail", new EngineBuiltInAsset(5, true));
			m_shaders.Add("Normal-VertexLit", new EngineBuiltInAsset(6, true));
			m_shaders.Add("Normal-Diffuse", new EngineBuiltInAsset(7, true));
			m_shaders.Add("Normal-Parallax", new EngineBuiltInAsset(8, true));
			m_shaders.Add("Normal-ParallaxSpec", new EngineBuiltInAsset(9, true));
			m_shaders.Add("Illumin-Diffuse", new EngineBuiltInAsset(10, true));
			m_shaders.Add("Illumin-Bumped", new EngineBuiltInAsset(11, true));
			m_shaders.Add("Illumin-Glossy", new EngineBuiltInAsset(12, true));
			m_shaders.Add("Illumin-BumpSpec", new EngineBuiltInAsset(13, true));
			m_shaders.Add("Illumin-VertexLit", new EngineBuiltInAsset(14, true));
			m_shaders.Add("Illumin-Parallax", new EngineBuiltInAsset(15, true));
			m_shaders.Add("Illumin-ParallaxSpec", new EngineBuiltInAsset(16, true));
			m_shaders.Add("Reflect-Diffuse", new EngineBuiltInAsset(20, true));
			m_shaders.Add("Reflect-Bumped", new EngineBuiltInAsset(21, true));
			m_shaders.Add("Reflect-Glossy", new EngineBuiltInAsset(22, true));
			m_shaders.Add("Reflect-BumpSpec", new EngineBuiltInAsset(23, true));
			m_shaders.Add("Reflect-VertexLit", new EngineBuiltInAsset(24, true));
			m_shaders.Add("Reflect-BumpNolight", new EngineBuiltInAsset(25, true));
			m_shaders.Add("Reflect-BumpVertexLit", new EngineBuiltInAsset(26, true));
			m_shaders.Add("Reflect-Parallax", new EngineBuiltInAsset(27, true));
			m_shaders.Add("Reflect-ParallaxSpec", new EngineBuiltInAsset(28, true));
			m_shaders.Add("Alpha-Diffuse", new EngineBuiltInAsset(30, true));
			m_shaders.Add("Alpha-Bumped", new EngineBuiltInAsset(31, true));
			m_shaders.Add("Alpha-Glossy", new EngineBuiltInAsset(32, true));
			m_shaders.Add("Alpha-BumpSpec", new EngineBuiltInAsset(33, true));
			m_shaders.Add("Alpha-VertexLit", new EngineBuiltInAsset(34, true));
			m_shaders.Add("Alpha-Parallax", new EngineBuiltInAsset(35, true));
			m_shaders.Add("Alpha-ParallaxSpec", new EngineBuiltInAsset(36, true));
			m_shaders.Add("Lightmap-VertexLit", new EngineBuiltInAsset(40, true));
			m_shaders.Add("Lightmap-Diffuse", new EngineBuiltInAsset(41, true));
			m_shaders.Add("Lightmap-Bumped", new EngineBuiltInAsset(42, true));
			m_shaders.Add("Lightmap-Glossy", new EngineBuiltInAsset(43, true));
			m_shaders.Add("Lightmap-BumpSpec", new EngineBuiltInAsset(44, true));
			m_shaders.Add("AlphaTest-VertexLit", new EngineBuiltInAsset(50, true));
			m_shaders.Add("AlphaTest-Diffuse", new EngineBuiltInAsset(51, true));
			m_shaders.Add("AlphaTest-Bumped", new EngineBuiltInAsset(52, true));
			m_shaders.Add("AlphaTest-Glossy", new EngineBuiltInAsset(53, true));
			m_shaders.Add("AlphaTest-BumpSpec", new EngineBuiltInAsset(54, true));
			//m_shaders.Add("Shader", new EngineBuildInAsset(100, true));
			m_shaders.Add("Flare", new EngineBuiltInAsset(101, true));
			m_shaders.Add("skybox cubed", new EngineBuiltInAsset(103, true));
			m_shaders.Add("Skybox", new EngineBuiltInAsset(104, true));
			m_shaders.Add("Particle Add", new EngineBuiltInAsset(200, true));
			m_shaders.Add("Particle AddMultiply", new EngineBuiltInAsset(201, true));
			m_shaders.Add("Particle AddSmooth", new EngineBuiltInAsset(202, true));
			m_shaders.Add("Particle Alpha Blend", new EngineBuiltInAsset(203, true));
			m_shaders.Add("Particle Multiply", new EngineBuiltInAsset(205, true));
			m_shaders.Add("Particle MultiplyDouble", new EngineBuiltInAsset(206, true));
			m_shaders.Add("Particle Premultiply Blend", new EngineBuiltInAsset(207, true));
			m_shaders.Add("Particle VertexLit Blended", new EngineBuiltInAsset(208, true));
			m_shaders.Add("VertexLit", new EngineBuiltInAsset(10500, true));
			m_shaders.Add("WavingGrass", new EngineBuiltInAsset(10501, true));
			m_shaders.Add("WavingGrassBillboard", new EngineBuiltInAsset(10502, true));
			m_shaders.Add("AddPass", new EngineBuiltInAsset(10503, true));
			m_shaders.Add("FirstPass", new EngineBuiltInAsset(10505, true));
			m_shaders.Add("BillboardTree", new EngineBuiltInAsset(10507, true));
			m_shaders.Add("TreeSoftOcclusionBarkRendertex", new EngineBuiltInAsset(10508, true));
			m_shaders.Add("TreeSoftOcclusionBark", new EngineBuiltInAsset(10509, true));
			m_shaders.Add("TreeSoftOcclusionLeavesRendertex", new EngineBuiltInAsset(10510, true));
			m_shaders.Add("TreeSoftOcclusionLeaves", new EngineBuiltInAsset(10511, true));
			m_shaders.Add("AlphaTest-SoftEdgeUnlit", new EngineBuiltInAsset(10512, true));
			m_shaders.Add("TreeCreatorBark", new EngineBuiltInAsset(10600, true));
			m_shaders.Add("TreeCreatorLeaves", new EngineBuiltInAsset(10601, true));
			m_shaders.Add("TreeCreatorBarkRendertex", new EngineBuiltInAsset(10602, true));
			m_shaders.Add("TreeCreatorLeavesRendertex", new EngineBuiltInAsset(10603, true));
			m_shaders.Add("TreeCreatorBarkOptimized", new EngineBuiltInAsset(10604, true));
			m_shaders.Add("TreeCreatorLeavesOptimized", new EngineBuiltInAsset(10605, true));
			m_shaders.Add("TreeCreatorLeavesFast", new EngineBuiltInAsset(10606, true));
			m_shaders.Add("TreeCreatorLeavesFastOptimized", new EngineBuiltInAsset(10607, true));
			m_shaders.Add("TerrBumpFirstPass", new EngineBuiltInAsset(10620, true));
			m_shaders.Add("TerrBumpAddPass", new EngineBuiltInAsset(10621, true));
			m_shaders.Add("Mobile-Skybox", new EngineBuiltInAsset(10700, true));
			m_shaders.Add("Mobile-VertexLit", new EngineBuiltInAsset(10701, true));
			m_shaders.Add("Mobile-Diffuse", new EngineBuiltInAsset(10703, true));
			m_shaders.Add("Mobile-Bumped", new EngineBuiltInAsset(10704, true));
			m_shaders.Add("Mobile-BumpSpec", new EngineBuiltInAsset(10705, true));
			m_shaders.Add("Mobile-BumpSpec-1DirectionalLight", new EngineBuiltInAsset(10706, true));
			m_shaders.Add("Mobile-VertexLit-OnlyDirectionalLights", new EngineBuiltInAsset(10707, true));
			m_shaders.Add("Mobile-Lightmap-Unlit", new EngineBuiltInAsset(10708, true));
			m_shaders.Add("Mobile-Particle-Add", new EngineBuiltInAsset(10720, true));
			m_shaders.Add("Mobile-Particle-Alpha", new EngineBuiltInAsset(10721, true));
			m_shaders.Add("Mobile-Particle-Alpha-VertexLit", new EngineBuiltInAsset(10722, true));
			m_shaders.Add("Mobile-Particle-Multiply", new EngineBuiltInAsset(10723, true));
			m_shaders.Add("Unlit-Alpha", new EngineBuiltInAsset(10750, true));
			m_shaders.Add("Unlit-AlphaTest", new EngineBuiltInAsset(10751, true));
			m_shaders.Add("Unlit-Normal", new EngineBuiltInAsset(10752, true));
			m_shaders.Add("UI-Unlit-Transparent", new EngineBuiltInAsset(10760, true));
			m_shaders.Add("UI-Unlit-Detail", new EngineBuiltInAsset(10761, true));
			m_shaders.Add("UI-Unlit-Text", new EngineBuiltInAsset(10762, true));
			m_shaders.Add("UI-Unlit-TextDetail", new EngineBuiltInAsset(10763, true));
			m_shaders.Add("UI-Lit-Transparent", new EngineBuiltInAsset(10764, true));
			m_shaders.Add("UI-Lit-Bumped", new EngineBuiltInAsset(10765, true));
			m_shaders.Add("UI-Lit-Detail", new EngineBuiltInAsset(10766, true));
			m_shaders.Add("UI-Lit-Refraction(ProOnly)", new EngineBuiltInAsset(10767, true));
			m_shaders.Add("UI-Lit-RefractionDetail(ProOnly)", new EngineBuiltInAsset(10768, true));
			m_shaders.Add("UI-Default", new EngineBuiltInAsset(10770, true));
			m_shaders.Add("UI-DefaultFont", new EngineBuiltInAsset(10782, true));
			m_shaders.Add("Sprites-Diffuse", new EngineBuiltInAsset(10800, true));
		}

		public static IReadOnlyDictionary<string, EngineBuiltInAsset> Materials => m_materials;
		public static IReadOnlyDictionary<string, EngineBuiltInAsset> Textures => m_textures;
		public static IReadOnlyDictionary<string, EngineBuiltInAsset> Meshes => m_meshes;
		public static IReadOnlyDictionary<string, EngineBuiltInAsset> Shaders => m_shaders;
		public static IReadOnlyDictionary<string, EngineBuiltInAsset> Fonts => m_fonts;
		public static IReadOnlyDictionary<string, EngineBuiltInAsset> Sprites => m_sprites;
		public static IReadOnlyDictionary<string, EngineBuiltInAsset> LightmapParams => m_lightmapParams;
		
		public static readonly EngineGUID EGUID = new EngineGUID(0x00000000, 0xE0000000, 0x00000000, 0x00000000);
		public static readonly EngineGUID FGUID = new EngineGUID(0x00000000, 0xF0000000, 0x00000000, 0x00000000);

		private static Dictionary<string, EngineBuiltInAsset> m_materials = new Dictionary<string, EngineBuiltInAsset>();
		private static Dictionary<string, EngineBuiltInAsset> m_textures = new Dictionary<string, EngineBuiltInAsset>();
		private static Dictionary<string, EngineBuiltInAsset> m_meshes = new Dictionary<string, EngineBuiltInAsset>();
		private static Dictionary<string, EngineBuiltInAsset> m_shaders = new Dictionary<string, EngineBuiltInAsset>();
		private static Dictionary<string, EngineBuiltInAsset> m_fonts = new Dictionary<string, EngineBuiltInAsset>();
		private static Dictionary<string, EngineBuiltInAsset> m_sprites = new Dictionary<string, EngineBuiltInAsset>();
		private static Dictionary<string, EngineBuiltInAsset> m_lightmapParams = new Dictionary<string, EngineBuiltInAsset>();
	}
}
