using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderKernel : IAssetReadable, IYamlExportable
	{
		public static bool HasThreadGroupSize(UnityVersion version) => version.IsGreaterEqual(5, 4, 0, UnityVersionType.Beta, 22);
		public static bool HasPreprocessedSource(UnityVersion version) => version.IsLess(2020, 2, 0, UnityVersionType.Alpha, 15);
		public static bool HasCacheKey(UnityVersion version) => version.IsLess(2020, 3, 2, UnityVersionType.Final, 1) ||
		                                                        (version.IsGreaterEqual(2021, 1, 0, UnityVersionType.Alpha, 2) && version.IsLess(2021, 2, 0, UnityVersionType.Beta, 7));
		public static bool HasRequirements(UnityVersion version) => version.IsGreaterEqual(2021, 1, 0, UnityVersionType.Alpha, 2);

		public void Read(AssetReader reader)
		{
			Name = reader.ReadAsset<FastPropertyName>();
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
			if (HasThreadGroupSize(reader.Version))
			{
				ThreadGroupSize = reader.ReadUInt32Array();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add("name", Name.ExportYaml(container));
			node.Add("cbs", Cbs.ExportYaml(container));
			node.Add("textures", Textures.ExportYaml(container));
			node.Add("builtinSamplers", BuiltinSamplers.ExportYaml(container));
			node.Add("inBuffers", InBuffers.ExportYaml(container));
			node.Add("outBuffers", OutBuffers.ExportYaml(container));
			node.Add("code", Code.ExportYaml());
			if (HasThreadGroupSize(container.Version))
			{
				node.Add("threadGroupSize", ThreadGroupSize.ExportYaml(true));
			}
			//Editor-only
			if (HasPreprocessedSource(container.Version))
			{
				node.Add("preprocessedSource", string.Empty);
			}
			if (HasRequirements(container.Version))
			{
				node.Add("requirements", default(long));
			}
			node.Add("keywords", Array.Empty<string>().ExportYaml());
			if (HasCacheKey(container.Version))
			{
				node.Add("cacheKey", new Hash128().ExportYaml(container));
			}
			node.Add("isCompiled", false);

			return node;
		}

		public FastPropertyName Name { get; set; }
		public ComputeShaderResource[] Cbs { get; set; }
		public ComputeShaderResource[] Textures { get; set; }
		public ComputeShaderBuiltinSampler[] BuiltinSamplers { get; set; }
		public ComputeShaderResource[] InBuffers { get; set; }
		public ComputeShaderResource[] OutBuffers { get; set; }
		public byte[] Code { get; set; }
		public uint[] ThreadGroupSize { get; set; }
	}
}
