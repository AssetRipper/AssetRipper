using AssetRipper.Core.Classes.Shader.Parameters;
using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public sealed class SerializedProgramParameters : IAssetReadable
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
	}
}
