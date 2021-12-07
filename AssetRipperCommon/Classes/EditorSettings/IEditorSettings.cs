using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.EditorSettings
{
	/// <summary>
	/// First introduced in 2.6.0
	/// </summary>
	public interface IEditorSettings : IUnityObjectBase
	{
	}

	public static class IEditorSettingsExtensions
	{
		private const string DefaultExtensions = "txt;xml;fnt;cd;asmdef;rsp";
		private const string AsmrefExtension = "asmref";
		private const string HiddenMeta = "Hidden Meta Files";
		private const string VisibleMeta = "Visible Meta Files";

		public static void SetToDefaults(this IEditorSettings settings)
		{
			settings.TrySetFieldValue("m_ExternalVersionControlSupport", VisibleMeta);
			settings.TrySetFieldValue("m_ExternalVersionControl", 0); //ExternalVersionControl.Disabled
			settings.TrySetFieldValue("m_SerializationMode", (int)SerializationMode.ForceText);
			settings.TrySetFieldValue("m_SpritePackerPaddingPower", 1);
			settings.TrySetFieldValue("m_EtcTextureCompressorBehavior", 1);
			settings.TrySetFieldValue("m_EtcTextureFastCompressor", 1);
			settings.TrySetFieldValue("m_EtcTextureNormalCompressor", 2);
			settings.TrySetFieldValue("m_EtcTextureBestCompressor", 4);
			settings.TrySetFieldValue("m_ProjectGenerationIncludedExtensions", DefaultExtensions);
			settings.TrySetFieldValue("m_ProjectGenerationRootNamespace", string.Empty);
			settings.TrySetFieldValue("m_UserGeneratedProjectSuffix", string.Empty);
			//settings.TrySetFieldValue("m_", "");//CollabEditorSettings, but I don't think it's necessary
			settings.TrySetFieldValue("m_EnableTextureStreamingInEditMode", true);
			settings.TrySetFieldValue("m_EnableTextureStreamingInPlayMode", true);
			settings.TrySetFieldValue("m_AsyncShaderCompilation", true);
			settings.TrySetFieldValue("m_AssetPipelineMode", (int)AssetPipelineMode.Version1);
			settings.TrySetFieldValue("m_CacheServerMode", (int)CacheServerMode.AsPreferences);
			settings.TrySetFieldValue("m_CacheServerEndpoint", string.Empty);
			settings.TrySetFieldValue("m_CacheServerNamespacePrefix", "default");
			settings.TrySetFieldValue("m_CacheServerEnableDownload", false);
			settings.TrySetFieldValue("m_CacheServerEnableUpload", false);
		}
	}
}
