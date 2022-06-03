using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShader : IAssetReadable, IYamlExportable
	{
		public SerializedProperties PropInfo => m_PropInfo;
		private SerializedProperties m_PropInfo = new();
		public SerializedSubShader[] SubShaders { get; set; }

		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public string[] KeywordNames { get; set; }

		/// <summary>
		/// 2021.2 and greater
		/// </summary>
		public byte[] KeywordFlags { get; set; }

		public string NameString { get; set; }
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
				KeywordNames = reader.ReadStringArray();
				reader.AlignStream();
				KeywordFlags = reader.ReadByteArray();
				reader.AlignStream();
			}

			NameString = reader.ReadString();
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

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_PropInfo", m_PropInfo.ExportYaml(container));
			node.Add("m_SubShaders", SubShaders.ExportYaml(container));
			if (HasKeywordData(container.ExportVersion))
			{
				node.Add("m_KeywordNames", KeywordNames.ExportYaml());
				node.Add("m_KeywordFlags", KeywordFlags.ExportYaml());
			}

			node.Add("m_Name", NameString);
			node.Add("m_CustomEditorName", CustomEditorName);
			node.Add("m_FallbackName", FallbackName);
			node.Add("m_Dependencies", Dependencies.ExportYaml(container));
			if (HasCustomEditorForRenderPipelines(container.ExportVersion))
			{
				node.Add("m_CustomEditorForRenderPipelines", CustomEditorForRenderPipelines.ExportYaml(container));
			}

			node.Add("m_DisableNoSubshadersMessage", DisableNoSubshadersMessage);
			return node;
		}
	}
}
