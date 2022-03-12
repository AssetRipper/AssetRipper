using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedCustomEditorForRenderPipeline : IAssetReadable, IYAMLExportable
	{
		public string CustomEditorName { get; set; }
		public string RenderPipelineType { get; set; }

		public void Read(AssetReader reader)
		{
			CustomEditorName = reader.ReadAlignedString();
			RenderPipelineType = reader.ReadAlignedString();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("customEditorName", CustomEditorName);
			node.Add("renderPipelineType", RenderPipelineType);
			return node;
		}
	}
}
