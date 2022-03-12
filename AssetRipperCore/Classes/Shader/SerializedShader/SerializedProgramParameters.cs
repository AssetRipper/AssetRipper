using AssetRipper.Core.Classes.Shader.Parameters;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProgramParameters : IAssetReadable, IYAMLExportable
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_VectorParams", VectorParams.ExportYAML(container));
			node.Add("m_MatrixParams", MatrixParams.ExportYAML(container));
			node.Add("m_TextureParams", TextureParams.ExportYAML(container));
			node.Add("m_BufferParams", BufferParams.ExportYAML(container));
			node.Add("m_ConstantBuffers", ConstantBuffers.ExportYAML(container));
			node.Add("m_ConstantBufferBindings", ConstantBufferBindings.ExportYAML(container));
			node.Add("m_UAVParams", UAVParams.ExportYAML(container));
			node.Add("m_Samplers", Samplers.ExportYAML(container));
			return node;
		}
	}
}
