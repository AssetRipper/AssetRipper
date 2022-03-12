using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedShader : IAssetReadable, ISerializedShader, IYAMLExportable
	{
		public ISerializedProperties PropInfo => m_PropInfo;
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
				KeywordNames = reader.ReadStringArray();
				reader.AlignStream();
				KeywordFlags = reader.ReadByteArray();
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_PropInfo", m_PropInfo.ExportYAML(container));
			node.Add("m_SubShaders", SubShaders.ExportYAML(container));
			if (HasKeywordData(container.ExportVersion))
			{
				node.Add("m_KeywordNames", KeywordNames.ExportYAML());
				node.Add("m_KeywordFlags", KeywordFlags.ExportYAML());
			}

			node.Add("m_Name", Name);
			node.Add("m_CustomEditorName", CustomEditorName);
			node.Add("m_FallbackName", FallbackName);
			node.Add("m_Dependencies", Dependencies.ExportYAML(container));
			if (HasCustomEditorForRenderPipelines(container.ExportVersion))
			{
				node.Add("m_CustomEditorForRenderPipelines", CustomEditorForRenderPipelines.ExportYAML(container));
			}

			node.Add("m_DisableNoSubshadersMessage", DisableNoSubshadersMessage);
			return node;
		}
	}
}
