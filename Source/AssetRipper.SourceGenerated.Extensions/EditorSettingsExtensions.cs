using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_159;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class EditorSettingsExtensions
	{
		private const string DefaultExtensions = "txt;xml;fnt;cd;asmdef;rsp" + ";" + AsmrefExtension;
		private const string AsmrefExtension = "asmref";
		private const string HiddenMeta = "Hidden Meta Files";
		private const string VisibleMeta = "Visible Meta Files";

		public static void SetToDefaults(this IEditorSettings settings)
		{
			settings.ExternalVersionControlSupport_C159_Utf8String = VisibleMeta;
			settings.ExternalVersionControlSupport_C159_Int32 = (int)ExternalVersionControl.Generic;

			settings.SerializationMode_C159E = SerializationMode.ForceText;
			settings.SerializeInlineMappingsOnOneLine_C159 = true;
			settings.SpritePackerPaddingPower_C159 = 1;
			settings.EtcTextureCompressorBehavior_C159 = 1;
			settings.EtcTextureFastCompressor_C159 = 1;
			settings.EtcTextureNormalCompressor_C159 = 2;
			settings.EtcTextureBestCompressor_C159 = 4;
			settings.ProjectGenerationIncludedExtensions_C159 = DefaultExtensions;
			settings.ProjectGenerationRootNamespace_C159 = Utf8String.Empty;
			if (settings.Has_CollabEditorSettings_C159())
			{
				settings.CollabEditorSettings_C159.InProgressEnabled = true;
			}
			settings.UserGeneratedProjectSuffix_C159 = Utf8String.Empty;
			settings.EnableTextureStreamingInEditMode_C159 = true;
			settings.EnableTextureStreamingInPlayMode_C159 = true;
			settings.AsyncShaderCompilation_C159 = true;

			//Version 2 is the default whenever this property is available.
			//Similarly, version 1 is marked obsolete in those versions.
			//https://docs.unity3d.com/Manual/AssetDatabase.html
			settings.AssetPipelineMode_C159E = AssetPipelineMode.Version2;

			settings.CacheServerMode_C159E = CacheServerMode_1.AsPreferences;
			settings.CacheServerEndpoint_C159 = Utf8String.Empty;
			settings.CacheServerNamespacePrefix_C159 = "default"u8;
			settings.CacheServerEnableDownload_C159 = true;
			settings.CacheServerEnableUpload_C159 = true;

			settings.ShowLightmapResolutionOverlay_C159 = true;
			settings.UseLegacyProbeSampleCount_C159 = true;
			settings.EnterPlayModeOptions_C159E = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
		}
	}
}
