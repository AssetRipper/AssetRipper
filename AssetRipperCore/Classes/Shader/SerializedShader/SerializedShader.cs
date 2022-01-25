using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public class SerializedShader : IAssetReadable, ISerializedShader
	{
		public ISerializedProperties PropInfo => m_PropInfo;
		private SerializedProperties m_PropInfo = new();
		public SerializedSubShader[] SubShaders { get; set; }
		public string Name { get; set; }
		public string CustomEditorName { get; set; }
		public string FallbackName { get; set; }
		public SerializedShaderDependency[] Dependencies { get; set; }
		public SerializedCustomEditorForRenderPipeline[] CustomEditorForRenderPipelines { get; set; }
		public bool DisableNoSubshadersMessage { get; set; }

		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasCustomEditorForRenderPipelines(UnityVersion version) => version.IsGreaterEqual(2021);

		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public static bool HasKeywordData(UnityVersion version) => version.IsGreaterEqual(2021, 2);

		public void Read(AssetReader reader)
		{
			m_PropInfo.Read(reader);
			SubShaders = reader.ReadAssetArray<SerializedSubShader>();
			if (HasKeywordData(reader.Version))
			{
				reader.ReadStringArray(); //KeywordNames
				reader.AlignStream();
				reader.ReadByteArray(); //KeywordFlags
				reader.AlignStream();
			}
			Name = reader.ReadString();
			CustomEditorName = reader.ReadString();
			FallbackName = reader.ReadString();
			Dependencies = reader.ReadAssetArray<SerializedShaderDependency>();
			if (HasCustomEditorForRenderPipelines(reader.Version))
			{
				CustomEditorForRenderPipelines = reader.ReadAssetArray<SerializedCustomEditorForRenderPipeline>();
			}
			DisableNoSubshadersMessage = reader.ReadBoolean();
			reader.AlignStream();
		}
	}
}
