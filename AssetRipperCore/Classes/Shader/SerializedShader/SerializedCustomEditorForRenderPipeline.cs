using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedCustomEditorForRenderPipeline : IAssetReadable, IYamlExportable
	{
		public string CustomEditorName { get; set; }
		public string RenderPipelineType { get; set; }

		public void Read(AssetReader reader)
		{
			CustomEditorName = reader.ReadAlignedString();
			RenderPipelineType = reader.ReadAlignedString();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("customEditorName", CustomEditorName);
			node.Add("renderPipelineType", RenderPipelineType);
			return node;
		}
	}
}
