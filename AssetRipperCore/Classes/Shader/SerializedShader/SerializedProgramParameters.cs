using AssetRipper.Core.Classes.Shader.Parameters;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProgramParameters : IAssetReadable, IYamlExportable
	{
		public VectorParameter[] VectorParams { get; set; }
		public MatrixParameter[] MatrixParams { get; set; }
		public TextureParameter[] TextureParams { get; set; }
		public BufferBinding[] BufferParams { get; set; }
		public ConstantBuffer[] ConstantBuffers { get; set; }
		public BufferBinding[] ConstantBufferBindings { get; set; }
		public UAVParameter[] UAVParams { get; set; }
		public SamplerParameter[] Samplers { get; set; }

		public void Read(AssetReader reader)
		{
			VectorParams = reader.ReadAssetArray<VectorParameter>();
			MatrixParams = reader.ReadAssetArray<MatrixParameter>();
			TextureParams = reader.ReadAssetArray<TextureParameter>();
			BufferParams = reader.ReadAssetArray<BufferBinding>();
			ConstantBuffers = reader.ReadAssetArray<ConstantBuffer>();
			ConstantBufferBindings = reader.ReadAssetArray<BufferBinding>();
			UAVParams = reader.ReadAssetArray<UAVParameter>();
			Samplers = reader.ReadAssetArray<SamplerParameter>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("m_VectorParams", VectorParams.ExportYaml(container));
			node.Add("m_MatrixParams", MatrixParams.ExportYaml(container));
			node.Add("m_TextureParams", TextureParams.ExportYaml(container));
			node.Add("m_BufferParams", BufferParams.ExportYaml(container));
			node.Add("m_ConstantBuffers", ConstantBuffers.ExportYaml(container));
			node.Add("m_ConstantBufferBindings", ConstantBufferBindings.ExportYaml(container));
			node.Add("m_UAVParams", UAVParams.ExportYaml(container));
			node.Add("m_Samplers", Samplers.ExportYaml(container));
			return node;
		}
	}
}
