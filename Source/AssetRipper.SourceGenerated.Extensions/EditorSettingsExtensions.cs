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
			settings.ExternalVersionControlSupport_Utf8String = VisibleMeta;
			settings.ExternalVersionControlSupport_Int32 = (int)ExternalVersionControl.Generic;

			settings.SerializationModeE = SerializationMode.ForceText;
			settings.SerializeInlineMappingsOnOneLine = true;
			settings.SpritePackerPaddingPower = 1;
			settings.EtcTextureCompressorBehavior = 1;
			settings.EtcTextureFastCompressor = 1;
			settings.EtcTextureNormalCompressor = 2;
			settings.EtcTextureBestCompressor = 4;
			settings.ProjectGenerationIncludedExtensions = DefaultExtensions;
			settings.ProjectGenerationRootNamespace = Utf8String.Empty;
			if (settings.Has_CollabEditorSettings())
			{
				settings.CollabEditorSettings.InProgressEnabled = true;
			}
			settings.UserGeneratedProjectSuffix = Utf8String.Empty;
			settings.EnableTextureStreamingInEditMode = true;
			settings.EnableTextureStreamingInPlayMode = true;
			settings.AsyncShaderCompilation = true;

			//Version 2 is the default whenever this property is available.
			//Similarly, version 1 is marked obsolete in those versions.
			//https://docs.unity3d.com/Manual/AssetDatabase.html
			settings.AssetPipelineModeE = AssetPipelineMode.Version2;

			settings.CacheServerModeE = CacheServerMode_1.AsPreferences;
			settings.CacheServerEndpoint = Utf8String.Empty;
			settings.CacheServerNamespacePrefix = (Utf8String)"default"u8;
			settings.CacheServerEnableDownload = true;
			settings.CacheServerEnableUpload = true;

			settings.ShowLightmapResolutionOverlay = true;
			settings.UseLegacyProbeSampleCount = true;
			settings.EnterPlayModeOptionsE = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
		}
	}
}
