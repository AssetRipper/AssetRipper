using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.EditorSettings
{
	/// <summary>
	/// First introduced in 2.6.0
	/// </summary>
	public interface IEditorSettings : IUnityObjectBase
	{
		string ExternalVersionControlSupport { get; set; }
		int SerializationMode { get; set; }
		int SpritePackerPaddingPower { get; set; }
		int EtcTextureCompressorBehavior { get; set; }
		int EtcTextureFastCompressor { get; set; }
		int EtcTextureNormalCompressor { get; set; }
		int EtcTextureBestCompressor { get; set; }
		string ProjectGenerationIncludedExtensions { get; set; }
		string ProjectGenerationRootNamespace { get; set; }
		string UserGeneratedProjectSuffix { get; set; }
		bool EnableTextureStreamingInEditMode { get; set; }
		bool EnableTextureStreamingInPlayMode { get; set; }
		bool AsyncShaderCompilation { get; set; }
		int AssetPipelineMode { get; set; }
		int CacheServerMode { get; set; }
		string CacheServerEndpoint { get; set; }
		string CacheServerNamespacePrefix { get; set; }
		bool CacheServerEnableDownload { get; set; }
		bool CacheServerEnableUpload { get; set; }
	}

	public static class IEditorSettingsExtensions
	{
		private const string DefaultExtensions = "txt;xml;fnt;cd;asmdef;rsp";
		private const string AsmrefExtension = "asmref";
		private const string HiddenMeta = "Hidden Meta Files";
		private const string VisibleMeta = "Visible Meta Files";

		public static void SetToDefaults(this IEditorSettings settings)
		{
			settings.ExternalVersionControlSupport = VisibleMeta;
			settings.SerializationMode = (int)SerializationMode.ForceText;
			settings.SpritePackerPaddingPower = 1;
			settings.EtcTextureCompressorBehavior = 1;
			settings.EtcTextureFastCompressor = 1;
			settings.EtcTextureNormalCompressor = 2;
			settings.EtcTextureBestCompressor = 4;
			settings.ProjectGenerationIncludedExtensions = DefaultExtensions;
			settings.ProjectGenerationRootNamespace = string.Empty;
			settings.UserGeneratedProjectSuffix = string.Empty;
			settings.EnableTextureStreamingInEditMode = true;
			settings.EnableTextureStreamingInPlayMode = true;
			settings.AsyncShaderCompilation = true;
			settings.AssetPipelineMode = (int)AssetPipelineMode.Version1;
			settings.CacheServerMode = (int)CacheServerMode.AsPreferences;
			settings.CacheServerEndpoint = string.Empty;
			settings.CacheServerNamespacePrefix = "default";
			settings.CacheServerEnableDownload = false;
			settings.CacheServerEnableUpload = false;
		}
	}
}
