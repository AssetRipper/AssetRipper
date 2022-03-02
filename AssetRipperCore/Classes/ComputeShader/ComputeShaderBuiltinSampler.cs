using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShaderBuiltinSampler : IAssetReadable, IYAMLExportable
	{
		public static bool HaUnsignedSampler(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 1);

		public void Read(AssetReader reader)
		{
			if (HaUnsignedSampler(reader.Version))
			{
				Sampler = reader.ReadUInt32();
			}
			else
			{
				SignedSampler = reader.ReadInt32();
			}

			BindPoint = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("sampler", HaUnsignedSampler(container.Version) ? Sampler : SignedSampler);
			node.Add("bindPoint", BindPoint);
			return node;
		}

		public uint Sampler { get; set; }
		public int SignedSampler { get; set; }
		public int BindPoint { get; set; }
	}
}
