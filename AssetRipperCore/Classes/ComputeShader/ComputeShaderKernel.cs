using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class  ComputeShaderKernel : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Cbs = reader.ReadAssetArray<ComputeShaderResource>();
			reader.AlignStream();
			Textures = reader.ReadAssetArray<ComputeShaderResource>();
			reader.AlignStream();
			BuiltinSamplers = reader.ReadAssetArray<ComputeShaderBuiltinSampler>();
			reader.AlignStream();
			InBuffers = reader.ReadAssetArray<ComputeShaderResource>();
			reader.AlignStream();
			OutBuffers = reader.ReadAssetArray<ComputeShaderResource>();
			reader.AlignStream();
			Code = reader.ReadByteArray();
			reader.AlignStream();
			ThreadGroupSize = reader.ReadUInt32Array();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name);
			node.Add("cbs", Cbs.ExportYAML(container));
			node.Add("textures", Textures.ExportYAML(container));
			node.Add("builtinSamplers", BuiltinSamplers.ExportYAML(container));
			node.Add("inBuffers", InBuffers.ExportYAML(container));
			node.Add("outBuffers", OutBuffers.ExportYAML(container));
			node.Add("code", Code.ExportYAML());
			node.Add("threadGroupSize", ThreadGroupSize.ExportYAML(true));
			return node;
		}

		public string Name { get; set; }
		public ComputeShaderResource[] Cbs { get; set; }
		public ComputeShaderResource[] Textures { get; set; }
		public ComputeShaderBuiltinSampler[] BuiltinSamplers { get; set; }
		public ComputeShaderResource[] InBuffers { get; set; }
		public ComputeShaderResource[] OutBuffers { get; set; }
		public byte[] Code { get; set; }
		public uint[] ThreadGroupSize { get; set; }
	}
}
