using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderKernel : IAssetReadable, IYAMLExportable
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name.ExportYAML(container));
			node.Add("cbs", Cbs.ExportYAML(container));
			node.Add("textures", Textures.ExportYAML(container));
			node.Add("builtinSamplers", BuiltinSamplers.ExportYAML(container));
			node.Add("inBuffers", InBuffers.ExportYAML(container));
			node.Add("outBuffers", OutBuffers.ExportYAML(container));
			node.Add("code", Code.ExportYAML());
			if (HasThreadGroupSize(container.Version))
			{
				node.Add("threadGroupSize", ThreadGroupSize.ExportYAML(true));
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
			node.Add("keywords", Array.Empty<string>().ExportYAML());
			if (HasCacheKey(container.Version))
			{
				node.Add("cacheKey", new Hash128().ExportYAML(container));
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
