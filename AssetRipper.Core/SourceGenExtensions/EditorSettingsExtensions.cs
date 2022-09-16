using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.EditorSettings;
using AssetRipper.SourceGenerated.Classes.ClassID_159;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class EditorSettingsExtensions
	{
		private const string DefaultExtensions = "txt;xml;fnt;cd;asmdef;rsp" + ";" + AsmrefExtension;
		private const string AsmrefExtension = "asmref";
		private const string HiddenMeta = "Hidden Meta Files";
		private const string VisibleMeta = "Visible Meta Files";

		public static void SetToDefaults(this IEditorSettings settings)
		{
			settings.ExternalVersionControlSupport_C159_Utf8String.TrySet(VisibleMeta);
			settings.ExternalVersionControlSupport_C159_Int32 = (int)ExternalVersionControl.Generic;

			settings.SerializationMode_C159 = (int)SerializationMode.ForceText;
			settings.SerializeInlineMappingsOnOneLine_C159 = true;
			settings.SpritePackerPaddingPower_C159 = 1;
			settings.EtcTextureCompressorBehavior_C159 = 1;
			settings.EtcTextureFastCompressor_C159 = 1;
			settings.EtcTextureNormalCompressor_C159 = 2;
			settings.EtcTextureBestCompressor_C159 = 4;
			settings.ProjectGenerationIncludedExtensions_C159.TrySet(DefaultExtensions);
			settings.ProjectGenerationRootNamespace_C159.TrySet(string.Empty);
			if (settings.Has_CollabEditorSettings_C159())
			{
				settings.CollabEditorSettings_C159.InProgressEnabled = true;
			}
			settings.UserGeneratedProjectSuffix_C159.TrySet(string.Empty);
			settings.EnableTextureStreamingInEditMode_C159 = true;
			settings.EnableTextureStreamingInPlayMode_C159 = true;
			settings.AsyncShaderCompilation_C159 = true;

			//Version 2 is the default whenever this property is available.
			//Similarly, version 1 is marked obsolete in those versions.
			//https://docs.unity3d.com/Manual/AssetDatabase.html
			settings.AssetPipelineMode_C159 = (int)AssetPipelineMode.Version2;

			settings.CacheServerMode_C159 = (int)CacheServerMode.AsPreferences;
			settings.CacheServerEndpoint_C159.TrySet(string.Empty);
			settings.CacheServerNamespacePrefix_C159.TrySet("default");
			settings.CacheServerEnableDownload_C159 = true;
			settings.CacheServerEnableUpload_C159 = true;

			settings.ShowLightmapResolutionOverlay_C159 = true;
			settings.UseLegacyProbeSampleCount_C159 = true;
			settings.EnterPlayModeOptions_C159 = (int)(EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload);
		}

		private static void TrySet(this Utf8StringBase? utf8String, string str)
		{
			if (utf8String is not null)
			{
				utf8String.String = str;
			}
		}

		public static SerializationMode GetSerializationMode(this IEditorSettings settings)
		{
			return (SerializationMode)settings.SerializationMode_C159;
		}

		public static AssetPipelineMode GetAssetPipelineMode(this IEditorSettings settings)
		{
			return (AssetPipelineMode)settings.AssetPipelineMode_C159;
		}

		public static CacheServerMode GetCacheServerMode(this IEditorSettings settings)
		{
			return (CacheServerMode)settings.CacheServerMode_C159;
		}

		public static EnterPlayModeOptions GetEnterPlayModeOptions(this IEditorSettings settings)
		{
			return (EnterPlayModeOptions)settings.EnterPlayModeOptions_C159;
		}
	}
}
